using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using Reactor.Utilities;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Modifiers.Impostor;

public sealed class PoisonerPoisonedModifier : BaseModifier
{
    public override string ModifierName => "Poisoned";

    public override string GetDescription()
    {
        return $"Your body will turn red after {OptionGroupSingleton<PoisonerOptions>.Instance.PoisonedBodyDelay} second(s).";
    }


    public static IEnumerator StartPoisoned(PlayerControl player)
    {
        if (!OptionGroupSingleton<PoisonerOptions>.Instance.IndicatePoisonKill)
        {
            yield break;
        }
        yield return new WaitForSeconds(OptionGroupSingleton<PoisonerOptions>.Instance.PoisonedBodyDelay.Value);
        var poisoned = Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == player.PlayerId);
        if (poisoned == null)
        {
            yield break;
        }

        Coroutines.Start(poisoned.CoSetBodyColor(TownOfUsColors.ImpSoft, 0f, 1f));
    }
}