using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ContainerRequirement : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private int minValue = 0;
    [SerializeField] private int maxValue = 5;
    
    [Header("UI")]
    [SerializeField] private TMP_Text statsText;      // Поле для отображения
    [SerializeField] private Button regenerateButton; // Кнопка "Перегенерировать"
    [SerializeField] private OrganStatsSummarizer summarizer; // Ссылка на сумматор статов
    
    // 🔑 Текущие требования (публичные, чтобы другие скрипты могли читать)
    public int RequiredMind { get; private set; }
    public int RequiredSoul { get; private set; }
    public int RequiredBody { get; private set; }
    
    // 🔑 Событие: вызывается, когда требования выполнены
    public static event Action<ContainerRequirement> OnRequirementsMet;
    public static event Action<ContainerRequirement> OnRequirementsChanged;

    private bool requirementsMet = false;

    private void Awake()
    {
        if (regenerateButton != null)
        {
            regenerateButton.onClick.AddListener(GenerateNewRequirements);
            regenerateButton.gameObject.SetActive(false); // Скрыть кнопку по умолчанию
        }
        
        if (summarizer == null) summarizer = GetComponent<OrganStatsSummarizer>();
        
        // Подписаться на изменения в инвентаре
        SlotController.OnInventoryChanged += CheckRequirements;
    }

    private void OnDestroy()
    {
        SlotController.OnInventoryChanged -= CheckRequirements;
        if (regenerateButton != null) regenerateButton.onClick.RemoveAllListeners();
    }

    private void Start() => GenerateNewRequirements();

    /// <summary>
    /// Генерирует новые случайные требования
    /// </summary>
    public void GenerateNewRequirements()
    {
        RequiredMind = UnityEngine.Random.Range(minValue, maxValue + 1);
        RequiredSoul = UnityEngine.Random.Range(minValue, maxValue + 1);
        RequiredBody = UnityEngine.Random.Range(minValue, maxValue + 1);
        
        UpdateDisplay();
        CheckRequirements();
        OnRequirementsChanged?.Invoke(this);
    }

    /// <summary>
    /// Проверяет, удовлетворяют ли текущие статы требованиям
    /// </summary>
    public void CheckRequirements()
    {
        if (summarizer == null) return;
        
        summarizer.CalculateStats();
        
        bool newlyMet = summarizer.TotalMind >= RequiredMind &&
                       summarizer.TotalSoul >= RequiredSoul &&
                       summarizer.TotalBody >= RequiredBody;

        if (newlyMet != requirementsMet)
        {
            requirementsMet = newlyMet;
            
            if (regenerateButton != null)
                regenerateButton.gameObject.SetActive(requirementsMet);
            
            if (requirementsMet)
                OnRequirementsMet?.Invoke(this);
        }
        
        UpdateDisplay();
    }

    /// <summary>
    /// Обновляет текст в UI
    /// </summary>
    private void UpdateDisplay()
    {
        if (statsText == null) return;
        
        string current = summarizer != null ? 
            $"{summarizer.TotalMind} {summarizer.TotalSoul} {summarizer.TotalBody}" : 
            "Текущие: -";
            
        string required = $"{RequiredMind} {RequiredSoul} {RequiredBody} ";
        
        statsText.text = $"{current}\n{required}";
    }
}
