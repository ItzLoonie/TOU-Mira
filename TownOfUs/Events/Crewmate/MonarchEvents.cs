using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Events.Modifiers;

public static class MonarchEvents
{
    [RegisterEvent]
    [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
    public static void OnKnightKilled(AfterMurderEvent @event)
    {
        var deadPlayer = @event.Target;

        if (!OptionGroupSingleton<MonarchOptions>.Instance.InformWhenKnightDies)
            return;

        if (!deadPlayer.HasModifier<KnightedModifier>())
            return;

        var monarch = PlayerControl.AllPlayerControls
            .ToArray()
            .FirstOrDefault(p => !p.HasDied() && p.Data.Role is MonarchRole);

        if (monarch == null || !monarch.AmOwner)
            return;

        var notif = Helpers.CreateAndShowNotification(
            $"<b>Your knight, {deadPlayer.Data.PlayerName}, has fallen...</b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: TouRoleIcons.Monarch.LoadAsset());

        notif.Text.SetOutlineThickness(0.4f);
    }
}
