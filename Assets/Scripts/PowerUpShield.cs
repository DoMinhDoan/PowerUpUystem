using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpShield : PowerUp
{
    public float _invulnTimeDuration = 5.0f;
    public GameObject _invulnParticle;

    protected override void PowerUpPayload()
    {
        base.PowerUpPayload();

        playerBrain.SetInvulnerability(true);

        if(_invulnParticle != null)
        {
            Instantiate(_invulnParticle, playerBrain.transform.position, playerBrain.transform.rotation, transform);
        }
    }

    protected override void PowerUpHasExpired()
    {
        playerBrain.SetInvulnerability(false);

        base.PowerUpHasExpired();
    }

    private void Update()
    {
        if (powerUpState == PowerUpState.IsCollected && _invulnTimeDuration - Time.time <= 0)
        {
            PowerUpHasExpired();
        }
    }
}
