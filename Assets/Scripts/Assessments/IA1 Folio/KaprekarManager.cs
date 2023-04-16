using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class KaprekarManager : MonoBehaviour
{
    public Text result;
    public void Generate()
    {
        //Display all kaprekan numbers up to 10,000 on one line when a button is pressed.
        string output = "";

        //Repeat check for all numbers to 10,000
        for (int i = 0; i < 10000; i++)
        {
            if (CheckKaprekar(i)) output += i + ", ";
        }

        //remove last ", " - UX
        //One line:
        result.text = output.Remove(output.Length - 2, 2);
    }
    bool CheckKaprekar(int num)
    {
        int length;

        //Get length
        //Get the logarithm (exponent of 10) of the number always rounded up for length, if 0 or 1 set to one (log10 doesn't like 0's and equals 0 for 1)
        if (num <= 1) length = 1;
        else length = (int)Mathf.Ceil(Mathf.Log10(num));

        //Get num squared
        int square = num * num;
        //get two number components based on initial digit length (d) from the number squared
        //Debug.Log($"{num} is {length} digits long and squares to be {square}");
        string[] strarr = square.ToString().Insert(((square <= 1) ? 1 : (int)Mathf.Ceil(Mathf.Log10(square))) - length, "/").Split('/');
        //Make new number array to be added
        int[] nums = new int[strarr.Length];

        //Loop through each part of the number (should always be two)
        for (int i = 0; i < strarr.Length; i++)
        {
            //Any blank values are considered to be zero - ("if one [piece] is empty, treat as zero")
            if (string.IsNullOrEmpty(strarr[i])) strarr[i] = "0";
            //(strarr[i] is null or "") also works, likely only recent versions of .NET supports it

            //Add to int[], parsing the char (no tryparse neccesarry as input is computer con
            nums[i] = int.Parse(strarr[i].ToString());
        }   
        //If the parts add to the original number, then its kaprekan, else its not
        if (nums[0] + nums[1] == num) return true;
        else return false;
    }
    public void MainMenu()
    {
        //Main menu button functionality
        SceneManager.LoadScene("Main");
    }
}
