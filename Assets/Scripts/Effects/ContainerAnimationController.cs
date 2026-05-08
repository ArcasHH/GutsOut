using UnityEngine;
using System.Collections;

public class ContainerAnimationController : MonoBehaviour
{
    [Header("Настройки")]
    public float duration = 0.5f;
    public Vector2 spawnOffset = new Vector2(0, 300);
    public Vector2 despawnOffset = new Vector2(0, -300);
    public Vector3 spawnScale = new Vector3(0.5f, 0.5f, 1f);

    private RectTransform rectTransform;
    private Vector2 targetPos;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        targetPos = rectTransform.anchoredPosition; // Anchor pivot must be (0.5;0.5) !!!
        //targetPos = new Vector2( rectTransform.sizeDelta.x * 0.5f,rectTransform.sizeDelta.y * -0.5f);
    }

    private void Start() => StartCoroutine(AnimateIn());

    public IEnumerator AnimateIn()
    {
        rectTransform.anchoredPosition = targetPos + spawnOffset;
        rectTransform.localScale = spawnScale;
        return AnimateTo(targetPos, Vector3.one);
    }

    public IEnumerator AnimateOut()
    {
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = startPos + despawnOffset;
        Vector3 startScale = rectTransform.localScale;
        return AnimateTo(endPos, Vector3.zero);
    }

    private IEnumerator AnimateTo(Vector2 targetPosition, Vector3 targetScaleValue)
    {
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector3 startScale = rectTransform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPosition, t);
            rectTransform.localScale = Vector3.Lerp(startScale, targetScaleValue, t);

            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
        rectTransform.localScale = targetScaleValue;
    }
}