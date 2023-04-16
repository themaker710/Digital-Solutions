using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using DateTime = System.DateTime;

public class BlackjackController : MonoBehaviour
{
    //Unity linking variables
    public GameObject optionsPanel;
    public GameObject endBannerPanel;
    public GameObject helpPanel;
    public GameObject pregameInput;
    public GameObject gameElements;
    public GameObject gameInput;
    public GameObject cardSelectionPanel;
    public GameObject cardOptionPrefab;
    public GameObject cardPrefab;
    public GameObject drawPile;
    public GameObject[] rebetElements;
    public Text volumeText;
    public Text betText;
    public Text playerWinPercentageText;
    public Text[] cashTexts;
    public Text dealerText;
    public Text playerText;

    public Text winLossText;
    public Text betInfoText;



    //Canvases
    public Canvas menuCanvas;
    public Canvas playCanvas;
    public Canvas shopCanvas;

    //Options interactables
    public Slider volSlider;
    public Toggle bettingToggle;
    public Toggle handValueToggle;

    //Game elements
    public Image[] chipImages;

    //Audio components
    public AudioSource cardAudio;
    public AudioMixer audioMixer;
    public AudioClip cardAudioClip;


    //General variables
    float vol = 1;

    bool bettingEnabled = true, showHandValue = true;

