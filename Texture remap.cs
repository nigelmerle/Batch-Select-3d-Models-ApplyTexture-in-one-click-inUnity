using UnityEngine;
using UnityEditor;
using System.IO;

public class BatchMaterialSettings : EditorWindow
{
    [MenuItem("Tools/Batch Material Settings")]
    public static void ShowWindow()
    {
        GetWindow<BatchMaterialSettings>("Batch Material Settings");
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch Material Settings", EditorStyles.boldLabel);

        if (GUILayout.Button("Apply Settings to Selected Models"))
        {
            ApplySettingsToSelectedModels();
        }
    }

    private void ApplySettingsToSelectedModels()
    {
        // Get selected objects
        Object[] selectedObjects = Selection.objects;

        foreach (Object obj in selectedObjects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Debug.LogWarning($"Selected object {obj.name} is not a valid asset.");
                continue;
            }

            // Check if the selected object is a model
            var modelImporter = AssetImporter.GetAtPath(path) as ModelImporter;
            if (modelImporter != null)
            {
                // Set material import settings
                modelImporter.materialImportMode = ModelImporterMaterialImportMode.ImportStandard; // Set to Standard (Legacy)
                modelImporter.materialLocation = ModelImporterMaterialLocation.External; // Use External Materials (Legacy)
                modelImporter.materialName = ModelImporterMaterialName.BasedOnTextureName; // By Base Texture Name
                modelImporter.materialSearch = ModelImporterMaterialSearch.RecursiveUp; // Recursive-Up

                // Save and reimport the model to apply changes
                modelImporter.SaveAndReimport();
                Debug.Log($"Updated material settings for model: {obj.name}");

                // Load the materials from the model's path
                string[] materialPaths = AssetDatabase.FindAssets("t:Material", new[] { Path.GetDirectoryName(path) });
                foreach (string materialPath in materialPaths)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(materialPath);
                    Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                    if (material != null)
                    {
                        // Set sRGB Albedo Colors
                        if (material.HasProperty("_MainTex"))
                        {
                            material.SetInt("_SRGBTexture", 1); // Enable sRGB Albedo Colors
                            EditorUtility.SetDirty(material); // Mark the material as dirty to ensure changes are saved
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Selected object {obj.name} is not a model.");
            }
        }

        // Refresh the Asset Database
        AssetDatabase.Refresh();
    }
}