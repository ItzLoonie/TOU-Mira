using System.Globalization;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class SeerRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public enum Prophecy
    {
        ImpostorKilling,
        ImpostorConcealing,
        ImpostorSupport,
        NeutralEvil,
        NeutralBenign,
        NeutralKilling
    }
    public Prophecy CurrentProphecy { get; private set; } = Prophecy.ImpostorKilling;
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string RoleName => "Seer";
    public string RoleDescription => "Compare Two Players For An Evil Alignment";
    public string RoleLongDescription => "Select two players and compare them with your selected Prophecy";
    public Color RoleColor => TownOfUsColors.Seer;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Seer,
        IntroSound = TouAudio.QuestionSound
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        stringB.Append(CultureInfo.InvariantCulture, $"\n<b>Prophecy:</b> {CurrentProphecy.GetProphecyString()}");

        return stringB;
    }

    public string GetAdvancedDescription()
    {
        return "The Seer is a Crewmate Investigative role that can compare two players to see if they match their Prophecy."
               + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Compare",
            "Compare two players with your selected Prophecy.\nIf at least one of them is your Prophecy, you will be informed.\n\n<b>You are not told which one matches your Prophecy.</b>",
            TouCrewAssets.SeerSprite),
        new("Change Prophecy",
            "Cycle your prophecy between all Impostor and Neutral alignments.",
            TouCrewAssets.ProphecySprite)
    ];

    [MethodRpc((uint)TownOfUsRpc.Compare, SendImmediately = true)]
    public static void RpcCompare(PlayerControl seer, byte player1, byte player2)
    {
        if (seer.Data.Role is not SeerRole seerRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcCompare - Invalid Seer");
            return;
        }

        var t1 = GetTarget(player1);
        var t2 = GetTarget(player2);

        if (t1 == null || t2 == null)
        {
            if (seer.AmOwner)
            {
                Coroutines.Start(MiscUtils.CoFlash(Color.red));
                ShowNotification($"<b>You need to pick a second target.</b>");
            }
            return;
        }

        if (t1 == seer || t2 == seer)
        {
            Coroutines.Start(MiscUtils.CoFlash(Color.red));
            ShowNotification($"<b>You can't compare yourself!</b>");
            return;
        }

        var play1 = MiscUtils.PlayerById(player1)!;
        var play2 = MiscUtils.PlayerById(player2)!;

        if (play1.TryGetModifier<InvulnerabilityModifier>(out var invic) && invic.AttackAllInteractions)
        {
            if (seer.AmOwner) play1.RpcCustomMurder(seer);
            return;
        }

        if (play2.TryGetModifier<InvulnerabilityModifier>(out var invic2) && invic2.AttackAllInteractions)
        {
            if (seer.AmOwner) play2.RpcCustomMurder(seer);
            return;
        }

        if (play1.HasModifier<VeteranAlertModifier>())
        {
            if (seer.AmOwner) play1.RpcCustomMurder(seer);
            return;
        }

        if (play2.HasModifier<VeteranAlertModifier>())
        {
            if (seer.AmOwner) play2.RpcCustomMurder(seer);
            return;
        }

        if (seer.AmOwner)
        {
            var button = CustomButtonSingleton<Seer1CompareButton>.Instance;
            button.ResetCooldownAndOrEffect();
        }


        var prophecy = seerRole.CurrentProphecy;
        var playerA = play1.CachedPlayerData.PlayerName;
        var playerB = play2.CachedPlayerData.PlayerName;

        void ShowNotification(string message)
        {
            if (seer.AmOwner)
            {
                var notif = Helpers.CreateAndShowNotification(
                    message, Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Seer.LoadAsset());
                notif.Text.SetOutlineThickness(0.35f);
            }
        }

        bool matchFound = false;
        switch (prophecy)
        {
            case Prophecy.ImpostorKilling:
                if (play1.Is(RoleAlignment.ImpostorKilling) || play2.Is(RoleAlignment.ImpostorKilling))
                    matchFound = true;
                break;
            // case Prophecy.ImpostorPower:
            //     if (play1.Is(RoleAlignment.ImpostorPower) || play2.Is(RoleAlignment.ImpostorPower))
            //         matchFound = true;
            //     break;
            case Prophecy.ImpostorSupport:
                if (play1.Is(RoleAlignment.ImpostorSupport) || play2.Is(RoleAlignment.ImpostorSupport))
                    matchFound = true;
                break;
            case Prophecy.ImpostorConcealing:
                if (play1.Is(RoleAlignment.ImpostorConcealing) || play2.Is(RoleAlignment.ImpostorConcealing))
                    matchFound = true;
                break;
            case Prophecy.NeutralBenign:
                if (play1.Is(RoleAlignment.NeutralBenign) || play2.Is(RoleAlignment.NeutralBenign))
                    matchFound = true;
                break;
            case Prophecy.NeutralEvil:
                if (play1.Is(RoleAlignment.NeutralEvil) || play2.Is(RoleAlignment.NeutralEvil))
                    matchFound = true;
                break;
            case Prophecy.NeutralKilling:
                if (play1.Is(RoleAlignment.NeutralKilling) || play2.Is(RoleAlignment.NeutralKilling))
                    matchFound = true;
                break;
        }

        if (matchFound)
        {
            ShowNotification($"<b>{Palette.ImpostorRed.ToTextColor()}At least one of {playerA} and {playerB} matches your {prophecy.GetProphecyString()} prophecy!</color></b>");
        }
        else
        {
            ShowNotification($"<b>{Palette.CrewmateBlue.ToTextColor()}Neither {playerA} nor {playerB} matches your {prophecy.GetProphecyString()} prophecy!</color></b>");
        }

        static MonoBehaviour? GetTarget(byte id)
        {
            var data = GameData.Instance.GetPlayerById(id);
            if (!data)
            {
                return null;
            }

            var body = Helpers.GetBodyById(id);
            if (data.IsDead && body)
            {
                return body;
            }

            var pc = data.Object;
            if (!pc)
            {
                return null;
            }

            return pc;
        }
    }
    [MethodRpc((uint)TownOfUsRpc.Prophecy, SendImmediately = true)]
    public static void RpcProphecy(PlayerControl seer)
    {
        if (seer.Data.Role is not SeerRole seerRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcProphecy - Invalid Seer");
            return;
        }

        seerRole.ChangeProphecy();
    }

    public void ChangeProphecy()
    {
        var values = (Prophecy[])Enum.GetValues(typeof(Prophecy));
        int currentIndex = Array.IndexOf(values, CurrentProphecy);
        int nextIndex = (currentIndex + 1) % values.Length;
        CurrentProphecy = values[nextIndex];

        if (PlayerControl.LocalPlayer.AmOwner)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>Prophecy has been changed to {CurrentProphecy.GetProphecyString()}</color></b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Seer.LoadAsset());
            notif1.Text.SetOutlineThickness(0.35f);
        }
    }
}

public static class ProphecyExtensions
{
    public static string GetProphecyString(this SeerRole.Prophecy prophecy)
    {
        return prophecy switch
        {
            SeerRole.Prophecy.ImpostorKilling => "Impostor Killing",
            // SeerRole.Prophecy.ImpostorPower => "Impostor Power",
            SeerRole.Prophecy.ImpostorConcealing => "Impostor Concealing",
            SeerRole.Prophecy.ImpostorSupport => "Impostor Support",
            SeerRole.Prophecy.NeutralEvil => "Neutral Evil",
            SeerRole.Prophecy.NeutralBenign => "Neutral Benign",
            SeerRole.Prophecy.NeutralKilling => "Neutral Killing",
            _ => "Unknown"
        };
    }
}
