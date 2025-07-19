﻿using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class JesterHauntButton : TownOfUsButton
{
    public override string Name => "Haunt";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Jester;
    public override float Cooldown => 0.01f;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.JesterHauntSprite;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override bool ShouldPauseInVent => false;
    public override bool UsableInDeath => true;
    public bool Show { get; set; }

    public override bool Enabled(RoleBehaviour? role)
    {
        return Show && ModifierUtils.GetActiveModifiers<MisfortuneTargetModifier>().Any();
    }

    protected override void OnClick()
    {
        var playerMenu = CustomPlayerMenu.Create();
        playerMenu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        playerMenu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        playerMenu.Begin(
            plr => !plr.HasDied() && plr.HasModifier<MisfortuneTargetModifier>() && !plr.HasModifier<InvulnerabilityModifier>() && plr != PlayerControl.LocalPlayer,
            plr =>
            {
                playerMenu.ForceClose();

                if (plr != null)
                {
                    PlayerControl.LocalPlayer.RpcCustomMurder(plr, teleportMurderer: false);
                    DeathHandlerModifier.RpcUpdateDeathHandler(plr, "Haunted", DeathEventHandlers.CurrentRound, DeathHandlerOverride.SetTrue, $"By {PlayerControl.LocalPlayer.Data.PlayerName}", lockInfo: DeathHandlerOverride.SetTrue);

                    foreach (var mod in ModifierUtils.GetActiveModifiers<MisfortuneTargetModifier>())
                    {
                        mod.ModifierComponent?.RemoveModifier(mod);
                    }
                    DeathHandlerModifier.RpcUpdateDeathHandler(PlayerControl.LocalPlayer, "Ejected", DeathEventHandlers.CurrentRound, DeathHandlerOverride.SetFalse, lockInfo: DeathHandlerOverride.SetTrue);

                    Show = false;
                }
            });
    }

    public override bool CanUse()
    {
        return ModifierUtils.GetActiveModifiers<MisfortuneTargetModifier>().Any();
    }
}