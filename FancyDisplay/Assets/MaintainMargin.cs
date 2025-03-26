using UnityEngine;

public class MaintainMargin : MonoBehaviour
{
    public Transform mainObject;  // 主物体
    public Transform outerFrame;  // 外层边框
    public float margin = 0.2f;   // 需要额外增大的边距

    void Update()
    {
        if (mainObject == null || outerFrame == null)
            return;

        // 获取主物体的当前缩放
        Vector3 mainScale = mainObject.localScale;

        // 根据margin调整外层边框的缩放
        // 只增大X、Y方向的边距，并保持Z方向一致
        outerFrame.localScale = new Vector3(
            mainScale.x + margin,
            mainScale.y + margin,
            mainScale.z
        );

        // 如果需要在位置上也做一些额外调整，可以继续在此处根据需求编写逻辑
    }
}