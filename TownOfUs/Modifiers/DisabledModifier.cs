using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;

namespace TownOfUs.Modifiers;

/// <summary>
/// This modifier is used to disable abilities on a player and can be set up to disable on an interval.
/// </summary>
[MiraIgnore]
public abstract class DisabledModifier(bool canBeInteractedWith = true, bool isConsideredAlive = true, bool canUseAbilities = false, bool canReport = false, float duration = 1f) : TimedModifier
{
    public override string ModifierName => "Disabled Modifier";

    public virtual bool CanBeInteractedWith => canBeInteractedWith;
    public virtual bool IsConsideredAlive => isConsideredAlive;
    public virtual bool CanUseAbilities => canUseAbilities;
    public virtual bool CanReport => canReport;
    public override bool HideOnUi => true;

    public override float Duration => duration;
    public override bool AutoStart => false;

    public override string GetDescription()
    {
        return "You are disabled!";
    }

    public override void OnDeath(DeathReason reason)
    {
        base.OnDeath(reason);
        ModifierComponent!.RemoveModifier(this);
    }
}
