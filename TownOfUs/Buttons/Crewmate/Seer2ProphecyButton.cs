using MiraAPI.Utilities.Assets;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class SeerProphecyButton : TownOfUsRoleButton<SeerRole>
{
    public override string Name => "Change Prophecy";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Seer;
    public override float Cooldown => 0.001f;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.ProphecySprite;

    protected override void OnClick()
    {
        SeerRole.RpcProphecy(PlayerControl.LocalPlayer);
    }
}