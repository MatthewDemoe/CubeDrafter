using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

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
        StartCoroutine(GetCardList());
    }

    IEnumerator GetCardList()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://cubecobra.com/cube/api/cubelist/31i0b");

        yield return www.SendWebRequest();

        if (UnityWebRequest.Result.ConnectionError == www.result || UnityWebRequest.Result.ProtocolError == www.result)
        {
            Debug.Log(www.error);
            StopAllCoroutines();
        }

        string listString = www.downloadHandler.text;
        cardNames = listString.Split('\n');

        www.Dispose();
        www = null;

        StartCoroutine(GetCardData());
    }

    IEnumerator GetCardData()
    {
        UnityWebRequest www;
        JObject scryfallData;

        cardList = new CardData[10];

        for (int i = 0; i < cardList.Length; i++)
        {
            string cardName = cardNames[i];

            www = UnityWebRequest.Get($"https://api.scryfall.com/cards/search?q={cardName}&pretty=true");
            yield return www.SendWebRequest();

            if (UnityWebRequest.Result.ConnectionError == www.result || UnityWebRequest.Result.ProtocolError == www.result)
            {
                Debug.Log(www.error);
                break;                
            }

            scryfallData = JObject.Parse(www.downloadHandler.text);
            cardList[i] = new CardData(scryfallData["data"][0]);

            yield return new WaitUntil(() => cardList[i].bIsFinishedLoading);

            yield return new WaitForSeconds(0.1f);
        }

        bIsLoaded = true;
    }
}
