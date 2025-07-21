using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;
using TownOfUs.Utilities;
using TownOfUs.Options.Roles.Impostor;

namespace TownOfUs.Buttons.Impostor;

public sealed class SpellslingerDetonateButton : TownOfUsRoleButton<SpellslingerRole, PlayerControl>
{
    public override string Name => "Detonate";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => OptionGroupSingleton<SpellslingerOptions>.Instance.DetonateCooldown + MapCooldown;
    public override int MaxUses => 1;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.DetonateButtonSprite;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && SpellslingerRole.EveryoneHexed();
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, predicate: x => x.HasModifier<SpellslingerHexedModifier>());
    }
    protected override void OnClick()
    {
        SpellslingerRole.RpcDetonate(PlayerControl.LocalPlayer);
    }
    public override bool CanUse()
    {
        if (!base.CanUse()) return false;

        var aliveNonImpostors = Helpers.GetAlivePlayers().Where(p => !p.IsImpostor()).ToList(); 

        var hexedPlayers = ModifierUtils.GetPlayersWithModifier<SpellslingerHexedModifier>(x =>
            aliveNonImpostors.Contains(x.Player));

        return hexedPlayers.Count() >= aliveNonImpostors.Count;
    }
}