using UnityEngine;

public class QuadAspectRatioFitter : MonoBehaviour
{
    [SerializeField] private Transform parentTransform;
    [SerializeField] private Texture2D imageTexture;

    private void Start()
    {
        if (parentTransform == null)
        {
            parentTransform = transform.parent;
        }

        FitQuadInParent();
    }

    public void FitQuadInParent()
    {
        if (parentTransform == null || imageTexture == null)
        {
            Debug.LogError("缺少必要的组件引用!");
            return;
        }

        // 获取父物体的本地缩放
        Vector3 parentScale = parentTransform.localScale;
        float parentWidth = parentScale.x;
        float parentHeight = parentScale.y;

        // 获取图片的原始宽高比
        float imageAspectRatio = (float)imageTexture.width / imageTexture.height;

        // 计算在父物体内的最大可能大小
        float width, height;

        // 比较父物体的宽高比与图片的宽高比
        float parentAspectRatio = parentWidth / parentHeight;

        if (imageAspectRatio >= parentAspectRatio)
        {
            // 图片比较宽，以父物体的宽度为基准
            width = parentWidth;
            height = width / imageAspectRatio;
        }
        else
        {
            // 图片比较高，以父物体的高度为基准
            height = parentHeight;
            width = height * imageAspectRatio;
        }

        // 设置Quad的缩放
        transform.localScale = new Vector3(width, height, 1f);

        // 确保Quad的MeshRenderer使用这个纹理
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.mainTexture = imageTexture;
        }
    }
}