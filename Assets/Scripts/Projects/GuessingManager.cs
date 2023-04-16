using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GuessingManager : MonoBehaviour
{
    public Text attemptsText, uScoreText, cScoreText, subtitleText, randText;

    int attempts = 3, uScore = 0, cScore = 0;
    private int rand;

    // Start is called before the first frame update
    void Start()
    {
        ResetGame();
    }
    public void Guess(int guess)
    { 
        //All buttons redirect here with int value assigned to them.
        //Check if guess is correct
        if (guess == rand)
        {
            uScore++;
            uScoreText.text = uScore.ToString();
            subtitleText.text = "Correct!";
            subtitleText.color = Color.green;
            ResetGame();
            return;
        } else if (attempts <= 1)
        {
            //If no more attempts, reset and add cscore
            cScore++;
            cScoreText.text = cScore.ToString();
            subtitleText.text = "No More attempts.";
            subtitleText.color = Color.red;
            ResetGame();
            return;
        }
        //If incorrect guess, takeaway attempts.
        subtitleText.text = "Incorrect.";
        subtitleText.color = Color.red;
        attempts--;
        attemptsText.text = attempts.ToString();

    }
    private void ResetGame()
    {
        //Gen Random
        rand = Random.Range(1, 10);
        Debug.Log("The number is: " + rand);
        attempts = 3;
        //Reset attempts, tell user
        attemptsText.text = attempts.ToString();
        randText.text = "New Secret Number";
        StartCoroutine(RandText());
            //RandText();
    }
    Color textColor = new Color(0.1960784f, 0.1960784f, 0.1960784f);
    IEnumerator RandText()
    {
        yield return new WaitForSeconds(2f);
        randText.text = "";

        //If application has reset
        if (attempts == 3)
        {
            subtitleText.text = "Guess the number!";
            subtitleText.color = textColor;
        }

    }


    public void MainMenu()
    {
        //Main menu button functionality
        SceneManager.LoadScene("Main");
    }
}
