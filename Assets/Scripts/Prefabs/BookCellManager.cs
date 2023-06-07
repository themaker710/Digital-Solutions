using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;

public class BookCellManager : MonoBehaviour
{
    public TextMeshProUGUI bookTitleText;
    public TextMeshProUGUI bookSubtitleText;
    public TextMeshProUGUI bookPriceText;
    public Image bookCover;
    public Button bookButton;

    internal void SetBook(Book book)
    {
        //Set book data to UI elements
        bookTitleText.text = book.title;
        bookSubtitleText.text = book.subtitle;
        bookPriceText.text = book.price;

        //Set button on click listener to open book details in external browser
        bookButton.onClick.AddListener(() => Application.OpenURL(book.url));

        //Download book cover image from URL
        StartCoroutine(LoadBookCover(book.image));
    }

    IEnumerator LoadBookCover(string url)
    {
        Debug.Log("Loading book cover from:\n" + url);

        //Download image from URL with UnityWebRequestTexture download handler
        using(UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                //Get downloaded texture from web request using helper method
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                //Set downloaded texture to sprite with correct size and pivot
                bookCover.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width / 2, texture.height / 2));

            }
            else
            {
                //Log error if present
                Debug.Log("An error occurred while downloading the image from:\n" + url);
                Debug.LogError(uwr.error);
            }
        }
    }
}
