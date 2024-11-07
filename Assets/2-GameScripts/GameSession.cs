using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
    [SerializeField] int playerLives = 3;
    [SerializeField] int score = 0;

    float intro = 0f; // Stopwatch starting value
    bool isGameActive = true; // Flag to check if the game is active

    [SerializeField] TextMeshProUGUI livesText;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI introText;
    [SerializeField] float endSequenceDelay = 4f; // Delay before returning to menu

    void Awake()
    {
        int numScenePersists = FindObjectsOfType<GameSession>().Length;
        if (numScenePersists > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        livesText.text = playerLives.ToString();
        scoreText.text = score.ToString();
        introText.text = "Time: " + intro.ToString("F2"); // Initial display of stopwatch
    }

    void Update()
    {
        // Stopwatch logic
        if (isGameActive)
        {
            intro += Time.deltaTime; // Increment the timer
            introText.text = "Time: " + intro.ToString("F2"); // Display the time with two decimal places
        }
    }

    public void ProcessPlayerDeath()
    {
        if (playerLives > 1)
        {
            TakeLife();
        }
        else
        {
            ResetGameSession();
        }
    }

    public void AddToScore(int pointsToAdd)
    {
        score += pointsToAdd;
        scoreText.text = score.ToString();
    }

    void ResetGameSession()
    {
        FindObjectOfType<ScenePersist>().ResetScenePersist();
        SceneManager.LoadScene(0);
        Destroy(gameObject);
    }

    void TakeLife()
    {
        playerLives--;
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
        livesText.text = playerLives.ToString();
    }

    public void TriggerEndSequence()
    {
        StartCoroutine(ShowCongratulationsAndReturnToMenu());
    }

    IEnumerator ShowCongratulationsAndReturnToMenu()
    {
        isGameActive = false; // Stop the stopwatch
        introText.text = "Congrats!\nTime: " + intro.ToString("F2"); // Display the congratulations message with final time

        yield return new WaitForSeconds(endSequenceDelay); // Wait for the congratulatory message to show

        // Return to main menu (assuming main menu is scene 0)
        SceneManager.LoadScene(0);
        Destroy(gameObject); // Reset the GameSession
    }

    public void HideIntro()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (currentSceneIndex == 6)
        {
            TriggerEndSequence();
        }
        else
        {
            introText.text = "";
        }
    }
}
