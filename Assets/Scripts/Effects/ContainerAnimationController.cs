using UnityEngine;
using System.Collections;

public class ContainerAnimationController : MonoBehaviour
{
    [Header("Settings")]
    public float duration = 0.5f;
    public Vector2 spawnOffset = new Vector2(0, 300);
    public Vector2 despawnOffset = new Vector2(0, -300);
    public Vector3 spawnScale = new Vector3(0.5f, 0.5f, 1f);

    private RectTransform rectTransform;
    private Vector2 targetPos;
    private Coroutine currentAnimation;
    private Vector2 forcedEndPosition;
    private Vector3 forcedEndScale;

    
    public System.Action OnAnimationComplete;

    private AnimationType currentAnimationType;
    private enum AnimationType
    {
        None,
        In,
        Out
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        targetPos = rectTransform.anchoredPosition;

        OnAnimationComplete += () => Invoke(nameof(DelayedEventsOnReplaceContainer), 0.1f); //Delay after animation ends - anti-spam buttons
    }

    private void Start() => PlayAnimateIn();

    private void OnDisable()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
            OnAnimationComplete.Invoke();
            if (currentAnimationType == AnimationType.In)
            {
                rectTransform.anchoredPosition = forcedEndPosition;
                rectTransform.localScale = forcedEndScale;
            }
            if (currentAnimationType == AnimationType.Out)
            {
                Destroy(gameObject);
            }
        }
    }

    private void DelayedEventsOnReplaceContainer()
    {
        EventBus.TriggerInventoryChanged();//to recalculate the parameters
    }

    public void PlayAnimateIn()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        forcedEndPosition = targetPos;
        forcedEndScale = Vector3.one;
        currentAnimationType = AnimationType.In;
        currentAnimation = StartCoroutine(AnimateIn());
    }

    public void PlayAnimateOut()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        forcedEndPosition = rectTransform.anchoredPosition + despawnOffset;
        forcedEndScale = Vector3.zero;
        currentAnimationType = AnimationType.Out;
        currentAnimation = StartCoroutine(AnimateOut());
    }

    private IEnumerator AnimateIn()
    {
        rectTransform.anchoredPosition = targetPos + spawnOffset;
        rectTransform.localScale = spawnScale;
        yield return StartCoroutine(AnimateTo(targetPos, Vector3.one));
        currentAnimation = null;
        currentAnimationType = AnimationType.None;
        //OnAnimationComplete?.Invoke();
    }

    private IEnumerator AnimateOut()
    {
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = startPos + despawnOffset;
        Vector3 startScale = rectTransform.localScale;
        yield return StartCoroutine(AnimateTo(endPos, Vector3.zero));
        currentAnimation = null;
        //OnAnimationComplete?.Invoke();
        Destroy(gameObject);
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

        OnAnimationComplete.Invoke();
    }
}