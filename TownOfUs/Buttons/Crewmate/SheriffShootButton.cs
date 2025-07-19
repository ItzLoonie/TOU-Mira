﻿using System.Collections;
using Il2CppInterop.Runtime;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class SheriffShootButton : TownOfUsRoleButton<SheriffRole, PlayerControl>, IKillButton
{
    public override string Name => "Shoot";
    public override string Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Sheriff;
    public override float Cooldown => OptionGroupSingleton<SheriffOptions>.Instance.KillCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.SheriffShootSprite;

    public bool Usable { get; set; } =
        OptionGroupSingleton<SheriffOptions>.Instance.FirstRoundUse || TutorialManager.InstanceExists;

    public bool FailedShot { get; set; }

    public override bool CanUse()
    {
        return base.CanUse() && Usable && !FailedShot;
    }

    private void Misfire()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Misfire: Target is null");
            return;
        }

        var missType = OptionGroupSingleton<SheriffOptions>.Instance.MisfireType;
        SheriffRole.RpcSheriffMisfire(PlayerControl.LocalPlayer);

        if (missType is MisfireOptions.Target or MisfireOptions.Both)
        {
            PlayerControl.LocalPlayer.RpcCustomMurder(Target);
        }

        if (missType is MisfireOptions.Sheriff or MisfireOptions.Both)
        {
            PlayerControl.LocalPlayer.RpcCustomMurder(PlayerControl.LocalPlayer);
            DeathHandlerModifier.RpcUpdateDeathHandler(PlayerControl.LocalPlayer, "Suicide", DeathEventHandlers.CurrentRound, DeathHandlerOverride.SetTrue, lockInfo: DeathHandlerOverride.SetTrue);
        }

        FailedShot = true;

        var notif1 = Helpers.CreateAndShowNotification("<b>You have lost the ability to shoot!</b>", Color.white,
            spr: TouRoleIcons.Sheriff.LoadAsset());

        notif1.Text.SetOutlineThickness(0.35f);
        notif1.transform.localPosition = new Vector3(0f, 1f, -20f);

        Coroutines.Start(MiscUtils.CoFlash(Color.red));
    }

    private static IEnumerator CoSetBodyReportable(byte bodyId)
    {
        var waitDelegate =
            DelegateSupport.ConvertDelegate<Il2CppSystem.Func<bool>>(() => Helpers.GetBodyById(bodyId) != null);
        yield return new WaitUntil(waitDelegate);
        var body = Helpers.GetBodyById(bodyId);

        if (body != null)
        {
            body.gameObject.layer = LayerMask.NameToLayer("Ship");
            body.Reported = true;
        }
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Sheriff Shoot: Target is null");
            return;
        }

        if (Target.HasModifier<FirstDeadShield>())
        {
            return;
        }

        if (Target.HasModifier<BaseShieldModifier>())
        {
            return;
        }

        var alignment = RoleAlignment.CrewmateSupport;
        var options = OptionGroupSingleton<SheriffOptions>.Instance;

        if (Target.Data.Role is ITownOfUsRole touRole)
        {
            alignment = touRole.RoleAlignment;
        }
        else if (Target.IsImpostor())
        {
            alignment = RoleAlignment.ImpostorSupport;
        }

        if (!(PlayerControl.LocalPlayer.TryGetModifier<AllianceGameModifier>(out var allyMod) &&
              !allyMod.GetsPunished) &&
            !(Target.TryGetModifier<AllianceGameModifier>(out var allyMod2) && !allyMod2.GetsPunished))
        {
            switch (alignment)
            {
                case RoleAlignment.ImpostorConcealing:
                case RoleAlignment.ImpostorKilling:
                case RoleAlignment.ImpostorSupport:
                    PlayerControl.LocalPlayer.RpcCustomMurder(Target);
                    break;

                case RoleAlignment.NeutralBenign:
                case RoleAlignment.CrewmateInvestigative:
                case RoleAlignment.CrewmateKilling:
                case RoleAlignment.CrewmateProtective:
                case RoleAlignment.CrewmatePower:
                case RoleAlignment.CrewmateSupport:
                    Misfire();
                    break;

                case RoleAlignment.NeutralKilling:
                    if (!options.ShootNeutralKiller)
                    {
                        Misfire();
                    }
                    else
                    {
                        PlayerControl.LocalPlayer.RpcCustomMurder(Target);
                    }

                    break;

                case RoleAlignment.NeutralEvil:
                    if (!options.ShootNeutralEvil)
                    {
                        Misfire();
                    }
                    else
                    {
                        PlayerControl.LocalPlayer.RpcCustomMurder(Target);
                    }

                    break;
                default:
                    Misfire();
                    break;
            }
        }
        else
        {
            PlayerControl.LocalPlayer.RpcCustomMurder(Target);
        }

        if (!OptionGroupSingleton<SheriffOptions>.Instance.SheriffBodyReport)
        {
            Coroutines.Start(CoSetBodyReportable(Target.PlayerId));
        }
    }

    public override PlayerControl? GetTarget()
    {
        if (!OptionGroupSingleton<LoversOptions>.Instance.LoversKillEachOther && PlayerControl.LocalPlayer.IsLover())
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false, x => !x.IsLover());
        }
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }
}