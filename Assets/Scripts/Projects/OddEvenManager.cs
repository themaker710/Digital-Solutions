using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OddEvenManager : MonoBehaviour
{
    public GameObject OddPanel;
    public GameObject EvenPanel;
    private Outline OddOutline;
    private Outline EvenOutline;
    public Text Mock;

    public InputField userinput;

    decimal num;
    private void Start()
    {
        OddOutline = OddPanel.GetComponent<Outline>();
        EvenOutline = EvenPanel.GetComponent<Outline>();
    }

    public void OnCalculate()
    {
        if (!decimal.TryParse(userinput.text, out num))
        {
            Mock.text = "Please enter a valid integer";
            StopAllCoroutines();
            EvenOutline.enabled = false;
            OddOutline.enabled = false;
            return;
        }
        
        Mock.text = "Surely you can work this one out for yourself";
        StopAllCoroutines();
        EvenOutline.enabled = false;
        OddOutline.enabled = false;

        if (num % 2 == 0)
        {
            Debug.Log("Even");
            StartCoroutine(Blink(false));
        }
        else
        {
            Debug.Log("Odd");
            StartCoroutine(Blink(true));
        }
    }
    //Make it blink
    public IEnumerator Blink(bool odd)
    {
        while (true)
        {

            if (odd) OddOutline.enabled = true;
            else EvenOutline.enabled = true;
            yield return new WaitForSeconds(.5f);
            if (odd) OddOutline.enabled = false;
            else EvenOutline.enabled = false;
            yield return new WaitForSeconds(.5f);
        }

    }
    public void MainMenu()
    {
        //Main menu button functionality
        SceneManager.LoadScene("Main");
    }
}
