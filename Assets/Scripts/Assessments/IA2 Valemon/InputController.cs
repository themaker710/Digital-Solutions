using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Mail;
using System;

public class InputController : MonoBehaviour
{
    public TMP_InputField passwordInput;
    public TMP_InputField passwordConfInput;

    public TMP_InputField nameInput;
    public TMP_InputField addressInput;

    public TMP_InputField emailInput;
    public TMP_InputField emailConfInput;

    public RawImage visibilityImage;

    public Texture2D visibleTexture;
    public Texture2D hiddenTexture;

    //Profile inputs
    public TMP_InputField profileNameInput;
    public TMP_InputField profileAddressInput;
    public TMP_InputField profileEmailInput;
    public TMP_InputField profileCurrPassInput;
    public TMP_InputField profileNewPassInput;

    public TMP_Text paymentButtonText;

    public bool isPasswordValid, isEmailValid, paymentConnected = false;
    public bool AllowRegister() => !(addressInput.text.IsNullOrWhiteSpace() || nameInput.text.IsNullOrWhiteSpace()) && PasswordsMatch(hashedPass, passwordConfInput.text) && paymentConnected;
    
    public bool AllowUpdate() => !(profileAddressInput.text.IsNullOrWhiteSpace() || profileNameInput.text.IsNullOrWhiteSpace()) && PasswordsMatch(hashedPass, profileCurrPassInput.text);
    
    internal string hashedPass;

    public bool PasswordsMatch(string hashed, string provided) => Extensions.VerifyHashedPassword(hashed, provided);

    private void Start()
    {
        //Reset type & icon
        passwordInput.contentType = TMP_InputField.ContentType.Password;
        visibilityImage.texture = hiddenTexture;

        isPasswordValid = false;
       
    }

    public void OnPaymentClick()
    {
        paymentConnected = !paymentConnected;

        if (paymentConnected)
        {
            paymentButtonText.text = "Connected";
            paymentButtonText.color = Color.green;
        }
        else
        {
            paymentButtonText.text = "Connect Paymet Method";
            paymentButtonText.color = new Color(50f,50f,50f,255);
        }

        Debug.Log("Connected external payment service");


    }

    public void VisibilityOnClick()
    {
        //Toggle type & icon
        if (passwordInput.contentType == TMP_InputField.ContentType.Standard)
        {
            passwordInput.contentType = TMP_InputField.ContentType.Password;
            visibilityImage.texture = hiddenTexture;
        }
        else
        {
            passwordInput.contentType = TMP_InputField.ContentType.Standard;
            visibilityImage.texture = visibleTexture;
        }

        //passwordInput.ActivateInputField();
        passwordInput.Select();
    }

    public void ValidatePasswordStrength(string pass)
    {
        //Check if password matches a weak regex condtion

        //Regex pattern matching anything less than the minimum requirements (>8 characters, a digit and both cases)
        const string pattern = "^(.{0,7}|[^0-9]*|[^A-Z]*|[^a-z]*|[a-zA-Z0-9]*)$";

        if (Regex.IsMatch(pass, pattern))
            isPasswordValid = false;
        else
            isPasswordValid = true;
    }
    public void ValidateEmail(string email)
    {
        //Check if email matches the correct form using the MailAddress class that return a FormatException if the email is malformed

        if (email.IsNullOrWhiteSpace()) return;

        isEmailValid = true;
        string address;
        try { address = new MailAddress(email).Address; } catch(FormatException) { isEmailValid = false; }

    }
}
