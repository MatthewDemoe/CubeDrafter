using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPack : MonoBehaviour
{
    [SerializeField]
    GameObject cardPrefab; 

    List<CardObject> cards = new List<CardObject>();

    void Start()
    {
        StartCoroutine(WaitForCubeList());
    }

    IEnumerator WaitForCubeList()
    {
        yield return new WaitUntil(() => CubeListRetriever.Instance.bIsLoaded);

        ConstructPack(CubeListRetriever.Instance.cardList);
    }

    private void ConstructPack(CardData[] cardList)
    {
        int randIndex = 0;
        for (int i = 0; i < 5; i++)
        {
            randIndex = Random.Range(0, cardList.Length);

            CardData cardData = cardList[randIndex];
            CardObject newCard = Instantiate(cardPrefab, gameObject.transform).GetComponent<CardObject>();
            newCard.Initialize(cardData);

            cards.Add(newCard);
        }
    }
}
