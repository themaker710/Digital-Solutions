using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static Extensions;
using SFB;

public class VendingManager : MonoBehaviour
{

    //Unity Linking Variables
    //General
    public Text outputText;
    public RectTransform selectionPanel;
    public Canvas MainCanvas;
    public Canvas OwnerCanvas;
    public RectTransform restockParent;

    //Owner Panel
    public Text informationText;

    //Product edit fields
    public InputField imagePathField;
    public InputField lastRestockField;
    public InputField totalUnitsSold;
    public InputField[] productInformationFields = new InputField[5]; //set via inspector

    //Controls
    public Text positionText;

    //Prefabs
    GameObject rowPrefab;

    //General variable definitions
    bool fundsAdded, changesSaved = true, loopFlash = false;
    string output = "";
    int curID = 0;

    // Product data array defintions
    string[] productNames;
    string[] productImageURL;
    string[] productLastRestock;
    int[] productQuantities;
    int[] productLimits;
    int[] productsSold;
    decimal[] productRetailPrices;
    decimal[] productWholesalePrices;

    int productNumber;

    void Start()
    {
        rowPrefab = Resources.Load<GameObject>("Prefabs/QuickRestockRow");
        StartVending();
    }

    //Reset vending machine
    void StartVending()
    {
        //Update/Get product information
        GetData();
        UpdateText();
        //Reset variables
        fundsAdded = false;
        output = "";
        outputText.text = "Please add funds to begin transaction";
    }

    internal void GetData()
    {
        //Start DB connection
        DBManager.InitiateConnection("/vending.db");

        //Get number of products
        DBManager.QueryDB("SELECT COUNT(*) FROM Products");
        DBManager.reader.Read();

        productNumber = DBManager.reader.GetInt32(0);

        DBManager.reader.Close();

        //Create variables with appropriate length (however many option panels there are)
        productsSold = new int[productNumber];
        productNames = new string[productNumber];
        productQuantities = new int[productNumber];
        productLimits = new int[productNumber];
        productRetailPrices = new decimal[productNumber];
        productImageURL = new string[productNumber];
        productLastRestock = new string[productNumber];
        productWholesalePrices = new decimal[productNumber];

        //Loop through all selection options
        for (int i = 0; i < productNumber; i++)
        {
            //Query DB for database row of id - plus one as ID in db start at 1
            DBManager.QueryDB("SELECT UnitsSold, ProductName, CurrentQuantity, RetailPrice, RestockLimit, ImageURL, LastRestock, WholesalePrice FROM Products WHERE ID = " + (i + 1));

            //Declare intermediatary variables
            decimal retailPrice = 0m;
            decimal wholesalePrice = 0m;
            int quantity = 0;
            int limit = 0;
            int sold = 0;
            string name = "";
            string imageurl = "";
            string restockdate = "";

            //Read info from DB reader datastream
            while (DBManager.reader.Read())
            {
                sold = DBManager.reader.SafeGet<int>(0);
                name = DBManager.reader.SafeGet<string>(1);
                quantity = DBManager.reader.SafeGet<int>(2);
                retailPrice = DBManager.reader.SafeGet<decimal>(3);
                limit = DBManager.reader.SafeGet<int>(4);
                imageurl = DBManager.reader.SafeGet<string>(5);
                restockdate = DBManager.reader.SafeGet<string>(6);
                wholesalePrice = DBManager.reader.SafeGet<decimal>(7);
            }

            //Input values into array for use in app
            productsSold[i] = sold;
            productNames[i] = name;
            productQuantities[i] = quantity;
            productRetailPrices[i] = retailPrice;
            productLimits[i] = limit;
            productImageURL[i] = imageurl;
            productLastRestock[i] = restockdate;
            productWholesalePrices[i] = wholesalePrice;

            //Cleanup 
            DBManager.reader.Close();
            
        }
        //Cleanup
        DBManager.CloseConnection();
    }

