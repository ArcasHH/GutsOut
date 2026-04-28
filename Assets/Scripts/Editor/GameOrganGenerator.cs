using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class GameOrganGenerator : EditorWindow
{
    [System.Serializable]
    public class OrganData
    {
        public string name;
        public ObjectType objType;
        public CategoryType categoryType;
        public QualityType qualityType;
        public int mind;
        public int soul;
        public int body;
    }

    private List<OrganData> organs = new List<OrganData>();
    private string basePath = "Assets/Resources/ScriptableOrgans";
    private Vector2 scrollPosition;
    private Vector2 existingScrollPosition;
    private bool showExisting = true;
    private Dictionary<string, GameOrgan> existingOrgans = new Dictionary<string, GameOrgan>();

    [MenuItem("Tools/Game Organ Generator")]
    public static void ShowWindow()
    {
        GetWindow<GameOrganGenerator>("Organ Generator");
    }

    private void OnEnable()
    {
        RefreshExistingAssets();
    }

    private void OnGUI()
    {
        GUILayout.Label("Organ Generator", EditorStyles.boldLabel);

        // Путь сохранения
        EditorGUILayout.LabelField("Save Path:", basePath);
        if (GUILayout.Button("Refresh Path & Load Existing"))
        {
            EnsureDirectoriesExist();
            RefreshExistingAssets();
        }

        // Кнопки управления
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Organ"))
        {
            organs.Add(new OrganData() { name = "NewOrgan" });
        }
        if (GUILayout.Button("Generate All"))
        {
            GenerateAllAssets();
            RefreshExistingAssets(); // обновим список после генерации
        }
        if (GUILayout.Button("Clear List"))
        {
            organs.Clear();
        }
        if (GUILayout.Button("Refresh Existing"))
        {
            RefreshExistingAssets();
        }
        EditorGUILayout.EndHorizontal();

        // Разделитель
        EditorGUILayout.Space(10);

        // Блок существующих ассетов
        showExisting = EditorGUILayout.Foldout(showExisting, $"Existing Organs ({existingOrgans.Count})", true);
        if (showExisting)
        {
            existingScrollPosition = EditorGUILayout.BeginScrollView(existingScrollPosition, GUILayout.Height(250));
            DrawExistingAssets();
            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.Space(10);

        // Таблица для новых/редактируемых органов
        GUILayout.Label("Organs to Generate / Edit", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.BeginVertical("box");

        // Заголовки
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Name", GUILayout.Width(150));
        GUILayout.Label("Type", GUILayout.Width(80));
        GUILayout.Label("Category", GUILayout.Width(80));
        GUILayout.Label("Quality", GUILayout.Width(80));
        GUILayout.Label("Mind", GUILayout.Width(50));
        GUILayout.Label("Soul", GUILayout.Width(50));
        GUILayout.Label("Body", GUILayout.Width(50));
        GUILayout.Label("", GUILayout.Width(60));
        EditorGUILayout.EndHorizontal();

        // Строки данных
        for (int i = 0; i < organs.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            organs[i].name = EditorGUILayout.TextField(organs[i].name, GUILayout.Width(150));
            organs[i].objType = (ObjectType)EditorGUILayout.EnumPopup(organs[i].objType, GUILayout.Width(80));
            organs[i].categoryType = (CategoryType)EditorGUILayout.EnumPopup(organs[i].categoryType, GUILayout.Width(80));
            organs[i].qualityType = (QualityType)EditorGUILayout.EnumPopup(organs[i].qualityType, GUILayout.Width(80));
            organs[i].mind = EditorGUILayout.IntField(organs[i].mind, GUILayout.Width(50));
            organs[i].soul = EditorGUILayout.IntField(organs[i].soul, GUILayout.Width(50));
            organs[i].body = EditorGUILayout.IntField(organs[i].body, GUILayout.Width(50));

            if (GUILayout.Button("X", GUILayout.Width(30)))
            {
                organs.RemoveAt(i);
                i--;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

        // Статистика
        EditorGUILayout.LabelField($"Total Organs in list: {organs.Count}");
    }

    private void DrawExistingAssets()
    {
        if (existingOrgans.Count == 0)
        {
            GUILayout.Label("No existing assets found. Click 'Refresh Path & Load Existing'.");
            return;
        }

        // Группировка по типу органа
        var grouped = existingOrgans.Values.GroupBy(o => o.obj_type);
        foreach (var group in grouped)
        {
            EditorGUILayout.LabelField($"=== {group.Key} ===", EditorStyles.boldLabel);
            foreach (var organ in group)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(organ.name, GUILayout.Width(150));
                EditorGUILayout.LabelField(organ.category_type.ToString(), GUILayout.Width(80));
                EditorGUILayout.LabelField(organ.qulity_type.ToString(), GUILayout.Width(80));
                EditorGUILayout.LabelField($"M:{organ.mind} S:{organ.soul} B:{organ.body}", GUILayout.Width(120));

                // Кнопка загрузки в список для редактирования
                if (GUILayout.Button("Load to Edit", GUILayout.Width(80)))
                {
                    LoadOrganForEditing(organ);
                }

                // Кнопка удаления
                GUI.color = Color.red;
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    DeleteOrganAsset(organ);
                }
                GUI.color = Color.white;

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space(5);
        }
    }

    private void LoadOrganForEditing(GameOrgan organ)
    {
        // Проверим, нет ли уже такого имени в списке
        if (organs.Any(o => o.name == organ.name))
        {
            EditorUtility.DisplayDialog("Duplicate", $"Organ '{organ.name}' is already in the edit list.", "OK");
            return;
        }

        organs.Add(new OrganData
        {
            name = organ.name,
            objType = organ.obj_type,
            categoryType = organ.category_type,
            qualityType = organ.qulity_type,
            mind = organ.mind,
            soul = organ.soul,
            body = organ.body
        });
    }

    private void DeleteOrganAsset(GameOrgan organ)
    {
        if (!EditorUtility.DisplayDialog("Delete Asset",
            $"Are you sure you want to delete '{organ.name}'?\nThis action cannot be undone.",
            "Delete", "Cancel"))
            return;

        string assetPath = AssetDatabase.GetAssetPath(organ);
        if (!string.IsNullOrEmpty(assetPath))
        {
            AssetDatabase.DeleteAsset(assetPath);
            Debug.Log($"Deleted: {assetPath}");
            RefreshExistingAssets();
        }
    }

    private void RefreshExistingAssets()
    {
        existingOrgans.Clear();
        EnsureDirectoriesExist();

        foreach (ObjectType type in System.Enum.GetValues(typeof(ObjectType)))
        {
            string folderPath = $"{basePath}/{type}";
            if (!Directory.Exists(folderPath)) continue;

            string[] assetFiles = Directory.GetFiles(folderPath, "*.asset");
            foreach (string assetPath in assetFiles)
            {
                GameOrgan organ = AssetDatabase.LoadAssetAtPath<GameOrgan>(assetPath);
                if (organ != null && !existingOrgans.ContainsKey(organ.name))
                {
                    existingOrgans.Add(organ.name, organ);
                }
            }
        }
        // Сортируем для удобства
        existingOrgans = existingOrgans.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        Repaint();
    }

    private void GenerateAllAssets()
    {
        EnsureDirectoriesExist();

        int created = 0;
        int skipped = 0;
        int updated = 0;

        foreach (var organData in organs)
        {
            if (string.IsNullOrEmpty(organData.name))
            {
                Debug.LogWarning("Skipping organ with empty name");
                skipped++;
                continue;
            }

            string typeFolder = $"{basePath}/{organData.objType}";
            string fullPath = $"{typeFolder}/{organData.name}.asset";

            // Если ассет уже существует, предлагаем перезаписать
            if (File.Exists(fullPath))
            {
                if (EditorUtility.DisplayDialog("Asset exists",
                    $"'{organData.name}' already exists. Overwrite?",
                    "Yes", "No"))
                {
                    UpdateOrganAsset(organData, fullPath);
                    updated++;
                }
                else
                {
                    skipped++;
                }
                continue;
            }

            CreateOrganAsset(organData, fullPath);
            created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        RefreshExistingAssets();

        Debug.Log($"Generated: {created}, Updated: {updated}, Skipped: {skipped}");
        EditorUtility.DisplayDialog("Complete", $"Generated: {created}\nUpdated: {updated}\nSkipped: {skipped}", "OK");
    }

    private void CreateOrganAsset(OrganData data, string fullPath)
    {
        GameOrgan organ = CreateInstance<GameOrgan>();
        organ.name = data.name;
        organ.obj_type = data.objType;
        organ.qulity_type = data.qualityType;
        organ.category_type = data.categoryType;
        organ.mind = data.mind;
        organ.soul = data.soul;
        organ.body = data.body;

        AssetDatabase.CreateAsset(organ, fullPath);
        Debug.Log($"Created: {fullPath}");
    }

    private void UpdateOrganAsset(OrganData data, string fullPath)
    {
        GameOrgan organ = AssetDatabase.LoadAssetAtPath<GameOrgan>(fullPath);
        if (organ == null) return;

        organ.name = data.name;
        organ.obj_type = data.objType;
        organ.qulity_type = data.qualityType;
        organ.category_type = data.categoryType;
        organ.mind = data.mind;
        organ.soul = data.soul;
        organ.body = data.body;

        EditorUtility.SetDirty(organ);
        Debug.Log($"Updated: {fullPath}");
    }

    private void EnsureDirectoriesExist()
    {
        if (!Directory.Exists(basePath))
            Directory.CreateDirectory(basePath);

        foreach (ObjectType type in System.Enum.GetValues(typeof(ObjectType)))
        {
            string folderPath = $"{basePath}/{type}";
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }

        AssetDatabase.Refresh();
    }
}