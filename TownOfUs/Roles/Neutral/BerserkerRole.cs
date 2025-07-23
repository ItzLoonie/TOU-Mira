using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class BerserkerRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public int KillCount { get; set; }
    private bool roleChangePending;
    private float roleChangeTimer;
    public DoomableType DoomHintType => DoomableType.Relentless;
    public string RoleName => "Berserker";
    public string RoleDescription => $"Kill {(int)OptionGroupSingleton<BerserkerOptions>.Instance.KillsForWar} players to become {TownOfUsColors.War.ToTextColor()}War</color>";
    public string RoleLongDescription => $"Kill {(int)OptionGroupSingleton<BerserkerOptions>.Instance.KillsForWar} players to transform into War, Horseman of the Apocalypse!";
    public Color RoleColor => TownOfUsColors.Berserker;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseVent = KillCount >= (int)OptionGroupSingleton<BerserkerOptions>.Instance.KillsForVent,
        IntroSound = TouAudio.WarlockIntroSound,
        Icon = TouRoleIcons.Berserker,
        MaxRoleCount = 1,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public bool HasImpostorVision => KillCount >= (int)OptionGroupSingleton<BerserkerOptions>.Instance.KillsForVision;

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        stringB.Append(CultureInfo.InvariantCulture, $"\n<b>Kill Count:</b> {KillCount}/{(int)OptionGroupSingleton<BerserkerOptions>.Instance.KillsForWar}");

        return stringB;
    }

    public bool WinConditionMet()
    {
        if (Player.HasDied())
        {
            return false;
        }

        var result = Helpers.GetAlivePlayers().Count <= 2 && MiscUtils.KillersAliveCount == 1;
        return result;
    }

    public string GetAdvancedDescription()
    {
        return
            $"The Berserker is a Neutral Killing role that wins by being the last killer alive. They progressively gain the Vent ability and enhanced vision until eventually transforming into War, Horseman of the Apocalypse.\n\nThe vent ability is unlocked at {(int)OptionGroupSingleton<BerserkerOptions>.Instance.KillsForVent} kill(s).\nEnhanced vision is unlocked at {(int)OptionGroupSingleton<BerserkerOptions>.Instance.KillsForVision} kill(s).\nWar transformation is at {(int)OptionGroupSingleton<BerserkerOptions>.Instance.KillsForWar} kill(s)." +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Hunger (Passive)",
            $"Gain abilities progressively when killing.\nYou will become War when you reach {(int)OptionGroupSingleton<BerserkerOptions>.Instance.KillsForWar} kill(s).",
            TouNeutAssets.BersKillSprite),
    ];
    public override void Initialize(PlayerControl player)
    {
        roleChangePending = false;
        roleChangeTimer = 0;

        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouNeutAssets.BersVentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Berserker);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouAssets.VentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Impostor);
        }
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }
    public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not BerserkerRole || Player.HasDied())
            return;

        if (!roleChangePending &&
            KillCount >= (int)OptionGroupSingleton<BerserkerOptions>.Instance.KillsForWar)
        {
            roleChangePending = true;
            roleChangeTimer = 0.75f;
        }

        if (roleChangePending)
        {
            roleChangeTimer -= Time.fixedDeltaTime;
            if (roleChangeTimer <= 0f)
            {
                roleChangePending = false;

                Player.ChangeRole(RoleId.Get<WarRole>());

                var cooldown = OptionGroupSingleton<BerserkerOptions>.Instance.WarKillCooldown;
                CustomButtonSingleton<War1KillButton>.Instance.SetTimer(cooldown);
                CustomButtonSingleton<War2KillButton>.Instance.SetTimer(cooldown);

                if (OptionGroupSingleton<BerserkerOptions>.Instance.AnnounceWar)
                {
                    var notif = Helpers.CreateAndShowNotification(
                        $"<b>The {TownOfUsColors.Berserker.ToTextColor()}Berserker</color> has transformed into {TownOfUsColors.War.ToTextColor()}War</color>, Horseman of the Apocalypse!</b>",
                        Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.War.LoadAsset());
                    notif.Text.SetOutlineThickness(0.35f);
                }

            }
        }
    }
}