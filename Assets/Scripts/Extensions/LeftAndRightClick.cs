using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LeftAndRightClick : MonoBehaviour, IPointerClickHandler
{
    public BlackjackController script;
    public int pos;

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log(eventData.button);
        if (eventData.button == PointerEventData.InputButton.Right)
            script.ChipPressed(pos, true);
        else if (eventData.button == PointerEventData.InputButton.Left)
            script.ChipPressed(pos, false);
    }

}