    internal void UpdateText()
    {
        //Loop through all selection options
        for (int i = 0; i < productNames.Length; i++)
        {
            
            //Get the option panel to be edited
            Transform optionPanel = selectionPanel.transform.GetChild(i).transform;
            
            //Set the name text element with the database retrieved info
            optionPanel.GetChild(0).GetComponent<Text>().text = productNames[i];

            //Set the price text element with database retrieved info. This is then formatted using 'Standard Numeric Format Strings' seen in the ToString method
            //F2 means a Fixed point, with 2 representing the number of values past the decimal point.
            optionPanel.GetChild(2).GetComponent<Text>().text = "$" + productRetailPrices[i].ToString("F2");

            //Set the quantity text element with database retrieved info.
            //Similar to above, with D = Decimal and the 2 specifying the set number of digits
            optionPanel.GetChild(3).GetComponent<Text>().text = productQuantities[i].ToString("D2");
        }
    }
    public void SelectImage()
    {
        ExtensionFilter[] extensions = new[] {
                new ExtensionFilter("Image Files ", "png", "jpg", "jpeg" ),
                //new ExtensionFilter("Sound Files", "mp3", "wav" ),
                //new ExtensionFilter("All Files", "*" ),
            };

        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select Image", "", extensions, false);
        if (paths.Length > 0)
        {
            Debug.Log($"File selection operation returned the path: '{paths[0]}'");
            string imagePath = paths[0];
            imagePathField.text = imagePath;
            changesSaved = false;
        }
        else
        {
            Debug.Log("File selection operation cancelled");
        }
    }

    public void RestockAll()
    {
        DBManager.InitiateConnection("/vending.db");
        //Update database by setting all quantities to 10
        for (int i = 0; i < productQuantities.Length; i++)
        {
            string time = System.DateTime.Now.ToString();
            DBManager.QueryDB($"UPDATE Products SET CurrentQuantity = {productLimits[i]} WHERE ID = {i+1}");
            DBManager.QueryDB($"UPDATE Products SET LastRestock = '{time}' WHERE ID = {i+1}");
            productQuantities[i] = productLimits[i];
            productLastRestock[i] = time;
        }
        DBManager.CloseConnection();
        UpdateQuickRestock();
    }

    public void KeyPadPress(string input)
    {

        //User must add funds first (click button)
        if (!fundsAdded)
        {
            outputText.text = "Please add funds to begin transaction";
            return;
        }
        //If input cleared
        if (input == "*")
        {
            output = "";
            outputText.text = output;
            return;
        }
        //If input is submit
        if (input == "#")
        {
            //Parse the input to be tested - tryparse will simply skip if it fails.
            int.TryParse(output, out int id);
            //If input is empty
            if (output == "")
            {
                outputText.text = "Please input a drink position";
                return;
            }
            //If input is ownercode
            else if (id == 1234)
            {
                InitializeOwnerPanel();
                return;
            }
            //If selected number is an option
            else if (id <= 9 && id > 0)
            {
                //Array index: 0 based
                int i = id - 1;

                //If there is none of the drink left
                if (productQuantities[i] == 0)
                {
                    outputText.text = $"No {productNames[i]} stock\nChoose another drink";
                    output = "";
                    return;
                }

                //Give user feedback that product was purchased
                outputText.text = $"{productNames[i]} Dispensed for {"$" + productRetailPrices[i].ToString("F2")}. Please collect below";
                //Update DB - UPDATE Products SET Stock = Stock - 1 WHERE ID = SelectedProduct
                DBManager.InitiateConnection("/vending.db");
                DBManager.QueryDB($"UPDATE Products SET CurrentQuantity = CurrentQuantity - 1 WHERE ID = {id}");
                DBManager.QueryDB($"UPDATE Products SET UnitsSold = UnitsSold + 1 WHERE ID = {id}");
                DBManager.CloseConnection();
                //Reset interface
                StartVending();
                return;
            }
            //If input is invalid or unknown i.e. not caught by any other conditions above
            else
            {
                outputText.text = "Please enter a valid code e.g. 01, 1, 02, 2, etc";
                output = "";
                return;
            }
        }

        //Append inputted value to the output
        output += input;
        //Update UI element
        outputText.text = output;
    }

