using UnityEngine;

[ExecuteAlways]
public class QuadFitter : MonoBehaviour
{
    [SerializeField]
    private Camera targetCamera;

    [SerializeField]
    private float distanceFromCamera = 100f;

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
        FitQuad();
    }

    private void Update()
    {
        // 如果需要实时适配，可在 Update 中调用
        // FitQuad();
    }

    private void FitQuad()
    {
        if (targetCamera == null) return;

        // 根据距离计算所需的高度和宽度
        if (targetCamera.orthographic)
        {
            // 正交相机
            float quadHeight = targetCamera.orthographicSize * 2f;
            float quadWidth = quadHeight * targetCamera.aspect;
            transform.localPosition = new Vector3(0f, 0f, distanceFromCamera);
            transform.localScale = new Vector3(quadWidth, quadHeight, 1f);
        }
        else
        {
            // 透视相机
            float fovRad = targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad;
            float quadHeight = 2f * distanceFromCamera * Mathf.Tan(fovRad);
            float quadWidth = quadHeight * targetCamera.aspect;
            transform.localPosition = new Vector3(0f, 0f, distanceFromCamera);
            transform.localScale = new Vector3(quadWidth, quadHeight, 1f);
        }
    }
}