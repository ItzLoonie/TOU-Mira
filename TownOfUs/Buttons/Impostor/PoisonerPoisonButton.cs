using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class PoisonerPoisonButton : TownOfUsRoleButton<PoisonerRole, PlayerControl>
{
    public override string Name => "Poison";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => OptionGroupSingleton<PoisonerOptions>.Instance.PoisonCooldown + MapCooldown;
    public override float EffectDuration => OptionGroupSingleton<PoisonerOptions>.Instance.PoisonDelay;
    public override int MaxUses => (int)OptionGroupSingleton<PoisonerOptions>.Instance.MaxPoison;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.PoisonButtonSprite;
    public PlayerControl? _poisonedTarget;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(false, Distance);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        OverrideName("Poisoned");

        _poisonedTarget = Target;

        if (PlayerControl.LocalPlayer.AmOwner)
        {
            var notif = Helpers.CreateAndShowNotification(
                $"<b>You poisoned {_poisonedTarget.CachedPlayerData.PlayerName}. They will die in {OptionGroupSingleton<PoisonerOptions>.Instance.PoisonDelay} second(s)!</b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Poisoner.LoadAsset());
            notif.Text.SetOutlineThickness(0.35f);
        }
    }

public override void OnEffectEnd()
{
    OverrideName("Poison");

    if (_poisonedTarget == null) return;

    PlayerControl.LocalPlayer.SetKillTimer(PlayerControl.LocalPlayer.GetKillCooldown());

    PoisonerRole.RpcPoison(PlayerControl.LocalPlayer, _poisonedTarget);
    _poisonedTarget = null;
}

}
