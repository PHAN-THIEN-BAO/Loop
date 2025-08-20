using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System.IO;

public class CloudLutSaver : MonoBehaviour
{
    [SerializeField] private Color topColor = Color.white;
    [SerializeField] private Color bottomColor = new Color(0f, 0.4f, 0.8f); // Màu xanh biển
    [SerializeField] private int textureHeight = 64;
    [SerializeField] private string defaultFileName = "CloudLut.png";

    private Texture2D _cloudLutTexture;

    public void CreateCloudLut()
    {
        // Tạo texture LUT
        _cloudLutTexture = new Texture2D(1, textureHeight, TextureFormat.RGBAFloat, false);
        _cloudLutTexture.name = "Cloud Color Gradient LUT";
        _cloudLutTexture.wrapMode = TextureWrapMode.Clamp;
        _cloudLutTexture.filterMode = FilterMode.Bilinear;

        // Tạo gradient từ dưới lên trên
        for (int y = 0; y < textureHeight; y++)
        {
            float t = (float)y / (textureHeight - 1);

            // Kênh R: Profile Coverage - điều chỉnh độ phủ của mây theo chiều cao
            float coverage = Mathf.Lerp(0.8f, 0.4f, t);

            // Kênh G: Erosion - độ xói mòn của mây, ảnh hưởng đến màu sắc
            float erosion = Mathf.Lerp(0.3f, 0.7f, t);

            // Kênh B: Ambient Occlusion - độ tối do che khuất ánh sáng xung quanh
            float ambientOcclusion = Mathf.Lerp(0.1f, 0.5f, t);

            // Tạo pixel với các giá trị đã tính
            Color pixelColor = new Color(coverage, erosion, ambientOcclusion, 1.0f);
            _cloudLutTexture.SetPixel(0, y, pixelColor);
        }

        _cloudLutTexture.Apply();
        Debug.Log("Đã tạo Cloud LUT texture thành công.");
    }

    // Phương thức lưu texture vào Assets hoặc đường dẫn khác
    public void SaveTextureToAssets()
    {
        if (_cloudLutTexture == null)
        {
            CreateCloudLut();
        }

#if UNITY_EDITOR
        // Chuyển đổi texture sang định dạng PNG hoặc EXR
        byte[] bytes = _cloudLutTexture.EncodeToEXR(Texture2D.EXRFlags.CompressZIP);

        // Đường dẫn trong thư mục Assets
        string path = UnityEditor.EditorUtility.SaveFilePanelInProject(
            "Lưu Cloud LUT Texture",
            defaultFileName,
            "exr",
            "Chọn nơi lưu Cloud LUT texture"
        );

        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllBytes(path, bytes);
            UnityEditor.AssetDatabase.Refresh();

            // Cập nhật import settings để texture được xử lý đúng
            UnityEditor.TextureImporter importer = UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
            if (importer != null)
            {
                importer.textureType = UnityEditor.TextureImporterType.Default;
                importer.sRGBTexture = false; // Quan trọng để dữ liệu không bị thay đổi màu
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;

                UnityEditor.EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }

            Debug.Log("Đã lưu Cloud LUT texture tại: " + path);

            // Hiển thị texture trong project window
            UnityEditor.Selection.activeObject = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
#else
        Debug.LogWarning("Tính năng lưu texture chỉ hoạt động trong Unity Editor");
#endif
    }

