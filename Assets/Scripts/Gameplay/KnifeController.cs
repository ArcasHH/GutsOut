using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class KnifeController : MonoBehaviour
{
    [SerializeField] private Transform knifeSlot;
    [SerializeField] private GameObject knifePrefab;

    private GameObject currentKnifeInstance;
    [SerializeField] private TMP_Text knifeCostText;
    void Start()
    {
        SpawnKnife();
    }

    public void UpdateKnifeUI(int knife_cost)
    {
        if (knifeCostText != null)
            knifeCostText.text = knife_cost.ToString();
    }
    private void SpawnKnife()
    {
        if (knifePrefab == null || knifeSlot == null || currentKnifeInstance != null) 
            return;

        currentKnifeInstance = Instantiate(knifePrefab, knifeSlot);
        var knifeItem = currentKnifeInstance.GetComponent<KnifeItem>();
        if (knifeItem != null)
        {
            knifeItem.parentController = this;
        }

        var rt = currentKnifeInstance.GetComponent<RectTransform>();
        if (rt != null) rt.anchoredPosition = Vector2.zero;
    }
}
