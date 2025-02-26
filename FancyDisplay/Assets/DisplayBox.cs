using RenderHeads.Media.AVProVideo;
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

    public void SetImgMov(MediaPlayer mediaPlayerPrefab, DisplayBox displayBox, string file)
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
            renderer.material.mainTexture = LoadTexture(file);
            needDestroyTexture = true;
        }
        else if (FileUtils.IsMovFile(file))
        {
            MediaPlayer mediaPlayer = Instantiate(mediaPlayerPrefab, displayBox.transform);
            ApplyToMaterial applyToMaterial = mediaPlayer.GetComponent<ApplyToMaterial>();
            applyToMaterial.Material = renderer.material;
            string videoPath = file;
            mediaPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, videoPath, true);
        }
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