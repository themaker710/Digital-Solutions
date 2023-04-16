using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoverOnMouseover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject target;
    public GameObject popupPrefab;

    GameObject popup;

    bool mouseIn = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Create object
        popup = Instantiate(popupPrefab, target.transform);

        Text text = popup.GetComponent<Text>();
        text.text = "Test";
        text.fontSize = 30;
        text.color = Color.white;
        //Start location loop
        mouseIn = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //End loop
        mouseIn = false;
        //Destroy object
        Destroy(popup);
    }
    // Update is called once per frame
    void Update()
    {
        if (mouseIn)
        {
            popup.transform.position = Input.mousePosition;
        }
    }
}
