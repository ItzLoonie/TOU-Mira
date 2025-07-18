﻿using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class HaunterArrowModifier(PlayerControl owner, Color color) : ArrowTargetModifier(owner, color, 0)
{
    public override string ModifierName => "Haunter Arrow";
}