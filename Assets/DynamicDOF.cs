using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DynamicDOF : MonoBehaviour
{
    [SerializeField] private float minDistance = 1.0f;
    [SerializeField] private float maxDistance = 10.0f;
    [SerializeField] private float focusSpeed = 1.0f;
    [SerializeField] private Volume volume;

    private DepthOfField depthOfField;
    private Coroutine focusCoroutine;

    private float targetDistance;

    private void Start()
    {
        volume.profile.TryGet(out depthOfField);
    }

    private void Update()
    {
        CHeckFocus();
    }

    private void CHeckFocus()
    {
        var hitInfo = CalculateHit();
        if (!hitInfo.HasValue)
        {
            SwitchFocus(maxDistance);
            return;
        }

        var focusDistance = CalculateFocusDistance(hitInfo.Value);
        SwitchFocus(focusDistance);
    }

    private float CalculateFocusDistance(RaycastHit hitInfo)
    {
        var distance = Vector3.Distance(transform.position, hitInfo.point);
        return Mathf.Clamp(distance, minDistance, maxDistance);
    }

    private void SwitchFocus(float distance)
    {
        if (Mathf.Approximately(depthOfField.focusDistance.value, distance)) return;
        if (Mathf.Approximately(targetDistance, distance)) return;

        targetDistance = distance;
        if (focusCoroutine != null) return;
        focusCoroutine = StartCoroutine(FocusCoroutine());
    }

    private IEnumerator FocusCoroutine()
    {
        var elapsedTime = 0.0f;
        var startDistance = depthOfField.focusDistance.value;

        while (elapsedTime < focusSpeed)
        {
            depthOfField.focusDistance.value = Mathf.Lerp(startDistance, targetDistance, elapsedTime / focusSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        depthOfField.focusDistance.value = targetDistance;
        focusCoroutine = null;
    }

    private RaycastHit? CalculateHit()
    {
        if (Physics.Raycast(transform.position, transform.forward, out var hit, maxDistance))
        {
            return hit;
        }

        return null;
    }
}