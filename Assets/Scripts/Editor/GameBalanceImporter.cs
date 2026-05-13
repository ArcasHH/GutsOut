#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading;

public class CSVToGameBalanceImporter : EditorWindow
{
    private string csvFilePath = "";
    private string outputPath = "Assets/Resources/BalanceData";
    
    [MenuItem("Tools/Import CSV to Game Balance")]
    public static void ShowWindow()
    {
        GetWindow<CSVToGameBalanceImporter>("CSV Importer");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Import CSV to GameBalance ScriptableObjects", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("CSV File Path:", csvFilePath);
        if (GUILayout.Button("Select CSV File"))
        {
            csvFilePath = EditorUtility.OpenFilePanel("Select CSV File", "", "csv");
        }
        
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Output Path:", outputPath);
        if (GUILayout.Button("Select Output Folder"))
        {
            string path = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.Contains(Application.dataPath))
                {
                    outputPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Please select a folder inside the Assets directory", "OK");
                }
            }
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Import CSV", GUILayout.Height(30)))
        {
            if (string.IsNullOrEmpty(csvFilePath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a CSV file first", "OK");
                return;
            }
            
            ImportCSV();
        }
    }
    
    private void ImportCSV()
    {
        try
        {
            // Устанавливаем культуру для парсинга чисел с запятой
            Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
            
            string[] lines = File.ReadAllLines(csvFilePath);
            if (lines.Length < 2)
            {
                EditorUtility.DisplayDialog("Error", "CSV file is empty or has no data", "OK");
                return;
            }
            
            // Парсим заголовки (сложности)
            string[] headers = ParseCSVLine(lines[0]);
            // Пропускаем Category, Type, остальные - это сложности
            string[] difficulties = new string[headers.Length - 2];
            for (int i = 2; i < headers.Length; i++)
            {
                difficulties[i - 2] = headers[i].Trim();
            }
            
            // Создаем объекты для каждой сложности
            GameBalance[] balances = new GameBalance[difficulties.Length];
            for (int i = 0; i < difficulties.Length; i++)
            {
                balances[i] = ScriptableObject.CreateInstance<GameBalance>();
                balances[i].difficultyName = difficulties[i];
            }
            
            // Парсим данные
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                
                string[] values = ParseCSVLine(lines[i]);
                if (values.Length < 2) continue;
                
                string category = values[0].Trim();
                string type = values[1].Trim();
                
                // Для каждой сложности
                for (int j = 0; j < difficulties.Length; j++)
                {
                    if (j + 2 >= values.Length) continue;
                    
                    string value = values[j + 2].Trim();
                    float floatValue = ParseFloatValue(value);
                    int intValue = Mathf.RoundToInt(floatValue);
                    
                    ApplyValueToBalance(balances[j], category, type, floatValue, intValue);
                }
            }
            
            // Сохраняем ассеты
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            
            foreach (var balance in balances)
            {
                string fileName = $"GameBalance_{balance.difficultyName}.asset";
                string fullPath = Path.Combine(outputPath, fileName);
                
                // Удаляем старый ассет если существует
                if (File.Exists(fullPath))
                {
                    AssetDatabase.DeleteAsset(fullPath);
                }
                
                AssetDatabase.CreateAsset(balance, fullPath);
                EditorUtility.SetDirty(balance);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Success", $"Successfully imported {difficulties.Length} GameBalance assets to {outputPath}", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to import CSV: {e.Message}\n\n{e.StackTrace}", "OK");
            Debug.LogError($"CSV Import Error: {e.Message}\n{e.StackTrace}");
        }
    }
    
    private string[] ParseCSVLine(string line)
    {
        var result = new System.Collections.Generic.List<string>();
        bool inQuotes = false;
        string currentField = "";
        
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField);
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }
        
        result.Add(currentField);
        return result.ToArray();
    }
    
    private float ParseFloatValue(string value)
    {
        if (string.IsNullOrEmpty(value)) return 0f;
        
        // Заменяем запятую на точку для парсинга
        value = value.Replace(',', '.');
        
        if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float result))
        {
            return result;
        }
        
        return 0f;
    }
    
    private void ApplyValueToBalance(GameBalance balance, string category, string type, float floatValue, int intValue)
    {
        switch (category)
        {
            case "Base":
                switch (type)
                {
                    case "Cursed": balance.cursedBase = floatValue; break;
                    case "Bad": balance.badBase = floatValue; break;
                    case "Ordinary": balance.ordinaryBase = floatValue; break;
                    case "Good": balance.goodBase = floatValue; break;
                    case "Rare": balance.rareBase = floatValue; break;
                    case "Epic": balance.epicBase = floatValue; break;
                    case "Legendary": balance.legendaryBase = floatValue; break;
                }
                break;
                
            case "Mul":
                switch (type)
                {
                    case "Cursed": balance.cursedMul = floatValue; break;
                    case "Bad": balance.badMul = floatValue; break;
                    case "Ordinary": balance.ordinaryMul = floatValue; break;
                    case "Good": balance.goodMul = floatValue; break;
                    case "Rare": balance.rareMul = floatValue; break;
                    case "Epic": balance.epicMul = floatValue; break;
                    case "Legendary": balance.legendaryMul = floatValue; break;
                }
                break;
                
            case "PlayerNeeds":
                switch (type)
                {
                    case "StartReq": balance.startRec = intValue; break;
                    case "MulDay": balance.mulDay = floatValue; break;
                    case "Pow": balance.pow = floatValue; break;
                    case "DivLowReq": balance.divLowReq = floatValue; break;
                    case "baseReqRelease": balance.baseReqRelease = intValue; break;
                    case "typeReqRelease": balance.typeReqRelease = intValue; break;
                }
                break;
                
            case "KarmaRewards":
                switch (type)
                {
                    case "RewardForOne": balance.rewardForOne = intValue; break;
                    case "RewardForTwo": balance.rewardForTwo = intValue; break;
                    case "RewardForThree": balance.rewardForThree = intValue; break;
                }
                break;
                
            case "Knife":
                switch (type)
                {
                    case "BaseCost": balance.knifeBaseCost = intValue; break;
                    case "CostIncrease": balance.knifeCostIncrease = intValue; break;
                }
                break;
        }
    }
}
#endif