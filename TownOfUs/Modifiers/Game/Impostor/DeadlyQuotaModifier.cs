using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Impostor;

public sealed class DeadlyQuotaModifier : TouGameModifier, IWikiDiscoverable
{
    public int KillCount { get; set; }
    public override string ModifierName => "Deadly Quota";
    public override string IntroInfo => "You also lose if you do not meet your quota.";
    public override Color FreeplayFileColor => new Color32(255, 25, 25, 255);

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.DeadlyQuota;
    public override ModifierFaction FactionType => ModifierFaction.ImpostorPassive;

    public string GetAdvancedDescription()
    {
        return
            "You lose if you do not complete your quota."
            + MiscUtils.AppendOptionsText(GetType());
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription()
    {
        return $"Meet your quota or lose!\n\nKills: {KillCount} / {OptionGroupSingleton<DeadlyQuotaOptions>.Instance.KillQuota}";
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.DeadlyQuotaChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.DeadlyQuotaAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsImpostor();
    }
    public override bool? DidWin(GameOverReason reason)
    {
        return (reason is GameOverReason.ImpostorsByVote or GameOverReason.ImpostorsBySabotage
            or GameOverReason.ImpostorsByKill) && KillCount >= OptionGroupSingleton<DeadlyQuotaOptions>.Instance.KillQuota;
    }

    public override void OnMeetingStart()
    {
        if (Player.AmOwner && KillCount == 0)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>You haven't started working on your quota. You WILL lose if Impostors win and you do not meet your quota of {OptionGroupSingleton<DeadlyQuotaOptions>.Instance.KillQuota} by the end of the game.</b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: TouModifierIcons.DeadlyQuota.LoadAsset());


            notif1.Text.SetOutlineThickness(0.4f);
        }
    }

}