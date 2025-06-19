using UnityEngine;

public class MoveObjectToCameraEdge : MonoBehaviour
{
    public Camera mainCamera;
    public float depth = 10f; // 根据需要调整物体距离摄像机的深度
    public float pixelOffset = 10f; // 距离屏幕边缘的像素偏移

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        // 将像素偏移转换为视口坐标偏移，然后转换为世界坐标
        float viewportOffsetX = pixelOffset / mainCamera.pixelWidth;
        Vector3 offsetPosition = mainCamera.ViewportToWorldPoint(new Vector3(viewportOffsetX, 0.5f, depth));
        transform.position = new Vector3(offsetPosition.x, transform.position.y, transform.position.z);
    }
}