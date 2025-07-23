using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class BerserkerOptions : AbstractOptionGroup<BerserkerRole>
{
    public override string GroupName => "Berserker";

    [ModdedNumberOption("Kill Cooldown (Berserker)", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float BerserkerKillCooldown { get; set; } = 25f;
    [ModdedNumberOption("Kill Cooldown (War)", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float WarKillCooldown { get; set; } = 20f;

    [ModdedNumberOption("Kills For Impostor Vision", 0f, 5f, 1f, MiraNumberSuffixes.Seconds)]
    public float KillsForVision { get; set; } = 1f;
    [ModdedNumberOption("Kills For Vent Ability", 0f, 5f, 1f, MiraNumberSuffixes.Seconds)]
    public float KillsForVent { get; set; } = 2f;
    [ModdedNumberOption("Kills For War Transformation", 0f, 5f, 1f, MiraNumberSuffixes.Seconds)]
    public float KillsForWar { get; set; } = 3f;
    [ModdedToggleOption("Announce War")]
    public bool AnnounceWar { get; set; } = true;

}