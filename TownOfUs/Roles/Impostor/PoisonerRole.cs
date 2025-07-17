using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class PoisonerRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    [HideFromIl2Cpp] public Bomb? Bomb { get; set; }

    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<TrapperRole>());
    public DoomableType DoomHintType => DoomableType.Trickster;
    public string RoleName => "Poisoner";
    public string RoleDescription => "Kill crewmates with poison";
    public string RoleLongDescription => "Use your poison on crewmates to kill them with a delay";
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Poisoner,
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            $"The Poisoner is an Impostor Killing role that can poison a player, killing them after {OptionGroupSingleton<PoisonerOptions>.Instance.PoisonDelay} second(s)" +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Poison",
            $"Poison someone, killing them {OptionGroupSingleton<PoisonerOptions>.Instance.PoisonDelay} second(s) later",
            TouImpAssets.PlaceSprite)
    ];

    [MethodRpc((uint)TownOfUsRpc.Poison, SendImmediately = true)]
    public static void RpcPoison(PlayerControl player, PlayerControl target)
    {
        var targetName = target.CachedPlayerData.PlayerName;
        var icon = TouRoleIcons.Poisoner.LoadAsset();

        if (target.HasDied())
        {
            if (player.AmOwner)
            {
                ShowNotification($"{targetName} died before your poison could take effect.");
            }
            return;
        }

        if (target.HasModifier<BaseShieldModifier>())
        {
            if (player.AmOwner)
            {
                ShowNotification($"{targetName} was protected from your poison!");
            }
            return;
        }

        if (target.HasModifier<FirstDeadShield>())
        {
            if (player.AmOwner)
            {
                ShowNotification($"{targetName}'s first dead shield protected them from your poison!");
            }
            return;
        }

        
        if (player.AmOwner)
        {
            ShowNotification($"{targetName} died to your poison!");
        }

        if (target.AmOwner)
        {
            ShowNotification($"You died to poison!");
        }

        player?.RpcCustomMurder(target, teleportMurderer: false);

        void ShowNotification(string message)
        {
            var notif = Helpers.CreateAndShowNotification($"<b>{message}</b>", Color.white, new Vector3(0f, 1f, -20f), spr: icon);
            notif.Text.SetOutlineThickness(0.35f);
        }
    }

}