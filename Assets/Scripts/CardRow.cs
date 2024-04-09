using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardRow : MonoBehaviour
{
    private CardPack pack;
    private RectTransform packTransform;

    public const int MAX_CARDS = 5;
    public int numCards => cards.Count;
    public bool isRowFull => numCards >= MAX_CARDS;

    private List<CardObject> cards = new List<CardObject>();

    void Start()
    {
        pack = GetComponentInParent<CardPack>();
        packTransform = pack.GetComponent<RectTransform>();
    }

    void Update()
    {
        LayoutCards();
    }

    public void AddCard(CardObject cardObject)
    {
        cards.Add(cardObject);
    }

    public void LayoutCards()
    {
        float ratio = UtilMath.Lmap(numCards, 1, MAX_CARDS -1, 0.0f, 0.75f);
        float width = packTransform.rect.width * ratio;
        float maxDistance = width / 2;

        for (int i = 0; i < cards.Count; i++)
        {
            float normalizedPlace = numCards > 1 ? UtilMath.Lmap(i, 0, numCards - 1, -1.0f, 1.0f) : 0;
            float xPos = normalizedPlace * maxDistance;

            cards[i].transform.localPosition = new Vector3(xPos, 0, 0);
        }
    }
}
