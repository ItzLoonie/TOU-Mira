using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Modifiers.Game.Impostor;
using TownOfUs.Modifiers.Game.Neutral;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Modifiers.Crewmate;

namespace TownOfUs.Roles.Impostor;

public sealed class CultistRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Perception;
    public string RoleName => "Cultist";
    public string RoleDescription => "Convert a crewmate into an Impostor";
    public string RoleLongDescription => "Turn a crewmate into an Impostor to gain a new member";
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Cultist,
        MaxRoleCount = 1,
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            "The Cultist is an Impostor Support role that can indoctrinate a crewmate, turning them into a Traitor."
            + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities => new()
    {
        new CustomButtonWikiDescription(
            "Indoctrinate",
            $"Indoctrinate a player, turning them into a Traitor.\n\nYou cannot indoctrinate a revealed Mayor.\nYou {(OptionGroupSingleton<CultistOptions>.Instance.IndoctrinateNeutralBenign ? "can" : "cannot")} convert Neutral Benigns.\nYou {(OptionGroupSingleton<CultistOptions>.Instance.IndoctrinateNeutralEvil ? "can" : "cannot")} convert Neutral Evils.\nYou {(OptionGroupSingleton<CultistOptions>.Instance.IndoctrinateNeutralKiller ? "can" : "cannot")} convert Neutral Killers.",
            TouImpAssets.IndoctrinateButtonSprite)
    };

    [MethodRpc((uint)TownOfUsRpc.Indoctrinate, SendImmediately = true)]
    public static void RpcIndoctrinate(PlayerControl player, PlayerControl target)
    {
        var convertable = PlayerControl.AllPlayerControls.ToArray()
        .Where(x =>
            !x.HasDied() &&
            (
                (x.IsCrewmate() && !x.IsRole<MayorRole>()) ||
                (x.Is(RoleAlignment.NeutralBenign) && OptionGroupSingleton<CultistOptions>.Instance.IndoctrinateNeutralBenign) ||
                (x.Is(RoleAlignment.NeutralEvil) && OptionGroupSingleton<CultistOptions>.Instance.IndoctrinateNeutralEvil) ||
                (x.Is(RoleAlignment.NeutralKilling) && OptionGroupSingleton<CultistOptions>.Instance.IndoctrinateNeutralKiller)
            ))
        .ToList();


        if (player.Data.Role is not CultistRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcIndoctrinate - Invalid Cultist");
            return;
        }

        if (convertable.Contains(target))
        {
            target.ChangeRole(RoleId.Get<TraitorRole>());
            target.SetKillTimer(PlayerControl.LocalPlayer.GetKillCooldown());
            target.RemoveModifier<NeutralKillerAssassinModifier>();
            // player.ChangeRole((ushort)RoleTypes.Impostor);
            player.SetKillTimer(PlayerControl.LocalPlayer.GetKillCooldown());

            if (target.HasModifier<ToBecomeTraitorModifier>())
            {
                target.GetModifier<ToBecomeTraitorModifier>()!.Clear();
            }


            if (player.AmOwner)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>You indoctrinated {target.CachedPlayerData.PlayerName} and is now a {TownOfUsColors.Impostor.ToTextColor()}Traitor</color>!</b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Cultist.LoadAsset());
                notif1.Text.SetOutlineThickness(0.35f);
            }

            if (target.AmOwner)
            {
                var notif2 = Helpers.CreateAndShowNotification(
                    $"<b>A {TownOfUsColors.Impostor.ToTextColor()}Cultist</color> indoctrinated you. You are now a {TownOfUsColors.Impostor.ToTextColor()}Traitor</color>!</b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Cultist.LoadAsset());
                notif2.Text.SetOutlineThickness(0.35f);
            }

            if (OptionGroupSingleton<CultistOptions>.Instance.IndoctrinateCanGuess)
            {
                target.AddModifier<ImpostorAssassinModifier>();
            }

            return;
        }

        if (player.AmOwner)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{target.CachedPlayerData.PlayerName} is immune to indoctrination.</b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Cultist.LoadAsset());
            notif1.Text.SetOutlineThickness(0.35f);
        }

    }

}