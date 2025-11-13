using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Xml.Linq;

public class WeatherManager : MonoBehaviour
{
    [SerializeField] private Vector2 coordinates;

    private const string xmlApi = "https://api.openweathermap.org/data/2.5/weather?lat=28.5384&lon=81.3789&appid=338d1bb124320c2c9208a0b12ad1a906";
    private string apiURL;

    private bool isDay;
    private Color lightColor;
    private float lightIntensity;

    private void Start()
    {
        apiURL = "https://api.openweathermap.org/data/2.5/weather?lat=" + coordinates.x + "&lon=" + coordinates.y + "&appid=338d1bb124320c2c9208a0b12ad1a906";
        StartCoroutine(GetWeatherXML(OnXMLDataLoaded));
    }

    private IEnumerator CallAPI(string url, Action<string> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError($"network problem: {request.error}");
            }
            else if (request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"response error: {request.responseCode}");
            }
            else
            {
                callback(request.downloadHandler.text);
            }
        }
    }

    public IEnumerator GetWeatherXML(Action<string> callback)
    {
        Debug.Log(CallAPI(xmlApi, callback));
        return CallAPI(xmlApi, callback);
        //return CallAPI(apiURL, callback);
    }

    public void OnXMLDataLoaded(string data)
    {
        //ParseXML(data);
        XDocument _xml = XDocument.Parse(data);
        Debug.Log(data);
    }

    private void ParseXML(XDocument _doc)
    {
        //Read for things that change the skybox, light, and day/time
        //This is:
        ////Timezone
        ///city.sun
        ///temperature.value
        ///clouds.value
        ///weather.number or weather.value

        var _timezone = _doc.Element("timezone");
        var _sunRise = _doc.Element("sun rise");
        var _sunSet = _doc.Element("sun set");
        var _temp = _doc.Element("temperature value");
        var _cloudiness = _doc.Element("clouds value");
        var _weather = _doc.Element("weather number");

        //Convert these into data for variables for scene

    }

    private void CheckIfDayTime(float _timezone)
    {
        //Use timzeone and sunrise/sunset time to determine if it is day or night

        var _currentTime = System.DateTime.Now;
    }


    private void CalculateLightIntensity(float _temp, float _cloud, float _weather)
    {
        //Less cloudy = higher light
        //Higher temp = higher light
        //sunny weather = higher light
        //Overriden by if it is nighttime

        float _tmpIntensity = 0; //1 is most intense, 0 least

        if(isDay)
        {
            //Weather conditions
            if(_weather == 800) ///Sunny
            {
                _tmpIntensity = 1;
            }
            else if(_weather > 800 && _weather < 805) //Cloudy
            {
                int _i = (int)_weather % 10;
                _i = 100 - (_i * 25);
                _tmpIntensity = _i / 100;
            }
            else //Other varying bad weather conditions
            {
                _tmpIntensity = 0.5f;
            }

            //Temperature
            _tmpIntensity += (_temp / 15);

            //Cloudiness, may just rely on weather value for this?
            _tmpIntensity -= (_cloud / 10);
        }
    }
}
