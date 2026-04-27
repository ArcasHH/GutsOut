using UnityEngine;
using UnityEngine.InputSystem;

public class PupilFollowMouse : MonoBehaviour
{
    [SerializeField] private float maxRadius = 400f;
    [SerializeField] private float smoothTime = 1f;
    [SerializeField] private float maxTiltAngle = 90f;
    [SerializeField] private float tiltSmoothTime = 0.5f;

    private RectTransform pupilRect;
    private RectTransform parentRect;
    private Vector2 currentVelocity;
    private Vector2 targetLocalPos;

    private float currentAngle;
    private float angleVelocity;

    void Start()
    {
        pupilRect = GetComponent<RectTransform>();
        parentRect = transform.parent as RectTransform;
        targetLocalPos = Vector2.zero;
        pupilRect.anchoredPosition = Vector2.zero;
        currentAngle = 0f;
    }

    void FixedUpdate()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, mouseScreenPos, null, out Vector2 mouseLocalPos
        );

        targetLocalPos = Vector2.ClampMagnitude(mouseLocalPos, maxRadius);

        pupilRect.anchoredPosition = Vector2.SmoothDamp(
            pupilRect.anchoredPosition, targetLocalPos, ref currentVelocity, smoothTime
        );

        float horizontalTilt = 0f;
        if (maxRadius > 0.01f)
            horizontalTilt = Mathf.Clamp(targetLocalPos.x / maxRadius, -1f, 1f) * maxTiltAngle;
        currentAngle = Mathf.SmoothDamp(currentAngle, horizontalTilt, ref angleVelocity, tiltSmoothTime);
        pupilRect.localRotation = Quaternion.Euler(0, 0, currentAngle);
    }
}