using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class SerialKillerOptions : AbstractOptionGroup<SerialKillerRole>
{
    public override string GroupName => "Serial Killer";

    [ModdedNumberOption("Kill Cooldown (Primary)", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float PrimaryKillCooldown { get; set; } = 25f;
    [ModdedNumberOption("Kill Cooldown (Secondary)", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float SecondaryKillCooldown { get; set; } = 35f;

    [ModdedNumberOption("Amount of Extra Kills", 0f, 5f, 1f, MiraNumberSuffixes.None)]
    public float AmtOfExtraKills { get; set; } = 0f;

    [ModdedToggleOption("Serial Killer Can Vent")]
    public bool CanVent { get; set; } = true;
}