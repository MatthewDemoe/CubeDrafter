using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;

public class CubeListRetriever : MonoBehaviour
{
    private static CubeListRetriever _instance;
  
    public static CubeListRetriever Instance
    {
        get
        {
            if (null == _instance)
                new GameObject("Cube List").AddComponent<CubeListRetriever>();

            return _instance;
        }
    }

    [SerializeField]
    string[] cardNames;

    public CardData[] cardList { get; private set; }
    List<CardData> tempCards = new List<CardData>();


    public bool bIsLoaded { get; private set; } = false;
    private const int MAX_REQUESTS = 10;

    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        print(Application.persistentDataPath);

        StartCoroutine(GetCardList());
    }

    IEnumerator GetCardList()
    {
        UnityWebRequest webRequest = UnityWebRequest.Get("https://cubecobra.com/cube/api/cubelist/14e36fc7-002f-4351-8978-9c03aeb81d8e");

        yield return webRequest.SendWebRequest();

        if (UnityWebRequest.Result.ConnectionError == webRequest.result || UnityWebRequest.Result.ProtocolError == webRequest.result)
        {
            Debug.Log(webRequest.error);
            StopAllCoroutines();
        }

        string listString = webRequest.downloadHandler.text;
        cardNames = listString.Split('\n');

        webRequest.Dispose();
        webRequest = null;

        StartCoroutine(RetrieveCardData());
    }

    IEnumerator RetrieveCardData()
    {
        print(cardNames.Length);

        cardList = new CardData[cardNames.Length];

        int i = 0;

        while (i < cardNames.Length)
        {
            if (tempCards.Count >= MAX_REQUESTS)
                yield return new WaitUntil(() => tempCards.Count < 10);

            string cardName = cardNames[i];
            print(cardName);

            if (!CardDatabase.Instance.IsCardCached(cardName))
            {
                StartCoroutine(SendScryfallRequest(i));
                yield return new WaitForSeconds(0.1f);
            }

            else
                cardList[i] = CardDatabase.Instance.GetCardData(cardName);

            i++;
        }

        yield return new WaitUntil(() => cardList.All(cardData => cardData.isFinishedLoading));
        bIsLoaded = true;
    }

    IEnumerator SendScryfallRequest(int index)
    {
        string cardName = cardNames[index];

        JObject scryfallData;
        UnityWebRequest webRequest = UnityWebRequest.Get($"https://api.scryfall.com/cards/search?q=!\"{cardName}\"&pretty=true");
        print(webRequest.url);

        yield return webRequest.SendWebRequest();

        if (UnityWebRequest.Result.ConnectionError == webRequest.result || UnityWebRequest.Result.ProtocolError == webRequest.result)
        {
            Debug.Log(webRequest.error);
            yield break;
        }

        scryfallData = JObject.Parse(webRequest.downloadHandler.text);

        CardData newCard = new CardData(scryfallData["data"][0]);
        tempCards.Add(newCard);
        cardList[index] = newCard;

        newCard.OnFinishedLoading.AddListener(() => tempCards.Remove(newCard));
        newCard.OnFinishedLoading.AddListener(() => CardDatabase.Instance.InsertCardData(newCard));
    }
}
