using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCamera : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private float rectWidth = 1;

    private void OnEnable()
    {
        if (_camera != null)
        {
            // 设置 Viewport Rect 的宽度，高度和位置保持不变
            Rect rect = _camera.rect;
            rect.width = rectWidth;
            _camera.rect = rect;
        }
    }
}
