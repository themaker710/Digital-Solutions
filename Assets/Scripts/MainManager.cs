using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    public RectTransform ParentPanel;
    public Text version, pageText;
    public InputField sceneTestCount;
    public Button toggleButton;
    public Dropdown catDropdown;
    public GameObject ConfirmationModal;
    GameObject buttonPrefab;
    GameObject[] pageChangers;

    /// <summary>
    /// All scenes, seperated into arrays based on category. [0] is all scenes.
    /// </summary>
    Category[] scenes = { };

    string[] hiddenNames = { }, scenePaths = { };

    string[] dbNames = { "words.db", "vending.db", "productivity.db", "valemon.db"};
    int page = 0, maxpages = 0, sceneCount, cat = 0;
    bool testingPages = false;

    void Start()
    {

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        CheckDB();
        Debug.Log("Production");
#endif
#if UNITY_EDITOR
        Debug.Log("Editor");
        
#endif
        version.text = "Version: " + Application.version;
        buttonPrefab = Resources.Load<GameObject>("Prefabs/SceneMenuButton");
        pageChangers = GameObject.FindGameObjectsWithTag("pageControl");

        //Get largest natively supported resolution
        Resolution screenRes = Screen.resolutions[Screen.resolutions.Length-1];
        //Reset resolution (in the case of mobile format scenes)
        Screen.SetResolution(screenRes.width, screenRes.height, true);

        PopulateData();
    }
    void PopulateData() 
    {
        //page = 0;
        //maxpages = 0;
        
        List<string> hiddenTemp = new List<string>();
        List<string> scenePathTemp = new List<string>();
        sceneCount = SceneManager.sceneCountInBuildSettings;

        for (int i = 0; i < sceneCount; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = Path.GetFileNameWithoutExtension(path);

            if (path.Contains("Hidden"))
            {
                hiddenTemp.Add(name);
                //Debug.Log("Hidden: \n" + name);
            }
            else if (!path.Contains("Main"))
            {
                scenePathTemp.Add(path);
                //Debug.Log($"Valid scene: {name}");
            }

        }
        hiddenTemp.Sort();
        scenePathTemp.Sort();

        hiddenNames = hiddenTemp.ToArray();
        scenePaths = scenePathTemp.ToArray();

        scenes = new Category[catDropdown.options.Count];

        for (int i = 0; i < scenes.Length; i++)
        {
            Dropdown.OptionData cat = catDropdown.options[i];

            Debug.Log($"Category {cat.text} discovered");

            scenes[i].name = cat.text;
        }

        Debug.Log($"Hidden Scenes: {hiddenNames.Length}");

        List<string>[] scenesTemp = new List<string>[scenes.Length];

        for (int i = 0; i < scenesTemp.Length; i++)
            scenesTemp[i] = new List<string>();


        foreach (string path in scenePaths)
        {
            string name = Path.GetFileNameWithoutExtension(path);

            for (int i = 0; i < scenes.Length; i++)
            {
                if (path.Contains(scenes[i].name) && !path.Contains("Hidden"))
                {
                    Debug.Log($"{scenes[i].name}: " + name);
                    scenesTemp[i].Add(name);
                }


            }
            if (!path.Contains("Hidden") && !path.Contains("Main"))
                scenesTemp[0].Add(name);
        }

        int c = 0;
        foreach (List<string> list in scenesTemp)
        {
            list.Sort();
            scenes[c].sceneNames = list.ToArray();
            Debug.Log($"{scenes[c].name} Scenes: {list.Count}");
            c++;
        }

        Initialize();
    }

    void Initialize()
    {

        if (testingPages)
        {
            sceneCount = (sceneTestCount.text == "") ? 34 : int.Parse(sceneTestCount.text) + 1;
            scenes[0].sceneNames = new string[sceneCount];
            scenes[0].name = "Testing";

            Debug.Log("Testing pages");
            catDropdown.enabled = false;
            for (int i = 0; i < sceneCount; i++)
            {
                scenes[0].sceneNames[i] = i.ToString();
            }
        }
        else
        {
            catDropdown.enabled = true;
            //sceneCount = (cat == 1) ? assessmentNames.Length : (cat == 2) ? projectNames.Length : sceneNames.Length;
            //sceneCount = cat switch
            //{
            //    0 => sceneNames.Length,
            //    1 => assessmentNames.Length,
            //    2 => projectNames.Length,
            //    3 => mobileNames.Length,
            //    _ => sceneNames.Length
            //};
            sceneCount = scenes[cat].sceneNames.Length;

        }

        Debug.Log(sceneCount + " scenes found");

        NavigationButtons();

        ConstructPage(page);
    }
    void NavigationButtons()
    {

        maxpages = (int)Mathf.Ceil(sceneCount / 12);

        bool pages = (maxpages > 0);

        //deal with buttons
        foreach (GameObject go in pageChangers)
        {
            if (pages)
            {
                go.SetActive(true);
            } 
            else
            {
                go.SetActive(false);
            }
        }

        //Plus 1 to be 1 based instead of 0 based - More user friendly
        pageText.text = (page + 1) + "/" + (maxpages + 1);
    }
    void ConstructPage(int pagenum)
    {
        // delete the existing buttons if any
        foreach (Transform child in ParentPanel.transform)
        {
            Destroy(child.gameObject);
        }


        int xpos = -625, i = (pagenum * 12);
        //Number of buttons on the page: 12 or total - already shown buttons.
        int limit = (pagenum == maxpages) ? sceneCount - (pagenum * 12) : 12;
        Debug.Log($"Page Limit: {limit}");

        for (int position = 0; position < limit; position++)
        {
            //string curScene = (cat == 1) ? assessmentNames[i] : (cat == 2) ? projectNames[i] : sceneNames[i];

            string curScene = scenes[cat].sceneNames[i];

            //Skipping Conditions
            //if (curScene == "Main") i++;
            //if (hiddenNames.Contains(curScene)) i++;
            //if (scenes[cat].sceneNames.Contains(curScene)) i++;

            //Debug.Log("Position: " + position + "\nScene Index: " + i);

            //Instantiate button
            GameObject goButton = Instantiate(buttonPrefab);
            //Organise instatiated buttons to be easily deleted
            goButton.transform.SetParent(ParentPanel, false);
            //Fix default scale
            goButton.transform.localScale = new Vector3(1, 1, 1);
            //Set x position and row
            goButton.transform.localPosition = new Vector3(xpos, position <= 5 ? 150 : -150, 0);
            //Get object as a button object instead of GO for button specific tasks.

            string curName = "";

            //Add user readability i.e. spaces between words
            foreach (char c in curScene)
            {
                if (char.IsUpper(c))
                {
                    curName += ' ';
                    curName += c;
                }
                else
                {
                    curName += c;
                }
            }
            curName.Trim();

            //Dynamic Components - Button text and OnClick event
            Button tempButton = goButton.GetComponent<Button>();
            tempButton.GetComponentInChildren<Text>().text = curName;
            tempButton.onClick.AddListener(() => ButtonClicked(curScene));

            //New row if top row is full
            xpos = position == 5 ? -625 : xpos += 250;

            i++;
        }
        Debug.Log($"Page {pagenum + 1} Buttons Loaded");
    }

    //Following three methods are wrapper functions due to a ridiculous restriction by the Unity inspector UI system - no support for more than one method parameter.
    public void ConfirmButtonPressed()
    {
        ForceDBUpdate(true, true);
    }
    public void CancelButtonPressed()
    {
        ForceDBUpdate(false, true);
    }
    public void StartModalPressed()
    {
        ForceDBUpdate(false, false);
    }

    void ForceDBUpdate(bool confirmed = false, bool closeWindow = false)
    {
        //Launch confirmation modal
        if (!confirmed && !closeWindow)
        {
            ConfirmationModal.gameObject.SetActive(true);
        } else if (!confirmed && closeWindow)
        {
            ConfirmationModal.gameObject.SetActive(false);
        } else if (confirmed && closeWindow)
        {
            ConfirmationModal.gameObject.SetActive(false);
            CheckDB(confirmed);
        }
                    
    }
    void CheckDB(bool isForced = false)
    {
        for (int i = 0; i < dbNames.Length; i++)
        {
            int fileCount = Directory.EnumerateFiles(Application.streamingAssetsPath, "*.db", SearchOption.TopDirectoryOnly).Count();

            if(fileCount != dbNames.Length)
            {
                Debug.LogError("Check that the list of databases in MainManager.cs matches the StreamingAssets folder.");
                //throw new FileLoadException();
            }


            string clientPath = Path.Combine(Application.persistentDataPath, dbNames[i]);
            if (!File.Exists(clientPath) || isForced)
            {
                byte[] data;

                try
                {
                    data = File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, dbNames[i]));
                }
                catch (FileNotFoundException)
                {
                    Debug.LogError($"Please add the {dbNames[i]} file to StreamingAssets before copying operations can take place");
                    throw new FileNotFoundException();
                }

                if (isForced && File.Exists(clientPath))
                {
                    File.Delete(clientPath);
                }

                File.WriteAllBytes(clientPath, data);
                Debug.Log($"DB {dbNames[i]} copied");
                
            }
        }
    }

    void ButtonClicked(string scene)
    {
        Debug.Log("Scene " + scene + " loading...");

        if (!testingPages) SceneManager.LoadScene(scene);
        else Debug.Log(scene + " Testing");


        Debug.Log("Loaded!");

    }
    public void ChangePage(bool up)
    {
        //Next page is clicked and not at the last page turn the page
        if (up && !(page == maxpages)) page++;
        //Back page click and not at first page then turn page
        else if (!up && !(page == 0)) page--;
        else return;
        pageText.text = (page + 1) + "/" + (maxpages + 1);
        ConstructPage(page);
    }
    public void ToggleTesting()
    {
        testingPages = (testingPages) ? false : true;
        toggleButton.GetComponentInChildren<Text>().text = "Test Page System: " + testingPages.ToString().ToUpper();
        string type = (testingPages) ? " DEBUG" : " PROD";
        version.text = "Version: " + Application.version + type;

        catDropdown.value = 0;
        cat = catDropdown.value;
        page = 0;

        //foreach (Transform child in ParentPanel.transform)
        //{
        //    Destroy(child.gameObject);
        //}

        if (testingPages) Initialize();
        else PopulateData();
    }
    public void ChangeCat()
    {
        //0 = All, 1 = Assessments, 2 = Projects
        cat = catDropdown.value;
        page = 0;
        Debug.Log($"Category: {cat}");

        Initialize();
    }
    public void StopApplication()
    {
        Application.Quit();
    }
}
internal struct Category {
    internal string name;
    internal string[] sceneNames;
}
