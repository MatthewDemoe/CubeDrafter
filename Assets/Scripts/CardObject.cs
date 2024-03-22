using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardObject : MonoBehaviour
{
    RawImage cardImage;
    public CardData cardData { get; private set; } = null;

    private void Awake()
    {
        cardImage = GetComponent<RawImage>();
    }

    public void Initialize(CardData cardData)
    {
        this.cardData = cardData;
        cardImage.texture = cardData.cardImage;
    }

    public void HoverBeginAction(BaseEventData data)
    {
        Debug.Log("Hovered");
    }

    public void HoverEndAction(BaseEventData data)
    {
        Debug.Log("Hover Ended");
    }
}
