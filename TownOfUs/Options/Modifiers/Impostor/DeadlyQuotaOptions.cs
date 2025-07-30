using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using TownOfUs.Modifiers.Game.Impostor;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Impostor;

public sealed class DeadlyQuotaOptions : AbstractOptionGroup<DeadlyQuotaModifier>
{
    public override string GroupName => "Deadly Quota";
    public override Color GroupColor => Palette.ImpostorRoleHeaderRed;
    public override uint GroupPriority => 40;

    [ModdedNumberOption("Kill Quota", 1f, 5f, 1f)]
    public float KillQuota { get; set; } = 3f;
    [ModdedToggleOption("Temporary Shield Until Quota Is Met")]
    public bool QuotaShield { get; set; } = true;
}