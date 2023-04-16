using UnityEngine;
using UnityEngine.UI;

public class QuickRestockManager : MonoBehaviour
{
    public Text productNameText;
    public Text productQuantityText;

    int productID;

    public void PopulateFields(string name, int quantity, int id)
    {
        productID = id + 1;
        productNameText.text = name;
        productQuantityText.text = quantity.ToString();
    }

    public void Restock()
    {
        DBManager.InitiateConnection("/vending.db");
        DBManager.QueryDB($"UPDATE Products SET CurrentQuantity = (SELECT RestockLimit FROM Products WHERE ID = {productID}) WHERE ID = {productID}");
        DBManager.QueryDB($"UPDATE Products SET LastRestock = '{System.DateTime.Now}'");
        DBManager.CloseConnection();
        Debug.Log($"Quick restock of product {productNameText.text} successful");
        Destroy(gameObject);
    }
}
