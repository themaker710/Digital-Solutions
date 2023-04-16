using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Data;
using System.Collections.Generic;
 
public class HangmanManager : MonoBehaviour
{
    public Text numWordsText;
    public Dropdown difficultyDropdown;

    internal static int difficulty = 0;
    internal static int[] numWords;




    void Start()
    {
        GetNumWords();
        DifficultyChange();
    }
    public void DifficultyChange()
    {
        // 0 = All, 1 = Easy, 2 = Medium, 3 = Hard, 4 = Very Hard
        difficulty = difficultyDropdown.value;
        Debug.Log(difficulty);
        numWordsText.text = numWords[difficulty].ToString() + " words";
    }
    public void PlayGame()
    {
        SceneManager.LoadScene("HangmanGame");
    }
    public void MainMenu()
    {
        //Main menu button functionality
        SceneManager.LoadScene("Main");
    }
    void GetNumWords()
    {
        Debug.Log("Getting number of words");
        DBManager.InitiateConnection("/words.db");
        List<int> wordsList = new List<int>();
        string sqlQuery =
          "SELECT Length, count(*) as 'Count' FROM \"Random Words\" GROUP BY Length\n" +
          "UNION\n" +
          "SELECT 0, count(*) as 'Count' FROM \"Random Words\"";

        DBManager.QueryDB(sqlQuery);
        while (DBManager.reader.Read()) { wordsList.Add(DBManager.reader.GetInt32(1)); };

        DBManager.CloseConnection();

        numWords = wordsList.ToArray();
    }

}


