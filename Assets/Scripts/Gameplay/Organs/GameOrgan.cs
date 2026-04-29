using UnityEngine;

public enum ObjectType { Heart, Brain, Lungs, Gut }
public enum CategoryType { Organ, Mechanic, Insects, None }
public enum StatType { Mind, Soul, Body }
public enum QualityType { Cursed, Bad, Ordinary, Good, Rare, Legendary, Epic }

[CreateAssetMenu(fileName = "NewObjectData", menuName = "Game/Game Organ")]
public class GameOrgan : ScriptableObject
{

    public ObjectType obj_type;
    public CategoryType category_type;
    public QualityType qulity_type;

    public int mind;
    public int soul;
    public int body;

    public int GetStat(StatType stat) => stat switch
    {
        StatType.Mind => mind,
        StatType.Soul => soul,
        StatType.Body => body,
        _ => 0
    };
}