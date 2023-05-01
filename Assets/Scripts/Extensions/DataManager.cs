using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using DB = AsyncDBManager;

public class DataManager : MonoBehaviour
{
    string destPath;
    string dbPath;

    string[] sourcePaths, sourceFiles;

    string fileHash;

    public GameObject loadingScreen;
    public Image blur;
    public TMP_Text loadingText;
    public TMP_Text taskText;
    string currentTask;

    Type[] types;

    public ValemonController controller;

    int preferredSource = 0;

    internal bool loading = true;

    //PokeID, ArrayID
    internal Dictionary<int, int> pokeIDFilter = new Dictionary<int, int>();
    internal Dictionary<int, int> pokeIDMain = new Dictionary<int, int>();

    void Start()
    {
        sourcePaths = new string[] { "http://10.64.116.12/12dis/Valemon/", "https://ihelensvaleshs.com/valemon/" };
        //sourcePaths = new string[] { "https://ihelensvaleshs.com/valemon/" };
        destPath = Application.persistentDataPath;

        types = new Type[] {
            new Type() { name = "Water", color = new Color(99, 144, 240)},
            new Type() { name = "Ground", color = new Color(226, 191, 101) },
            new Type() { name = "Rock", color = new Color(182, 161, 54) },
            new Type() { name = "Dark", color = new Color(112, 87, 70) },
            new Type() { name = "Dragon", color = new Color(111, 53, 252) },
            new Type() { name = "Bug", color = new Color(166, 185, 26) },
            new Type() { name = "Electric", color = new Color(247, 208, 44) },
            new Type() { name = "Fire", color = new Color(238, 129, 48) },
            new Type() { name = "Fairy", color = new Color(214, 133, 173) },
            new Type() { name = "Fighting", color = new Color(194, 46, 40) },
            new Type() { name = "Flying", color = new Color(169, 143, 243) },
            new Type() { name = "Ghost", color = new Color(115, 87, 151) },
            new Type() { name = "Grass", color = new Color(122, 199, 76) },
            new Type() { name = "Ice", color = new Color(150, 217, 214) },
            new Type() { name = "Normal", color = new Color(168, 167, 122) },
            new Type() { name = "Poison", color = new Color(163, 62, 161) },
            new Type() { name = "Psychic", color = new Color(249, 85, 135) },
            new Type() { name = "Steel", color = new Color(183, 183, 206) }
        };


        StartCoroutine(LoadDataIndicator());

        fileHash = PlayerPrefs.GetString("FileHash", string.Empty);

        File.Delete(Path.Combine(destPath, "pokemon.csv.old"));

        //Rename current file to pokemon.old.csv for comparison 
        if (File.Exists(Path.Combine(destPath, "pokemon.csv")))
            File.Move(Path.Combine(destPath, "pokemon.csv"), Path.Combine(destPath, "pokemon.csv.old"));

        //Download and parse CSV file
        GetCSV("pokemon.csv");
    }
    //Filtering method very rudimentary, can be improved
    internal Pokemon[] GetFilteredPokemon()
    {
        //Search query
        string search = controller.searchInput.text;

        //Filter Variables
        string gen = controller.generationDropdown.options[controller.generationDropdown.value].text;
        string classification = controller.classDropdown.options[controller.classDropdown.value].text;
        string type = controller.typeDropdown.options[controller.typeDropdown.value].text;

        Pokemon[] p = controller.pokemon;

        List<Pokemon> filteredList = new List<Pokemon>();

        pokeIDFilter = new Dictionary<int, int>();

        //Fix the algorithm so this isnt neccessary
        if (gen == "All" && classification == "All" && type == "All" && search.IsNullOrWhiteSpace())
        {
            pokeIDFilter = pokeIDMain;
            return p;
        }



        //Get all pokemon from variable p that match the filter variables
        //Currently does not work in combination with each other. 
        foreach (Pokemon pokemon in p)
        {
            bool match = false;
            //Match generation
            if (gen != "All")
                if (pokemon.generation.ToString() == gen)
                    match = true;

            //Match Classification
            if (classification != "All")
                if (pokemon.classification == classification)
                    match = true;

            //Match Type
            if (type != "All")
                if (pokemon.type1.ToString() == type || pokemon.type2.ToString() == type)
                    match = true;

            if (match)
            {
                //Match search query (Broken)
                if (!search.IsNullOrWhiteSpace())
                {
                    bool searchmatch = false;
                    //Match name
                    if (pokemon.name.ToLower().Contains(search.ToLower())) searchmatch = true;
                    //Match ID
                    if (pokemon.id.ToString() == search) searchmatch = true;
                    //Match StatTotal if int
                    //if (int.TryParse(search, out int isearch))
                    //{
                    //    int total = pokemon.StatTotal;
                    //    if ((isearch - 30) <= total && total <= (isearch + 30))
                    //        searchmatch = true;
                    //}

                    if (searchmatch) filteredList.Add(pokemon);
                }
                else filteredList.Add(pokemon);
            }
        }

        Pokemon[] rtn = filteredList.OrderBy(p => p.id).ToArray();

        for (int i = 0; i < rtn.Length; i++)
            pokeIDFilter.Add(rtn[i].id, i);

        return rtn;
    }
    public IEnumerator LoadPokemon()
    {
        //Load all pokemon from the DB
        var reader = DB.QueryDB("SELECT COUNT(*) FROM pokemon");
        int count;
        if (reader.Read())
        {
            count = reader.SafeGet<int>(0);
        }
        else
        {
            Debug.LogError("DB not correctly loaded. Check your internet connection");
            yield break;
        }

        Pokemon[] pokemon = new Pokemon[count];
        reader = DB.QueryDB("SELECT ID, name, classfication, type1, type2, abilities1, abilities2, abilities3, abilities4, abilities5, abilities6, generation, defense, attack, sp_attack, sp_defense, hp, height_m, weight_kg, speed FROM pokemon");
        int i = 0;
        while (reader.Read())
        {
            pokemon[i] = new Pokemon
            {
                //Get values from reader using SafeGet<T>
                id = reader.SafeGet<int>(0),
                name = reader.SafeGet<string>(1),
                classification = reader.SafeGet<string>(2),
                //Select the type from a set of predetermined name & color combinations.
                type1 = types.SingleOrDefault((x) => x.name.ToLower() == reader.SafeGet<string>(3)),
                type2 = types.SingleOrDefault((x) => x.name.ToLower() == reader.SafeGet<string>(4)),
                abilities = new string[6] { reader.SafeGet<string>(5), reader.SafeGet<string>(6), reader.SafeGet<string>(7), reader.SafeGet<string>(8), reader.SafeGet<string>(9), reader.SafeGet<string>(10) },
                generation = int.Parse(reader.SafeGet<string>(11)),
                defense = int.Parse(reader.SafeGet<string>(12)),
                attack = int.Parse(reader.SafeGet<string>(13)),
                sp_attack = int.Parse(reader.SafeGet<string>(14)),
                sp_defense = int.Parse(reader.SafeGet<string>(15)),
                hp = int.Parse(reader.SafeGet<string>(16)),
                height = reader.SafeGet<string>(17),
                weight = reader.SafeGet<string>(18),
                speed = int.Parse(reader.SafeGet<string>(19))
            };

            //Get texture from web without blocking thread (i.e. project freezing while 648 images are loaded into memory)
            StartCoroutine(GetTexture(pokemon[i].id));
            Debug.Log("Pokemon " + pokemon[i].id + " data loaded into memory");
            pokeIDMain.Add(pokemon[i].id, i);
            i++;
        }
        reader.Close();

        pokemon = pokemon.OrderBy(x => x.id).ToArray();

        //Make sure the pokemon array is assigned as a new variable in memory (direct assignment caused reference/pointer issues)
        controller.pokemon = (Pokemon[])pokemon.Clone();

        Debug.Log("All pokemon loaded into memory");
    }

