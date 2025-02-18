// 水面管理脚本
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class WaterSurface : MonoBehaviour
{
    public Camera reflectionCamera;
    private RenderTexture reflectionTexture;
    private Material waterMaterial;

    private void Start()
    {
        // 获取材质
        waterMaterial = GetComponent<MeshRenderer>().material;

        // 创建反射相机的渲染纹理
        if (reflectionCamera)
        {
            reflectionTexture = new RenderTexture(1024, 1024, 16);
            reflectionCamera.targetTexture = reflectionTexture;
            waterMaterial.SetTexture("_ReflectionTex", reflectionTexture);
        }
    }

    private void Update()
    {
        if (reflectionCamera)
        {
            // 更新反射相机位置
            Vector3 pos = transform.position;
            pos.y = transform.position.y * -1;
            reflectionCamera.transform.position = pos;

            // 更新反射相机旋转
            Vector3 euler = Camera.main.transform.eulerAngles;
            euler.x = -euler.x;
            reflectionCamera.transform.eulerAngles = euler;
        }
    }

    private void OnDestroy()
    {
        // 清理资源
        if (reflectionTexture)
        {
            reflectionTexture.Release();
            Destroy(reflectionTexture);
        }
    }
}