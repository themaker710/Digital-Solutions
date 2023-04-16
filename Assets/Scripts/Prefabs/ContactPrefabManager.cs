using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ContactPrefabManager : MonoBehaviour, IPointerClickHandler
{
    public Button removeButton;
    
    public Text nameText;

    private int contactID;

    private bool remove = false;

    private ContactsController cc;

    internal void PopulateFields(string fname, string nick, string lname, int ID, ContactsController controller)
    {
        contactID = ID;

        cc = controller;

        nameText.text = string.IsNullOrEmpty(nick) ? fname + " " + lname : nick + " " + lname;
    }
    
    //Extend existing event hook
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        //Simulating long press
        if (eventData.button == PointerEventData.InputButton.Right)
            ToggleRemove();
        else if (eventData.button == PointerEventData.InputButton.Left)
            cc.LoadContact(contactID);
    }

    private void ToggleRemove()
    {
        remove = !remove;
        removeButton.gameObject.SetActive(remove);
    }

    public void DeleteEntry()
    {

        //Remove from DB (through to method in main script)

        cc.DeleteContact(contactID);
        
        Destroy(gameObject);
    }
}
