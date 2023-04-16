using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class ContactsController : MonoBehaviour
{
    Resolution screenRes;


    List<ContactPrefabManager> contactsList;
    
    GameObject contactPrefab;
    GameObject detailRowPrefab;

    public GameObject mainScrollContent;
    public GameObject detailsScrollContent;

    string query;

    public InputField searchField;

    public Canvas[] canvases;

    //View Contact Linking Elements

    public Text[] nameOccTexts;
    public Text[] birthdateTexts;
    public InputField notesInputField;
    public Text primaryDetailText;
    public Text addressText;

    //internal List<Detail> details;
    //internal List<Address> addresses;

    internal Dictionary<int, int> addressIDs;

    internal Dictionary<string, int> monthValues = new Dictionary<string, int>() 
    { 
        {"January", 1},
        {"February", 2},
        {"March", 3},
        {"April", 4},
        {"May", 5},
        {"June", 6},
        {"July", 7},
        {"August", 8},
        {"September", 9},
        {"October", 10},
        {"November", 11},
        {"December", 12}
    };

    public Button editPrimaryDetailButton;

    //Modals
    public GameObject detailModal;
    public GameObject addressModal;

    //Detail Modal interactables
    public Text detailTitleText;
    public InputField valueInputField;
    public InputField detailTagInputField;
    public Toggle primaryToggle;
    public Dropdown detailTagDropDown;
    public Text redDetailButtonText;

    //Address Modal interactables
    public Text addressTitleText;
    public InputField[] addressInputFields;
    public InputField addressTagInputField;
    public Dropdown addressTagDropDown;
    public Text redAddressButtonText;

    //Edit/Add screen interactables
    public InputField[] editInputFields;
    public Dropdown[] dobDropDowns;
    public Button saveButton;

    int currContactID, currDetailID, currAddressID, pDetailID;
    bool first = true, saveDOB = false, editValueChanged = false;
    
    
    void Start()
    {
        //Set screen res
        screenRes = Screen.resolutions[Screen.resolutions.Length-1];
        Screen.SetResolution((screenRes.height / 16) * 9, screenRes.height, true);
        contactPrefab = Resources.Load<GameObject>("Prefabs/ContactRow");
        detailRowPrefab = Resources.Load<GameObject>("Prefabs/ContactDetailRow");

        //Populate year dropdown
        List<string> years = new List<string>();
        dobDropDowns[2].ClearOptions();
        for (int i = 1900; i <= System.DateTime.Now.Year; i++)
        {
            years.Add(i.ToString());
        }
        years.Reverse();
        dobDropDowns[2].AddOptions(years);

        dobDropDowns[0].interactable = false;
        dobDropDowns[1].interactable = false;
        UpdateDropDownLabels();

        LoadContactRows();
    }

    void LoadContactRows()
    {
        //Remove any existing children
        foreach (Transform child in mainScrollContent.transform)
            Destroy(child.gameObject);

        //Query DB
        DBManager.InitiateConnection("productivity.db");

        string q = $"SELECT FirstName, LastName, Nickname, ContactID FROM Contact WHERE " +
            $"FirstName LIKE \"%{query}%\" OR " +
            $"Nickname LIKE \"%{query}%\" OR " +
            $"LastName LIKE \"%{query}%\" OR " +
            $"OtherName LIKE \"%{query}%\" OR " +
            $"ContactID LIKE \"{query}\"";

        DBManager.QueryDB(q);

        contactsList = new List<ContactPrefabManager>();

        //Make prefab and assign values
        while (DBManager.reader.Read())
        {
            GameObject contactRow = Instantiate(contactPrefab, mainScrollContent.transform);
            ContactPrefabManager cpm = contactRow.GetComponent<ContactPrefabManager>();
            cpm.PopulateFields(DBManager.reader.SafeGet<string>(0), DBManager.reader.SafeGet<string>(2), DBManager.reader.SafeGet<string>(1), DBManager.reader.SafeGet<int>(3), this);
            contactsList.Add(cpm);
        }

        DBManager.CloseConnection();
    }
    void Update()
    {
        //Profiling shows this reduces redundant calls inherent in GetKeyDown method.
        if (Input.anyKeyDown)
        {
            //UX Feature - Quick delete entire search string
            if (Input.GetKeyDown(KeyCode.Delete) && EventSystem.current.currentSelectedGameObject == searchField.gameObject)
            {
                searchField.text = "";
                UpdateQuery();
            }
        }
    }


    public void UpdateQuery()
    {
        query = searchField.text;
        query.Trim();
        if (string.IsNullOrWhiteSpace(query))
            query = "";
        
        LoadContactRows();
    }

    internal void DeleteContact(int id)
    {
        Debug.Log("Deleting contact with id: " + id);

        //Saftey
        //if (true) return;

        DBManager.InitiateConnection("productivity.db");
        //Main Table
        DBManager.QueryDB("DELETE FROM Contact WHERE ContactID = " + id);
        //Details Table
        DBManager.QueryDB("DELETE FROM Details WHERE ContactID = " + id);
        //Address Table
        DBManager.QueryDB("DELETE FROM Address WHERE ContactID = " + id);

        DBManager.CloseConnection();


    }
    
    //Change labels on DOB dropdowns to their names
    internal void UpdateDropDownLabels()
    {
        if (first)
        {
            dobDropDowns[2].captionText.text = "Year";
            first = false;
        }
        dobDropDowns[0].captionText.text = "Day";
        dobDropDowns[1].captionText.text = "Month";
    }
    //Clear any DOB selections
    public void ClearDOB()
    {
        dobDropDowns[0].value = 0;
        dobDropDowns[1].value = 0;
        dobDropDowns[2].value = 0;
        first = true;
        saveDOB = false;
        dobDropDowns[0].interactable = false;
        dobDropDowns[1].interactable = false;

        UpdateDropDownLabels();
    }
    public void EditAddValueChanged()
    {
        editValueChanged = true;

        //Required field (first name)
        if (editInputFields[0].text.IsNullOrWhiteSpace())
            saveButton.interactable = false;
        else
            saveButton.interactable = true;
    }

    //When the year dropdown is changed, update the month dropdown to only show valid month strings
    public void UpdateMonthDropDown()
    {
        List<string> months = new List<string>();
        int year = int.Parse(dobDropDowns[2].options[dobDropDowns[2].value].text);
        
        for (int i = 1; i < 13; i++)
        {
            //Add each month, but stop after the current month & year is reached (any more would be invalid)
            if (i >= System.DateTime.Now.Month + 1 && year == System.DateTime.Now.Year)
                continue;
            months.Add(monthValues.GetKey(i));
        }

        //Update values in dropdown
        dobDropDowns[1].ClearOptions();
        dobDropDowns[1].AddOptions(months);

        UpdateDayDropDown();
    }
    //When the month dropdown is changed, update the day dropdown to only show valid days
    public void UpdateDayDropDown()
    {
        List<string> days = new List<string>();
        int year = int.Parse(dobDropDowns[2].options[dobDropDowns[2].value].text);
        int month = monthValues[dobDropDowns[1].options[dobDropDowns[1].value].text];
        int currDay = int.Parse(dobDropDowns[0].options[dobDropDowns[0].value].text);
        int maxDays = System.DateTime.DaysInMonth(year, month);
        
        if (currDay > maxDays)
        {
            //Reset day value to closest if new value exceeds max days
            dobDropDowns[0].value = maxDays - 1;
        }
        for (int i = 1; i < maxDays + 1; i++)
        {
            //Go through all the days, but stop if the current month/year/day combo is reached.
            if (i >= System.DateTime.Now.Day + 1 && month == System.DateTime.Now.Month && year == System.DateTime.Now.Year)
                continue;
            days.Add(i.ToString());
        }

        //Update values in dropdown
        dobDropDowns[0].ClearOptions();
        dobDropDowns[0].AddOptions(days);

        dobDropDowns[0].interactable = true;
        dobDropDowns[1].interactable = true;

        saveDOB = true;
        //How to determine if dropdowns changed by user or by code?
        

        //UpdateDropDownLabels();
        
    }

    public void EditAddContact()
    {
        //If id is -1, then we are adding a new contact
        if (currContactID == -1)
        {
            //Clear all input fields
            foreach (InputField inputField in editInputFields)
            {
                inputField.text = "";
            }
            saveButton.interactable = false;
            //Clear all dropdowns
            ClearDOB();
        }
        else
        {
            //Load contact data
            DBManager.InitiateConnection("productivity.db");
            DBManager.QueryDB("SELECT FirstName, LastName, Nickname, OtherName, JobTitle, Department, Company, Notes, DOB FROM Contact WHERE ContactID = " + currContactID);

            saveButton.interactable = true;

            //Set input fields
            if (DBManager.reader.Read())
            {
                //Set each input field
                for (int i = 0; i < editInputFields.Length; i++)
                    editInputFields[i].text = DBManager.reader.SafeGet<string>(i);

                //Set DOB dropdowns
                if (DBManager.reader.SafeGet<string>(8) != "")
                {
                    System.DateTime dob = System.DateTime.ParseExact(DBManager.reader.SafeGet<string>(8), "d/M/yyyy", null);
                    dobDropDowns[2].value = dobDropDowns[2].options.FindIndex(x => x.text == dob.Year.ToString());
                    UpdateMonthDropDown();
                    dobDropDowns[0].value = dob.Day - 1;
                    dobDropDowns[1].value = dob.Month - 1;
                }
                else ClearDOB();
            }

            DBManager.CloseConnection();
        }

        editValueChanged = false;
        saveButton.interactable = false;
    }

    //Save contact information
    public void SaveContact()
    {
        //Get DOB
        string dob = "";
        if (saveDOB)
            dob = dobDropDowns[0].options[dobDropDowns[0].value].text + "/" + monthValues[dobDropDowns[1].options[dobDropDowns[1].value].text] + "/" + dobDropDowns[2].options[dobDropDowns[2].value].text;
        
        if (currContactID == -1) 
        {
            //If no first name return
            if (string.IsNullOrWhiteSpace(editInputFields[0].text))
                return;

            Debug.Log("Adding new contact");

            //Save info as new contact
            DBManager.InitiateConnection("productivity.db");
            DBManager.QueryDB("INSERT INTO Contact (FirstName, LastName, Nickname, OtherName, JobTitle, Department, Company, Notes, DOB) VALUES ('" + editInputFields[0].text + "', '" + editInputFields[1].text + "', '" + editInputFields[2].text + "', '" + editInputFields[3].text + "', '" + editInputFields[4].text + "', '" + editInputFields[5].text + "', '" + editInputFields[6].text + "', '" + editInputFields[7].text + "', '" + dob + "')");

        }
        else
        {
            //Get all input fields
            string[] inputFields = new string[editInputFields.Length];
            for (int i = 0; i < editInputFields.Length; i++)
                inputFields[i] = editInputFields[i].text;

            Debug.Log("Updating contact");

            //Save to DB
            DBManager.InitiateConnection("productivity.db");
            DBManager.QueryDB("UPDATE Contact SET FirstName = '" + inputFields[0] + "', LastName = '" + inputFields[1] + "', Nickname = '" + inputFields[2] + "', OtherName = '" + inputFields[3] + "', JobTitle = '" + inputFields[4] + "', Department = '" + inputFields[5] + "', Company = '" + inputFields[6] + "', Notes = '" + inputFields[7] + "', DOB = '" + dob + "' WHERE ContactID = " + currContactID);
            DBManager.CloseConnection();
        }

        //Update contact rows
        LoadContactRows();
    }

    internal void LoadContact(int id)
    {
        
        Debug.Log("Loading contact with id: " + id);

        //Clear birthday texts
        foreach (Text t in birthdateTexts)
            t.text = "";


        DBManager.InitiateConnection("productivity.db");

        currContactID = id;

        //Switch View
        canvases[0].gameObject.SetActive(false);
        canvases[1].gameObject.SetActive(true);

        //Change text fields
        DBManager.QueryDB("SELECT FirstName, Nickname, LastName, OtherName, DOB, Company, Department, JobTitle, Notes, PrimaryDetailID FROM Contact WHERE ContactID = " + id);
        while (DBManager.reader.Read())
        {
            nameOccTexts[0].text = DBManager.reader.SafeGet<string>(0) + " " + DBManager.reader.SafeGet<string>(2);
            nameOccTexts[1].text = ((DBManager.reader.SafeGet<string>(1) != "") ? ("'" + DBManager.reader.SafeGet<string>(1) + "'") : "") + ((DBManager.reader.SafeGet<string>(3) == "" || DBManager.reader.SafeGet<string>(1) == "") ? "" : " | ") + DBManager.reader.SafeGet<string>(3);
            Debug.Log(nameOccTexts[1].text);
            nameOccTexts[2].text = DBManager.reader.SafeGet<string>(7) + ((DBManager.reader.SafeGet<string>(6) == "") ? "" : " in ") + DBManager.reader.SafeGet<string>(6) + ((DBManager.reader.SafeGet<string>(5) == "") ? "" : " at ") + DBManager.reader.SafeGet<string>(5);
            Debug.Log(DBManager.reader.SafeGet<string>(4));
            string dobStr = DBManager.reader.SafeGet<string>(4);
            if (dobStr == "")
                birthdateTexts[0].text = "No birthdate entered";
            else
            {
                System.DateTime dob = System.DateTime.ParseExact(dobStr, "d/M/yyyy", null);

                birthdateTexts[0].text = dob.ToString("d MMMM yyyy");

                //Determine age

                //Total days divided by the statistical average # of days in the Gregorian Calendar Year, rounded (placeholder method)
                birthdateTexts[1].text = (int)(System.DateTime.Now.Subtract(dob).Duration().TotalDays / 365.2425) + " years old";
            }


            notesInputField.text = DBManager.reader.SafeGet<string>(8);

            pDetailID = DBManager.reader.SafeGet<int>(9);
        }

        DBManager.reader.Close();

        //Load and add contact details

        DBManager.QueryDB("SELECT Contact, DetailID, TypeTag FROM Details WHERE ContactID = " + id);

        bool hasPrimary = false;

        //details = new List<Detail>();

        foreach (Transform child in detailsScrollContent.transform)
            Destroy(child.gameObject);
        
        while (DBManager.reader.Read())
        {
            string t = DBManager.reader.SafeGet<string>(2) + (DBManager.reader.SafeGet<string>(2) == "" ? "" : ": ") + DBManager.reader.SafeGet<string>(0);

            //Set primary detail if it exists (if primary detail is deleted, the field will remain. The following logic should remain consistent despite this anomaly)
            if (DBManager.reader.SafeGet<int>(1) == pDetailID)
            {
                primaryDetailText.text = t;
                editPrimaryDetailButton.interactable = true;
                editPrimaryDetailButton.onClick.RemoveAllListeners();
                int did = DBManager.reader.SafeGet<int>(1);
                editPrimaryDetailButton.onClick.AddListener(() => { LoadDetailModal(did); });
                hasPrimary = true;
                continue;
            }
            //Build array and list
            //Show all contact at first, refine with filter options after
            GameObject contactDetailRow = Instantiate(detailRowPrefab, detailsScrollContent.transform);
            ContactDetailPrefabManager cdpm = contactDetailRow.GetComponent<ContactDetailPrefabManager>();
            cdpm.cc = this;
            cdpm.valueText.text = t;
            cdpm.detailID = DBManager.reader.SafeGet<int>(1);

            //details.Add(new Detail(DBManager.reader.SafeGet<string>(0), DBManager.reader.SafeGet<string>(2), DBManager.reader.SafeGet<int>(1)));
        }

        if (!hasPrimary)
        {
            primaryDetailText.text = "No primary contact set";
            editPrimaryDetailButton.interactable = false;
        }

        
        DBManager.reader.Close();

        addressIDs = new Dictionary<int, int>();

        //Add entry to indicate new address
        addressIDs.Add(-1, -1);

        //Parse contact addresses
        DBManager.QueryDB("SELECT AddressID FROM Address WHERE ContactID = " + id);
        
        int i = 0;
        while (DBManager.reader.Read())
        {
            //Add to dictionary
            addressIDs.Add(i, DBManager.reader.SafeGet<int>(0));
            i++;

            currAddressID = 0;
        }

        LoadAddress();

        DBManager.CloseConnection();
    }
    public void ChangeAddress(bool up)
    {
        //If current id is -1, reset to 0
        if (currAddressID == -1)
            currAddressID = 0;

        if (up)
        {
            if (currAddressID + 2 < addressIDs.Count)
                currAddressID++;
        }
        else
        {
            if (currAddressID - 1 >= 0)
                currAddressID--;
        }
        LoadAddress();
    }    
    
    internal void LoadAddress()
    {
        //If current id == -1, then reset to 0
        if (currAddressID == -1)
            currAddressID = 0;


        if (!addressIDs.TryGetValue(currAddressID, out int addressID))
        {
            addressText.text = "No address found";
            return;
        }
            
        Debug.Log("Loading address with DB id: " + addressID + " (local id: " + currAddressID + ")");

        //Load address
        DBManager.InitiateConnection("productivity.db");
        DBManager.QueryDB("SELECT Street1, Street2, Suburb, State, PostCode, Country, TypeTag FROM Address WHERE AddressID = " + addressID);
        if (DBManager.reader.Read())
        {
            Address a = new Address()
            {
                street1 = DBManager.reader.SafeGet<string>(0),
                street2 = DBManager.reader.SafeGet<string>(1),
                suburb = DBManager.reader.SafeGet<string>(2),
                state = DBManager.reader.SafeGet<string>(3),
                pcode = DBManager.reader.SafeGet<string>(4),
                country = DBManager.reader.SafeGet<string>(5),
                typeTag = DBManager.reader.SafeGet<string>(6)
            };

            addressText.text = a.value;
        }
        DBManager.CloseConnection();
    }

    //Link dropdown and text field
    public void ChangeDetailType(int index)
    {
        Debug.Log("Changed dropdown");
        if (index == 0)
            detailTagInputField.text = "";
        else
            detailTagInputField.text = detailTagDropDown.options[index].text;
    }
    //Link dropdown and text field for address
    public void ChangeAddressType(int index)
    {
        Debug.Log("Changed dropdown");
        if (index == 0)
            addressTagInputField.text = "";
        else
            addressTagInputField.text = addressTagDropDown.options[index].text;
    }
    //Set detail dropdown to default (custom) when text field is changed
    public void ChangeDetailTag(string tag)
    {
        Debug.Log("Changed text field");
        
        int i = detailTagDropDown.options.FindIndex(x => x.text == tag);
        
        if (i == -1)
            detailTagDropDown.value = 0;
        else detailTagDropDown.value = i;
    }
    //Set address dropdown to default when field is changed
    public void ChangeAddressTag(string tag)
    {
        Debug.Log("Changed text field");

        int i = addressTagDropDown.options.FindIndex(x => x.text == tag);

        if (i == -1)
            addressTagDropDown.value = 0;
        else addressTagDropDown.value = i;
    }
    //Load the address modal and set the input fields to the current address values

    public void LoadAddressModal()
    {

        if (!addressIDs.TryGetValue(currAddressID, out int addressID))
            return;

        //Clear fields
        addressTagInputField.text = "";
        foreach (InputField i in addressInputFields)
            i.text = "";
        addressTagDropDown.value = 0;
        redAddressButtonText.text = "Delete";


        //If id is 0, then this is a new address
        if (currAddressID == -1)
        {

            Debug.Log("Loading new address");

            addressTitleText.text = "New Address";
            redAddressButtonText.text = "Cancel";
            addressModal.SetActive(true);
        }
        else
        {
            //Load address
            DBManager.InitiateConnection("productivity.db");
            DBManager.QueryDB("SELECT Street1, Street2, Suburb, PostCode, State, Country, TypeTag FROM Address WHERE AddressID = " + addressID);
            if (DBManager.reader.Read())
            {
                addressInputFields[0].text = DBManager.reader.SafeGet<string>(0);
                addressInputFields[1].text = DBManager.reader.SafeGet<string>(1);
                addressInputFields[2].text = DBManager.reader.SafeGet<string>(2);
                addressInputFields[3].text = DBManager.reader.SafeGet<string>(3);
                addressInputFields[4].text = DBManager.reader.SafeGet<string>(4);
                addressInputFields[5].text = DBManager.reader.SafeGet<string>(5);
                addressTagInputField.text = DBManager.reader.SafeGet<string>(6);
                ChangeAddressTag(addressTagInputField.text);
            }
            DBManager.CloseConnection();
        }
        addressModal.SetActive(true);
    }
    //Save address details from modal
    public void SaveAddress()
    {
        int addressID = addressIDs[currAddressID];
        DBManager.InitiateConnection("productivity.db");

        //if all fields are empty, then return
        if (addressInputFields[0].text == "" && addressInputFields[1].text == "" && addressInputFields[2].text == "" && addressInputFields[3].text == "" && addressInputFields[4].text == "" && addressInputFields[5].text == "") return;

        //If id is -1, then this is a new address
        if (currAddressID == -1)
            DBManager.QueryDB("INSERT INTO Address (Street1, Street2, Suburb, PostCode, State, Country, TypeTag, ContactID) VALUES ('" + addressInputFields[0].text + "', '" + addressInputFields[1].text + "', '" + addressInputFields[2].text + "', '" + addressInputFields[3].text + "', '" + addressInputFields[4].text + "', '" + addressInputFields[5].text + "', '" + addressTagInputField.text + "', " + currContactID + ")");
        else
            DBManager.QueryDB("UPDATE Address SET Street1 = '" + addressInputFields[0].text + "', Street2 = '" + addressInputFields[1].text + "', Suburb = '" + addressInputFields[2].text + "', PostCode = '" + addressInputFields[3].text + "', State = '" + addressInputFields[4].text + "', Country = '" + addressInputFields[5].text + "', TypeTag = '" + addressTagInputField.text + "' WHERE AddressID = " + addressID);
        
        
        DBManager.CloseConnection();
        
        //Reload contact
        LoadContact(currContactID);
    }

    //Load the detail modal and set the detail ID to the one passed in
    public void LoadDetailModal(int id)
    {
        currDetailID = id;
        
        //Clear fields
        detailTitleText.text = "Edit Detail";
        redDetailButtonText.text = "Delete";
        primaryToggle.interactable = true;
        primaryToggle.isOn = id == pDetailID;
        valueInputField.text = "";
        detailTagInputField.text = "";
        detailTagDropDown.value = 0;

        //If id is 0, then create new detail
        if (id == 0)
        {
            Debug.Log("New detail, opening empty modal");

            primaryToggle.interactable = false;
            primaryToggle.isOn = false;
            detailTitleText.text = "Add Detail";
            redDetailButtonText.text = "Cancel";
            detailModal.SetActive(true);
            return;
        }

        //Load detail data from database and fill fields
        Debug.Log("Loading detail modal with id: " + id);

        DBManager.InitiateConnection("productivity.db");
        DBManager.QueryDB("SELECT Contact, TypeTag FROM Details WHERE DetailID = " + id);
        if (DBManager.reader.Read())
        {
            valueInputField.text = DBManager.reader.SafeGet<string>(0);

            detailTagInputField.text = DBManager.reader.SafeGet<string>(1);
            ChangeDetailTag(detailTagInputField.text);
        }

        DBManager.CloseConnection();

        detailModal.SetActive(true);
    }
    public void SaveDetailInfo()
    {
        DBManager.InitiateConnection("productivity.db");
        //If value is empty, return
        if (valueInputField.text == "")
        {
            Debug.Log("Value is empty");
            return;
        }

        //If id is 0, then create new detail
        if (currDetailID == 0)
            DBManager.QueryDB("INSERT INTO Details (Contact, TypeTag, ContactID) VALUES ('" + valueInputField.text + "', '" + detailTagInputField.text + "', '" + currContactID + "')");
        else
            DBManager.QueryDB("UPDATE Details SET Contact = '" + valueInputField.text + "', TypeTag = '" + detailTagInputField.text + "' WHERE DetailID = " + currDetailID);
        
        if (primaryToggle.isOn)
            DBManager.QueryDB("UPDATE Contact SET PrimaryDetailID = " + currDetailID + " WHERE ContactID = " + currContactID);
        else if (currDetailID == pDetailID)
            DBManager.QueryDB("UPDATE Contact SET PrimaryDetailID = NULL WHERE ContactID = " + currContactID);

        DBManager.CloseConnection();
        
        //Reload contact details
        LoadContact(currContactID);
    }

    //Delete current address
    public void DeleteAddress()
    {
        //If id -1 reset to first position
        if (currAddressID == -1)
            currAddressID = 0;
        else
        {
            //Delete
            DBManager.InitiateConnection("productivity.db");
            DBManager.QueryDB("DELETE FROM Address WHERE AddressID = " + addressIDs[currAddressID]);
            DBManager.CloseConnection();
        }
        
        //Reload
        LoadContact(currContactID);
    }
    public void AddContact()
    {
        currContactID = -1;
        EditAddContact();
    }
    public void AddAddress()
    {
        currAddressID = -1;
        LoadAddressModal();
    }
    //Delete Current Detail Info
    public void DeleteDetailInfo()
    {
        //If new return (cancel)
        if (currDetailID == 0)
            return;
        Debug.Log("Deleting detail with id: " + currDetailID);
        //Delete from database
        DBManager.InitiateConnection("productivity.db");
        DBManager.QueryDB("DELETE FROM Details WHERE DetailID = " + currDetailID);
        DBManager.CloseConnection();

        //Reload contact details
        LoadContact(currContactID);
    }

  
    private void OnApplicationQuit()
    {
        //Reset Screen Resolution
        Screen.SetResolution(screenRes.width, screenRes.height, true);
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("Main");
        
    }
}


[System.Serializable]
struct Address
{
    /// <summary>
    /// Preformatted address value, assuming Australian locale.
    /// </summary>
    public string value => string.Join("\n", string.Join(" ", street1, street2), string.Join(" ", suburb, pcode), string.Join(" ", state, country)).Trim();
    
    //Data
    internal string street1;
    internal string street2;
    internal string suburb;
    internal string state;
    internal string pcode;
    internal string country;

    internal string typeTag;
    internal int id;
}

