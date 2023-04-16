using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FloydManager : MonoBehaviour
{
	//Declare unity variables
	public InputField userinput;
	public Text outputText;
	public Dropdown dropdown;

	public void OnGenerate()
	{
		//Declare variables
		string output = "";
        int f, num = 0;
		int[][] outputArray;
		bool reverse = false;

		//Get input
		if (!int.TryParse(userinput.text, out int height))
		{
			outputText.text = "Please enter a valid integer";
			return;
		}
		
		//Set Variables based on input
		int absheight = Mathf.Abs(height) + 1;
		outputArray = new int[absheight][];

		//Original method for changing method of display to allow for use of best fit and content fitters for larger sequences. Worked when changing manually in inspector, however coding method did not work.
		//if (absheight > 6)
		//{
		//	outputText.GetComponent<ContentSizeFitter>().enabled = false;
		//	outputText.GetComponent<RectTransform>().transform.position = outputText.rectTransform.anchoredPosition3D;
		//}
		//else outputText.GetComponent<ContentSizeFitter>().enabled = true;

		//Populate jagged array
		for (int i = 0; i < absheight; i++)
		{
			//Initialise each internal array
			outputArray[i] = new int[i];

			for (int k = 0; k < i; k++)
			{
				//Set position
				num++;
				outputArray[i][k] = num;
			}
		}

		//Check if inversed
		if (height < 0) reverse = true;

		//Display numbers from jagged array
        for (int i = 0; i < outputArray.Length; i++)
		{
			//Reverse the index if needed, adjusting for base 0 etc
			if (reverse) f = outputArray.Length - i - 1;
			else f = i;

			//Get each value in row f (i)
			foreach (int n in outputArray[f])
            {
				//Add value to output
				output += n + " ";
			}
			//New line
			output += "\n";
		}

		//Remove any extra new lines
		outputText.text = output.Trim();
	}

	public void Symmetry()
    {
		//Switch between options when dropdown changed
		outputText.alignment = (dropdown.value == 0) ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
    }

	//Main Menu button functionality
	public void MainMenu()
    {
		SceneManager.LoadScene("Main");
    }
}
