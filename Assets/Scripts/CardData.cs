using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.IO;
using UnityEditor;

[System.Serializable]
public class CardData
{
    public UnityEvent OnFinishedLoading = new UnityEvent();

    private UnityWebRequest www;
    public string imagePath => $"{Paths.cardTextureSavePath}/{cardName}.png";
    public string cardName { get; private set; } = string.Empty;
    public string imageUri { get; private set; } = string.Empty;

    public string cardLayout { get; private set; } = string.Empty;

    public Texture cardImage { get; private set; }

    public bool isFinishedLoading { get; private set; } = false;

    //All the potentially double-sided cards. 
    List<string> transformCardStrings = new List<string>() { "transform", "modal_dfc" };
    List<string> splitCardStrings = new List<string>() { "adventure" };

    public CardData(JToken cardToken)
    {
        Debug.Log(cardToken);
        cardName = cardToken["name"].ToString();

        cardLayout = cardToken["layout"].ToString();

        //Get the name of the front-side of the card. 
        if (transformCardStrings.Contains(cardLayout))
        {
            imageUri = cardToken["card_faces"][0]["image_uris"]["png"].ToString();
            cardName = cardToken["card_faces"][0]["name"].ToString();
        }

        else if (splitCardStrings.Contains(cardLayout))
        {
            imageUri = cardToken["image_uris"]["png"].ToString();
            cardName = cardToken["card_faces"][0]["name"].ToString();
        }

        else
            imageUri = cardToken["image_uris"]["png"].ToString();

        InitializeCard();
    }

    public CardData(string name, string imageUri, string cardLayout)
    {
        cardName = name;
        this.imageUri = imageUri;
        this.cardLayout = cardLayout;

        InitializeCard();
    }

    private async void InitializeCard()
    {
        if (!File.Exists(imagePath))
        {
            www = UnityWebRequestTexture.GetTexture(imageUri);
            www.SendWebRequest();

            while (!www.isDone)
                await Task.Delay(300);

            if (UnityWebRequest.Result.ConnectionError == www.result || UnityWebRequest.Result.ProtocolError == www.result)
            {
                Debug.Log(www.error);
            }

            while (!www.downloadHandler.isDone)
                await Task.Delay(300);

            cardImage = ((DownloadHandlerTexture)www.downloadHandler).texture;

            Texture2D texture = cardImage as Texture2D;
            byte[] textureBytes = texture.EncodeToPNG();

            File.WriteAllBytes(imagePath, textureBytes);

            AssetDatabase.Refresh();
        }
            
        cardImage = (Resources.Load($"{Paths.cardTextureLoadPath}/{cardName}") as Texture2D);
        isFinishedLoading = true;
        OnFinishedLoading.Invoke();
    }
}
