using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class CheckerManager : MonoBehaviour
{
    //Define Variables
    public Text resultText;
    public Text loading;
    public Text factorsText;
    public InputField userinput;
    double candidate = 0;
    Image panel;
    bool processed = false;


    void Start()
    {
        panel = GameObject.Find("Panel").GetComponent<Image>();
        //panel.color = Color.white;
    }
    public void onSubmit()
    {
        Debug.ClearDeveloperConsole();
        factorsText.text = "";
        //Validate input
        if (!double.TryParse(userinput.text, out candidate))
        {
            resultText.text = "Please enter a valid integer";
            return;
        }

        StartCoroutine(loadingCheck());
        Debug.Log(candidate);
        //Check prime status and display result
        if (IsPrime(candidate))
        {
            resultText.text = "Prime";
            panel.color = Color.green;
            Debug.Log("Yes");
        }
        else
        {
            panel.color = Color.red;
            resultText.text = "Not Prime";
            Debug.Log("No");
            //IF not prime, show factors
            string factors = "Factors: ";
            foreach (var num in GetFactors(candidate))
            {
                factors += num + ", ";           
            }
            factors = factors.Remove(factors.Length - 2, 2);
            factorsText.text = factors;
        }
        processed = true;
    }
    //Used tutorial and example for this
    static IEnumerable<int> GetFactors(double n)
    {
        return from a in Enumerable.Range(1, (int)n)
               where n % a == 0
               select a;
    }
    public static bool IsPrime(double number)
    {
        //IF number is divisable by anything other than one and itself, its not a prime
        int i;
        for (i = 2; i <= number - 1; i++)
        {
            if (number % i == 0)
            {
                return false;
            }
        }
        if (i == number)
        {
            return true;
        }
        return false;
    }
    IEnumerator loadingCheck()
    {
        loading.text = "Loading...";
        while (!processed) yield return new WaitForSeconds(0.1f);
        yield return new WaitForSeconds(0.1f);
        processed = false;
        loading.text = "";
    }

    public void MainMenu()
    {
        //Main menu button functionality
        SceneManager.LoadScene("Main");
    }

}
