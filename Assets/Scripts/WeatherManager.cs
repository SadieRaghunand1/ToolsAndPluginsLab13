using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Xml.Linq;

public class WeatherManager : MonoBehaviour
{
    public enum Cities
    {
        NewYork,
        London,
        Tokyo,
        MelbourneAU,
        Orlando
    }

    [Header("City Settings")]
    public Cities selectedCity; 
    [SerializeField] private Vector2 coordinates;

    [Header("Skybox Settings")]
    // 0-Day, 1-Sunset, 2-Night, 3-Rain, 4-Snow
    [SerializeField] private Material[] allSkyMats; 

    [SerializeField] private Light directionalLight;

    private bool isDay;

    private const string apiKey = "338d1bb124320c2c9208a0b12ad1a906";

    private void OnValidate()
    {
        UpdateCoordinates();
        if (Application.isPlaying)
            StartCoroutine(GetWeatherXML(OnXMLDataLoaded));
    }

    private void Start()
    {
        UpdateCoordinates();
        StartCoroutine(GetWeatherXML(OnXMLDataLoaded));
    }

    private void UpdateCoordinates()
    {
        switch (selectedCity)
        {
            case Cities.NewYork: coordinates = new Vector2(40.7128f, -74.0060f); break;
            case Cities.London: coordinates = new Vector2(51.5074f, -0.1278f); break;
            case Cities.Tokyo: coordinates = new Vector2(35.6895f, 139.6917f); break;
            case Cities.MelbourneAU: coordinates = new Vector2(37.8136f, 144.9631f); break;
            case Cities.Orlando: coordinates = new Vector2(28.5384f, -81.3789f); break;
        }
        Debug.Log($"City changed to {selectedCity}, coords: {coordinates}");
    }

    #region API
    private IEnumerator CallAPI(string url, Action<string> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
                Debug.LogError($"Request failed: {request.error}");
            else
                callback(request.downloadHandler.text);
        }
    }

    private IEnumerator GetWeatherXML(Action<string> callback)
    {
        string apiURL = $"https://api.openweathermap.org/data/2.5/weather?lat={coordinates.x}&lon={coordinates.y}&appid={apiKey}&mode=xml";
        Debug.Log("Fetching: " + apiURL);
        yield return StartCoroutine(CallAPI(apiURL, callback));
    }
    #endregion

    #region XML Parsing
    private void OnXMLDataLoaded(string data)
    {
        XDocument xml = XDocument.Parse(data);
        ParseXML(xml);
    }

    private void ParseXML(XDocument doc)
    {
        var city = doc.Element("current")?.Element("city");
        var sun = city?.Element("sun");
        var temperature = doc.Element("current")?.Element("temperature");
        var clouds = doc.Element("current")?.Element("clouds");
        var weather = doc.Element("current")?.Element("weather");

        if (city == null || sun == null || temperature == null || clouds == null || weather == null)
        {
            Debug.LogError("XML missing required elements.");
            return;
        }

        // Parse sunrise/sunset
        if (!DateTime.TryParse(sun.Attribute("rise")?.Value, out DateTime sunRise) ||
            !DateTime.TryParse(sun.Attribute("set")?.Value, out DateTime sunSet))
        {
            Debug.LogError("Failed to parse sunrise/sunset.");
            return;
        }

        // Day/night check
        DateTime nowUTC = DateTime.UtcNow;
        isDay = nowUTC > sunRise.ToUniversalTime() && nowUTC < sunSet.ToUniversalTime();

        // Parse temperature, clouds, weather
        float tempK = float.Parse(temperature.Attribute("value")?.Value ?? "0");
        float tempF = (tempK - 273.15f) * 9 / 5 + 32;
        float cloudiness = float.Parse(clouds.Attribute("value")?.Value ?? "0");
        int weatherCode = int.Parse(weather.Attribute("number")?.Value ?? "0");

        // Show info in logs
        Debug.Log("City: " + city.Attribute("name")?.Value);
        Debug.Log("Sunrise: " + sunRise);
        Debug.Log("Sunset: " + sunSet);
        Debug.Log("Temp K: " + tempK);
        Debug.Log("Temp F: " + tempF);
        Debug.Log("Clouds: " + cloudiness);
        Debug.Log("Weather Code: " + weatherCode);
        Debug.Log("Daytime? " + isDay);

        // Update lighting and skybox
        UpdateLighting(tempK, cloudiness, weatherCode);
        UpdateSkybox(weatherCode);
    }
    #endregion

    #region Helpers
    private void UpdateLighting(float tempK, float cloudiness, int weatherCode)
    {
        if (directionalLight == null) return;

        float intensity = 0f;

        if (isDay)
        {
            // sunny
            if (weatherCode == 800) intensity = 1f; 
            // cloudy
            else if (weatherCode >= 801 && weatherCode <= 804) intensity = 0.75f; 
            // rain/snow
            else intensity = 0.5f; 

            intensity += (tempK - 273.15f) / 150f;
            intensity -= cloudiness / 100f;
        }

        intensity = Mathf.Clamp(intensity, 0f, 1f);
        directionalLight.intensity = intensity;
        directionalLight.color = isDay ? Color.white : Color.gray;
    }

    private void UpdateSkybox(int weatherCode)
    {
        int idx = 0;

        if (weatherCode >= 500 && weatherCode < 600) idx = 3; // Rain
        else if (weatherCode >= 600 && weatherCode < 700) idx = 4; // Snow
        else if (!isDay) idx = 2; // Night
        else if (DateTime.Now.Hour >= 18) idx = 1; // Sunset
        else idx = 0; // Day

        if (allSkyMats.Length > idx)
            RenderSettings.skybox = allSkyMats[idx];
    }
    #endregion

    #region Public Buttons
    // Change city at runtime
    public void SelectNewYork() { selectedCity = Cities.NewYork; OnValidate(); }
    public void SelectLondon() { selectedCity = Cities.London; OnValidate(); }
    public void SelectTokyo() { selectedCity = Cities.Tokyo; OnValidate(); }
    public void SelectMelbourneAU() { selectedCity = Cities.MelbourneAU; OnValidate(); }
    public void SelectOrlando() { selectedCity = Cities.Orlando; OnValidate(); }
    #endregion
}