    // Phương thức lưu texture ra ngoài đường dẫn tùy chọn
    public void SaveTextureToFile()
    {
        if (_cloudLutTexture == null)
        {
            CreateCloudLut();
        }

#if UNITY_EDITOR
        byte[] bytes = _cloudLutTexture.EncodeToPNG();

        string path = UnityEditor.EditorUtility.SaveFilePanel(
            "Lưu Cloud LUT Texture",
            "",
            defaultFileName,
            "png"
        );

        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllBytes(path, bytes);
            Debug.Log("Đã lưu Cloud LUT texture tại: " + path);
        }
#else
        Debug.LogWarning("Tính năng lưu texture chỉ hoạt động trong Unity Editor");
#endif
    }

    // Phương thức vừa tạo, vừa lưu, vừa áp dụng vào volumetric clouds
    public void CreateSaveAndApply()
    {
        CreateCloudLut();
        SaveTextureToAssets();
        ApplyToVolume();
    }

    // Áp dụng texture vào Volume
    // Áp dụng texture vào Volume
    public void ApplyToVolume()
    {
        if (_cloudLutTexture == null)
        {
            CreateCloudLut();
        }

        // Trước tiên, lưu texture
        string savedPath = "";

#if UNITY_EDITOR
        byte[] bytes = _cloudLutTexture.EncodeToEXR(Texture2D.EXRFlags.CompressZIP);

        // Lưu tạm vào thư mục project
        string tempPath = "Assets/Temp_CloudLUT.exr";
        File.WriteAllBytes(tempPath, bytes);
        UnityEditor.AssetDatabase.Refresh();

        // Cập nhật import settings
        UnityEditor.TextureImporter importer = UnityEditor.AssetImporter.GetAtPath(tempPath) as UnityEditor.TextureImporter;
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

        // Lấy texture đã import
        Texture2D savedTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(tempPath);
        savedPath = tempPath;
#endif

        Volume volume = FindObjectOfType<Volume>();
        if (volume != null && volume.profile != null)
        {
            if (volume != null && volume.profile != null)
            {
                volume.profile = Object.Instantiate(volume.profile);

            }

            if (volume.profile.TryGet<VolumetricClouds>(out var clouds))
            {
                try
                {
#if UNITY_EDITOR
                    if (savedTexture != null)
                    {
                        clouds.enable.value = true;
                        clouds.cloudControl.value = VolumetricClouds.CloudControl.Manual;
                        clouds.cloudLut.value = savedTexture;
                        clouds.scatteringTint.value = bottomColor;
                        clouds.powderEffectIntensity.value = 1.0f;
                        clouds.multiScattering.value = 0.7f;

                        Debug.Log("Đã áp dụng Cloud LUT texture vào Volume từ file: " + savedPath);
                    }
                    else
#endif
                    {
                        Debug.LogError("Không thể tạo texture từ asset để gán vào Volume");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Lỗi khi áp dụng LUT: {ex.Message}\n{ex.StackTrace}");
                }
            }
            else
            {
                Debug.LogWarning("Không tìm thấy component VolumetricClouds trong Volume Profile");
            }
        }
        else
        {
            Debug.LogWarning("Không tìm thấy Volume component trong scene");
        }

#if UNITY_EDITOR
        // Xóa file tạm sau khi đã áp dụng (tùy chọn)
        if (File.Exists(tempPath))
        {
            // Nếu muốn giữ lại file, hãy comment dòng dưới đây
            //UnityEditor.AssetDatabase.DeleteAsset(tempPath);
        }
#endif
    }



}

#if UNITY_EDITOR
// Custom Editor để dễ dàng sử dụng
[UnityEditor.CustomEditor(typeof(CloudLutSaver))]
public class CloudLutSaverEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CloudLutSaver cloudLutSaver = (CloudLutSaver)target;

        GUILayout.Space(10);
        GUILayout.Label("Actions", UnityEditor.EditorStyles.boldLabel);

        if (GUILayout.Button("Chỉ tạo Cloud LUT"))
        {
            cloudLutSaver.CreateCloudLut();
        }

        if (GUILayout.Button("Lưu vào Assets"))
        {
            cloudLutSaver.SaveTextureToAssets();
        }

        if (GUILayout.Button("Lưu vào File"))
        {
            cloudLutSaver.SaveTextureToFile();
        }

        if (GUILayout.Button("Áp dụng vào Volume"))
        {
            cloudLutSaver.ApplyToVolume();
        }

        GUILayout.Space(5);
        if (GUILayout.Button("Tạo, lưu và áp dụng", GUILayout.Height(30)))
        {
            cloudLutSaver.CreateSaveAndApply();
        }
    }
}
#endif
