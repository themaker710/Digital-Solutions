using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class CaesarManager : MonoBehaviour
{
    public InputField plainField;
    public InputField cipherField;
    public InputField keyInput;
    public Text resultText;

    int key;
    public void ButtonPress(bool input)
    {
        //True = Encrypt, False = Decrypt
        //Input is controlled at unity level, no tryparse necessary

        if (CheckKey()) Crypt(input);
        
    }
    public bool CheckKey()
    {
        resultText.color = Color.black;
        if (string.IsNullOrWhiteSpace(keyInput.text))
        {
            keyInput.text = "1";
            resultText.text = "Did you forget the key? Default value of 1 entered. To confirm press a button";
            return false;
        }
        else if (keyInput.text == "26")
        {
            resultText.color = Color.red;
            resultText.text = "WARNING! A key of 26 will not produce a cipher but will copy the text. Choose another key.";
            return false;
        }
        else
        {
            key = int.Parse(keyInput.text);

            resultText.text = "";
            if (key >= 26) resultText.text = "WARNING! Any key above 26 will be the same as the key minus 26 (no added security). Request processed regardless.";
            return true;
        }

    }
    private protected void Crypt(bool isEncrypt)
    {
        char[] alphabet = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        //Key

        //Debug.Log(System.Array.IndexOf(alphabet, 'b') + 1);
        //Debug.Log(alphabet.IndexOf('b') + 1);

        if (isEncrypt)
        {
            //Encrypt and result
            char[] plainCharArr = plainField.text.ToCharArray();
            char[] encoded = new char[plainCharArr.Length];
            int i = 0;
            foreach (char c in plainCharArr)
            {
                if (c.IsLetter()) encoded[i] = alphabet[(key + alphabet.IndexOf(c.ToUpper())) % 26];
                else encoded[i] = c;
                i++;
            }

            //Display Result

            cipherField.text = string.Concat(encoded);

        }
        else
        {
            //Decrypt and result
            char[] cipherCharArr = cipherField.text.ToCharArray();
            char[] decoded = new char[cipherCharArr.Length];
            int i = 0;
            foreach (char c in cipherCharArr)
            {
                
                if (c.IsLetter())
                {
                    int index = 0;
                    index = alphabet.IndexOf(c.ToUpper()) - key % 26;
                    if (index < 0) index += 26;
                    decoded[i] = alphabet[index];
                }
                else decoded[i] = c;
                //Debug.Log(c + " : " + index + "  :  " + decoded[i]);
                i++;
          
            }

            plainField.text = string.Concat(decoded);
        }
    }
    
    public void MainMenu()
    {
        SceneManager.LoadScene("Main");
    }
}
