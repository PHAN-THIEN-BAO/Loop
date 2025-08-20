using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System.IO;

public class CreateCloudMap : MonoBehaviour
{
    [SerializeField] private Color bottomColor = new Color(0f, 0.4f, 0.8f);
    [SerializeField] private int cloudMapWidth = 1024;
    [SerializeField] private int cloudMapHeight = 1024;
    [SerializeField] private string cloudMapFileName = "CloudMap.exr";

    private Texture2D _cloudMapTexture;
    private Texture2D _cloudLutTexture; // Thêm biến này để lưu Cloud LUT

    public void CreateCloudMapFuntion()
    {
        _cloudMapTexture = new Texture2D(cloudMapWidth, cloudMapHeight, TextureFormat.RGBAFloat, false);
        _cloudMapTexture.name = "Cloud Coverage Map";
        _cloudMapTexture.wrapMode = TextureWrapMode.Repeat;
        _cloudMapTexture.filterMode = FilterMode.Bilinear;

        // Tạo mẫu mây - đây là một ví dụ đơn giản
        for (int y = 0; y < cloudMapHeight; y++)
        {
            for (int x = 0; x < cloudMapWidth; x++)
            {
                // Tính toán giá trị cho từng kênh
                // R: Coverage (0-1) - độ phủ của mây
                // G: Rain (0-1) - độ mưa, tác động đến độ dày
                // B: Type (0-1) - loại mây, từ 0 (cumulus) đến 1 (cirrus)

                // Tạo mây ngẫu nhiên với Perlin noise
                float coverage = Mathf.PerlinNoise(x * 0.01f, y * 0.01f);
                float rain = Mathf.PerlinNoise(x * 0.02f + 100, y * 0.02f + 100) * 0.5f;
                float type = Mathf.PerlinNoise(x * 0.005f + 200, y * 0.005f + 200);

                // Điều chỉnh coverage để có các khu vực trống
                coverage = Mathf.Pow(coverage, 1.5f);

                Color pixelColor = new Color(coverage, rain, type, 1.0f);
                _cloudMapTexture.SetPixel(x, y, pixelColor);
            }
        }

        _cloudMapTexture.Apply();
        Debug.Log("Đã tạo Cloud Map texture thành công.");
    }

    // Thêm phương thức tạo Cloud LUT
    public void CreateCloudLut()
    {
        int textureHeight = 64;
        _cloudLutTexture = new Texture2D(1, textureHeight, TextureFormat.RGBAFloat, false);
        _cloudLutTexture.name = "Cloud Color Gradient LUT";
        _cloudLutTexture.wrapMode = TextureWrapMode.Clamp;
        _cloudLutTexture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < textureHeight; y++)
        {
            float t = (float)y / (textureHeight - 1);

            // Kênh R: Profile Coverage - điều chỉnh độ phủ của mây theo chiều cao
            float coverage = Mathf.Lerp(0.8f, 0.4f, t);

            // Kênh G: Erosion - độ xói mòn của mây, ảnh hưởng đến màu sắc
            float erosion = Mathf.Lerp(0.3f, 0.7f, t);

            // Kênh B: Ambient Occlusion - độ tối do che khuất ánh sáng xung quanh
            float ambientOcclusion = Mathf.Lerp(0.1f, 0.5f, t);

            Color pixelColor = new Color(coverage, erosion, ambientOcclusion, 1.0f);
            _cloudLutTexture.SetPixel(0, y, pixelColor);
        }

