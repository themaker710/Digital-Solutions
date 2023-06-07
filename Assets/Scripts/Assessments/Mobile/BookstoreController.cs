using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Security.Policy;
using UnityEngine.Networking;
using System.Runtime.CompilerServices;

public class BookstoreController : MonoBehaviour
{
    public GameObject bookCellPrefab;
    public Transform bookCellContainer;
    public TMP_InputField searchInputField;
    public TMP_InputField userPageInput;

    public TextMeshProUGUI pageText;
    public TextMeshProUGUI searchStatusText;

    bool loading = false;

    // Start is called before the first frame update
    void Start()
    {
        //Set search input field text to empty string
        searchInputField.text = "";
        //Set user page input field text to 1
        userPageInput.text = "1";
        //Set page text to default state
        pageText.text = "/1";
        //Set search status text to empty string
        searchStatusText.text = "";
        
    }

    public void SearchBooks(string query)
    {
        if (query.IsNullOrWhiteSpace()) return;
        string url = "https://api.itbook.store/1.0/search/" + query;
        StartCoroutine(LoadBooks(url));
    }

    IEnumerator LoadingAnimation()
    {
        //Animate dots on loading text every 0.5 seconds to indicate progress
        string dots = "";

        while (loading)
        {
            dots += ".";
            if (dots.Length > 3)
                dots = "";
            searchStatusText.text = "Loading books" + dots;

            yield return new WaitForSeconds(0.5f);
        }

        searchStatusText.text = "";
    }

    IEnumerator LoadBooks(string url)
    {
        //Clear book cells from container
        foreach (Transform child in bookCellContainer)
        {
            Destroy(child.gameObject);
        }

        //Create new UnityWebRequest with URL
        using (UnityWebRequest uwr = UnityWebRequest.Get(url))
        {
            //Send web request and wait for response
            loading = true;
            StartCoroutine(LoadingAnimation());
            yield return uwr.SendWebRequest();
            loading = false;

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                //Create new BookSearchResult object from JSON response using JsonUtility
                BookSearchResult searchResult = JsonUtility.FromJson<BookSearchResult>(uwr.downloadHandler.text);

                if (searchResult.error != "0")
                {
                    //Log error if present
                    Debug.Log("An API error occured when retrieving from:\n" + url);
                    Debug.LogError(searchResult.error);
                    yield break;
                }   
                if (searchResult.total == "0")
                {
                    Debug.Log("No books found for query:\n" + url);
                    //Tell user no books were found
                    searchStatusText.text = "No books found for query. Try searching something else";
                    yield break;
                }

                //Log total books found
                Debug.Log("Total books found: " + searchResult.total);

                //Set page text to total books divided by 10. Parse will succeed as input is always a number
                pageText.text = "/" + (int.Parse(searchResult.total) / 10);
                //Set page input field text to current page
                userPageInput.text = searchResult.page;

                //Loop through search results
                foreach (Book book in searchResult.books)
                {
                    //Instantiate book cell prefab
                    GameObject bookCellObject = Instantiate(bookCellPrefab, bookCellContainer);
                    //Get book cell manager component from book cell object
                    BookCellManager bookCellManager = bookCellObject.GetComponent<BookCellManager>();
                    //Set book data to book cell manager
                    bookCellManager.SetBook(book);
                }
            }
            else
            {
                //Log error if present
                Debug.Log("An error occurred while retrieving data from:\n" + url);
                Debug.LogError(uwr.error);
            }
        }
    }

}

[System.Serializable]
public class BookSearchResult
{
    public string error;
    public string total;
    public string page;

    public Book[] books;

}
[System.Serializable]
public class Book
{
    public string title;
    public string subtitle;
    public string isbn;
    public string price;
    public string url;
    public string image;
}
