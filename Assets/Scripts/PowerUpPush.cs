/*
 * Copyright (c) 2017 Razeware LLC
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * Notwithstanding the foregoing, you may not use, copy, modify, merge, publish, 
 * distribute, sublicense, create a derivative work, and/or sell copies of the 
 * Software in any work that is designed, intended, or marketed for pedagogical or 
 * instructional purposes related to programming, coding, application development, 
 * or information technology.  Permission for such use, copying, modification,
 * merger, publication, distribution, sublicensing, creation of derivative works, 
 * or sale is expressly withheld.
 *    
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
using UnityEngine;

/// <summary>
/// Gives player ability to push enemies out of the way when 'p' is pressed, limited uses
/// Power Ups checklist:
/// 1. Implement PowerUpPayload() [Done]
/// 2. Optionally Implement PowerUpHasExpired() to remove what was given in the payload [Done]
/// 3. Call PowerUpHasExpired() when the power up has expired or tick ExpiresImmediately in inspector [Done - when timer expires]
/// </summary>
class PowerUpPush : PowerUp
{
    public int numberOfUsesRemaining = 3;
    public float radius = 5f;
    public float power = 10f;
    public GameObject pushParticles;
    public AudioClip pushSoundEffect;

    protected override void PowerUpPayload ()
    {
        base.PowerUpPayload ();
        // The fact that powerUpState is now == IsCollected, is enough to offer payload to player in Update()
    }

    private void Update ()
    {
        if (powerUpState == PowerUpState.IsCollected && numberOfUsesRemaining > 0)
        {
            if (Input.GetKeyDown ("p"))
            {
                PushSpecialEffects ();
                PushPhysics ();
                numberOfUsesRemaining--;
                if (numberOfUsesRemaining <= 0)
                {
                    PowerUpHasExpired ();
                }
            }
        }
    }

    void PushSpecialEffects ()
    {
        if (pushParticles != null)
        {
            // Could also keep handle to below (it returns the created gameobject) and restart particle effect
            Instantiate (pushParticles, playerBrain.gameObject.transform.position, playerBrain.gameObject.transform.rotation, transform);
        }

        if (pushSoundEffect != null)
        {
            MainGameController.main.PlaySound (pushSoundEffect);
        }
    }

    void PushPhysics ()
    {
        // Bit shift the index of the Enemy layer to get a bit mask, this will affect only colliders in Enemy layer
        int enemyLayerIndex = LayerMask.NameToLayer ("Enemy");
        int layerMask = 1 << enemyLayerIndex;

        Vector3 explosionPos = transform.position;
        Collider2D[] colliders = Physics2D.OverlapCircleAll (explosionPos, radius, layerMask);
        foreach (Collider2D hit in colliders)
        {
            Rigidbody2D rb = hit.GetComponent<Rigidbody2D> ();

            if (rb != null)
            {
                AddExplosionForce (rb, power, explosionPos, radius);
            }
        }
    }

    public static void AddExplosionForce (Rigidbody2D body, float explosionForce, Vector3 explosionPosition, float explosionRadius)
    {
        var dir = (body.transform.position - explosionPosition);
        float wearoff = 1 - (dir.magnitude / explosionRadius);
        body.AddForce(dir.normalized * explosionForce * wearoff);
    }
}

