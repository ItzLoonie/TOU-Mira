using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class CultistIndoctrinateButton : TownOfUsRoleButton<CultistRole, PlayerControl>
{
    public override string Name => "Indoctrinate";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => OptionGroupSingleton<CultistOptions>.Instance.IndoctrinateCooldown + MapCooldown;
    public override int MaxUses => 1;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.IndoctrinateButtonSprite;

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

        UsesLeft--;
        if (UsesLeft != 0) 
        {
            CultistRole.RpcIndoctrinate(PlayerControl.LocalPlayer, Target);
        }
    }
}
