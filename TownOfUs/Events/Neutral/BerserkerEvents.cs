using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Events.Neutral;

public static class BerserkerEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        if (!source.AmOwner || source.Data.Role is not BerserkerRole berserker || MeetingHud.Instance)
        {
            return;
        }

        berserker.KillCount++;
    }
}