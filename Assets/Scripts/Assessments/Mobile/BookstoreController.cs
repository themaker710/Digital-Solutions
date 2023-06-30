using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections.Concurrent;

public class BookstoreController : MonoBehaviour
{
    public GameObject bookCellPrefab;
    public Transform bookCellContainer;
    public TMP_InputField searchInputField;
    public TMP_InputField userPageInput;

    public TextMeshProUGUI pageText;
    public TextMeshProUGUI searchStatusText;

    public BookSearchContext searchContext;

    bool loading = false;

    public int currSearchID = 0;

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
        userPageInput.text = "1";
        currSearchID++;
        BookSearchContext context = new BookSearchContext
        {
            query = query,
            searchID = currSearchID
        };
        StartCoroutine(LoadBooks(context));
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
    }

    IEnumerator LoadBooks(BookSearchContext context)
    {
        //Clear book cells from container
        foreach (Transform child in bookCellContainer)
        {
            Destroy(child.gameObject);
        }

        //Create new UnityWebRequest with URL
        using (UnityWebRequest uwr = UnityWebRequest.Get(context.url))
        {
            //Send web request and wait for response
            loading = true;
            StartCoroutine(LoadingAnimation());
            yield return uwr.SendWebRequest();
            loading = false;
            StopCoroutine(LoadingAnimation());
            searchStatusText.text = "";

            //Check if web request was successful
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                //Create new BookSearchResult object from JSON response using JsonUtility
                BookSearchResult searchResult = JsonUtility.FromJson<BookSearchResult>(uwr.downloadHandler.text);
                //Merge search context with search result
                searchResult.query = context.query;
                searchResult.searchID = context.searchID;                

                //Cache search context
                searchContext = searchResult;

                if (searchResult.error != "0")
                {
                    //Log error if present
                    Debug.Log("An API error occured when retrieving from:\n" + context.url);
                    Debug.LogError(searchResult.error);
                    //Tell user there was an error
                    searchStatusText.text = "An error occurred. Try searching something else";
                }
                else if (searchResult.total == "0")
                {
                    Debug.Log("No books found for query: " + context.query);
                    //Tell user no books were found
                    searchStatusText.text = "No books found for query. Try searching something else";
                }

                if (searchResult.total == "0" || searchResult.error != "0")
                {
                    //Clear page text
                    pageText.text = "/ 0";
                    //Break out of coroutine
                    yield break;
                }
                //Check if search ID has changed since web request was sent
                if (context.searchID != currSearchID)
                {
                    //If search ID has changed, stop loading books from this request
                    yield break;
                }

                //Calculate page total
                searchResult.pageTotal = int.Parse(searchResult.total) / 20;
                //Recache search context
                searchContext = searchResult;

                //Log total books found
                Debug.Log("Found " + searchResult.total + " books for query: '" + searchResult.query + "'");

                //Set page text to total books divided by 20. Parse will succeed as input is always a number
                pageText.text = "/ " + searchContext.pageTotal;
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

                Debug.Log("Books loaded for query '" + context.query + "' on page " + context.page + "\n" + context.url);
            }
            else
            {
                //Log error if present
                Debug.Log("An error occurred while retrieving data from:\n" + context.url);
                Debug.LogError(uwr.error);
            }
        }
    }


    void ChangePage(int page)
    {
        //Change input field text to current page
        userPageInput.text = page.ToString();

        //Load books for page in current context
        currSearchID++;
        searchContext.searchID = currSearchID;
        searchContext.page = page.ToString();
        StartCoroutine(LoadBooks(searchContext));
    }

    public void SetPage(string page)
    {
        int pageInt = int.Parse(page);

        //Check if page is valid
        if (pageInt < 1 || pageInt > searchContext.pageTotal)
        {
            //If page is invalid, reset page input field text to current page
            userPageInput.text = searchContext.page;
            return;
        }

        //Change page
        ChangePage(pageInt);
    }

    public void NextPage()
    {
        //Get current page from search context
        int page = int.Parse(searchContext.page);
        //Increment page if possible
        if (page < searchContext.pageTotal) page++;
        else return;
        //Change page
        ChangePage(page);
    }

    public void PrevPage()
    {
        //Get current page from search context
        int page = int.Parse(searchContext.page);
        //Decrement page if possible
        if (page > 1) page--;
        else return;
        //Change page
        ChangePage(page);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Main");
    }
}

[System.Serializable]

public class BookSearchContext
{
    internal string endpoint;
    public string total;
    public string query;
    public int searchID;
    public int pageTotal;
    public string page;
    public string url => endpoint + query + "/" + page;

    //Constructor to set default values
    public BookSearchContext() { page = "1"; endpoint = "https://api.itbook.store/1.0/search/"; }
}
public class BookSearchResult : BookSearchContext
{
    public string error;

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
