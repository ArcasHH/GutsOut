using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

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
    private string basePath = "Assets/Resources/Organs";
    private Vector2 scrollPosition;

    [MenuItem("Tools/Game Organ Generator")]
    public static void ShowWindow()
    {
        GetWindow<GameOrganGenerator>("Organ Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Organ Generator", EditorStyles.boldLabel);

        // Путь сохранения
        EditorGUILayout.LabelField("Save Path:", basePath);
        if (GUILayout.Button("Refresh Path"))
        {
            EnsureDirectoriesExist();
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
        }
        if (GUILayout.Button("Clear List"))
        {
            organs.Clear();
        }
        EditorGUILayout.EndHorizontal();

        // Таблица
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.BeginVertical("box");

        // Заголовки
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Name", GUILayout.Width(150));
        GUILayout.Label("Type", GUILayout.Width(80));
        GUILayout.Label("Quality", GUILayout.Width(80));
        GUILayout.Label("Mind", GUILayout.Width(50));
        GUILayout.Label("Soul", GUILayout.Width(50));
        GUILayout.Label("Body", GUILayout.Width(50));
        GUILayout.Label("", GUILayout.Width(30));
        EditorGUILayout.EndHorizontal();

        // Строки данных
        for (int i = 0; i < organs.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            organs[i].name = EditorGUILayout.TextField(organs[i].name, GUILayout.Width(150));
            organs[i].objType = (ObjectType)EditorGUILayout.EnumPopup(organs[i].objType, GUILayout.Width(80));
            organs[i].qualityType = (QualityType)EditorGUILayout.EnumPopup(organs[i].qualityType, GUILayout.Width(80));
            organs[i].categoryType = (CategoryType)EditorGUILayout.EnumPopup(organs[i].categoryType, GUILayout.Width(80));
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
        EditorGUILayout.LabelField($"Total Organs: {organs.Count}");
    }

    private void GenerateAllAssets()
    {
        EnsureDirectoriesExist();

        int created = 0;
        int skipped = 0;

        foreach (var organData in organs)
        {
            if (string.IsNullOrEmpty(organData.name))
            {
                Debug.LogWarning("Skipping organ with empty name");
                skipped++;
                continue;
            }

            CreateOrganAsset(organData);
            created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Generated {created} organs. Skipped {skipped}");
        EditorUtility.DisplayDialog("Complete", $"Generated {created} organs!", "OK");
    }

    private void CreateOrganAsset(OrganData data)
    {
        string typeFolder = $"{basePath}/{data.objType}";
        string fullPath = $"{typeFolder}/{data.name}.asset";

        // Проверяем, не существует ли уже
        if (File.Exists(fullPath))
        {
            Debug.LogWarning($"Asset already exists: {fullPath}");
            return;
        }

        // Создаем ассет
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