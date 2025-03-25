using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;

public class DisplayBox : MonoBehaviour
{
    [SerializeField]
    public Renderer frontRenderer;

    public TextMeshPro[] xls_Texs;

    private bool needDestroyTexture;

    public bool keepAspectRatio = false;

    public float borderPadding = 0.8f; //预留padding

    private void OnEnable()
    {
        if (frontRenderer == null)
        {
            frontRenderer = GetComponentInChildren<Renderer>();
        }
    }

    public void SetText(string name, string education, string school)
    {
        xls_Texs[0].text = name;
        xls_Texs[1].text = education;
        xls_Texs[2].text = school;
    }

    public void SetText(int index, string text)
    {
        xls_Texs[index].text = text;
    }

    public void SetText(DataRow rowData)
    {
        for (int i = 0; i < xls_Texs.Length; i++)
        {
            xls_Texs[i].text = rowData[i].ToString();
        }
    }

    public void SetImgMov(DisplayBox displayBox, string file, MediaPlayer mediaPlayerPrefab = null)
    {
        // 动态生成material
        Renderer renderer = frontRenderer;
        if (renderer != null)
        {
            renderer.material = new Material(renderer.material);
        }
        // 在展示框上添加AVPro视频播放器或图片
        if (FileUtils.IsImgFile(file))
        {
            Texture2D texture = LoadTexture(file);
            SetImg(displayBox, texture, keepAspectRatio);
        }
        else if (mediaPlayerPrefab != null && FileUtils.IsMovFile(file))
        {
            MediaPlayer mediaPlayer = Instantiate(mediaPlayerPrefab, displayBox.transform);
            ApplyToMaterial applyToMaterial = mediaPlayer.GetComponent<ApplyToMaterial>();
            applyToMaterial.Material = renderer.material;
            string videoPath = file;
            mediaPlayer.OpenMedia(new MediaPath(videoPath, MediaPathType.AbsolutePathOrURL), true);
        }
    }

    public void SetImg(DisplayBox displayBox, Texture2D texture, bool keepAspectRatio = false)
    {
        Renderer renderer = frontRenderer;
        if (renderer != null)
        {
            // 建议先置空并销毁旧贴图，避免残留引用
            if (renderer.material.mainTexture != null)
            {
                Destroy(renderer.material.mainTexture);
                renderer.material.mainTexture = null;
            }

            // 如有需要也可以先销毁旧材质再重新创建材质
            // Destroy(renderer.material);
            // renderer.material = new Material(Shader.Find("Standard"));

            // 动态生成新的材质
            renderer.material = new Material(renderer.material);

            if (keepAspectRatio && texture != null)
            {
                SetlocalScale(displayBox, texture);
            }

            // 指定新的贴图给材质
            if (texture != null)
            {
                renderer.material.mainTexture = texture;
            }
        }
    }

    private void SetlocalScale(DisplayBox displayBox, Texture2D texture)
    {
        displayBox.frontRenderer.gameObject.SetActive(true);

        Texture2D imageTexture = texture;
        displayBox.frontRenderer.material.SetTexture("_MainTex", imageTexture);

        // Get the parent's scale in world space
        Vector3 parentScale = displayBox.frontRenderer.transform.parent.transform.lossyScale;
        float parentWidth = parentScale.x;
        float parentHeight = parentScale.y;

        // Get image dimensions and aspect ratio
        float imageWidth = imageTexture.width;
        float imageHeight = imageTexture.height;
        float imageAspect = imageWidth / imageHeight;

        // Calculate new dimensions to fit inside parent
        float newWidth, newHeight;
        float parentAspect = parentWidth / parentHeight;

        if (imageAspect > parentAspect)
        {
            // Image is wider than parent (relative to height)
            newWidth = parentWidth;
            newHeight = newWidth / imageAspect;
        }
        else
        {
            // Image is taller than parent (relative to width)
            newHeight = parentHeight;
            newWidth = newHeight * imageAspect;
        }


        // Apply local scale with respect to parent's scale
        Vector3 localScale = new Vector3(
            newWidth / parentScale.x * borderPadding,
            newHeight / parentScale.y * borderPadding,
            1.0f
        );

        displayBox.frontRenderer.transform.localScale = localScale;
    }

    private Texture2D LoadTexture(string filePath)
    {
        byte[] fileData = System.IO.File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }

    private void OnDestroy()
    {
        if (needDestroyTexture && frontRenderer != null)
        {
            if (frontRenderer.material.mainTexture != null)
            {
                Destroy(frontRenderer.material.mainTexture);
            }
        }
    }
}