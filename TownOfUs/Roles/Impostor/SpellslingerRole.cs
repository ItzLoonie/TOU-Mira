using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Modifiers.Impostor;
using MiraAPI.Networking;

namespace TownOfUs.Roles.Impostor;

public sealed class SpellslingerRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string RoleName => "Spellslinger";
    public string RoleDescription => "Hex everyone then detonate them all";
    public string RoleLongDescription => "Hex all non-Impostors to set them off in a Hex Bomb";
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Spellslinger,
        MaxRoleCount = 1,
        IntroSound = TouAudio.ArsoIgniteSound,
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            "The Spellslinger is an Impostor Killing role that can hex a player, priming them for detonation.\n\nOnce all non Impostors are hexed, the Spellslinger can detonate them all."
            + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new CustomButtonWikiDescription(
            "Hex",
            $"Hex a player, priming them for detonation.",
            TouImpAssets.HexButtonSprite),

        new CustomButtonWikiDescription(
            "Detonate",
            $"Kill all hexed players.\n\nDetonate may only be used if everyone is hexed.",
            TouImpAssets.DetonateButtonSprite)
    ];

    [MethodRpc((uint)TownOfUsRpc.Hex, SendImmediately = true)]
    public static void RpcHex(PlayerControl player, PlayerControl target)
    {
        var canBeHexed = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied() && !x.IsImpostor()).ToList();

        if (player.Data.Role is not SpellslingerRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcHex - Invalid Spellslinger");
            return;
        }

        if (canBeHexed.Contains(target))
        {
            target.AddModifier<SpellslingerHexedModifier>();

            if (player.AmOwner)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{target.CachedPlayerData.PlayerName} is hexed!</b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Spellslinger.LoadAsset());
                notif1.Text.SetOutlineThickness(0.35f);
            }

            return;
        }

        if (player.AmOwner)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{target.CachedPlayerData.PlayerName} could not be hexed! <color=#ff0000>(A bug occurred)</color></b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Spellslinger.LoadAsset());
            notif1.Text.SetOutlineThickness(0.35f);
        }

    }

    [MethodRpc((uint)TownOfUsRpc.Detonate, SendImmediately = true)]
    public static void RpcDetonate(PlayerControl player)
    {
        if (player.Data.Role is not SpellslingerRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcDetonate - Invalid Spellslinger");
            return;
        }

        var hexed = PlayerControl.AllPlayerControls.ToArray()
            .Where(p => !p.HasDied() && p.HasModifier<SpellslingerHexedModifier>())
            .ToList();

        if (hexed.Count == 0)
        {
            if (player.AmOwner)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>Nobody is hexed?? <color=#ff0000>(A bug occurred)</color></b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Spellslinger.LoadAsset());
                notif1.Text.SetOutlineThickness(0.35f);
            }
            return;
        }

        TouAudio.PlaySound(TouAudio.ArsoIgniteSound);
        foreach (var target in hexed)
        {
            player.RpcCustomMurder(target, teleportMurderer: false, showKillAnim: false, playKillSound: false);
            target.RemoveModifier<SpellslingerHexedModifier>();

            if (player.AmOwner && target == player)
            {
                var selfNotif = Helpers.CreateAndShowNotification(
                    $"<b>You detonated... yourself?</b>",
                    Color.red, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Spellslinger.LoadAsset());
                selfNotif.Text.SetOutlineThickness(0.4f);
            }
        }

        if (player.AmOwner)
        {
            var notif = Helpers.CreateAndShowNotification(
                $"<b>All {hexed.Count} hexed players detonated!</b>", 
                Color.magenta, new Vector3(0f, 1f, -20f), 
                spr: TouRoleIcons.Spellslinger.LoadAsset());
            notif.Text.SetOutlineThickness(0.4f);
        }

    }

    public static bool EveryoneHexed()
    {
        return PlayerControl.AllPlayerControls
            .ToArray()
            .Where(p => !p.HasDied() && !p.IsImpostor())
            .All(p => p.HasModifier<SpellslingerHexedModifier>());
    }

}