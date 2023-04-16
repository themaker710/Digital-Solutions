using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CountManager : MonoBehaviour
{
    public Text outputText;
    public InputField userinput;

    int countby, total;
    string output;
    public void Calculate()
    {
        if (!(int.TryParse(userinput.text, out countby) && countby <= 10000 && countby >= 1))
        {
            outputText.text = "Please enter a valid integer between 1 and 10,000";
            return;
        }
        total = 0;
        output = "";

        for (int i = 0; total <= 10000; i++)
        {
            output += total + ", ";
            total = total + countby;
        }
        outputText.text = output.Remove(output.Length - 2, 2);
    }
    public void MainMenu()
    {
        //Main menu button functionality
        SceneManager.LoadScene("Main");
    }
}
