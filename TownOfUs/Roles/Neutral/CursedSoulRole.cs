using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Game.Impostor;
using TownOfUs.Modifiers.Game.Neutral;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class CursedSoulRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<MysticRole>());
    public DoomableType DoomHintType => DoomableType.Death;
    public string RoleName => "Cursed Soul";
    public string RoleDescription => "Swap souls before the game ends!";
    public string RoleLongDescription => "Steal someone's role before the game ends";
    public Color RoleColor => TownOfUsColors.CursedSoul;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = TouAudio.GlitchSound,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        Icon = TouRoleIcons.CursedSoul
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            "The Cursed Soul is a Neutral Evil role that gains access to a new role by stealing it from another player. Use the role you steal to win the game.\nBeware though, the player you steal from becomes a Cursed Soul!\n\nOther Cursed Souls cannot be swapped with." +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Soul Swap",
            $"Steal the role of another player. The player you steal from will become a Cursed Soul.\n\nBe careful though, you have a {(int)OptionGroupSingleton<CursedSoulOptions>.Instance.SoulSwapRandomness}% chance to swap with someone random!",
            TouNeutAssets.SoulSwapButtonSprite)
    ];


    public override bool DidWin(GameOverReason gameOverReason)
    {
        return false;
    }

    [MethodRpc((uint)TownOfUsRpc.SoulSwap, SendImmediately = true)]
    public static void RpcSoulSwap(PlayerControl player, PlayerControl target)
    {
        var reset = OptionGroupSingleton<CursedSoulOptions>.Instance.KillCooldownOnSoulSwap;
        var swappable = PlayerControl.AllPlayerControls.ToArray()
            .Where(x =>
                !x.HasDied() &&
                !x.IsRole<CursedSoulRole>())
            .ToList();

        if (player.Data.Role is not CursedSoulRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcSoulSwap - Invalid CursedSoul");
            return;
        }

        var randomness = OptionGroupSingleton<CursedSoulOptions>.Instance.SoulSwapRandomness;
        var rng = UnityEngine.Random.Range(0f, 100f);

        if (rng < randomness)
        {
            swappable.Remove(target);
            if (swappable.Count > 0)
            {
                target = swappable[UnityEngine.Random.Range(0, swappable.Count)];
            }
        }
        else
        {
            if (!swappable.Contains(target) && swappable.Count > 0)
            {
                target = swappable[UnityEngine.Random.Range(0, swappable.Count)];
            }
        }

        var roleWhenAlive = target.GetRoleWhenAlive();

        var touAbilityEvent = new TouAbilityEvent(AbilityType.CursedSoulSoulSwap, player, target);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        target.ChangeRole(RoleId.Get<CursedSoulRole>());
        player.ChangeRole((ushort)roleWhenAlive.Role);

        if (player.Data.Role is VigilanteRole or DoomsayerRole)
        {
            player.RemoveModifier<NeutralKillerAssassinModifier>();
            player.RemoveModifier<ImpostorAssassinModifier>();
        }

        player.SetKillTimer(reset);


        if (player.Data.Role is InquisitorRole inquis)
        {
            inquis.Targets = [.. ModifierUtils.GetPlayersWithModifier<InquisitorHereticModifier>()];
            inquis.TargetRoles = [.. ModifierUtils.GetActiveModifiers<InquisitorHereticModifier>().Select(x => x.TargetRole).OrderBy(x => x.NiceName)];
        }

        if (player.Data.Role is MayorRole mayor)
        {
            mayor.Revealed = true;
        }

        if (player.Data.Role is ExecutionerRole exe)
        {
            if (exe.Target == null)
            {
                exe.AssignTargets();
            }

            if (exe.Target != null)
            {
                exe.CheckTargetDeath(exe.Target);
            }
        }

        if (player.Data.Role is GuardianAngelTouRole ga)
        {
            if (ga.Target == null)
            {
                ga.AssignTargets();
            }

            if (ga.Target != null)
            {
                ga.CheckTargetDeath(player, ga.Target);
            }
        }

        if (player.AmOwner)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>You swapped souls with someone and got {player.Data.Role.TeamColor.ToTextColor()}{player.Data.Role.NiceName}</color>!</b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.CursedSoul.LoadAsset());
            notif1.Text.SetOutlineThickness(0.35f);
        }

        if (target.AmOwner)
        {
            var notif2 = Helpers.CreateAndShowNotification(
                $"<b>A {TownOfUsColors.CursedSoul.ToTextColor()}Cursed Soul</color> has swapped souls with you!</b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.CursedSoul.LoadAsset());
            notif2.Text.SetOutlineThickness(0.35f);
        }
    }



}