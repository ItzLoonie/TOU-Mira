﻿using MiraAPI.GameOptions;
using TownOfUs.Options;

namespace TownOfUs.Modifiers.Game.Impostor;

public sealed class ImpostorAssassinModifier : AssassinModifier
{
    public override string ModifierName => "Assassin (Impostor)";

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<AssassinOptions>.Instance.NumberOfImpostorAssassins;
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<AssassinOptions>.Instance.ImpAssassinChance;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role.TeamType == RoleTeamTypes.Impostor;
    }
}