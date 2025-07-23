using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class PoisonerOptions : AbstractOptionGroup<PoisonerRole>
{
    public override string GroupName => "Poisoner";

    [ModdedNumberOption("Poison Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float PoisonCooldown { get; set; } = 30f;

    [ModdedNumberOption("Max Poison Injections", 0f, 5f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float MaxPoison { get; set; } = 1f;

    [ModdedNumberOption("Poison Kill Delay", 1f, 15f, 1f, MiraNumberSuffixes.Seconds)]
    public float PoisonDelay { get; set; } = 5f;
    [ModdedToggleOption("Indicate Poison Kill")]
    public bool IndicatePoisonKill { get; set; } = true;
    public ModdedNumberOption PoisonedBodyDelay { get; } = new("Poisoned Kill Indicator Delay", 3f, 0f, 10f, 0.5f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<PoisonerOptions>.Instance.IndicatePoisonKill
    };
}