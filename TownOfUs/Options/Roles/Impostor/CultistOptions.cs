using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class CultistOptions : AbstractOptionGroup<CultistRole>
{
    public override string GroupName => "Cultist";

    [ModdedNumberOption("Indoctrinate Cooldown", 1f, 30f, suffixType: MiraNumberSuffixes.Seconds)]
    public float IndoctrinateCooldown { get; set; } = 20f;

    [ModdedToggleOption("Cultist Can Indoctrinate Neutral Evils")]
    public bool IndoctrinateNeutralEvil { get; set; } = false;
    [ModdedToggleOption("Cultist Can Indoctrinate Neutral Benigns")]
    public bool IndoctrinateNeutralBenign { get; set; } = false;
    [ModdedToggleOption("Cultist Can Indoctrinate Neutral Killers")]
    public bool IndoctrinateNeutralKiller { get; set; } = false;
    [ModdedToggleOption("Indoctrinated Gains Assassin Ability")]
    public bool IndoctrinateCanGuess { get; set; } = false;
}