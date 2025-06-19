using UnityEngine;

public class DirectMove : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public Vector3 direct = Vector3.left;
    public float destroyX = -100f;

    private Vector3 _lastPosition;
    private Vector3 _targetPosition;

    private void Start()
    {
        // 确保目标帧率设置
        Application.targetFrameRate = 60;
        // 确保垂直同步开启
        QualitySettings.vSyncCount = 1;

        _lastPosition = transform.position;
        _targetPosition = transform.position;
    }

    private void FixedUpdate()
    {
        // 在物理更新循环中计算位置，可以获得更稳定的移动
        _targetPosition += direct * moveSpeed * Time.fixedDeltaTime;
        transform.position = _targetPosition;
        CheckIfOffScreen();
    }

    private void CheckIfOffScreen()
    {
        if (transform.position.x < destroyX)
        {
            Destroy(gameObject);
        }
    }
}