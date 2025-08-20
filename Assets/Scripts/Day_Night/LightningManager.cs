using Unity.VisualScripting;
using UnityEngine;
[ExecuteAlways]
public class LightningManager : MonoBehaviour
{
    //References
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private Light MoonLight;
    [SerializeField] private LightingPreset Preset;
    //Material skybox
    [SerializeField] private Material DaySkybox;
    [SerializeField] private Material NightSkybox;
    //Variables
    [SerializeField, Range(0, 24)] private float TimeOfDay;
    [SerializeField, Range(0, 10)] private float SpeedOfDay = 1f;
    [SerializeField] private float StartSunRiseTemperature = 4000f;
    [SerializeField] private float AfternoonSunTemperature = 15000f;
    [SerializeField] private float SunsetTemperature = 15000f;
    [SerializeField] private AnimationCurve ExposureCurve;
    [SerializeField] private AnimationCurve SunSize;
    //Colors
    [SerializeField] private Color MorningSkyTint;
    [SerializeField] private Color AfternoonSkyTint;
    [SerializeField] private Color EveningSkyTint;
    [SerializeField] private AnimationCurve SunTemperature;





    private void Update()
    {
        if(Preset == null)
        {
            return; 
        }
        if (Application.isPlaying)
        {

            UpdateLighting(CalculateTime() / 24f);
            UpdateSkybox(CalculateTime() / 24f);

        }
        else
        {
            UpdateLighting(TimeOfDay / 24f);
            UpdateSkybox(TimeOfDay / 24f);
        }
        if (RenderSettings.skybox != null)
        {
            SkyColorAttributes(TimeOfDay);
            SkyStartAttributes(TimeOfDay, StartSunRiseTemperature, AfternoonSunTemperature, SunsetTemperature);
            RenderSettings.skybox.SetFloat("_Exposure", ExposureCurve.Evaluate(TimeOfDay));
            RenderSettings.skybox.SetFloat("_SunSize", SunSize.Evaluate(TimeOfDay));
        }


    }



    private void SkyColorAttributes(float hour)
    {
        Color lerpedColor;

        if (hour >= 6f && hour < 15f)
        {
            float t = Mathf.InverseLerp(6f, 15f, hour);
            lerpedColor = Color.Lerp(MorningSkyTint, MorningSkyTint, t);
        }
        else if (hour >= 15f && hour < 18f)
        {
            float t = Mathf.InverseLerp(15f, 17f, hour);
            lerpedColor = Color.Lerp(MorningSkyTint, AfternoonSkyTint, t);
        }
        else if (hour >= 17f && hour < 23f)
        {
            float t = Mathf.InverseLerp(17f, 23f, hour);
            lerpedColor = Color.Lerp(AfternoonSkyTint, EveningSkyTint, t);
        }
        else if (hour >= 23f && hour <= 24f)
        {
            float t = Mathf.InverseLerp(23f, 24f, hour);
            lerpedColor = Color.Lerp(EveningSkyTint, EveningSkyTint, t);
        }
        else if (hour >= 0f && hour <= 4f)
        {
            float t = Mathf.InverseLerp(0f, 4f, hour);
            lerpedColor = Color.Lerp(EveningSkyTint, EveningSkyTint, t);
        }
        else
        {
            float t = Mathf.InverseLerp(4f, 6f, hour);

            lerpedColor = Color.Lerp(EveningSkyTint, MorningSkyTint, t);
        }

        RenderSettings.skybox.SetColor("_SkyTint", lerpedColor);
        
    }



