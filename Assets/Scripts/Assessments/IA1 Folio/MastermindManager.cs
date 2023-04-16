using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class MastermindManager : MonoBehaviour
{
    //Unity variables
    public RectTransform ParentObject;
    public InputField userinput;
    public Text resultText;
    public Dropdown diffDropdown;

    //Win panel
    public Text numberText;
    public GameObject winPanel;

    //Table resources
    GameObject rowPrefab;
    List<TableData> guesses;

    //Define variables
    int rand, userguess, numCorrect, placeCorrect, difficulty = 3, upper, lower;

    char[] digitsRand;
    //int[,] tablearr;
    void Start()
    {
        //Load table row prefab
        rowPrefab = Resources.Load<GameObject>("Prefabs/GuessRow");
        InitializeGame();
    }
    public void InitializeGame()
    {
        //Reset table data
        guesses = new List<TableData>();
        //Set difficulty
        difficulty = diffDropdown.value + 3;

        //Generate upper and lower values for this difficulty - also links to input validation
        string upperS = "9", lowerS = "1";
        for (int i = 0; i < difficulty - 1; i++)
        {
            upperS += "9";
            lowerS += "0";
        }
        //Define limits for rand and input validation
        upper = int.Parse(upperS);
        lower = int.Parse(lowerS);

        //Generate random number based on above value
        rand = Random.Range(lower, upper);

        //Convert random number into char array
        digitsRand = rand.ToString().ToCharArray();

        //Reset text fields
        resultText.text = $"Enter a {difficulty} digit guess";
        userinput.text = "";

        //Delete any potentially existing table entries that aren't the table headers
        foreach (Transform child in ParentObject.transform)
        {
            if (child.name != "Headers") Destroy(child.gameObject);
        }

        //Close the win panel
        winPanel.SetActive(false);
    }
    public void SubmitGuess()
    {
        //Reset variables and text fields
        resultText.text = "";
        placeCorrect = 0;
        numCorrect = 0;
        
        //Make sure user input is valid - an integer, and in range based on the difficulty
        if (!(int.TryParse(userinput.text, out userguess) && userguess <= upper && userguess >= lower))
        {
            resultText.text = $"Please enter a valid {difficulty} digit integer";
            return;
        }
        //Would referencing userinput.text or userguess.ToString() be more efficient?
        //Check if correct
        if (userinput.text == rand.ToString())
        {
            //If correct, show win panel
            numberText.text = rand.ToString();
            winPanel.SetActive(true);
            return;
        }

        //Convert user guess into an char array of the digits.
        char[] digitsUser = userinput.text.ToCharArray();

        //For each of the numbers in the user guess
        for (int i = 0; i < digitsUser.Length; i++)
        {
            //Check if the digit in the same position in both arrays is the same
            if (digitsRand[i] == digitsUser[i]) placeCorrect++;
            //Else, check if the digit is in there at all
            else if (digitsRand.Contains(digitsUser[i])) numCorrect++;
            //Can use a foreach to check if the number is in the char[], however LINQ contains explicitly does the same thing.
            //Additionally you no longer have to worry about repeated numbers being counted twice - this is a true or false, no extra logic necessary
        }

        AddRow();

    }
    public void AddRow()
    {
        //Make new list (table) entry
        TableData row = new TableData();
        //Set object values
        row.guessCol = userguess;
        row.correctnumCol = numCorrect;
        row.bothcorrectCol = placeCorrect;
        //Add to list
        guesses.Add(row);

        //Let user know in english how the guess went
        resultText.text = $"'{userguess}' has {placeCorrect} number{((placeCorrect == 1) ? "" : "s")} in the correct position, and {numCorrect} that {((numCorrect == 1) ? "was" : "were")} in the number.";
       
        UpdateTable();
    }
    void UpdateTable()
    {
        //Delete previous to iterate over all again (can be optimised)
        foreach (Transform child in ParentObject.transform)
        {
            if (child.name != "Headers") Destroy(child.gameObject);
        }
        //Iterate through all list entries and instantiate on table
        for (int i = 0; i < guesses.Count; i++)
        {
            Debug.Log($"Row: {i}" +
            $"\nGuess #: {guesses[i].guessCol}" +
            $"\nCorrect Numbers: {guesses[i].correctnumCol}" +
            $"\nCorrect Numbers: {guesses[i].bothcorrectCol}");

            //Instantiate button
            GameObject guessRow = Instantiate(rowPrefab);
            //Fix default scale
            guessRow.transform.localScale = new Vector3(1, 1, 1);
            //Organise instatiated buttons to be easily deleted
            guessRow.transform.SetParent(ParentObject, false);

            //Set x position and row - HANDLED BY VERTICAL CONTENT COMPONENT
            //guessRow.transform.localPosition = new Vector3(-25, ypos, 0);

            //Get all three text objects in the prefab
            Text[] texts = guessRow.GetComponentsInChildren<Text>();

            //Update them to appropriate values
            texts[0].text = guesses[i].guessCol.ToString();
            texts[1].text = guesses[i].correctnumCol.ToString();
            texts[2].text = guesses[i].bothcorrectCol.ToString();
        }
        
    }
    public void MainMenu()
    {
        //Main menu button functionality
        SceneManager.LoadScene("Main");
    }
}
//Custom row class definition - so that one entry in the list has all three values. Simplified table generating.
//Multi dimensional arrays also considered
internal class TableData
{
    internal int guessCol;
    internal int correctnumCol;
    internal int bothcorrectCol;
}

