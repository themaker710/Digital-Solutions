using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MetricsManager : MonoBehaviour
{
    //define unity elements
    public Text perimeter;
    public Text area;
    public Text sa;
    public Text volume;
    public Text buttonText;
    public InputField userinput;

    //define variables
    double side = 0;
    int i;

    //define identifier and result arrays
    double[] calculatedArray = new double[4];
    string[] typeArray = { "perimeter", "area", "sa", "volume" };
    bool[] scientificNote = { false, false, false, false };

    public void OnCalculate()
    {
        buttonText.text = "Calculate";

        //input validation
        if (!double.TryParse(userinput.text, out side))
        {
            buttonText.text = "Please enter a valid integer";
            return;
        }

        //calculations
        calculatedArray[0] = side * 4;
        calculatedArray[1] = side * side;
        calculatedArray[2] = calculatedArray[1] * 6;
        calculatedArray[3] = side * side * side;
 

        for (i=0; i<=3; i++)
        {
            //Should scientific notation be calculated? 5 is the length limit for the number.
            if (calculatedArray[i].ToString().Length > 5) scientificNote[i] = true;
            
            //Alternate code for above, if statment more readable. Below is "Ternary Statment"            
            //scientificNote[i] = (calculatedArray[i].ToString().Length > 5) ? true : false;

            //setting resultant text
            switch (typeArray[i])
            {
                case "perimeter":
                    if (!scientificNote[i]) perimeter.text = calculatedArray[i].ToString();
                    else perimeter.text = scientificNotation(calculatedArray[i]);
                    break;
                case "area":
                    if (!scientificNote[i]) area.text = calculatedArray[i].ToString();
                    else area.text = scientificNotation(calculatedArray[i]);
                    break;
                case "sa":
                    if (!scientificNote[i]) sa.text = calculatedArray[i].ToString();
                    else sa.text = scientificNotation(calculatedArray[i]);
                    break;
                case "volume":
                    if (!scientificNote[i]) volume.text = calculatedArray[i].ToString();
                    else volume.text = scientificNotation(calculatedArray[i]);
                    break;
                default: break;
            }
        }
        //set all back to default (false)
        scientificNote = new bool[4];
    }
    string scientificNotation(double num)
    {
        //calculate exponent
        int exponent = num == 0 ? 0 : (int)Math.Floor(Math.Log10(Math.Abs(num)));
        //Put exponent into unicode superscript
        //2070 is superscript, 2080 is sub
        var chars = exponent.ToString().Select(c => (char)('\u2070' + c - '0'));
        string superscript = new string(chars.ToArray());

        //Construct the output string, taking the decimals
        string numerals = num.ToString();
        //Flaw: Lack of rounding for final decimal place because of cheaty method used in creating decimal number
        numerals = numerals.Substring(0, 1) + "." + numerals.Substring(1, 3) + " x 10" + superscript;

        return numerals;
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("Main");
    }
}
    