    public IEnumerator GetTexture(int id)
    {
        //Download texture 'id.jpg' from preferred source, then send it back through the callback
        string source = Path.Combine(sourcePaths[preferredSource], "sprites", id + ".png");

        using(UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(source))
        {
            //After a sustained number of new connections, a X509 cert error is thrown. This automatically accepts any certificate (read: insecure)
            //Another solution is to determine the signing key used in the sites SSL certificate and always accept ONLY that one.
            uwr.certificateHandler = new BypassCertificate();

            var op = uwr.SendWebRequest();

            //Non blocking wait method
            while (!op.isDone)
            {
                yield return null;
            }

            if(uwr.result == UnityWebRequest.Result.Success)
            {
                //Purpose made download handler optimised for images and loading textures
                var texture = DownloadHandlerTexture.GetContent(uwr);
                Debug.Log("Texture downloaded for pokemon " + id);
                controller.pokemon[pokeIDMain[id]].texture = texture;
                //Ensure that late download's update their texture on the main menu list if active
                if(controller.pokemon[pokeIDMain[id]].row != null)
                    if (!controller.pokemon[pokeIDMain[id]].row.hasTexture)
                        controller.pokemon[pokeIDMain[id]].row.pokemonImage.texture = texture;
            }
            else
            {
                Debug.LogError("Texture retrieval failed for:\n" + source);
                Debug.LogError(uwr.error);
            }
        }
    }
    internal void UpdatePurchased(int uid)
    {
        //Get all purchased pokemon for the current user from the "transactions" table
        var reader = DB.QueryDB("SELECT PokemonID FROM sales WHERE UserID = " + uid);
        List<int> purchased = new List<int>();
        while (reader.Read())
            purchased.Add(reader.SafeGet<int>(0));

        //Update all appropriate rows from the 'controller.pokemon' variable
        foreach (int i in purchased)
        {
            controller.pokemon[pokeIDMain[i]].owned = true;
            //If the pokemon is in the main list, update it
            if (controller.pokemon[pokeIDMain[i]].row != null)
                controller.pokemon[pokeIDMain[i]].row.UpdateData(controller.pokemon[pokeIDMain[i]], controller);
        }
    }