        _cloudLutTexture.Apply();
        Debug.Log("Đã tạo Cloud LUT texture thành công.");
    }

    public void SaveCloudMapToAssets()
    {
        if (_cloudMapTexture == null)
        {
            CreateCloudMapFuntion();
        }

#if UNITY_EDITOR
        byte[] bytes = _cloudMapTexture.EncodeToEXR(Texture2D.EXRFlags.CompressZIP);

        string path = UnityEditor.EditorUtility.SaveFilePanelInProject(
            "Lưu Cloud Map Texture",
            cloudMapFileName,
            "exr",
            "Chọn nơi lưu Cloud Map texture"
        );

        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllBytes(path, bytes);
            UnityEditor.AssetDatabase.Refresh();

            // Cập nhật import settings
            UnityEditor.TextureImporter importer = UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
            if (importer != null)
            {
                importer.textureType = UnityEditor.TextureImporterType.Default;
                importer.sRGBTexture = false;
                importer.mipmapEnabled = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Repeat;

                UnityEditor.EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }

            Debug.Log("Đã lưu Cloud Map texture tại: " + path);
            UnityEditor.Selection.activeObject = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
#endif
    }

    public void SaveCloudLutToAssets()
    {
        if (_cloudLutTexture == null)
        {
            CreateCloudLut();
        }

#if UNITY_EDITOR
        byte[] bytes = _cloudLutTexture.EncodeToEXR(Texture2D.EXRFlags.CompressZIP);

        string path = UnityEditor.EditorUtility.SaveFilePanelInProject(
            "Lưu Cloud LUT Texture",
            "CloudLut.exr",
            "exr",
            "Chọn nơi lưu Cloud LUT texture"
        );

        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllBytes(path, bytes);
            UnityEditor.AssetDatabase.Refresh();

            // Cập nhật import settings
            UnityEditor.TextureImporter importer = UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
            if (importer != null)
            {
                importer.textureType = UnityEditor.TextureImporterType.Default;
                importer.sRGBTexture = false;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;

                UnityEditor.EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }

            Debug.Log("Đã lưu Cloud LUT texture tại: " + path);
            UnityEditor.Selection.activeObject = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
#endif
    }

    // Cập nhật phương thức ApplyToVolume để áp dụng cả Cloud Map
    public void ApplyToVolume()
    {
        // Tạo Cloud LUT nếu chưa có
        if (_cloudLutTexture == null)
        {
            CreateCloudLut();
        }

        // Tạo Cloud Map nếu chưa có
        if (_cloudMapTexture == null)
        {
            CreateCloudMapFuntion();
        }

        string savedLutPath = "";
        string savedCloudMapPath = "";
        Texture2D savedLutTexture = null;
        Texture2D savedCloudMapTexture = null;

#if UNITY_EDITOR
        // Lưu Cloud LUT vào project
        byte[] lutBytes = _cloudLutTexture.EncodeToEXR(Texture2D.EXRFlags.CompressZIP);
        string tempLutPath = "Assets/Temp_CloudLUT.exr";
        File.WriteAllBytes(tempLutPath, lutBytes);

        // Lưu Cloud Map vào project
        byte[] cloudMapBytes = _cloudMapTexture.EncodeToEXR(Texture2D.EXRFlags.CompressZIP);
        string tempCloudMapPath = "Assets/Temp_CloudMap.exr";
        File.WriteAllBytes(tempCloudMapPath, cloudMapBytes);

        // Cập nhật AssetDatabase
        UnityEditor.AssetDatabase.Refresh();

        // Cập nhật import settings cho Cloud LUT
        UnityEditor.TextureImporter lutImporter = UnityEditor.AssetImporter.GetAtPath(tempLutPath) as UnityEditor.TextureImporter;
        if (lutImporter != null)
        {
            lutImporter.textureType = UnityEditor.TextureImporterType.Default;
            lutImporter.sRGBTexture = false;
            lutImporter.mipmapEnabled = false;
            lutImporter.filterMode = FilterMode.Bilinear;
            lutImporter.wrapMode = TextureWrapMode.Clamp;
            UnityEditor.EditorUtility.SetDirty(lutImporter);
            lutImporter.SaveAndReimport();
        }

        // Cập nhật import settings cho Cloud Map
        UnityEditor.TextureImporter cloudMapImporter = UnityEditor.AssetImporter.GetAtPath(tempCloudMapPath) as UnityEditor.TextureImporter;
        if (cloudMapImporter != null)
        {
            cloudMapImporter.textureType = UnityEditor.TextureImporterType.Default;
            cloudMapImporter.sRGBTexture = false;
            cloudMapImporter.mipmapEnabled = true;
            cloudMapImporter.filterMode = FilterMode.Bilinear;
            cloudMapImporter.wrapMode = TextureWrapMode.Repeat;
            UnityEditor.EditorUtility.SetDirty(cloudMapImporter);
            cloudMapImporter.SaveAndReimport();
        }

        // Lấy textures đã import
        savedLutTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(tempLutPath);
        savedCloudMapTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(tempCloudMapPath);
        savedLutPath = tempLutPath;
        savedCloudMapPath = tempCloudMapPath;
#endif

        // Áp dụng cả Cloud Map và Cloud LUT vào Volume
        Volume volume = FindObjectOfType<Volume>();
        if (volume != null && volume.profile != null)
        {
            // Tạo một bản sao của profile
            volume.profile = Object.Instantiate(volume.profile);

            if (volume.profile.TryGet<VolumetricClouds>(out var clouds))
            {
                try
                {
#if UNITY_EDITOR
                    clouds.enable.value = true;
                    clouds.cloudControl.value = VolumetricClouds.CloudControl.Manual;

                    // Áp dụng Cloud LUT
                    if (savedLutTexture != null)
                    {
                        clouds.cloudLut.value = savedLutTexture;
                    }

                    // Áp dụng Cloud Map
                    if (savedCloudMapTexture != null)
                    {
                        clouds.cloudMap.value = savedCloudMapTexture;
                    }

                    // Các thiết lập khác
                    clouds.scatteringTint.value = bottomColor;
                    clouds.powderEffectIntensity.value = 1.0f;
                    clouds.multiScattering.value = 0.7f;

                    Debug.Log("Đã áp dụng Cloud LUT và Cloud Map vào Volume thành công.");
#endif
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Lỗi khi áp dụng texture: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }
    }
}

#if UNITY_EDITOR
// Custom Editor để dễ dàng sử dụng
[UnityEditor.CustomEditor(typeof(CreateCloudMap))]
public class CreateCloudMapEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CreateCloudMap cloudMapCreator = (CreateCloudMap)target;

        GUILayout.Space(10);
        GUILayout.Label("Cloud LUT Actions", UnityEditor.EditorStyles.boldLabel);

        if (GUILayout.Button("Tạo Cloud LUT"))
        {
            cloudMapCreator.CreateCloudLut();
        }

        if (GUILayout.Button("Lưu Cloud LUT vào Assets"))
        {
            cloudMapCreator.SaveCloudLutToAssets();
        }

        GUILayout.Space(10);
        GUILayout.Label("Cloud Map Actions", UnityEditor.EditorStyles.boldLabel);

        if (GUILayout.Button("Tạo Cloud Map"))
        {
            cloudMapCreator.CreateCloudMapFuntion();
        }

        if (GUILayout.Button("Lưu Cloud Map vào Assets"))
        {
            cloudMapCreator.SaveCloudMapToAssets();
        }

        GUILayout.Space(10);
        GUILayout.Label("Apply Actions", UnityEditor.EditorStyles.boldLabel);

        if (GUILayout.Button("Áp dụng vào Volume"))
        {
            cloudMapCreator.ApplyToVolume();
        }

        GUILayout.Space(5);
        if (GUILayout.Button("Tạo, lưu và áp dụng tất cả", GUILayout.Height(30)))
        {
            cloudMapCreator.CreateCloudLut();
            cloudMapCreator.CreateCloudMapFuntion();
            cloudMapCreator.ApplyToVolume();
        }
    }
}
#endif
