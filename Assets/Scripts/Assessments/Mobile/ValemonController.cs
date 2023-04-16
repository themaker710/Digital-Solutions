using UnityEngine;
using TMPro;
using DB = AsyncDBManager;
using System.Data;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ValemonController : MonoBehaviour
{
    Resolution screenRes;

    public GameObject SignupModal;

    public TMP_Text errorText;
    public TMP_Text errorDialogText;
    public TMP_Text searchEmptyText;

    public InputController inputController;


    //Prefab for main list rows
    public GameObject pokemonRowPrefab;

    //Canvases
    public Canvas mainMenuCanvas;
    public Canvas authCanvas;
    public Canvas pokeInfoCanvas;

    public GameObject scrollContent;
    public TMP_InputField searchInput;

    //Filters
    public TMP_Dropdown typeDropdown;
    public TMP_Dropdown generationDropdown;
    public TMP_Dropdown classDropdown;

    public DataManager dataManager;

    internal Pokemon[] pokemon;

    int curUID;

    public void Register()
    {
        errorText.color = Color.red;
        //If no valid password, return
        if (!inputController.isPasswordValid)
        {
            errorText.text = "Password should contain 8 characters, a number, and a symbol";
            return;
        };
        //Apply regex to email
        if (!inputController.isEmailValid)
        {
            errorText.text = "Please enter a valid email";
            return;
        }
        //if no entered values in email or password, return
        if (inputController.emailInput.text == "" || inputController.passwordInput.text == "")
        {
            errorText.text = "Please enter an email and password";
            return;
        }

        //If user account email already exists, return
        IDataReader reader = DB.QueryDB($"SELECT * FROM users WHERE Email = '{inputController.emailInput.text}'");
        if (reader.Read())
        {
            errorText.text = "Email already exists. Login instead";
            return;
        }
        reader.Close();

        //Temp save password hash for comparison
        inputController.hashedPass = Extensions.HashPassword(inputController.passwordInput.text);

        //Unity based method (not secure)
        //inputController.hashedPass = Hash128.Compute(passwordInput.text).ToString();
        Debug.Log(inputController.hashedPass);

        inputController.emailConfInput.text = inputController.emailInput.text;

        //Clear inputs for modal
        inputController.passwordConfInput.text = "";
        inputController.nameInput.text = "";
        inputController.addressInput.text = "";

        //Open info modal to get finalise user details
        SignupModal.SetActive(true);
    }

    public void CompleteRegistration()
    {
        if (!inputController.AllowRegister())
        {
            errorDialogText.text = "Check your passwords match and you have filled all fields";
            return;
        }
        
        //Add user to DB
        DB.QueryDB($"INSERT INTO users (Email, HashedPassword, Name, Address) VALUES ('{inputController.emailInput.text}', '{inputController.hashedPass}', '{inputController.nameInput.text}', '{inputController.addressInput.text}')");

        SignupModal.SetActive(false);

        //Clear inputs
        inputController.emailInput.text = "";
        inputController.passwordInput.text = "";
        inputController.passwordConfInput.text = "";
        inputController.nameInput.text = "";
        inputController.addressInput.text = "";

        //clear temp saved hash
        inputController.hashedPass = "";

        errorText.color = Color.green;
        errorText.text = "Registration successful. Please login";
    }

    public void Login()
    {
        errorText.color = Color.red;
        //If no entered values in email or password, return
        if (inputController.emailInput.text.IsNullOrWhiteSpace() || inputController.passwordInput.text.IsNullOrWhiteSpace())
        {
            errorText.text = "Please enter an email and password";
            return;
        }

        //Apply email validation
        if (!inputController.isEmailValid)
        {
            errorText.text = "Please enter a valid email";
            return;
        }

        //Get user from DB
        IDataReader reader = DB.QueryDB($"SELECT ID, HashedPassword FROM users WHERE Email LIKE '{inputController.emailInput.text}'");
        if (reader.Read())
        {
            //Get hashed password from DB
            string correctHash = reader.SafeGet<string>(1);

            //Compare hashed password to entered password - SOMETHING WRONG HERE
            if (Extensions.VerifyHashedPassword(correctHash, inputController.passwordInput.text))
            {
                //If passwords match, load main menu
                errorText.color = Color.green;
                errorText.text = "Login successful, loading...";
                curUID = reader.SafeGet<int>(0);

                authCanvas.gameObject.SetActive(false);
                LoadMainList();
            }
            else
            {
                errorText.text = "Incorrect password";
            }
        }
        else
        {
            errorText.text = "Email not found";
        }
        reader.Close();
    }
    internal void LoadMainList()
    {
        //Populate filter dropdowns

        //Populate type dropdown
        typeDropdown.ClearOptions();
        typeDropdown.AddOptions(new List<string>() { "All" });
        typeDropdown.AddOptions(dataManager.GetTypes());

        //Populate generation dropdown
        generationDropdown.ClearOptions();
        generationDropdown.AddOptions(new List<string>() { "All" });
        generationDropdown.AddOptions(dataManager.GetGenerations());

        //Populate class dropdown
        classDropdown.ClearOptions();
        classDropdown.AddOptions(new List<string>() { "All" });
        classDropdown.AddOptions(dataManager.GetClasses());

        //Populate scrollview with pokemon
        UpdateList();

        mainMenuCanvas.gameObject.SetActive(true);
    }
    public void UpdateList()
    {
        foreach (Transform child in scrollContent.transform)
            Destroy(child.gameObject);

        Pokemon[] p = dataManager.GetFilteredPokemon();

        searchEmptyText.gameObject.SetActive(false);

        if (p.Length == 0) searchEmptyText.gameObject.SetActive(true);
        else
        {
            foreach (Pokemon poke in p)
            {
                GameObject go = Instantiate(pokemonRowPrefab, scrollContent.transform);
                var row = go.GetComponent<PokemonRowController>();
                pokemon[dataManager.pokeIDFilter[poke.id]].row = row;
                row.UpdateData(poke, this);
            }
        }

    }

    //Test method, bypass login
    public void QuickLogin()
    {
        curUID = 2;
        authCanvas.gameObject.SetActive(false);
        LoadMainList();
    }

    internal void ViewPokemonDetails(int id)
    {
        Debug.Log("Viewing page for pokemon: " + id + "\n" + pokemon[dataManager.pokeIDMain[id]].ToString());
    }

    void Awake()
    {
        DB.InitiateConnection("valemon.db");
    }

    private void OnApplicationQuit()
    {
        DB.CloseConnection();
    }

    // Start is called before the first frame update
    void Start()
    {
        errorText.text = "";
        screenRes = Screen.resolutions[Screen.resolutions.Length - 1];
        Screen.SetResolution((screenRes.height / 16) * 9, screenRes.height, true);
    }

    //This is called by the DataManager script when the DB has been validated and loaded, or when the user refreshes the main menu
    

    public void MainMenu()
    {
        SceneManager.LoadScene("Main");
    }
}
internal class Pokemon
{
    internal int id;
    internal string name;
    internal Type? type1;
    internal Type? type2;
    internal string[] abilities;
    internal string classification;
    internal int generation;
    internal int defense;
    internal int attack;
    internal int sp_attack;
    internal int sp_defense;
    internal int hp;
    internal string height;
    internal string weight;
    internal int speed;

    internal PokemonRowController row;

    internal Texture texture;

    internal int StatTotal => defense + attack + sp_attack + sp_defense + hp + speed;
    internal float Cost => Mathf.Ceil((StatTotal / 16f) * 0.05f) / 0.05f;

    public override string ToString()
    {
        //String list of stats seperated onto newlines with '\n'
        return $"Name: {name}\n" +
            $"Type: {type1?.ToString() ?? "None"}{(type2.HasValue ? $"/{type2?.ToString()}" : "")}\n" +
            $"Abilities: {string.Join(", ", abilities)}\n" +
            $"Classification: {classification}\n" +
            $"Generation: {generation}\n" +
            $"Stats: {StatTotal}\n" +
            $"\tDefense: {defense}\n" +
            $"\tAttack: {attack}\n" +
            $"\tSp. Attack: {sp_attack}\n" +
            $"\tSp. Defense: {sp_defense}\n" +
            $"\tHP: {hp}\n" +
            $"\tSpeed: {speed}\n" +
            $"Height: {height}\n" +
            $"Weight: {weight}\n" +
            $"Cost: {Cost}\n";
    }
}

struct Type
{
    public string name;
    public Color color;

    public override string ToString() => name;
}
