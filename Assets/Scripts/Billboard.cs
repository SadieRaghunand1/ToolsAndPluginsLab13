using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Billboard : MonoBehaviour
{
    private const string image1 = "https://upload.wikimedia.org/wikipedia/commons/thumb/7/7f/Little_dog_and_toy.jpg/640px-Little_dog_and_toy.jpg";
    private const string image2 = "https://upload.wikimedia.org/wikipedia/commons/thumb/f/fe/Butterfly_in_a_flower_tree.jpg/640px-Butterfly_in_a_flower_tree.jpg";
    private const string image3 = "https://upload.wikimedia.org/wikipedia/commons/thumb/7/78/Pac-Man_eating_dots.svg/640px-Pac-Man_eating_dots.svg.png";

    [SerializeField] private MeshRenderer[] rend;
    Texture2D targetText;

    [Tooltip("Debugging tool - True if want all three images shown, false if only want one image shown on all 3 billboards")]
    public bool loadDiffImages;

    private void Start()
    {
        //Check if user wants all billboards to show the same image
        //If not, show different ones
        if(loadDiffImages)
        {
            StartCoroutine(GetWebImage(RetriveImageURL, image1, rend[0]));
            StartCoroutine(GetWebImage(RetriveImageURL, image2, rend[1]));
            StartCoroutine(GetWebImage(RetriveImageURL, image3, rend[2]));
        }
        //If so, show the same one
        else
        {
            for(int i = 0; i < rend.Length; i++)
            {
                StartCoroutine(GetWebImage(RetriveImageURL, image1, rend[i]));
            }
            
            
        }


    }

    public IEnumerator DownloadImage(Action<Texture2D> callback, string _imageURL, MeshRenderer _mr)
    {
        
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(_imageURL);
        yield return request.SendWebRequest();
        //Check if null texture to avoid downoading the same texture multiple times
        if(targetText == null || loadDiffImages)
        {
            callback(DownloadHandlerTexture.GetContent(request));
            //Assign retrieved texture to the appropriate variable
            targetText = DownloadHandlerTexture.GetContent(request);
        }

        
        _mr.material.mainTexture = targetText;


    }

    //Start dowloading image
   public IEnumerator GetWebImage(Action<Texture2D> callback, string _imageURL, MeshRenderer _mr)
    {
        yield return StartCoroutine(DownloadImage(callback, _imageURL, _mr));
    }

    //Callback
    public void RetriveImageURL(Texture2D _text)
    {
        Debug.Log(_text);
    }
}
