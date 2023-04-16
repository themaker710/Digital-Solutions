using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class DarkenOnMouse : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler   
{
    public Image[] targets;

    public float originalAlpha = 0.75f;
    public float darkenedAlpha = 1f;

    void Start()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (Image target in targets)
        {
            Color c = target.color;
            c.a = darkenedAlpha;
            target.color = c;
        }

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (Image target in targets)
        {
            Color c = target.color;
            c.a = originalAlpha;
            target.color = c;
        }
    }

}
