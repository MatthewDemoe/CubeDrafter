using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class CardData
{
    private UnityWebRequest www;

    public string sCardName { get; private set; } = string.Empty;
    public string sImageUri { get; private set; } = string.Empty;

    public string sCardLayout { get; private set; } = string.Empty;

    public Texture cardImage { get; private set; }

    public bool bIsFinishedLoading { get; private set; } = false;

    public CardData(JToken cardToken)
    {
        Debug.Log(cardToken);
        sCardName = cardToken["name"].ToString();

        sCardLayout = cardToken["layout"].ToString();

        if (sCardLayout.Equals("transform"))
        {
            sImageUri = cardToken["card_faces"][0]["image_uris"]["png"].ToString();
            sCardName = cardToken["card_faces"][0]["name"].ToString();
        }

        else
            sImageUri = cardToken["image_uris"]["png"].ToString();

        InitializeCard();
    }

    public CardData(string name, string imageUri, string cardLayout) 
    {
        sCardName = name;
        sImageUri = imageUri;
        sCardLayout = cardLayout;

        InitializeCard();
    }

    private async void InitializeCard()
    {
        www = UnityWebRequestTexture.GetTexture(sImageUri);
        www.SendWebRequest();

        while (!www.isDone)
            await Task.Delay(1000 / 30);

        if (UnityWebRequest.Result.ConnectionError == www.result || UnityWebRequest.Result.ProtocolError == www.result)
        {
            Debug.Log(www.error);
        }

        cardImage = ((DownloadHandlerTexture)www.downloadHandler).texture;
        bIsFinishedLoading = true;
    }
}
