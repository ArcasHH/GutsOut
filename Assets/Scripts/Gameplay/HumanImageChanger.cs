using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class HumanImageChanger : MonoBehaviour
{
    private string spritesFolderPath = "Sprites/GameSprites/Human";
    void Start()
    {
        Image image = GetComponent<Image>();
        Sprite[] sprites = Resources.LoadAll<Sprite>(spritesFolderPath);

        if (sprites != null && sprites.Length > 0)
        {
            image.sprite = sprites[Random.Range(0, sprites.Length)];
        }
        else
        {
            Debug.LogWarning($"No sprites found in Resources/{spritesFolderPath}");
        }
    }

}
