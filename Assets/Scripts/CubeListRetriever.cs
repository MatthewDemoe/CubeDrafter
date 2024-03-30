using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

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

    public bool bIsLoaded { get; private set; } = false;

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
        UnityWebRequest webRequest = UnityWebRequest.Get("https://cubecobra.com/cube/api/cubelist/31i0b");

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
        UnityWebRequest webRequest;
        JObject scryfallData;

        cardList = new CardData[10];

        for (int i = 0; i < cardList.Length; i++)
        {
            string cardName = cardNames[i];

            if (CardDatabase.Instance.IsCardCached(cardName))
                cardList[i] = CardDatabase.Instance.GetCardData(cardName);

            else
            {
                webRequest = UnityWebRequest.Get($"https://api.scryfall.com/cards/search?q={cardName}&pretty=true");
                yield return webRequest.SendWebRequest();

                if (UnityWebRequest.Result.ConnectionError == webRequest.result || UnityWebRequest.Result.ProtocolError == webRequest.result)
                {
                    Debug.Log(webRequest.error);
                    break;                
                }

                scryfallData = JObject.Parse(webRequest.downloadHandler.text);
                cardList[i] = new CardData(scryfallData["data"][0]);
                CardDatabase.Instance.InsertCardData(cardList[i]);

                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitUntil(() => cardList[i].bIsFinishedLoading);
        }

        bIsLoaded = true;
    }
}
