using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class CloudSunLightTest : MonoBehaviour
{
    [Range(0f, 1f)]
    public float powderIntensity = 1.0f; // Giá trị mong muốn

    void Start()
    {
        // Lấy Volume hiện tại trong scene
        var volume = FindObjectOfType<Volume>();
        if (volume != null && volume.profile != null)
        {
            // Tìm VolumetricClouds trong Volume Profile
            if (volume.profile.TryGet<VolumetricClouds>(out var clouds))
            {
                clouds.powderEffectIntensity.value = powderIntensity;
            }
            Debug.Log($"the powderEffectIntensity value now{clouds.powderEffectIntensity.value}");
        }
         
    }
}
