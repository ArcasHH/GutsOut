using UnityEngine;

public enum ObjectType { Heart, Brain, Lungs, Gut }
public enum StatType { Mind, Soul, Instinct }
public enum QualityType { Bad, Ordinary, Good }

[CreateAssetMenu(fileName = "NewObjectData", menuName = "Game/Object Type Data")]
public class ObjectTypeData : ScriptableObject
{

    public ObjectType obj_type;
    public QualityType qulity_type;
    //public string displayName = "Unnamed";

    public int mind;
    public int soul;
    public int instinct;

    public int GetStat(StatType stat) => stat switch
    {
        StatType.Mind => mind,
        StatType.Soul => soul,
        StatType.Instinct => instinct,
        _ => 0
    };
}