    //Card values
    string[] cardSuits = { "c", "d", "h", "s" };
    string[] cardValues = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };


    public List<Card> cardsInOrder;
    public List<Card> cardsInPlay;

    public Hand playerHand;
    public Hand dealerHand;

    internal int cash
    {
        get { return _cash; }
        set
        {
            for (int i = 0; i < cashTexts.Length; i++)
            {
                cashTexts[i].text = "$" + value.ToString();
                //Debug.Log("Cash text set to $" + value);
            }
            _cash = value;
            PlayerPrefs.SetInt("cash", _cash);
        }
    }
    //Expression body definition (=>). Saves writing a whole method.
    private string RateText => "Your Win Rate:\n" + (_wonTimes / (_wonTimes + _lossTimes) * 100).ToString("F2") + "%";

    internal float wonTimes
    {
        get { return _wonTimes; }
        set 
        {
            _wonTimes = value;
            if (!(_wonTimes == 0 || _lossTimes == 0))
                playerWinPercentageText.text = RateText;
            PlayerPrefs.SetFloat("won", _wonTimes);
            Debug.Log(RateText);
            Debug.Log($"Total Win times: {wonTimes} | Total lost times: {lossTimes}");
        }
    }
    internal float lossTimes
    {
        get { return _lossTimes; }
        set
        {
            _lossTimes = value;
            if (!(_wonTimes == 0 || _lossTimes == 0))
                playerWinPercentageText.text = RateText;
            PlayerPrefs.SetFloat("loss", _lossTimes);
            Debug.Log(RateText);
            Debug.Log($"Total Win times: {wonTimes} | Total lost times: {lossTimes}");
        }
    }

    private int _cash = 1000;
        
    private float _wonTimes = 0, _lossTimes = 0;

    Vector3 playerHandPos = new Vector3(-145, -210, 0);
    Vector3 dealerHandPos = new Vector3(125, 110, 0);
    Quaternion playerHandRot = new Quaternion(0, 0, -180, 0);
    Quaternion dealerHandRot = new Quaternion(0, 0, 180, 0);

    bool moving = false, userTurn = true;

    //<1000 - 5, 10, 20, 50, 100, 250
    //>1000 - 25, 50, 100, 250, 500, 1000
    //>2000 - 50, 100, 250, 500, 1000, 2000
    //>5000 - 100, 250, 500, 1000, 2000, 5000
    int[][] bettingOptions = { new int[] { 5, 10, 20, 50, 100, 250 }, new int[] { 25, 50, 100, 250, 500, 1000 }, new int[] { 50, 100, 250, 500, 1000, 2000 }, new int[] { 100, 250, 500, 1000, 2000, 5000 } };
    internal int howRich = 0, cardPack = 0, bet = 0;

    // Start is called before the first frame update
    void Start()
    {
        //Set option values from storage if present

        bettingEnabled = PlayerPrefs.GetInt("bettingEnabled") == 1;
        bettingToggle.isOn = bettingEnabled;

        showHandValue = PlayerPrefs.GetInt("showHandValue") == 1;
        handValueToggle.isOn = showHandValue;

        vol = PlayerPrefs.GetFloat("volume");
        volSlider.value = vol;
        ChangeVolume(vol);

        cardPack = PlayerPrefs.GetInt("selectedPack");

        _lossTimes = PlayerPrefs.GetInt("loss");
        wonTimes = PlayerPrefs.GetInt("won");

        //Ensure default pack is selected
        PlayerPrefs.SetInt("pack0", 1);



        // Set cash amount
        if (PlayerPrefs.HasKey("cash"))
            _cash = PlayerPrefs.GetInt("cash");

        //If first time opening
        if (!PlayerPrefs.HasKey("openedPrev"))
        {
            // Show help menu
            helpPanel.SetActive(true);
        }

        DateTime latestLogon;
        DateTime today = DateTime.Now.Date;

        //Daily Logon reward
        if (PlayerPrefs.HasKey("logonDate"))
            latestLogon = DateTime.Parse(PlayerPrefs.GetString("logonDate"));
        else
            latestLogon = today;



        if (DateTime.Compare(latestLogon, today) < 0)
        {
            //Reward
            Debug.Log("Daily Reward");
            cash += 500;

        }
        else if (DateTime.Compare(latestLogon, today) == 0)
        {
            //Last opened today, welcome back
        }
        else if (DateTime.Compare(latestLogon, today) > 0)
        {
            //Impossible, user changed device clock.
            Debug.Log("DON'T MESS WITH THE CLOCK");
            //Punishment for trying to cheat
            //cash = 0;
        }

        //Set logon date
        PlayerPrefs.SetString("logonDate", DateTime.Now.ToString());

        //Game has now been opened before
        PlayerPrefs.SetInt("openedPrev", 1);

        //Initialize card decks
        cardsInOrder = new List<Card>();

        cardsInPlay = new List<Card>();
            
        //For each suit
        foreach (string suit in cardSuits)
        {
            //And each card value
            foreach (string denomination in cardValues)
            {
                //Make card and assign variables
                Card c = new Card
                {
                    suit = suit,
                    denomination = denomination
                };

                //Add card to deck
                cardsInOrder.Add(c);
            }
        }
    }

    public void InitializeGame()
    {
        //Start game
        Debug.Log("play button pressed");
        playCanvas.gameObject.SetActive(true);
        menuCanvas.gameObject.SetActive(false);

        //Open appropriate controls
        endBannerPanel.SetActive(false);
        pregameInput.SetActive(true);
        gameInput.SetActive(false);
        gameElements.SetActive(false);

        //Change draw pile picture
        drawPile.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Blackjack/Cards/" + cardPack + "/top1");

        //Remove existing cards
        foreach (Card c in playerHand.cards)
        {
            Destroy(c.go);
        }
        foreach (Card c in dealerHand.cards)
        {
            Destroy(c.go);
        }

        //Renable user input
        foreach (Transform child in gameInput.transform)
        {
            child.gameObject.GetComponent<Button>().interactable = true;
        }

        //Change chips
        LoadChipImages();

        //Shuffle
        ShuffleCards();

        //Reset variables

        //Force update of text elements
        cash = _cash;

        betText.text = "$" + bet.ToString();

        playerText.text = 0.ToString();
        playerText.color = Color.white;
        dealerText.text = "?";
        dealerText.color = Color.white;

        userTurn = true;

        playerHand = new Hand
        {
            cards = new List<Card>(),
            text = playerText
        };
        dealerHand = new Hand 
        { 
            cards = new List<Card>(),
            text = dealerText
        };

        Debug.Log($"Total Win times: {wonTimes} | Total lost times: {lossTimes}");
    }
    void LoadChipImages()
    {
        //Decide which chips to show
        howRich = cash switch
        {
            int n when n < 1000 => 0,
            int n when n >= 1000 && n <= 1999 => 1,
            int n when n >= 2000 && n <= 4999 => 2,
            int n when n >= 5000 => 3,
            _ => 0
        };

        //Show correct chips
        for (int i = 0; i < chipImages.Length; i++)
        {
            chipImages[i].sprite = Resources.Load<Sprite>($"Images/Blackjack/Chips/{bettingOptions[howRich][i]}");
        }

    }
    void ShuffleCards()
    {
        Debug.Log("Shuffling cards");

        //Populate/Reset array

        //The below sets pointer i.e. changes both lists
        //cardsInPlay = cardsInOrder;

        //Linq term makes new dataset
        cardsInPlay = cardsInOrder.ToList();

        //Also an option without linq:
        //cardsInPlay = new List<Card>(cardsInPlay);

        //Repeat 1000 times
        for (int i = 0; i < 1000; i++)
        {
            //Select two random cards
            int c1 = Random.Range(0, 52);
            int c2 = Random.Range(0, 52);

            //Switch them
            Card c1v = cardsInPlay[c1];
            cardsInPlay[c1] = cardsInPlay[c2];
            cardsInPlay[c2] = c1v;
        }


        int c = 0;
        //Check how well the shuffle worked
        for (int i = 0; i < cardsInPlay.Count; i++)
            if (cardsInPlay[i] == cardsInOrder[i]) c++;
        
        Debug.Log($"Number of cards in the same spot: {c}");
    }
    public void DealCards(bool allIn = false)
    {
        if (allIn)
        {
            bet += cash;
            cash = 0;
            betText.text = "$" + bet.ToString();
            return;
        }

        betText.text = "$" + bet.ToString();
        //Open game elements
        pregameInput.SetActive(false);
        gameElements.SetActive(true);
        gameInput.SetActive(true);


        //Deal Hand
        StartCoroutine(nameof(IDealCards));
    }

    public void HitCard()
    {
        //Don't allow hit if card already moving to deck
        if (moving) return;

        //Deal card to player
        SpawnMoveCard();
    }
    public void Stand()
    {
        if (moving) return;
        Debug.Log("stand");
        //Disable user input
        foreach (Transform child in gameInput.transform)
        {
            child.gameObject.GetComponent<Button>().interactable = false;
        }

        userTurn = false;

        //flip dealer card

        dealerHand.cards[1].go.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Blackjack/Cards/" + cardPack + "/" + dealerHand.cards[1].Name);

        dealerText.text = dealerHand.HandValue().ToString();

        //Optimization for potential hands
        if (dealerHand.HandValue() == 21)
        {
            //dealer blackjack
            dealerHand.text.color = Color.green;
            FinishGame();
            return;
        }
        else if (playerHand.HandValue() > 21)
        {
            //Player bust
            FinishGame(); 
            return;
        }
            

        //Hit until limit for dealer, then finish game

        StartCoroutine(ICheckHitDealer(FinishGame));

    }
    void FinishGame()
    {
        //Finish game

        Debug.Log("game finished");

        int prevBet = bet;

        //Determine Winner
        switch (playerHand.Compare(dealerHand))
        {
            case 0:
                //Player wins, double bet
                winLossText.text = "You Won!";
                winLossText.color = Color.green;
                betInfoText.text = $"Bet payed out double (${bet*2})";
                wonTimes++;
                cash += bet * 2;
                bet = 0;
                break;
            case 1:
                //House wins, lose bet
                winLossText.text = "You lost";
                winLossText.color = Color.red;
                betInfoText.text = "Bet lost to house";
                lossTimes++;
                bet = 0;
                break;
            case 2:
                //No winners, bet returned
                winLossText.text = "Game Pushed";
                winLossText.color = Color.yellow;
                betInfoText.text = "Bet returned to wallet";
                cash += bet;
                bet = 0;
                break;
            default:
                break;

        }

        //If enough cash for rebet
        if (cash >= prevBet && prevBet != 0)
        {
            //Activate the button and label
            foreach (GameObject go in rebetElements) go.SetActive(true);
            //Change label text
            rebetElements[1].GetComponent<Text>().text = $"Rebet ${prevBet}";
            Button b = rebetElements[0].GetComponent<Button>();
            //Add appropriate onClick event
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => Rebet(prevBet));
        }
        else
            foreach (GameObject go in rebetElements) go.SetActive(false);

        //Show the panel
        endBannerPanel.SetActive(true);
    }
    public void Rebet(int prevBet)
    {
        //Add previous bet
        bet = prevBet;
        //Remove from cash - no validation as already has occured
        cash -= prevBet;

        //Continue game
        DealCards();
    }

    IEnumerator ICheckHitDealer(System.Action action)
    {
        bool hit = true;
        while (hit)
        {
            Debug.Log(dealerHand.cards.Count);
            //When hand is less than or equal to soft 17, hit
            if ((dealerHand.HandValue() < 17 || (dealerHand.HandValue() == 17 && dealerHand.softHand)) && dealerHand.cards.Count <= 4)
            {
                SpawnMoveCard(0.75f);
            }
            else hit = false;

            //Delay
            yield return new WaitForSeconds(1f);
        }

        //Call finish game
        action();
    }
    public void ChipPressed(int pos, bool less)
    {
        int value = bettingOptions[howRich][pos];

        //right click (less) removes
        if (less)
        {
            //Return correct amount to player cash
            if (bet <= value)
            {
                cash += bet;
                bet = 0;
            }
            else
            {
                bet -= value;
                cash += value;
            }
        }
        else
        {
            //If player has enough money add bet
            if (cash >= value)
            {
                bet += value;
                cash -= value;
            }

        }

        //Update Display
        betText.text = "$" + bet.ToString();
        //cashText.text = "$" + cash.ToString();

    }
    IEnumerator IDealCards()
    {

        float speed = 1f;

        //Dealer hand
        for (int i = 0; i < 2; i++)
        {
            userTurn = false;
            //Spawn card
            GameObject c = SpawnMoveCard(speed);
            dealerText.text = "?";
            //If second card
            if (i == 1)
            {
                //Hide value
                c.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Blackjack/Cards/" + cardPack + "/top1");
            }

            yield return new WaitForSeconds(1);
        }

        dealerText.text = "?";

        //Player hand
        for (int i = 0; i < 2; i++)
        {
            userTurn = true;
            //Spawn card
            SpawnMoveCard(speed);

            yield return new WaitForSeconds(1);
        }

        //Set player hand value


    }

    private GameObject SpawnMoveCard(float speed = 1f)
    {
        //Make card object
        Card card = cardsInPlay[0];

        card.go = Instantiate(cardPrefab, gameElements.transform);
        //Move to the deck location
        card.go.transform.localPosition = drawPile.transform.localPosition;
        card.go.transform.localRotation = drawPile.transform.localRotation;

        //Set final positions - either player or dealer deck position. Offset cards so others can be seen
        Quaternion rot = userTurn ? playerHandRot : dealerHandRot;
        Vector3 pos = userTurn ? playerHandPos - new Vector3(playerHand.cards.Count * -35f, 0, 0) : dealerHandPos - new Vector3(dealerHand.cards.Count * 35f, 0, 0);

        //Add card face value
        card.go.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Blackjack/Cards/" + cardPack + "/" + cardsInPlay[0].Name);

        //Lerp to final position at specified speed
        moving = true;
        StartCoroutine(IMoveCard(card.go, pos, speed, UpdateText));
        StartCoroutine(ISpinCard(card.go, rot, speed));

        

        //Add card to appropriate hand
        if (userTurn)
            playerHand.AddCard(card);
        else
            dealerHand.AddCard(card);
        
        //Remove from dealing deck
        cardsInPlay.RemoveAt(0);

        //Return card in case image or position needs to be changed again
        return card.go;
    }

    //Called after a card has finished moving
    void UpdateText()
    {
        Hand h = userTurn ? playerHand : dealerHand;
        moving = false;


        //Set hand value text if not dealer hand being dealt
        if (!userTurn && h.cards.Count < 2) { }
        else h.text.text = h.HandValue().ToString();

        if (h.HandValue() > 21)
        {
            //Visual indication of bust
            h.text.color = Color.red;
            //Bust logic
            if (userTurn)
                Stand();
        } 
        else if (h.HandValue() == 21 && h.cards.Count == 2)
        {
            //Blackjack
            h.text.color = Color.green;
            //Auto stand on blackjack
            if (userTurn)
                Stand();
        }
        if (h.cards.Count == 5)
        {
            //Force stand on 5 cards
            if (userTurn)
                Stand();
        }
    }
    IEnumerator ISpinCard(GameObject go, Quaternion newRot, float duration)
    {
        Quaternion currentRot = go.transform.rotation;
        
        float counter = 0;
        while (counter < duration)
        {
            //Change rotation
            go.transform.rotation = Quaternion.Lerp(currentRot, newRot, counter / duration * 0.55f);
            //Add time between frames
            counter += Time.deltaTime;
            //Next frame
            yield return null;
            
        }

        //go.transform.rotation = newRot;
    }
    IEnumerator IMoveCard(GameObject go, Vector3 newPos, float duration, System.Action action)
    {
        Vector3 currentPos = go.transform.localPosition;

        float counter = 0;
        while (counter < duration)
        {
            //Change location
            go.transform.localPosition = Vector3.Lerp(currentPos, newPos, counter / duration);
            //Add time between frames
            counter += Time.deltaTime;
            //Next frame
            yield return null;
        }

        action();
    }


    public void OpenShop()
    {
        shopCanvas.gameObject.SetActive(true);

        //Force cash update
        cash = _cash;

        //Remove any potential options from previous openings
        foreach (Transform go in cardSelectionPanel.transform)
        {
            Destroy(go.gameObject);
        }

        // Change when adding new card packs into assets
        int numOfPacks = 3;
        //Make card packs
        for (int i = 0; i < numOfPacks; i++)
        {
            //Create object and get inbuilt script
            CardPackManager cpm = Instantiate(cardOptionPrefab, cardSelectionPanel.transform).GetComponent<CardPackManager>();

            //Run script
            cpm.PopulateOption(i);
        }

        // Hide the menu
        menuCanvas.gameObject.SetActive(false);
    }

    public void HomeButtonPressed()
    {
        if (moving) return;
        //Remove money stored in bet
        cash += bet;
        bet = 0;
        //Go back to the main menu
        Debug.Log("home button pressed");
        playCanvas.gameObject.SetActive(false);
        menuCanvas.gameObject.SetActive(true);
    }

    public void OpenOptions()
    {
        //Open options panel
        Debug.Log("option button pressed");
        optionsPanel.SetActive(true);
        
    }
    public void CloseOptions()
    {
        //Save preferences over restarts
        PlayerPrefs.SetFloat("volume", vol);
        PlayerPrefs.SetInt("bettingEnabled", bettingToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("showHandValue", handValueToggle.isOn ? 1 : 0);

        //Disable panel
        optionsPanel.SetActive(false);
    }

    public void ChangeVolume(float sliderValue)
    {
        //Change actual game volume - Log used to match volume scaling (not linear)
        audioMixer.SetFloat("masterVol", Mathf.Log10(sliderValue) * 20);

        //Update internal variable
        vol = sliderValue;

        //Display volume as percentage
        volumeText.text = Mathf.RoundToInt(sliderValue * 100).ToString() + "%";
    }

    bool mute = false;
    float tempvolume = 0f;
    public void MuteAudio()
    {
        //Switch value
        mute = !mute;
        tempvolume = vol;

        if (mute)
            ChangeVolume(0f);
        else
            ChangeVolume(tempvolume);
    }

    public void BuyCash(int amount)
    {
        
        decimal cost = amount switch
        {
            1000 => 6.99m,
            2000 => 9.99m,
            5000 => 14.99m,
            10000 => 19.99m,
            20000 => 23.99m,
            _ => 0m
        };

        if (cost == 0m)
        {
            Debug.Log("There was an error. No charges were made");
            return;
        }

        //Payment code

        //Below formatting will turn cost into $x.XX format (two past decimal point). The same as cost.ToString("F2")
        Debug.Log($"${cost:F2} deducted from account.");

        cash += amount;
    }

    //Test methods
    public void ResetStoredVariables()
    {
        PlayerPrefs.DeleteAll();
        cash = 1000;
        PlayerPrefs.SetInt("pack0", 1);
        cardPack = 0;
        OpenShop();
    }
    public void MainMenu()
    {
        PlayerPrefs.SetInt("cash", _cash);

        //Usually called when exiting program (OnApplicationQuit), however preferences would be lost as scene is unloaded with this process of changing scenes (i.e. will not call OnApplicationQuit)
        PlayerPrefs.Save();

        SceneManager.LoadScene("Main");
    }
}
[System.Serializable]
public struct Card
{
    //Elements
    public string denomination;
    public string suit;
    public int value;
    public GameObject go;
    public readonly string Name => denomination + suit;
    // Operators

