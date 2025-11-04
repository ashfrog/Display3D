using UnityEngine;

/// <summary>
/// 3D画面只显示在屏幕左侧的一部分区域，平滑切换相机视口宽度
/// </summary>
public class CameraViewportAnimator : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [Header("3D画面左右占比")]
    [SerializeField] private float width_expand = 0.7f;
    private float defaultWidth = 1f;
    [SerializeField] private float transitionDuration = 0.5f;

    private float targetWidth;
    private Coroutine transitionCoroutine;

    private void Awake()
    {
        // 初始状态，可以是A或B
        targetWidth = defaultWidth;
        SetCameraWidth(targetWidth);
    }

    public void SwitchWidth(bool expand)
    {
        // 切换目标宽度
        targetWidth = expand ? width_expand : defaultWidth;

        // 如果有动画在进行，先停止
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(AnimateViewportWidth(targetWidth, transitionDuration));
    }

    private System.Collections.IEnumerator AnimateViewportWidth(float toWidth, float duration)
    {
        float fromWidth = targetCamera.rect.width;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentWidth = Mathf.Lerp(fromWidth, toWidth, t);
            SetCameraWidth(currentWidth);
            yield return null;
        }

        SetCameraWidth(toWidth);
        transitionCoroutine = null;
    }

    private void SetCameraWidth(float width)
    {
        Rect rect = targetCamera.rect;
        rect.width = width;
        targetCamera.rect = rect;
    }
}