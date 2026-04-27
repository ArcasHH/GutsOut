using UnityEngine;

public class OrganObject : MonoBehaviour
{
    [SerializeField] private ObjectTypeData data;

    public ObjectTypeData Data => data;

    private void Awake()
    {
        if (data == null)
            Debug.LogWarning($"[{name}] No ObjectTypeData! ");
    }

    public int GetStat(StatType stat) => data?.GetStat(stat) ?? 0;
}