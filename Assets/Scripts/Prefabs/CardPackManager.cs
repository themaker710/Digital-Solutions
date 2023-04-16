using UnityEngine.UI;
using UnityEngine;

public class CardPackManager : MonoBehaviour
{
    public Image front;
    public Image back;

    public Button btn;
    public Text buttonText;

    private int _cardPack;
    private int _cost;

    bool bought = false;

    BlackjackController bj;

    public Button PopulateOption(int packNum, int cost = 750)
    {
        front.sprite = Resources.Load<Sprite>("Images/Blackjack/Cards/" + packNum + "/As");
        back.sprite = Resources.Load<Sprite>("Images/Blackjack/Cards/" + packNum + "/top1");
        bj = GameObject.FindGameObjectWithTag("GameController").GetComponent<BlackjackController>();

        _cardPack = packNum;
        _cost = cost;

        bought = PlayerPrefs.GetInt("pack" + _cardPack) == 1;

        if (bought && bj.cardPack == _cardPack)
            buttonText.text = "Equipped";
        else if (bought)
            buttonText.text = "Equip";
        else
            buttonText.text = "$" + cost;

        return btn;
    }

    public void OnClickButton()
    {
        if (bought)
        {
           bj.cardPack = _cardPack;
        }
        else if (!(bj.cash < _cost))
        {
            bj.cash -= _cost;
            PlayerPrefs.SetInt("pack" + _cardPack, 1);
            bought = true;
        }
        //Refresh text - reload whole page. Could be optimized
        bj.OpenShop();
    }
}
