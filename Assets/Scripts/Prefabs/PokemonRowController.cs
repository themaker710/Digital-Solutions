using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PokemonRowController : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text costText;
    public TMP_Text statText;
    public TMP_Text genText;
    public TMP_Text idText;

    public RawImage pokemonImage;

    public Button btn;

    public Image type1Img;
    public Image type2Img;

    public TMP_Text type1Text;
    public TMP_Text type2Text;

    internal bool hasTexture = false;

    Pokemon p;
    ValemonController controller;

    internal void UpdateData(Pokemon pokemon, ValemonController con)
    {
        p = pokemon;
        controller = con;

        nameText.text = p.name;
        costText.text = "$"+ p.Cost.ToString();
        statText.text = p.StatTotal.ToString();
        genText.text = p.generation.ToString();
        idText.text = p.id.ToString();

        hasTexture = true;
        if (p.texture != null)
            pokemonImage.texture = p.texture;
        else
            hasTexture = false;

        //Apply type colors and text
        type1Text.text = p.type1.Value.name;
        type1Img.color = p.type1.Value.color;
        //Debug.Log(p.type1.Value.color.ToString());

        type2Text.text = p.type2.HasValue ? p.type2.Value.name : "";
        type2Img.color = p.type2.HasValue ? p.type2.Value.color : Color.clear;

        btn.onClick.AddListener(() => controller.ViewPokemonDetails(p.id));
    }
}
