using UnityEngine;
[System.Serializable]
[CreateAssetMenu(fileName = "Lighting Preset", menuName = "Scriptable/Lighting Preset", order = 1)] 
// Create menu rightclick -> Scriptable -> Lighting Preset 
public class LightingPreset : ScriptableObject
{
    public Gradient AmbientColor;
    public Gradient DirectionalColor;
    public Gradient FogColor;
}
