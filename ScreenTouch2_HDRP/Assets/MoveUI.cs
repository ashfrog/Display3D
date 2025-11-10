using DG.Tweening;
using UnityEngine;

public class MoveUI : MonoBehaviour
{
    public float x;
    public float moveTime;
    private void OnEnable()
    {
        RectTransform rt = GetComponent<RectTransform>();
        var pos = rt.anchoredPosition;
        pos.x = x;
        rt.anchoredPosition = pos;

        rt.DOAnchorPosX(0f, moveTime)   // 从 x 平滑移动到 0，时长 moveTime 秒，缓动可按需调整
          .SetEase(Ease.Linear)   // 可换 Ease.OutQuad 等
          .SetLink(rt.gameObject);  // 目标销毁时自动终止 tween（可选）
    }
}