    //Override default Equals operation and run custom implementation (used in form card.Equals(c))
    public override bool Equals(object obj) => obj is Card c && Equals(c);

    //Custom equals implementation
    public bool Equals(Card c)
    {
        //Null protection
        if(c == null) return false;
        //If all important values are the same, return true, if not return false
        return (denomination == c.denomination) && (suit == c.suit) && (go == c.go);
    }

    //Neccesary for internal calulations - VS encouraged (implementation from MS C# docs)
    public override int GetHashCode() => (denomination, suit, go).GetHashCode();

    //Direct operator calls to custom equals implementations
    public static bool operator ==(Card c1, Card c2) => c1.Equals(c2);

    //Inverse
    public static bool operator !=(Card c1, Card c2) => !(c1 == c2);
}
[System.Serializable]
public class Hand {
    //Elements
    public bool softHand;
    public List<Card> cards;
    public Text text;

    public void AddCard(Card c)
    {
        // Override add function for list and determine card value
        // Respect standard rules for aces and picture cards
        
        //Null protection
        if (c == null) return;

        //If normal number card, add value directly and skip the rest
        if (int.TryParse(c.denomination, out c.value)) { }
        else if (c.denomination == "A")
        {
            //Handle ace value

            //If hand is greater than or equal to 11, make ace equal 1 (bust otherwise)
            if (this.HandValue() >= 11) c.value = 1;
            else
            {
                //Otherwise make it 11, and let the hand know there is an 11 valued ace (soft hand)
                c.value = 11;
                this.softHand = true;
            }
        }
        else
        {
            //Other picture cards
            c.value = 10;
        }
        cards.Add(c);

        //Change Ace value if over
        if (this.HandValue() > 21)
        {
            //For each card
            for (int i = 0; i < cards.Count; i++)
            {
                //If 11 valued ace
                if (cards[i].value == 11)
                {
                    //Change to 1
                    Card card = cards[i];
                    card.value = 1;
                    cards.RemoveAt(i);
                    cards.Add(card);
                    this.softHand = false;
                }
            }
        }
    }
    //Make int array return to return win reason for user
    public int Compare(Hand h)
    {
        //Null protection
        if (h == null) return -1;

        //Bust check        
        if (h.HandValue() > 21 && !(this.HandValue() > 21))
            return 0;
        else if (!(h.HandValue() > 21) && this.HandValue() > 21)
            return 1;
        else if (h.HandValue() > 21 && this.HandValue() > 21)
            return 2;

        //Blackjack
        if (h.HandValue() == 21 && this.HandValue() == 21)
        {
            //Check for who has superior blackjack
            if (this.cards.Count == 2 && h.cards.Count != 2)
                return 0;
            else if (this.cards.Count != 2 && h.cards.Count == 2)
                return 1;
            else if (this.cards.Count == 2 && h.cards.Count == 2)
                return 2;
        } 
        else if (h.HandValue() == 21)
        {
            if (h.cards.Count == 2)
                return 1;
        }
        else if ( this.HandValue() == 21)
        {
            if (this.cards.Count == 2)
                return 0;
        }

        //5 under 21 rule
        if ((h.HandValue() < 21 && h.cards.Count == 5) && (this.HandValue() < 21 && this.cards.Count == 5))
            return 2;
        else if (h.HandValue() < 21 && h.cards.Count == 5)
            return 1;
        else if (this.HandValue() < 21 && this.cards.Count == 5)
            return 0;

        //Equal natural hands push
        if (h.HandValue() == this.HandValue())
            return 2;

        //Greater hand value
        if (h.HandValue() < this.HandValue())
            return 0;
        else if (h.HandValue() > this.HandValue()) 
            return 1;

        //Code ostensibly never reached
        return 2;
    }

    //Determine Hand value
    public int HandValue()
    {
        int value = 0;
        //Foreach card
        foreach (Card card in cards)
        {
            //Add card value
            value += card.value;
        }
        return value;
    }
}

