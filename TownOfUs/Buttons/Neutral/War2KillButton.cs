using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class War2KillButton : TownOfUsRoleButton<WarRole, PlayerControl>, IDiseaseableButton,
    IKillButton
{
    public override string Name => "Kill";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.War;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.WarKillSprite;
    public override float Cooldown => OptionGroupSingleton<BerserkerOptions>.Instance.WarKillCooldown + MapCooldown;

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("War Shoot: Target is null");
            return;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(Target);
    }



    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }
}