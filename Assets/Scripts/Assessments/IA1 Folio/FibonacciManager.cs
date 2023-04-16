using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FibonacciManager : MonoBehaviour
{
    //Define Unity (public) Variables
    public Text resultText;



    //Generate button links here
    public void OnGenerate()
    {
        //Define internal variables
        decimal num1 = 0m, num2 = 1m, num3;
        string str = "0, 1, ";
        //Loop for i < limit times, in this case 100 (base 0). 98 as first two begin in the string (see string def above) as they are the starting values
        for (int i = 2; i < 100; i++)
        {
            //Calculate the next number
            num3 = num1 + num2;
            //Set num1 to num2 (previous number in sequence)
            num1 = num2;
            //Num2 is now the cumulative sum.
            num2 = num3;
            //Add result to string
            str += num3 + ", ";
        }
        //Set resultant text, and remove last ", " for user experience and visual clarity
        resultText.text = str.Remove(str.Length - 2, 2);
    }
    public void MainMenu()
    {
        //Main menu button functionality
        SceneManager.LoadScene("Main");
    }
}
