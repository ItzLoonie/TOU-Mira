﻿using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Modifiers;
using Reactor.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Events;

public static class DeathEventHandlers
{
    public static int CurrentRound { get; set; } = 1;

    [RegisterEvent(-1)]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            CurrentRound = 1;
            Logger<TownOfUsPlugin>.Warning("Game Has Started");
        }
        else
        {
            ++CurrentRound;
            ModifierUtils.GetActiveModifiers<DeathHandlerModifier>().Do(x => x.DiedThisRound = false);
            Logger<TownOfUsPlugin>.Warning($"New Round Started: {CurrentRound}");
        }
    }

    [RegisterEvent(1000)]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        var victim = @event.Player;
        if (!victim.HasModifier<DeathHandlerModifier>())
        {
            var deathHandler = new DeathHandlerModifier();
            victim.AddModifier(deathHandler);
            var cod = "Disconnected";
            deathHandler.DiedThisRound = !MeetingHud.Instance && !ExileController.Instance;
            switch (@event.DeathReason)
            {
                case DeathReason.Exile:
                    cod = "Ejected";
                    deathHandler.DiedThisRound = false;
                    break;
                case DeathReason.Kill:
                    cod = "Killed";
                    break;
            }
            deathHandler.CauseOfDeath = cod;
            deathHandler.RoundOfDeath = CurrentRound;
        }
    }
    
    [RegisterEvent(500)]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        var exiled = @event.ExileController?.initData?.networkedPlayer?.Object;
        if (exiled == null)
        {
            return;
        }
        if (exiled.TryGetModifier<DeathHandlerModifier>(out var deathHandler) && !deathHandler.LockInfo)
        {
            deathHandler.CauseOfDeath = "Ejected";
            deathHandler.DiedThisRound = false;
            deathHandler.RoundOfDeath = CurrentRound;
            deathHandler.LockInfo = true;
        }
    }

    [RegisterEvent(500)]
    public static void PlayerReviveEventHandler(PlayerReviveEvent reviveEvent)
    {
        if (reviveEvent.Player.TryGetModifier<DeathHandlerModifier>(out var deathHandler))
        {
            reviveEvent.Player.RemoveModifier(deathHandler);
        }
    }

    [RegisterEvent(500)]
    public static void AfterMurderEventHandler(AfterMurderEvent murderEvent)
    {
        var source = murderEvent.Source;
        var target = murderEvent.Target;
        
        if (target == source && target.TryGetModifier<DeathHandlerModifier>(out var deathHandler) && !deathHandler.LockInfo)
        {
            deathHandler.CauseOfDeath = "Suicide";
            deathHandler.DiedThisRound = !MeetingHud.Instance && !ExileController.Instance;
            deathHandler.RoundOfDeath = CurrentRound;
            deathHandler.LockInfo = true;
        }
        
        else if (target.TryGetModifier<DeathHandlerModifier>(out var deathHandler2) && !deathHandler2.LockInfo)
        {
            var cod = "Killed";

            switch (source.GetRoleWhenAlive())
            {
                case SheriffRole or DeputyRole:
                    cod = "Shot";
                    break;
                case VeteranRole:
                    cod = "Blasted";
                    break;
                case JailorRole:
                    cod = "Executed";
                    break;
                case ArsonistRole:
                    cod = "Ignited";
                    break;
                case GlitchRole:
                    cod = "Bugged";
                    break;
                case JuggernautRole:
                    cod = "Destroyed";
                    break;
                case PestilenceRole:
                    cod = "Diseased";
                    break;
                case SoulCollectorRole:
                    cod = "Reaped";
                    break;
                case VampireRole:
                    cod = "Bit";
                    break;
                case WerewolfRole:
                    cod = "Rampaged";
                    break;
                case DoomsayerRole:
                    cod = "Doomed";
                    break;
                case JesterRole:
                    cod = "Haunted";
                    break;
                case ExecutionerRole:
                    cod = "Tormented";
                    break;
                case InquisitorRole:
                    cod = "Vanquished";
                    break;
                case SerialKillerRole:
                    cod = "Stabbed";
                    break;
                case BerserkerRole:
                case WarRole:
                    cod = "Destroyed";
                    break;
            }

            if (source.Data.Role is PhantomTouRole)
            {
                cod = "Spooked";
            }

            deathHandler2.CauseOfDeath = cod;
            deathHandler2.KilledBy = $"By {source.Data.PlayerName}";
            deathHandler2.DiedThisRound = !MeetingHud.Instance && !ExileController.Instance;
            deathHandler2.RoundOfDeath = CurrentRound;
            deathHandler2.LockInfo = true;
        }
    }

    [RegisterEvent]
    public static void PlayerLeaveEventHandler(PlayerLeaveEvent @event)
    {
        if (!MeetingHud.Instance)
        {
            return;
        }

        var player = @event.ClientData.Character;

        if (!player)
        {
            return;
        }

        var pva = MeetingHud.Instance.playerStates.First(x => x.TargetPlayerId == player.PlayerId);

        if (!pva)
        {
            return;
        }

        pva.AmDead = true;
        pva.Overlay.gameObject.SetActive(true);
        pva.Overlay.color = Color.white;
        pva.XMark.gameObject.SetActive(false);
        pva.XMark.transform.localScale = Vector3.one;

        MeetingMenu.Instances.Do(x => x.HideSingle(player.PlayerId));
    }
}