    void InitializeOwnerPanel()
    {
        //Ready the quick restock menu
        UpdateQuickRestock();
        //Database editor
        UpdateDatabaseEditor();
        //Switch canvases
        MainCanvas.gameObject.SetActive(false);
        OwnerCanvas.gameObject.SetActive(true);

        Debug.Log("Owner panel ready");
    }
    public void RefreshDatabaseEditor()
    {
        //Wrapper void type method for UI button
        UpdateDatabaseEditor();

        UpdateQuickRestock();
    }
    public bool UpdateDatabaseEditor()
    {
        decimal retailPrice = 0m;
        decimal wholesalePrice = 0m;
        int quantity = 0;
        int limit = 0;
        int sold = 0;
        string name = "";
        string imageurl = "";
        string restockdate = "";

        try
        {
            sold = productsSold[curID];
            name = productNames[curID];
            quantity = productQuantities[curID];
            retailPrice = productRetailPrices[curID];
            limit = productLimits[curID];
            imageurl = productImageURL[curID];
            restockdate = productLastRestock[curID];
            wholesalePrice = productWholesalePrices[curID];
        }
        catch (System.IndexOutOfRangeException e)
        {
            //Can land here when attempting to navigate to added product
            Debug.Log("Product page ID invalid and out of array range, data retrieval failed. Remaining on current page.\n Error: " + e.Message);
            return false;
        }
        catch (System.Exception e)
        {
            Debug.Log("An error occured loading locally stored product information: \n" + e.Message);
            return false;
        }

        //Set data to variables based on ID.
        productInformationFields[0].text = name;
        productInformationFields[1].text = retailPrice.ToString();
        productInformationFields[2].text = wholesalePrice.ToString();
        productInformationFields[3].text = quantity.ToString();
        productInformationFields[4].text = limit.ToString();

        imagePathField.text = imageurl;

        lastRestockField.text = restockdate;

        totalUnitsSold.text = sold.ToString();

        positionText.text = $"{curID + 1}/{productNumber}";

        changesSaved = true;

        Debug.Log($"Database modifier successfully initialized with product ID {curID+1}");
        return true;
    }

    public void SaveProductInformation()
    {
        // Check for null boxes and error to let user know which to fill in
        bool empty = false;
        for (int i = 0; i < productInformationFields.Length; i++)
        {
            //Not required - wholesale
            if (i == 2) i++;

            if (productInformationFields[i].text.IsNullOrWhiteSpace())
            {
                //Placeholder error, until modal works.
                Debug.Log(productInformationFields[i].name + " is empty, please add a value to this required space");

                //Reset field incase of whitespace so placeholder can be seen
                productInformationFields[i].text = "";

                //Flash field for user
                StartCoroutine(RequiredBlink(productInformationFields[i]));

                empty = true;
            }
        }

        if (empty) return;
        
        //If the user has changed the value then stock has been updated
        if (productInformationFields[3].text != productQuantities[curID].ToString())
        {
            productLastRestock[curID] = System.DateTime.Now.ToString();
        }
        //Save to db
        ManagedQuery("REPLACE INTO Products (ID, UnitsSold, ProductName, CurrentQuantity, RetailPrice, RestockLimit, ImageURL, LastRestock, WholesalePrice)" +
                $"VALUES ({curID + 1}, {productsSold[curID]}, '{productInformationFields[0].text}', {productInformationFields[3].text}, {productInformationFields[1].text}, {productInformationFields[4].text}, '{imagePathField.text}', '{productLastRestock[curID]}', {productInformationFields[2].text})");

        //Update local arrays
        GetData();

        changesSaved = true;
    }
    
    public void AddProduct()
    {
        //Reset fields onscreen
        productInformationFields[0].text = "";
        productInformationFields[1].text = "";
        productInformationFields[2].text = "";
        productInformationFields[3].text = "";
        productInformationFields[4].text = "";

        imagePathField.text = "";

        lastRestockField.text = "";

        totalUnitsSold.text = "";

        // To the end of the line (curID is one less - accessing arrays etc 0 based)
        curID = productNumber;
        productNumber++;
        positionText.text = $"{curID + 1}/{productNumber}";

        changesSaved = false;
    }

    public void DeleteProduct()
    {
        Debug.Log("Delete button pressed");
        //Confirmation modal currently hangs if executed through extension (due to threading management)
        //bool confirmed = OpenPopUp1("Confirm Action", "This action is irreversible. Are you sure you would like to continue?", "Continue", "Cancel", OwnerCanvas.gameObject, this);
        //if (!confirmed) return;
        //Debug.Log("deleting current entry")




        // Remove from database

        ManagedQuery($"DELETE FROM Products WHERE ID = {curID + 1}");

        Debug.Log("Current entry deleted from DB");

        changesSaved = true;

        //Update Arrays
        GetData();

        //Change Page

        productNumber--;
        ChangeProductPage(true);
    }

