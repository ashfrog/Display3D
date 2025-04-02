using UnityEngine;

/// <summary>
/// 调整渲染队列 保证文字比文本框后渲染
/// </summary>
public class AdjustRenderQueue : MonoBehaviour
{
    public Renderer quadRenderer;
    public Renderer textRenderer;

    void Start()
    {

        // Ensure the text is rendered after the quad
        if (quadRenderer != null && textRenderer != null)
        {
            textRenderer.material.renderQueue = quadRenderer.material.renderQueue + 1;
        }
    }
}