    IEnumerator LoadDataIndicator()
    {
        //Show loadingScreen while data is being loaded

        loadingScreen.SetActive(true);

        StartCoroutine(AnimateLoadText());

        while (loading)
        {
            taskText.text = currentTask;
            yield return null;
        }

        //Lower alpha to zero over 1 second
        while (blur.color.a > 0)
        {
            Color c = blur.color;
            c.a -= Time.deltaTime;
            c.a = Mathf.Clamp(c.a, 0, 1);
            blur.color = c;
            yield return null;
        }

        //Begin creation of pokemon array in memory. Occurs in the background while user is logging in.
        StartCoroutine(LoadPokemon());

        loadingScreen.SetActive(false);

        StopCoroutine(nameof(AnimateLoadText));
    }
    IEnumerator AnimateLoadText()
    {
        //Animate dots on loading text every 0.5 seconds to indicate progress
        string dots = "";

        while (loading)
        {
            dots += ".";
            if (dots.Length > 3)
                dots = "";
            loadingText.text = "Loading" + dots;

            yield return new WaitForSeconds(0.5f);
        }
    }

    void GetCSV(string fileName)
    {
        //Determine source and destination paths
        string destFile = Path.Combine(destPath, fileName);

        currentTask = "Determining retrieval locations";
        sourceFiles = new string[sourcePaths.Length];

        //add file onto end of each source path
        for (int i = 0; i < sourceFiles.Length; i++)
            sourceFiles[i] = Path.Combine(sourcePaths[i], fileName);

        Debug.Log(destFile);

        //Start threaded web request
        StartCoroutine(IGetRequest(sourceFiles, destFile));
    }

