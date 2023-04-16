using static Extensions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultsManager : MonoBehaviour
{
    // Unity linking variables
    public Text percentageButtonText;
    public Dropdown newDropdown;

    //Filter dropwdowns
    public Dropdown F_subjectDropdown;
    public Dropdown F_unitDropdown;
    public Dropdown F_yearDropdown;
    public Dropdown F_percentageDropdown;

    bool percentageLess = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchPercentage()
    {
        percentageLess = (percentageLess) ? false : true;
        percentageButtonText.text = (percentageLess) ? ">" : "<";
    }


    private void ManagedQuery(string query)
    {
        DBManager.InitiateConnection("/results.db");

        DBManager.QueryDB(query);

        DBManager.CloseConnection();

        Debug.Log($"Query {query} was executed");
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("Main");
    }
}
