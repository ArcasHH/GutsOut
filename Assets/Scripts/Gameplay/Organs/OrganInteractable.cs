using UnityEngine;
using TMPro; // Или UnityEngine.UI, если используете обычный Text

public class OrganInteractable : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private GameObject outlineObject;

    private OrganItem organData;
    private Camera mainCamera;

    private void Awake()
    {
        if (outlineObject != null)
            outlineObject.SetActive(false);
    }

    private void OnMouseEnter()
    {
        ShowOutline();
    }

    private void OnMouseExit()
    {
        HideOutline();
    }

    private void ShowOutline()
    {
        if (outlineObject != null)
            outlineObject.SetActive(true);
    }

    private void HideOutline()
    {
        if (outlineObject != null)
            outlineObject.SetActive(false);
    }

}