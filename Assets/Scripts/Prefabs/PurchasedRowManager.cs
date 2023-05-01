using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchasedRowManager : MonoBehaviour
{
    public TMP_Text nameText;
    public RawImage img;

    internal void PopulateData(string name, Texture texture)
    {
        nameText.text = name;
        img.texture = texture;
    }
}
