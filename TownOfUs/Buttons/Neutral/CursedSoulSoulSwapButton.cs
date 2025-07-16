using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class CursedSoulSoulSwapButton : TownOfUsRoleButton<CursedSoulRole>
{
    public override string Name => "Soul Swap";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.CursedSoul;
    public override float Cooldown => OptionGroupSingleton<CursedSoulOptions>.Instance.SoulSwapCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.SoulSwapButtonSprite;
    public override ButtonLocation Location => ButtonLocation.BottomRight;

    public override void ClickHandler()
    {
        if (!CanUse())
        {
            return;
        }

        OnClick();
        Button?.SetDisabled();
        Timer = Cooldown;
    }

    protected override void OnClick()
    {
        var playerMenu = CustomPlayerMenu.Create();
        playerMenu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        playerMenu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;

        playerMenu.Begin(
            plr => !plr.HasDied() && plr != PlayerControl.LocalPlayer,
            plr =>
            {
                playerMenu.ForceClose();

                if (plr != null)
                {
                    // TouAudio.PlaySound(TouAudio.MimicSound);
                    CursedSoulRole.RpcSoulSwap(PlayerControl.LocalPlayer, plr);
                }
            });

        // foreach (var panel in playerMenu.potentialVictims)
        // {
        //     panel.PlayerIcon.cosmetics.SetPhantomRoleAlpha(1f);
        // }

        foreach (var panel in playerMenu.potentialVictims)
        {
            if (panel == null)
                continue;

            panel.NameText.color = Color.white;
        }

    }

    public override bool CanUse()
    {
        return Timer <= 0 &&
               !PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>() &&
               !PlayerControl.LocalPlayer.HasModifier<DisabledModifier>();
    }
}
