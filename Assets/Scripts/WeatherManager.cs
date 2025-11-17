using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Xml.Linq;
using System.Globalization;
using Unity.VisualScripting;

public class WeatherManager : MonoBehaviour
{
    [SerializeField] private Vector2 coordinates;

    private const string xmlApi = "https://api.openweathermap.org/data/2.5/weather?lat=28.5384&lon=81.3789&appid=338d1bb124320c2c9208a0b12ad1a906";
    private string apiURL;

    private bool isDay;
    private Color lightColor;
    private float lightIntensity;

    [SerializeField] private Skybox skybox;
    [SerializeField] private Material[] allSkyMats;

    public string[] testTime;
        //0 - Day
        //1 - sunset
        //2 - night
        //3 - rain
        //4 - snow

    private void Start()
    {
        apiURL = "https://api.openweathermap.org/data/2.5/weather?lat=" + coordinates.x + "&lon=" + coordinates.y + "&appid=338d1bb124320c2c9208a0b12ad1a906&mode=xml";
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

        yield return StartCoroutine(CallAPI(apiURL, callback));
    }

    public void OnXMLDataLoaded(string data)
    {
        XDocument _xml = XDocument.Parse(data);
        Debug.Log(data);
        ParseXML(_xml);
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

        var city = _doc.Element("current").Element("city");
        var sun = city.Element("sun");
        var temperature = _doc.Element("current").Element("temperature");
        var clouds = _doc.Element("current").Element("clouds");
        var weather = _doc.Element("current").Element("weather");
        var _timezone = _doc.Element("current").Element("timezone_offset");

        var _sunRise = sun.Attribute("rise").Value;
        var _sunSet = sun.Attribute("set").Value;
        var _temp = temperature.Attribute("value").Value;
        var _cloudiness = clouds.Attribute("value").Value;
        var _weather = weather.Attribute("number").Value;
        float _parsedTemp = float.Parse(_temp);
        var _Ftemp = (_parsedTemp - 273.15f) * 9/5 + 32;

        Debug.Log("Sunrise: " + _sunRise);
        Debug.Log("Sunset: " + _sunSet);
        Debug.Log("Temp kelvin: " + _temp);
        Debug.Log("Temp fahrenheit: " + _Ftemp);
        Debug.Log("Clouds: " + _cloudiness);
        Debug.Log("Weather Code: " + _weather);
        Debug.Log("City?" + city);

       /* System.TimeSpan _sunRiseTime = System.TimeSpan.Parse(_sunRise);
        System.TimeSpan _sunSetTime = System.TimeSpan.Parse(_sunSet);
        string _t = _timezone.Value;

        CheckIfDayTime(_sunRiseTime, _sunSetTime, _t);*/

    }

    private void CheckIfDayTime(System.TimeSpan _rise, System.TimeSpan _set, string _timezoneOff)
    {
        //Use timzeone and sunrise/sunset time to determine if it is day or night

        System.DateTime _currentTime = System.DateTime.Now;
        System.TimeSpan _time = _currentTime.TimeOfDay;
        System.TimeSpan _off;
        System.TimeSpan.TryParse(_timezoneOff, out _off);
        System.TimeSpan _targetTime = _time + _off;

        //Parse for time
        if(_targetTime > _rise && _targetTime < _set)
        {
            //day
            isDay = true;
        }
        else if(_targetTime < _rise || _targetTime > _set)
        {
            //night
            isDay = false;
        }
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


    private void ChangeSkyBox(int _idx)
    {
       // skybox.GetComponent<Material>().exposure
       RenderSettings.skybox = allSkyMats[_idx];
    }
}
