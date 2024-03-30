using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardPack : MonoBehaviour
{
    [SerializeField]
    GameObject cardPrefab; 

    List<CardObject> cards = new List<CardObject>();
    List<CardRow> rows;

    void Start()
    {
        StartCoroutine(WaitForCubeList());
        rows = GetComponentsInChildren<CardRow>().ToList();
    }

    IEnumerator WaitForCubeList()
    {
        yield return new WaitUntil(() => CubeListRetriever.Instance.bIsLoaded);

        ConstructPack(CubeListRetriever.Instance.cardList);
    }

    private void ConstructPack(CardData[] cardList)
    {
        int randIndex = 0;
        for (int i = 0; i < 7; i++)
        {
            randIndex = Random.Range(0, cardList.Length);

            CardData cardData = cardList[randIndex];
            CardObject newCard = Instantiate(cardPrefab, gameObject.transform).GetComponent<CardObject>();
            newCard.Initialize(cardData);

            cards.Add(newCard);
        }

        CreateRows();
    }

    private void CreateRows()
    {
        int numRows = (cards.Count / 5) + (cards.Count % 5 > 0 ? 1 : 0);

        int rowCounter = 0;
        foreach (CardObject card in cards)
        {
            while (rows[rowCounter].isRowFull)
            {
                rowCounter++;
            }

            rows[rowCounter].AddCard(card);
            card.transform.parent = rows[rowCounter].transform;
        }      
    }
}
