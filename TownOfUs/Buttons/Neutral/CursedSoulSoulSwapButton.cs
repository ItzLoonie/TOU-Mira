using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class CursedSoulSoulSwapButton : TownOfUsRoleButton<CursedSoulRole, PlayerControl>
{
    public override string Name => "Soul Swap";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.CursedSoul;
    public override float Cooldown => OptionGroupSingleton<CursedSoulOptions>.Instance.SoulSwapCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.SoulSwapButtonSprite;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        CursedSoulRole.RpcSoulSwap(PlayerControl.LocalPlayer, Target);
    }
}