    //Method will try initial value in source list, and if failed will try all subsequent options in order
    IEnumerator IGetRequest(string[] sources, string destFile, int sid = 0)
    {
        Debug.Log("Attempting to connect to " + sources[sid]);
        currentTask = "Attempting connection with Source " + (sid + 1);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(sources[sid]))
        {
            // Request and wait for the desired page.
            webRequest.timeout = 5;
            var op = webRequest.SendWebRequest();
            Debug.Log("Start Download");

            //Wait for download to complete (or timeout) with non-UI blocking loop
            while (!op.isDone)
                yield return null;

            //Check for correct response
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                preferredSource = sid;
                Debug.Log("Download Succeeded");
                currentTask = "Downloaded file. Copying from buffer";
                //Save binary data to file
                FileStream stream = new FileStream(destFile, FileMode.Create);

                stream.Write(webRequest.downloadHandler.data, 0, webRequest.downloadHandler.data.Length);

                stream.Close();

                currentTask = "File saved";

                //Reaqcuire filename
                char[] separators = { '\\', '/' };
                string[] t = destFile.Split(separators);

                //Parse file into DB
                ParseCSV(t[t.Length - 1]);
            }
            else
            {
                //Error logging
                Debug.Log("Connection to server '" + sources[sid] + "' failed with error: ");
                currentTask = "Connection to Source " + (sid + 1) + " failed";
                Debug.LogError(webRequest.error);
                
                //If there are more sources, try them
                if (sid < sources.Length)
                    StartCoroutine(IGetRequest(sourceFiles, destFile, sid + 1));
                else
                    Debug.LogError("No more connections to try. Check the connection strings");
            }
        }
    }
    void ParseCSV(string fileName)
    {
        Debug.Log("File downloaded, parsing " + fileName);

        currentTask = "Calculating hash for " + fileName;
        string path = Path.Combine(destPath, fileName);

        string csvFile = Path.Combine(destPath, fileName);
        string oldFile = Path.Combine(destPath, fileName + ".old");

        //Minimum performance impact
        string newHash = Extensions.HashFile(path);

        //Debug.Log(newHash + "\n" + fileHash);

        //For testing and algorithm confirmation purposes
        bool isForced = true;

        if ((fileHash != newHash) || isForced)
        {
            //Read all lines
            string[] records = File.ReadAllLines(csvFile);

            //Get filename without extension (range operator)
            string tableName = fileName[..^4].ToLower();

            //Get all headers
            string[] headers = records[0].Split(',');

            //Create table with headers
            CreateTable(tableName, headers);
            currentTask = "Parsing file";

            //Construct insert query for each row of values.
            string sql = "INSERT INTO " + tableName + " (ID, ";

            //Construct insert beginning
            for (int i = 1; i < headers.Length; i++)
            {
                sql += headers[i];

                if (i < headers.Length - 1)
                    sql += ", ";
            }
            sql += ") VALUES (";


            //Append data for each line
            for (int i = 1; i < records.Length; i++)
            {
                //Get values
                string insertQuery = sql;
                string[] values = records[i].Split(',');

                //Add the ID as an int
                insertQuery += values[0] + ", ";

                //Add the rest as a string
                for (int j = 1; j < values.Length; j++)
                {
                    //Check all Name field characters for apostrophes and escape them
                    if (j == 1)
                    {
                        int len = values[j].Length;
                        for (int k = 0; k < len; k++)
                        {
                            if (values[j][k] == '\'')
                            {
                                values[j] = values[j].Insert(k, "'");
                                //Skip the added aphostrophe
                                k++;
                            }
                        }
                    }

                    insertQuery += "'" + values[j] + "', ";
                }
                
                //Remove last two characters (range operator)
                insertQuery = insertQuery[..^2];
                //Same as insertQuery.Remove(insertQuery.Length - 2);

                insertQuery += ");";

                DB.QueryDB(insertQuery);
            }

            PlayerPrefs.SetString("FileHash", newHash);
        }

        //Delete old file
        File.Delete(oldFile);
        currentTask = "Done";
        //Close the loading screen
        loading = false;
    }

    void CreateTable(string tableName, string[] headers)
    {
        currentTask = "Creating table from data structure";
        //Drop any existing table of the same name
        string sql = "DROP TABLE IF EXISTS " + tableName;

        DB.QueryDB(sql);

        //Create table with info
        sql = "CREATE TABLE " + tableName +
            " (ID INTEGER PRIMARY KEY AUTOINCREMENT, ";

        for (int i = 1; i < headers.Length; i++)
            sql += headers[i] + " TEXT, ";

        //Remove ', ' after last entry
        sql = sql.Substring(0, sql.Length - 2);
        sql += ");";

        //Execute query
        DB.QueryDB(sql);

        Debug.Log("Table " + tableName + " created");

    }

    internal List<string> GetTypes()
    {
        //SQL statement to get distinct types from fields 'type1' and 'type2' in the pokemon table, excluding null values and in alphabetical order
        string sql = "SELECT DISTINCT type1 FROM pokemon WHERE type1 IS NOT NULL UNION SELECT DISTINCT type2 FROM pokemon WHERE type2 IS NOT NULL ORDER BY type1";

        //Execute the query
        var reader = DB.QueryDB(sql);

        List<string> types = new List<string>();

        //Get all types from reader
        while (reader.Read())
            types.Add(reader.GetString(0));

        
        //Return the result
        return types.OrderBy(x => x).ToList();
    }

    internal List<string> GetGenerations()
    {
        //SQL statement to get distinct generations from field 'generation' in the pokemon table, excluding null values and in ascending order
        string sql = "SELECT DISTINCT generation FROM pokemon WHERE generation IS NOT NULL ORDER BY generation;";

        //Execute the query
        var reader = DB.QueryDB(sql);

        List<string> generations = new List<string>();

        //Get all genns from reader
        while (reader.Read())
            generations.Add(reader.GetString(0));

        //Return sorted list
        return generations.OrderBy(x => x).ToList();
    }

    internal List<string> GetClasses()
    {
        //SQL statement to get distinct classes from field 'classfication' in the pokemon table, excluding null values and in ascending order
        string sql = "SELECT DISTINCT classfication FROM pokemon WHERE classfication IS NOT NULL ORDER BY generation;";

        //Execute the query
        var reader = DB.QueryDB(sql);

        List<string> classes = new List<string>();

        //Get all classes from reader
        while (reader.Read())
            classes.Add(reader.GetString(0));

        //Return sorted list
        return classes.OrderBy(x => x).ToList();
    }
}
