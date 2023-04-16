using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PopUpManager : MonoBehaviour
{
    public Text titleText;
    public Text descriptionText;
    public Text button1Text;
    public Text button2Text;

    internal bool clicked = false;
      internal bool? result = null;

    internal void PopulateFields(string title, string content, string button1, string button2)
    {
        titleText.text = title;
        descriptionText.text = content;
        button1Text.text = button1;
        button2Text.text = button2;
}

    public void TruePressed()
    {
        result = true;
        clicked = true;
        Debug.Log("Modal button true pressed");
        
    }
    
    public void FalsePressed()
    {
        result = false;
        clicked = true;
        Debug.Log("Modal button false pressed");
    }

    internal IEnumerator GetResult(System.Action<bool> callback)
    {
        Debug.Log("Starting result listener");

        yield return new WaitUntil(() => result != null);

        //yield return new WaitForSeconds(4.0f);

        if (result == null)
        {
            Debug.Log("An error occured getting the result of the popup, returning false");

            //throw new System.ArgumentNullException("result is null");
        }

        Debug.Log("Result recieved");

        //Null Coalescing operator - what value should be if null i.e. (b.HasValue ? b.Value : false) === (b ?? false)
        callback(result ?? false);
    
    }
}
