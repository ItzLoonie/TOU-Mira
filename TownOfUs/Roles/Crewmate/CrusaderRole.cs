using System.Globalization;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class CrusaderRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;

    public PlayerControl? Fortified { get; set; }

    public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not CrusaderRole)
        {
            return;
        }

        if (Fortified != null && Fortified.HasDied())
        {
            Clear();
        }
    }

    public DoomableType DoomHintType => DoomableType.Protective;
    public string RoleName => "Crusader";
    public string RoleDescription => "Fortify crewmates";
    public string RoleLongDescription => "Fortify crewmates to kill anyone interacting with them";
    public Color RoleColor => TownOfUsColors.Crusader;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateProtective;

    public CustomRoleConfiguration Configuration => new(this)
    {
        // IntroSound = TouAudio.SpyIntroSound,
        Icon = TouRoleIcons.Crusader
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        if (Fortified != null)
        {
            stringB.Append(CultureInfo.InvariantCulture,
                $"\n<b>Fortified: </b>{Color.white.ToTextColor()}{Fortified.Data.PlayerName}</color>");
        }

        return stringB;
    }

    public string GetAdvancedDescription()
    {
        return
            "The Crusader is a Crewmate Protective role that can fortify players to kill anyone who interacts with their fortified target. "
            + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Fortify",
            "Fortify a player to prevent them from being interacted with. If anyone tries to interact with a fortified player, the ability will fail and the Crusader will kill the source of the interaction.",
            TouCrewAssets.FortifySprite)
    ];

    public void Clear()
    {
        SetFortifiedPlayer(null);
    }

    public override void OnDeath(DeathReason reason)
    {
        RoleBehaviourStubs.OnDeath(this, reason);

        Clear();
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        Clear();
    }

    public void SetFortifiedPlayer(PlayerControl? player)
    {
        Fortified?.RemoveModifier<CrusaderFortifiedModifier>();

        Fortified = player;

        Fortified?.AddModifier<CrusaderFortifiedModifier>(Player);
    }

    [MethodRpc((uint)TownOfUsRpc.CrusaderFortify, SendImmediately = true)]
    public static void RpcCrusaderFortify(PlayerControl player, PlayerControl target)
    {
        if (player.Data.Role is not CrusaderRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcCrusaderFortify - Invalid Crusader");
            return;
        }

        var Crusader = player.GetRole<CrusaderRole>();
        Crusader?.SetFortifiedPlayer(target);
    }

    [MethodRpc((uint)TownOfUsRpc.ClearCrusaderFortify, SendImmediately = true)]
    public static void RpcClearCrusaderFortify(PlayerControl player)
    {
        if (player.Data.Role is not CrusaderRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcClearCrusaderFortify - Invalid Crusader");
            return;
        }

        var Crusader = player.GetRole<CrusaderRole>();
        Crusader?.SetFortifiedPlayer(null);
    }

    [MethodRpc((uint)TownOfUsRpc.CrusaderNotify, SendImmediately = true)]
    public static void RpcCrusaderNotify(PlayerControl player, PlayerControl source, PlayerControl target)
    {
        if (player.Data.Role is not CrusaderRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcCrusaderNotify - Invalid Crusader");
            return;
        }

        // Logger<TownOfUsPlugin>.Error("RpcCrusaderNotify");
        if (player.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Crusader));
        }

        if (source.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Crusader));
        }
    }
}