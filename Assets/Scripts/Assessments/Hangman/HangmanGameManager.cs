using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Data;
using System.Linq;

public class HangmanGameManager : MonoBehaviour
{
    public Text difficultyText;
    public Text underscoreText;
    public Text correctText;
    public Text wordText, percentageText, _difficultyText, attemptText, titleText;
    public RawImage hangmanImage;
    public GameObject winLossPanel;

    IDataReader reader;


    Button[] btn = { };

    int[] numWords = HangmanManager.numWords;
    string[] displayStructure, underscoreStructure, difficulties = { "All", "Easy", "Medium", "Hard", "Very Hard" };
    char[] wordChar;
    int difficulty = HangmanManager.difficulty, id = 0, uses = 0, solves = 0, attempts = 0;
    string word = "";
    List<string> letterGuesses;

    void Start()
    {
        //dbassist = GetComponent<DBManager>();
        //Gets all buttons, improve so only targeted keyboard buttons
        btn = FindObjectsOfType<Button>();
        
        BeginGame();
    }

    public void BeginGame()
    {
        //Set all buttons to active
        foreach (Button btn in btn) { btn.interactable = true; }

        letterGuesses = new List<string>();
        winLossPanel.SetActive(false);
        attempts = 0;

        // 0 = All, 1 = Easy, 2 = Medium, 3 = Hard, 4 = Very Hard
        Debug.Log("Game started with difficulty " + difficulties[difficulty]);
        difficultyText.text = difficulties[difficulty];

        DBManager.InitiateConnection("/words.db");

        //Construct Query
        string query = $"SELECT ID, Word, Uses, Solves FROM (SELECT ID, Word, Uses, Solves, rank() OVER (ORDER BY ID) as 'Row1' FROM \"Random Words\"";
        int rand = Random.Range(1, numWords[difficulty]);
        if (!(difficulty == 0)) query += $" WHERE Length = {difficulty + 3})";
        else query += ")";
        query += $" WHERE Row1 = {rand}";

        //Temp difficulty override
        //rand = Random.Range(1, 326);
        //query = $"SELECT ID, Word, Uses, Solves FROM 'Random Words' WHERE ID = {rand}";


        //Run query and read+store result

        DBManager.QueryDB(query);
        while (DBManager.reader.Read())
        {
            id = DBManager.reader.GetInt32(0);
            word = DBManager.reader.GetString(1);
            uses = DBManager.reader.GetInt32(2) + 1;
            solves = DBManager.reader.GetInt32(3);

        }
        //Debug.Log($"Word: {word}, Uses: {uses}, Solves: {solves}, ID: {id}");

        //Iterate the uses column
        query = $"UPDATE 'Random Words' SET Uses = Uses + 1 WHERE ID = {id}";
        DBManager.QueryDB(query);
        DBManager.CloseConnection();

        //Make letters in word to char[] for testing
        wordChar = word.ToCharArray();


        //Generate dynamicly spaced letter spots
        underscoreStructure = new string[word.Length];
        for (int i = 0; i < underscoreStructure.Length; i++)
        {
            underscoreStructure[i] = "___";
        }

        underscoreText.text = string.Join("   ", underscoreStructure);



        hangmanImage.texture = Resources.Load<Texture>("Images/Hangman/Hangman0");
        UpdateText();
    }
    void UpdateText()
    {
        displayStructure = new string[word.Length];
        string curLetter;
        bool won = true;

        //For each letter in the word, check if the user has guessed it.
        //Spacing is sub optimal, what we really need is letter kerning (or tracking) IN UNITYYYY
        for (int i = 0; i < displayStructure.Length; i++)
        {
            curLetter = wordChar[i].ToString();

            if (letterGuesses.Contains(curLetter)) displayStructure[i] = curLetter.ToUpper();
            else
            {
                displayStructure[i] = "  ";
                won = false;
            }

            correctText.text = string.Join("    ", displayStructure);
        }
        //Win check
        if (won)
        {
            //Add one to solve
            DBManager.InitiateConnection("/words.db");

            string query = $"UPDATE 'Random Words' SET Solves = Solves + 1 WHERE ID = {id}";

            DBManager.QueryDB(query);
            //Keep internal var the same as external
            solves++;

            DBManager.CloseConnection();

            //Show the win panel
            ShowPanel(false);
        }
    }
    void IterateHangman()
    {
        attempts++;
        hangmanImage.texture = Resources.Load<Texture>($"Images/Hangman/Hangman{attempts}");
        if (attempts >= 6)
        {
            ShowPanel(true);
        }
        //CHange image
        //Failure checks
    }
    void ShowPanel(bool lost)
    {
        percentageText.text = solves / uses * 100 + "%";
        attemptText.text = uses.ToString();
        wordText.text = word.ToUpper();
        _difficultyText.text = "Difficulty: " + difficulties[difficulty];

        if (lost)
        {
            titleText.text = "You Lost";
        }
        else
        {
            titleText.text = "You Won!";
        }

        winLossPanel.SetActive(true);

    }

    public void ButtonPressed(string letter)
    {
        //Check through the whole keyboard
        foreach (Button btn in btn)
        {
            if (btn.name.ToUpper() == letter.ToUpper()) btn.interactable = false;
        }

        //Test if letter is in char array 
        if (word.Contains(letter))
        {
            letterGuesses.Add(letter);
            UpdateText();
        }

        //Consequence or successful letter
        else IterateHangman();
    }
    public void Restart()
    {
        SceneManager.LoadScene("Hangman");
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("Main");
    }

}