    private void SkyStartAttributes(float hour, float StartSunRiseTemperature, float AfternoonSunTemperature, float SunsetTemperature)
    {
        float intensity = 1;
        float temperature = 4000f;

        if (hour >= 6f && hour < 15f)
        {
            float t = Mathf.InverseLerp(6f, 15f, hour); // 6h - 15h
            temperature = Mathf.Lerp(StartSunRiseTemperature, AfternoonSunTemperature, t);
            if(hour >= 6f && hour < 7.5f)
            {
                 t = Mathf.InverseLerp(6f, 7.5f, hour);
                intensity = Mathf.Lerp(0.002f, 0.05f, t);
            }
            else if(hour >= 7.5f && hour < 15f)
            {
                 t = Mathf.InverseLerp(7.5f, 15f, hour);
                intensity = Mathf.Lerp(0.05f, 1f, t);
            }
        }
        else if (hour >= 15f && hour < 18f)
        {
            float t = Mathf.InverseLerp(15f, 18f, hour); //15h - 18h
            temperature = Mathf.Lerp(AfternoonSunTemperature, SunsetTemperature, t);
            intensity = Mathf.Lerp(1f, 0.001f, t);
            if (hour >= 15f && hour < 16f)
            {
                t = Mathf.InverseLerp(15f, 16f, hour);
                intensity = Mathf.Lerp(1f, 0.05f, t);
            }
            else if(hour >= 16f && hour <= 18f)
            {
                t = Mathf.InverseLerp(16f, 18f, hour);
                intensity = Mathf.Lerp(0.05f, 0.001f, t);
            }
        }
        else if(hour > 18f && hour < 24f)
        {
            float t = Mathf.InverseLerp(18f, 24f, hour); // 4h -6h
            temperature = Mathf.Lerp(SunsetTemperature, StartSunRiseTemperature, t);
            intensity = Mathf.Lerp(0.1f, 0.1f, t);
        }
        else
        {
            float t = Mathf.InverseLerp(0f, 6f, hour); // 4h -6h
            temperature = Mathf.Lerp(SunsetTemperature, StartSunRiseTemperature, t);
            intensity = Mathf.Lerp(0.1f, 0.002f, t);
        }

        DirectionalLight.useColorTemperature = true;
        DirectionalLight.colorTemperature = temperature;
        DirectionalLight.intensity = intensity;




    }
        

        

    

    private float CalculateTime()
    {
        TimeOfDay += Time.deltaTime * SpeedOfDay;
        TimeOfDay %= 24;
        return TimeOfDay;
    }



    private void UpdateLighting(float timePercent)
    {
        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.fogColor = Preset.FogColor.Evaluate(timePercent);

        if(DirectionalLight != null)
        {
            DirectionalLight.color = Preset.DirectionalColor.Evaluate(timePercent);
            DirectionalLight.transform.localRotation = 
                Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170, 0));
            // Turn on sun light
            DirectionalLight.enabled = (timePercent >= 0.25f && timePercent <= 0.75f); // from 6h to 18h
        }
        if (MoonLight != null)
        {
            MoonLight.transform.localRotation =
                Quaternion.Euler(new Vector3((timePercent * 360f) + 90f, 170, 0));

            // turn on moon light
            MoonLight.enabled = !(timePercent >= 0.25f && timePercent <= 0.75f); // from 18h to 6h
        }



    }



    /// <summary>
    /// set up DirectionalLight
    /// </summary>
    private void OnValidate()
    {
        if(DirectionalLight != null) 
        {
            return;
        }
        if(RenderSettings.sun != null)
        {
            DirectionalLight = RenderSettings.sun;
        }
        else
        {
            Light[] lights = GameObject.FindObjectsOfType<Light>(); // find all lights in sence and get first LightType.Directional 
            foreach (Light light in lights)
            {
                if(light.type == LightType.Directional)
                {
                    DirectionalLight = light;
                }
            }
        }
        
    }

    void UpdateSkybox(float timePercent)
    {
        bool isDayTime = timePercent >= 0.25f && timePercent <= 0.75f; // 6h - 18h

        if (RenderSettings.skybox != (isDayTime ? DaySkybox : NightSkybox))
        {
            RenderSettings.skybox = isDayTime ? DaySkybox : NightSkybox;
            DynamicGI.UpdateEnvironment(); // update lighting environment
        }
    }


}
