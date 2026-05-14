using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class GameOrganImporter : EditorWindow
{
    [MenuItem("Tools/Import Game Organ Data")]

    public static void ShowWindow()
    {
        GetWindow<GameOrganImporter>("Game Organ Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Import GameOrgan from CSV", EditorStyles.boldLabel);

        if (GUILayout.Button("Select CSV and Import", GUILayout.Height(30)))
        {
            string path = EditorUtility.OpenFilePanel("Select CSV file", "", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                ImportFromCSV(path);
            }
        }
    }

    private static void ImportFromCSV(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length < 2)
        {
            Debug.LogError("CSV file is empty or has no data rows!");
            return;
        }

        int importedCount = 0;
        int errorCount = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] fields = line.Split(',');

            if (fields.Length < 6)
            {
                Debug.LogWarning($"Line {i + 1}: Expected 6 fields, got {fields.Length}. Skipping.");
                errorCount++;
                continue;
            }

            try
            {
                if (!System.Enum.TryParse<ObjectType>(fields[0].Trim(), true, out ObjectType objType))
                {
                    Debug.LogWarning($"Line {i + 1}: Invalid ObjectType '{fields[0]}'. Skipping.");
                    errorCount++;
                    continue;
                }

                if (!System.Enum.TryParse<CategoryType>(fields[1].Trim(), true, out CategoryType categoryType))
                {
                    Debug.LogWarning($"Line {i + 1}: Invalid CategoryType '{fields[1]}'. Skipping.");
                    errorCount++;
                    continue;
                }

                if (!System.Enum.TryParse<QualityType>(fields[2].Trim(), true, out QualityType qualityType))
                {
                    Debug.LogWarning($"Line {i + 1}: Invalid QualityType '{fields[2]}'. Skipping.");
                    errorCount++;
                    continue;
                }

                if (!int.TryParse(fields[3].Trim(), out int mind))
                {
                    Debug.LogWarning($"Line {i + 1}: Invalid mind value '{fields[3]}'. Skipping.");
                    errorCount++;
                    continue;
                }

                if (!int.TryParse(fields[4].Trim(), out int soul))
                {
                    Debug.LogWarning($"Line {i + 1}: Invalid soul value '{fields[4]}'. Skipping.");
                    errorCount++;
                    continue;
                }

                if (!int.TryParse(fields[5].Trim(), out int body))
                {
                    Debug.LogWarning($"Line {i + 1}: Invalid body value '{fields[5]}'. Skipping.");
                    errorCount++;
                    continue;
                }

                CreateGameOrganAsset(objType, categoryType, qualityType, mind, soul, body);
                importedCount++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Line {i + 1}: Unexpected error - {ex.Message}");
                errorCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Import completed! Imported: {importedCount}, Errors: {errorCount}");
    }

    private static void CreateGameOrganAsset(ObjectType objType, CategoryType categoryType,
                                             QualityType qualityType, int mind, int soul, int body)
    {
        GameOrgan organ = ScriptableObject.CreateInstance<GameOrgan>();
        organ.obj_type = objType;
        organ.category_type = categoryType;
        organ.quality_type = qualityType;
        organ.mind = mind;
        organ.soul = soul;
        organ.body = body;

        string baseFolder = "Assets/Resources/ScriptableOrgans";

        string typeFolder = Path.Combine(baseFolder, objType.ToString());
        EnsureFolderExists(typeFolder);

        string fileName = $"{organ.ItemName}.asset";
        string fullPath = Path.Combine(typeFolder, fileName);

        if (AssetDatabase.LoadAssetAtPath<GameOrgan>(fullPath) != null)
        {
            Debug.LogWarning($"Asset {fullPath} already exists, replacing.");
            AssetDatabase.DeleteAsset(fullPath);
        }

        AssetDatabase.CreateAsset(organ, fullPath);
        Debug.Log($"Created: {fullPath}");
    }

    private static void EnsureFolderExists(string folderPath)
    {
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            string parentPath = Path.GetDirectoryName(folderPath).Replace('\\', '/');

            if (!string.IsNullOrEmpty(parentPath) && parentPath != "Assets" && !AssetDatabase.IsValidFolder(parentPath))
            {
                EnsureFolderExists(parentPath);
            }

            string folderName = Path.GetFileName(folderPath);
            string parentForCreate = string.IsNullOrEmpty(parentPath) ? "Assets" : parentPath;
            AssetDatabase.CreateFolder(parentForCreate, folderName);
        }
    }
}