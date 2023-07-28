using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine.UI;

public class CubeListRetriever : MonoBehaviour
{
    [SerializeField]
    string[] cardNames;

    [SerializeField]
    RawImage cardImage;

    Card[] cardList;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetCardList());
    }

    IEnumerator GetCardList()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://cubecobra.com/cube/api/cubelist/alh");

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

        StartCoroutine(GetCardImageList());
    }

    IEnumerator GetCardImageList()
    {
        UnityWebRequest www;
        JObject scryfallData;

        cardList = new Card[cardNames.Length];

        for (int cardIndex = 0; cardIndex < 10; cardIndex++)
        {
            www = UnityWebRequest.Get("https://api.scryfall.com/cards/search?q=" + cardNames[cardIndex] + "&pretty=true");
            yield return www.SendWebRequest();

            if (UnityWebRequest.Result.ConnectionError == www.result || UnityWebRequest.Result.ProtocolError == www.result)
            {
                Debug.Log(www.error);
                break;
            }

            scryfallData = JObject.Parse(www.downloadHandler.text);
            cardList[cardIndex] = new Card(scryfallData["data"][0]);

            yield return new WaitUntil(() => cardList[cardIndex].bIsFinishedLoading);
            cardImage.texture = cardList[cardIndex].cardImage;
            cardImage.SetNativeSize();
            cardImage.GetComponent<RectTransform>().localScale = Vector3.one * 0.5f;
        }
    }
}
