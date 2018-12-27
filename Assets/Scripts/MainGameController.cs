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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Singleton main game controller handles the UI, plays audio, listens and reports on events,
/// detects game win/lose, restarts game.  Issues messages when game won or lost.
/// </summary>
public class MainGameController : MonoBehaviour, IPlayerEvents, IPowerUpEvents
{
    public static MainGameController main;

    [Tooltip ("UI text to use for messages to player about pickups and gameover")]
    public Text uiText;
    [Tooltip ("UI subtext for additional messages to player")]
    public Text uiSubtext;
    [Tooltip ("UI canvas group for fading")]
    public CanvasGroup uiCanvasGroup;
    [Tooltip ("UI texts display this long after first appearing")]
    public float uiTextDisplayDuration = 5f;
    [Tooltip ("UI text to show whole list of active power ups")]
    public Text uiTextActivePowerUps;
    public AudioClip soundEffectWin;
    public AudioClip soundEffectLose;
    public GameObject specialEffectWin;

    private float uiTextDisplayTimer;
    private AudioSource audioSource;
    private List<PowerUp> activePowerUps;

    private enum MainGameState
    {
        Idle,
        Playing,
        GameOver
    }

    private MainGameState mainGameState = MainGameState.Idle;

    void IPlayerEvents.OnPlayerHurt (int newHealth)
    {
        // If game is already over, don't do anything
        if (mainGameState == MainGameState.GameOver)
        {
            return;
        }

        if (newHealth <= 0)
        {
            if (soundEffectLose != null)
            {
                PlaySound (soundEffectLose);
            }

            // UI and message broadcasting
            GameOverLose ();
        }
    }

    void IPlayerEvents.OnPlayerReachedExit (GameObject exit)
    {
        // If game is already over, don't do anything
        if (mainGameState == MainGameState.GameOver)
        {
            return;
        }

        if (soundEffectWin != null)
        {
            PlaySound (soundEffectWin);
        }

        if (specialEffectWin != null)
        {
            Instantiate (specialEffectWin, exit.transform.position, exit.transform.rotation, exit.transform);
        }

        // UI and message broadcasting
        GameOverWin ();
    }

    void IPowerUpEvents.OnPowerUpCollected (PowerUp powerUp, PlayerBrain player)
    {
        // We dont bother storing those that expire immediately
        if (!powerUp.expiresImmediately)
        {
            activePowerUps.Add (powerUp);
            UpdateActivePowerUpUi ();
        }

        uiText.text = powerUp.powerUpExplanation;
        uiSubtext.text = powerUp.powerUpQuote;
        uiTextDisplayTimer = uiTextDisplayDuration;
    }

    void IPowerUpEvents.OnPowerUpExpired (PowerUp powerUp, PlayerBrain player)
    {
        activePowerUps.Remove (powerUp);
        UpdateActivePowerUpUi ();
    }

    private void UpdateActivePowerUpUi ()
    {
        uiTextActivePowerUps.text = "Active Power Ups";

        if (activePowerUps == null || activePowerUps.Count == 0)
        {
            uiTextActivePowerUps.text += "\nNone";
            return;
        }

        foreach (PowerUp powerUp in activePowerUps)
        {
            uiTextActivePowerUps.text += "\n" + powerUp.powerUpName;
        }
    }

    /// <summary>
    /// Check we are singleton
    /// </summary>
    private void Awake ()
    {
        if (main == null)
        {
            main = this;
            audioSource = gameObject.GetComponent<AudioSource> ();
            activePowerUps = new List<PowerUp> ();
        } else
        {
            Debug.LogWarning ("GameController re-creation attempted, destroying the new one");
            Destroy (gameObject);
        }
    }

    // Use this for initialization
    void Start ()
    {
        mainGameState = MainGameState.Playing;
        UnityEngine.Random.InitState ((int)System.DateTime.Now.Ticks);
        uiTextDisplayTimer = uiTextDisplayDuration * 3; // leave instructions on screen for longer
        UpdateActivePowerUpUi ();
    }

    // Update is called once per frame
    void Update ()
    {
        // If game is over, we can restart
        if (mainGameState == MainGameState.GameOver)
        {
            if (Input.GetKeyDown (KeyCode.Space))
            {
                ReloadLevel ();
            }
        }

        // Fade away UI text in the last second of its life
        uiTextDisplayTimer -= Time.deltaTime;
        if (uiTextDisplayTimer < 1)
        {
            uiCanvasGroup.alpha = uiTextDisplayTimer;
        } else if (uiTextDisplayTimer < 0)
        {
            uiCanvasGroup.alpha = 0f;
        } else
        {
            uiCanvasGroup.alpha = 1f;
        }
    }

    public void ReloadLevel ()
    {
        SceneManager.LoadSceneAsync (0, LoadSceneMode.Single);
    }

    public void PlaySound (AudioClip audioClip)
    {
        audioSource.PlayOneShot (audioClip);
    }

    private void GameOverLose ()
    {
        mainGameState = MainGameState.GameOver;
      
        // UI
        uiText.text = "GAME OVER";
        uiSubtext.text = "Press Space to Restart";
        uiTextDisplayTimer = Mathf.Infinity;  // never fade this

        // Send message to any listeners
        foreach (GameObject go in EventSystemListeners.main.listeners)
        {
            ExecuteEvents.Execute<IMainGameEvents> (go, null, (x, y) => x.OnGameLost ());
        }
    }

    private void GameOverWin ()
    {
        mainGameState = MainGameState.GameOver;
      
        // UI
        uiText.text = "LEVEL COMPLETE";
        uiSubtext.text = "Press Space to Restart";
        uiTextDisplayTimer = Mathf.Infinity;  // never fade this
    
        // Send message to any listeners
        foreach (GameObject go in EventSystemListeners.main.listeners)
        {
            ExecuteEvents.Execute<IMainGameEvents> (go, null, (x, y) => x.OnGameWon ());
        }
    }
}
