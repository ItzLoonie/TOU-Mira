using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class CursedSoulOptions : AbstractOptionGroup<CursedSoulRole>
{
    public override string GroupName => "CursedSoul";

    [ModdedNumberOption("Soul Swap Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float SoulSwapCooldown { get; set; } = 25f;
    [ModdedNumberOption("Soul Swap Randomness", 0f, 100f, 2.5f, MiraNumberSuffixes.Percent)]
    public float SoulSwapRandomness { get; set; } = 50f;
    [ModdedNumberOption("Kill Cooldown On Soul Swap", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillCooldownOnSoulSwap { get; set; } = 10f;

}