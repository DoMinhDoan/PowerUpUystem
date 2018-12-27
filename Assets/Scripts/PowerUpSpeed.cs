using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpeed : PowerUp, IPlayerEvents
{
    [Range(1.0f, 4.0f)]
    public float speedMultiplier = 2.0f;

    protected override void PowerUpPayload()
    {
        base.PowerUpPayload();
        playerBrain.SetSpeedBoostOn(speedMultiplier);
    }

    protected override void PowerUpHasExpired()
    {
        playerBrain.SetSpeedBoostOff();
        base.PowerUpHasExpired();
    }

    public void OnPlayerHurt(int newHealth)
    {
        if(powerUpState != PowerUpState.IsCollected)
        {
            return;
        }

        PowerUpHasExpired();
    }

    public void OnPlayerReachedExit(GameObject exit)
    {

    }
}
