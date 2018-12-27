using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpStar : PowerUp
{
    public int healthBonus = 20;

    protected override void PowerUpPayload()
    {
        base.PowerUpPayload();

        playerBrain.SetHealthAdjustment(healthBonus);
    }
}