    public void ChangeProductPage(bool back)
    {
        if (!changesSaved)
        {
            Debug.Log("Changes unsaved, delete or restart");
            //Confirmation modal
            //See above function
            return;
        }

        int lastPage = curID;
        //ID logic and out of range proctection
        if (back)
        {
            if (curID == 0) return;

            curID--;
        } else
        {
            if (curID == (productNumber - 1)) return;

            curID++;
        }
        //Grey out buttons if unavaliable?


        if (!UpdateDatabaseEditor())
        {
            //Restore to previous successful page if data failure
            curID = lastPage;
            return;
        } 
        else
        {
            //Update text
            positionText.text = $"{curID + 1}/{productNumber}";
        }
    }

    internal void UpdateQuickRestock()
    {
        bool lowStock = false;
        List<int> quickRestockOptions = new List<int>();
        informationText.gameObject.SetActive(false);

        Debug.Log("Populating quick restock datalists");

        //Populate quick restock panel - any product with less than 10% of their restock limit left
        for (int i = 0; i < productQuantities.Length; i++)
        {
            if (productQuantities[i] <= Mathf.Round((float)(productLimits[i] * 0.1)))
            {
                quickRestockOptions.Add(i);
                lowStock = true;
            }
        }
        //Find product ID's with less than 10%
        //Instantiate and populate prefab list
        Debug.Log("Instantiating UI elements");
        //Prefab pretagged with 'Option'
        foreach (GameObject child in GameObject.FindGameObjectsWithTag("Option"))
        {
            Destroy(child);
        }

        if (lowStock)
        {
            foreach (int id in quickRestockOptions)
            {
                GameObject go = Instantiate(rowPrefab, restockParent.transform);
                QuickRestockManager script = go.GetComponent<QuickRestockManager>();
                script.PopulateFields(productNames[id], productQuantities[id], id);
            }
        } else
        {
            informationText.gameObject.SetActive(true);
        }

        Debug.Log("Quick restock ready");
    }
        
    public void InitializeMainPanel()
    {
        if (!changesSaved)
        {
            Debug.Log("Changes unsaved, delete or restart");
            //Confirmation modal
            //See OpenPopUp for flawed implementation
            return;
        }


        StartVending();
        MainCanvas.gameObject.SetActive(true);
        OwnerCanvas.gameObject.SetActive(false);
        Debug.Log("Main panel ready");
    }

    public void AddFunds()
    {
        //Button to add funds
        fundsAdded = true;
        Debug.Log("Funds added");
        output = "";
        outputText.text = "Select a drink by the bottom right corner number";
    }
    private void Update()
    {
        //Keypad Listener
        if (Input.GetKeyDown(KeyCode.KeypadMultiply) ||
            ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.Alpha8)))
        {
            KeyPadPress("*");
        }
        else if (Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.Alpha8))
        {
            KeyPadPress("8");
        }
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.Alpha3))
        {
            KeyPadPress("#");
        }
        else if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3))
        {
            KeyPadPress("3");
        }
        if (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0))
        {
            KeyPadPress("0");
        }
        if(Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1))
        {
            KeyPadPress("1");
        }
        if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2))
        {
            KeyPadPress("2");
        }
        if (Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4))
        {
            KeyPadPress("4");
        }
        if (Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Alpha5))
        {
            KeyPadPress("5");
        }
        if (Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.Alpha6))
        {
            KeyPadPress("6");
        }
        if (Input.GetKeyDown(KeyCode.Keypad7) || Input.GetKeyDown(KeyCode.Alpha7))
        {
            KeyPadPress("7");
        }
        if (Input.GetKeyDown(KeyCode.Keypad9) || Input.GetKeyDown(KeyCode.Alpha9))
        {
            KeyPadPress("9");
        }
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (output == "") return;
            output = output.Remove(output.Length - 1);
            outputText.text = output;
        }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            KeyPadPress("#");
        }

    }
    private void ManagedQuery(string query)
    {
        DBManager.InitiateConnection("/vending.db");

        DBManager.QueryDB(query);

        DBManager.CloseConnection();

        Debug.Log($"Query executed: {query}");
    }
    private IEnumerator RequiredBlink(InputField inputField)
    {
        ColorBlock cb = inputField.colors;
        loopFlash = true;
        
        while (loopFlash)
        {
            cb.normalColor = Color.red;
            inputField.colors = cb;
            yield return new WaitForSeconds(0.5f);
            cb.normalColor = Color.white;
            inputField.colors = cb;
            yield return new WaitForSeconds(1.0f);
        }
    }
    public void onInputChange()
    {
        loopFlash = false;
    }
    public void MainMenu()
    {
        if (!changesSaved)
        {
            //Confirmation modal
        }
        SceneManager.LoadScene("Main");
    }
}
