using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;

[RequireComponent(typeof(Loadable))]
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
    Loadable loadable;

    public int numLoadedCards { get; private set; } = 0;

    public bool bIsLoaded { get; private set; } = false;

    private const int MAX_REQUESTS = 10;

    private void Awake()
    {
        _instance = this;
        loadable = GetComponent<Loadable>();

        DontDestroyOnLoad(this);
    }

    void Start()
    {
        //print(Application.persistentDataPath);

        //StartCoroutine(GetCardList());
    }

    public void GetCardData()
    {
        StartCoroutine(GetCardListRoutine());
    }

    IEnumerator GetCardListRoutine()
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

        StartCoroutine(RetrieveCardDataRoutine());
    }

    IEnumerator RetrieveCardDataRoutine()
    {
        print(cardNames.Length);

        cardList = new CardData[cardNames.Length];

        numLoadedCards = 0;

        while (numLoadedCards < cardNames.Length)
        {
            if (tempCards.Count >= MAX_REQUESTS)
                yield return new WaitUntil(() => tempCards.Count < 10);

            string cardName = cardNames[numLoadedCards];
            print(cardName);

            if (!CardDatabase.Instance.IsCardCached(cardName))
            {
                StartCoroutine(SendScryfallRequestRoutine(numLoadedCards));
                yield return new WaitForSeconds(0.1f);
            }

            else
                cardList[numLoadedCards] = CardDatabase.Instance.GetCardData(cardName);

            numLoadedCards++;
            loadable.OnValueChanged.Invoke((float)numLoadedCards / cardList.Length);
        }

        yield return new WaitUntil(() => cardList.All(cardData => cardData.isFinishedLoading));
        AssetDatabase.Refresh();
        cardList.ToList().ForEach(cardData => cardData.Initialize());

        bIsLoaded = true;
    }

    IEnumerator SendScryfallRequestRoutine(int index)
    {
        string cardName = cardNames[index];

        JObject scryfallData;
        UnityWebRequest webRequest = UnityWebRequest.Get($"https://api.scryfall.com/cards/search?q=!\"{cardName}\"&pretty=true");
        print(webRequest.url);

        yield return webRequest.SendWebRequest();

        if (UnityWebRequest.Result.ConnectionError == webRequest.result || UnityWebRequest.Result.ProtocolError == webRequest.result)
        {
            Debug.Log(webRequest.error);

            StartCoroutine(SendScryfallRequestRoutine(index));
            yield break;
        }

        scryfallData = JObject.Parse(webRequest.downloadHandler.text);

        CardData newCard = new CardData(scryfallData["data"][0]);
        tempCards.Add(newCard);
        cardList[index] = newCard;

        newCard.OnFinishedLoading.AddListener(() => 
        {
            tempCards.Remove(newCard);
        });

        newCard.OnFinishedLoading.AddListener(() => CardDatabase.Instance.InsertCardData(newCard));
    }
}
