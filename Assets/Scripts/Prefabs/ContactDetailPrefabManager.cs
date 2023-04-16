using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContactDetailPrefabManager : MonoBehaviour
{
    public Text valueText;

    internal ContactsController cc;
    internal int detailID;

    public void LoadContactPopup()
    {
        cc.LoadDetailModal(detailID);
    }
}
