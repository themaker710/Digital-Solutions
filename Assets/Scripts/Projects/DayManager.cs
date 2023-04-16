using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DayManager : MonoBehaviour
{
    public Text result;
    public InputField dayInput, monthInput, yearInput;
    int day, month, year;
    DateTime date;

    // Doesn't handle input of fringe dates such as 30 of Feb
    public void Calculate()
    {
        result.fontSize = 65;
        if (!int.TryParse(dayInput.text, out day) || day <= 0 || day > 31)
        {
            result.text = "Please enter a valid day integer (d) e.g. 23, 5";
            return;
        }        
        if (!int.TryParse(monthInput.text, out month) || month <= 0 || month > 12)
        {
            result.text = "Please enter a valid month integer (M) e.g. 12, 4";
            return;
        } 
        if (!int.TryParse(yearInput.text, out year))
        {
            result.text = "Please enter a valid year integer (yyyy) e.g. 2003, 1984";
            return;
        }
        result.fontSize = 140;
        try
        {
            date = DateTime.ParseExact(day + "/" + month + "/" + year, "d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            result.fontSize = 65;
            result.text = "Please enter a valid date e.g. 12/3/1994, 25/2/2012";
            return;
        }


        result.text = "You are " + (DateTime.Now.Date - date.Date).TotalDays.ToString() + " days old";
    }
    public void MainMenu()
    {
        //Main menu button functionality
        SceneManager.LoadScene("Main");
    }

}
