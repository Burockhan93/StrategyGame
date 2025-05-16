using Shared.Game;
using Shared.HexGrid;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeatherManager : MonoBehaviour
{

    public ParticleSystem rain, snow, wind, storm, drizzle, thunder, fog;

    public TMP_Text popUpText;
    public enum Season { SPRING, SUMMER, AUTUMN, WINTER };

    [Header("Weather Settings")]
    public int weather_id;
    public float wind_speed;

    [Header("Light Settings")]
    public Season currentSeason;
    public Light sunLight;

    public bool isNight;

    public float springLightIntensity;
    public float summerLightIntensity;
    public float autumnLightIntensity;
    public float winterLightIntensity;

    public Color springLightColor;
    public Color summerLightColor;
    public Color autumnLightColor;
    public Color winterLightColor;

    public int currentYear;

    public String time;
    private void Start()
    {
        currentSeason = GetSeason();

        GetWeather();

        springLightColor = sunLight.color;
        springLightIntensity = sunLight.intensity;
    }
    public Season GetSeason()
    {

        DateTime dateToday = DateTime.Today;
        int month = dateToday.Month;

        if (month < 3 || month == 12)
        {
            currentSeason = Season.WINTER;
            LerpSunIntensity(this.sunLight, winterLightIntensity);
            LerpLightColor(this.sunLight, winterLightColor);
        }
        else if (month < 6)
        {
            currentSeason = Season.SPRING;
            this.sunLight.color = springLightColor;
            this.sunLight.intensity = springLightIntensity;
        }
        else if (month < 9)
        {
            currentSeason = Season.SUMMER;
            LerpSunIntensity(this.sunLight, summerLightIntensity);
            LerpLightColor(this.sunLight, summerLightColor);
        }
        else if (month < 12)
        {
            currentSeason = Season.AUTUMN;
            LerpSunIntensity(this.sunLight, autumnLightIntensity);
            LerpLightColor(this.sunLight, autumnLightColor);
        }
        return currentSeason;
    }

    // Doku: https://openweathermap.org/weather-conditions
    public void GetWeather()
    {
        if (GameLogic.grid == null)
        {
            return;
        }

        string rainText = "The rain attracts fish: +10% on fish production.\n";
        string windText = "The wind blew over some trees: +10% on wood production.\n";
        string sunText = "It's sunny: +10% on wheat production.\n";
        string snowText = "The snow attracts fish: +10% on fish production.\n";
        string blankText = "No weather buff.";
        //string effectText = "+{0}% on {1} production";
        //string toWrite = string.Format("{0}: {1}", rainText, string.Format(effectText, 10, "fish"));
        bool isWind = false;
        string toWrite = "";

        time = GameLogic.ctime;
        isNight = (time == "Night");
        switch (GameLogic.grid.GetWind(wind_speed))
        {
            case (Wind.WIND):
                {
                    toWrite = windText;
                    wind.Play();
                    storm.Stop();
                    isWind = true;
                    break;
                }
            case (Wind.STORM):
                {
                    toWrite = windText;
                    wind.Stop();
                    storm.Play();
                    isWind = true;
                    break;
                }
            default:
                {
                    wind.Stop();
                    storm.Stop();
                    break;
                }
        }
        switch (GameLogic.grid.GetPrecipitation(weather_id))
        {
            case (Precipitation.RAIN):
                {
                    toWrite += rainText;
                    if (isWind && !isNight)
                    {
                        weatherButtonImage.sprite = rainImageW;
                    }
                    else if (isWind && isNight)
                    {
                        weatherButtonImage.sprite = rainMoonImageW;
                    }
                    else if (!isWind && !isNight)
                    {
                        weatherButtonImage.sprite = rainImage;
                    }
                    else
                    {
                        weatherButtonImage.sprite = rainMoonImage;
                    }

                    rain.Play();
                    snow.Stop();
                    drizzle.Stop();
                    break;
                }
            case (Precipitation.SNOW):
                {
                    toWrite += snowText;
                    if (isWind)
                    {
                        weatherButtonImage.sprite = snowImageW;
                    }
                    else
                    {
                        weatherButtonImage.sprite = snowImage;
                    }
                    rain.Stop();
                    snow.Play();
                    drizzle.Stop();
                    break;
                }
            case (Precipitation.DRIZZLE):
                {
                    toWrite += rainText;
                    if (isWind)
                    {
                        weatherButtonImage.sprite = rainImageW;
                    }
                    else
                    {
                        weatherButtonImage.sprite = rainImage;
                    }
                    rain.Stop();
                    snow.Stop();
                    drizzle.Play();
                    break;
                }
            default:
                {
                    if (isWind)
                    {
                        weatherButtonImage.sprite = sunImageW;
                    }
                    else
                    {
                        weatherButtonImage.sprite = sunImage;
                    }
                    rain.Stop();
                    snow.Stop();
                    drizzle.Stop();
                    break;
                }
        }
        switch (GameLogic.grid.GetObscuration(weather_id))
        {
            case (Obscuration.FOG):
                {
                    if (isWind)
                    {
                        weatherButtonImage.sprite = fogImageW;
                    }
                    else
                    {
                        weatherButtonImage.sprite = fogImage;
                    }
                    fog.Play();
                    break;
                }
            default:
                {
                    fog.Stop();
                    break;
                }
        }
        switch (GameLogic.grid.GetDescriptor(weather_id))
        {
            case (Descriptor.THUNDERSTORM):
                {
                    if (isWind)
                    {
                        weatherButtonImage.sprite = thunderImageW;
                    }
                    else
                    {
                        weatherButtonImage.sprite = thunderImage;
                    }
                    thunder.Play();
                    break;
                }
            default:
                {
                    thunder.Stop();
                    break;
                }
        }
        switch (GameLogic.grid.GetCloudy(weather_id))
        {
            case (Cloudy.FEW):
                {
                    if (isWind && !isNight)
                    {
                        weatherButtonImage.sprite = fewCloudsImageW;
                    }
                    else if (isWind && isNight)
                    {
                        weatherButtonImage.sprite = moonFewCloudsImageW;
                    }
                    else if (!isWind && !isNight)
                    {
                        weatherButtonImage.sprite = fewCloudsImage;
                    }
                    else if (!isWind && isNight)
                    {
                        weatherButtonImage.sprite = moonFewCloudsImage;
                    }

                    break;
                }
            case (Cloudy.SCATTERD):
                {
                    if (isWind)
                    {
                        weatherButtonImage.sprite = scatteredCloudsImageW;
                    }
                    else
                    {
                        weatherButtonImage.sprite = scatteredCloudsImage;
                    }
                    break;
                }
            case (Cloudy.BROKEN):
                {
                    if (isWind)
                    {
                        weatherButtonImage.sprite = brokenCloudsImageW;
                    }
                    else
                    {
                        weatherButtonImage.sprite = brokenCloudsImage;
                    }
                    break;
                }
            default:
                {
                    break;
                }

        }
        switch (GameLogic.grid.GetSunny(weather_id))
        {
            case (Sunny.SUN):
                toWrite += sunText;
                {
                    if (isWind && !isNight)
                    {
                        weatherButtonImage.sprite = sunImageW;
                    }
                    else if (isWind && isNight)
                    {
                        weatherButtonImage.sprite = moonImageW;
                    }
                    else if (!isWind && !isNight)
                    {
                        weatherButtonImage.sprite = sunImage;
                    }
                    else if (!isWind && isNight)
                    {
                        weatherButtonImage.sprite = moonImage;
                    }
                    break;
                }
            default:
                {
                    break;
                }
        }
        if (toWrite == "")
            toWrite = blankText;

        popUpText.SetText(toWrite);

    }

    public Button weatherButton;
    public Image weatherButtonImage;
    //public GameObject popUpBox;
    //public Animator animator;

    public Sprite rainImage, snowImage, fogImage, sunImage, thunderImage, moonImage, rainMoonImage, rainImageW, snowImageW, fogImageW, sunImageW, thunderImageW, moonImageW, rainMoonImageW, fewCloudsImage;
    public Sprite moonFewCloudsImage, moonFewCloudsImageW, fewCloudsImageW, brokenCloudsImage, brokenCloudsImageW, scatteredCloudsImage, scatteredCloudsImageW;
    public void setImage(string weather)
    {

        // Images for the night
        switch (weather)
        {
            case "moon":
                weatherButtonImage.sprite = moonImage;
                break;
            case "rain_moon":
                weatherButtonImage.sprite = rainMoonImage;
                break;
            case "rain_moon+w":
                weatherButtonImage.sprite = rainMoonImageW;
                break;
            case "moon+w":
                weatherButtonImage.sprite = moonImageW;
                break;
        }
    }

    public void Update()
    {

        if (GameLogic.grid == null)
        {
            Debug.Log("Update: grid not set");
            return;
        }

        // comment out for debugging to set weather manually in unity
        if (weather_id != GameLogic.grid.weather_id || wind_speed != GameLogic.grid.wind_speed)
        {
            Debug.Log("debug weather " + weather_id);
            Debug.Log("weather from grid" + GameLogic.grid.weather_id);
            weather_id = GameLogic.grid.weather_id;
            wind_speed = GameLogic.grid.wind_speed;
            GetWeather();
        }

        if (currentSeason == Season.SPRING)
        {
            LerpSunIntensity(this.sunLight, springLightIntensity);
            LerpLightColor(this.sunLight, springLightColor);
        }

        if (currentSeason == Season.SUMMER)
        {
            LerpSunIntensity(sunLight, summerLightIntensity);
            LerpLightColor(sunLight, summerLightColor);
        }

        if (currentSeason == Season.AUTUMN)
        {
            LerpSunIntensity(sunLight, autumnLightIntensity);
            LerpLightColor(sunLight, autumnLightColor);
        }
        if (currentSeason == Season.WINTER)
        {
            LerpSunIntensity(sunLight, winterLightIntensity);
            LerpLightColor(sunLight, winterLightColor);
        }

    }
    // for day/night, currently used for season change
    private void LerpLightColor(Light light, Color c)
    {
        light.color = Color.Lerp(light.color, c, 0.2f * Time.deltaTime);
    }

    private void LerpSunIntensity(Light light, float intensity)
    {
        light.intensity = Mathf.Lerp(light.intensity, intensity, 0.2f * Time.deltaTime);
    }
}
