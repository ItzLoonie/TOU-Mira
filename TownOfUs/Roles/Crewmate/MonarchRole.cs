using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using TownOfUs.Modifiers;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class MonarchRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string RoleName => "Monarch";
    public string RoleDescription => "Knight Players To Give Extra Votes";
    public string RoleLongDescription => "Knight crewmates to aid in meetings, but don't knight Impostors";
    public Color RoleColor => TownOfUsColors.Monarch;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Monarch,
        IntroSound = TouAudio.ToppatIntroSound,
        MaxRoleCount = 1
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return $"The Monarch is a Crewmate Power role that can knight other players, granting {(int)OptionGroupSingleton<MonarchOptions>.Instance.VotesPerKnight} vote(s) each."
               + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Knight",
            $"Knight a player, giving them {(int)OptionGroupSingleton<MonarchOptions>.Instance.VotesPerKnight} vote(s)",
            TouCrewAssets.KnightButtonSprite)
    ];

    [MethodRpc((uint)TownOfUsRpc.Knight, SendImmediately = true)]
    public static void RpcKnight(PlayerControl player, PlayerControl target)
    {
        var targetName = target.CachedPlayerData.PlayerName;
        var icon = TouRoleIcons.Monarch.LoadAsset();

        if (target.HasDied())
        {
            if (player.AmOwner)
            {
                ShowNotification($"{targetName} died before you could knight them.");
            }
            return;
        }

        target.AddModifier<KnightedModifier>();

        if (player.AmOwner)
        {
            ShowNotification($"{targetName} was knighted!");
        }

        if (target.AmOwner)
        {
            ShowNotification($"You were knighted by a {TownOfUsColors.Monarch.ToTextColor()}Monarch</color>. You gained {(int)OptionGroupSingleton<MonarchOptions>.Instance.VotesPerKnight} vote(s)!");
        }


        void ShowNotification(string message)
        {
            var notif = Helpers.CreateAndShowNotification($"<b>{message}</b>", Color.white, new Vector3(0f, 1f, -20f), spr: icon);
            notif.Text.SetOutlineThickness(0.35f);
        }
    }

}