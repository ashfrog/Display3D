using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using TMPro;
using UnityEngine;

public class DevHistory : MonoBehaviour
{
    [SerializeField]
    PlaylistMediaPlayer playlistMediaPlayer;
    [SerializeField]
    TMP_Text tmpText;

    [SerializeField]
    TextFlyInEffect textFlyInEffect;

    [SerializeField]
    AudioSource audioSource;

    DataSet dataSet;
    DataTable dataTable;

    /// <summary>
    /// 证书文件
    /// </summary>
    [SerializeField]
    List<Texture2D> texture2Ds = new List<Texture2D>();

    /// <summary>
    /// 大记事 年 月 日
    /// </summary>
    [SerializeField]
    List<DateTime> textsDate = new List<DateTime>();
    [SerializeField]
    List<string> textsInfo = new List<string>();

    [SerializeField]
    MeshRenderer meshRenderer;

    [SerializeField]
    int index;

    [SerializeField]
    float revealSpeed = 0.1f;

    // 跟踪当前正在播放的媒体项
    private int currentPlayingItemIndex = -1;

    [SerializeField]
    DisplayBox displayBoxPrefab;
    [SerializeField]
    Vector3 prefabV3;

    /// <summary>
    /// 标记每个条目是否有对应的图片
    /// </summary>
    private List<bool> hasImage = new List<bool>();

    public bool en;

    private void Start()
    {
        displayBoxPrefab.gameObject.SetActive(false);
        dataSet = ExcelReader.ReadExcel("发展历程.xlsx");
        dataTable = dataSet.Tables[0];

        foreach (DataRow row in dataTable.Rows)
        {
            string file = Path.Combine(Application.streamingAssetsPath, ExcelReader.dataFolder, row[0].ToString());
            Texture2D texture2D = null;
            bool imageExists = false;

            // 尝试加载图片
            if (FileUtils.IsImgFile(file) && File.Exists(file))
            {
                texture2D = LoadTexture(file);
                if (texture2D != null)
                {
                    imageExists = true;
                }
            }
            else
            {
                Debug.LogWarning($"图片文件不存在: {file}，将隐藏图片框");
            }

            // 添加到列表中
            texture2Ds.Add(texture2D);
            hasImage.Add(imageExists);

            // 处理文本内容
            string row1 = row[1].ToString();
            string row2 = row[2].ToString();

            if (row1.EndsWith("年"))
            {
                if (!en)
                {
                    row1 = row1.Replace("年", "");
                    // 用富文本让年份日期变蓝色，后面加上换行
                    string coloredRow1 = $"<color=#2587BA><size=6>{row1}</size></color>";
                    coloredRow1 += "年";
                    coloredRow1 += "\n";
                    textsInfo.Add(coloredRow1 + row2);
                }
                else
                {
                    row1 = row1.Replace("年", "");
                    // 用富文本让年份日期变蓝色，后面加上换行
                    string coloredRow1 = "In " + $"<color=#2587BA><size=6>{row1}</size></color>";
                    coloredRow1 += "\n";
                    textsInfo.Add(coloredRow1 + row2);
                }
            }
            else
            {
                textsInfo.Add(row2);
            }
        }
    }

    float curt = 3f;
    float waitt = 6f;
    public float waitAfterCycle = 10f; // 循环结束后的等待时间
    private bool isWaitingAfterCycle = false;

    private void Update()
    {
        curt += Time.deltaTime;

        float currentWaitTime = isWaitingAfterCycle ? waitAfterCycle : waitt;

        if (curt >= currentWaitTime)
        {
            curt = 0;

            if (isWaitingAfterCycle)
            {
                // 结束等待，重新开始循环
                isWaitingAfterCycle = false;
                return;
            }

            audioSource.Play();
            DisplayBox displayBox = Instantiate(displayBoxPrefab, prefabV3, Quaternion.identity);
            displayBox.gameObject.SetActive(true);

            // 检查当前条目是否有图片
            if (hasImage[index])
            {
                // 有图片，正常显示
                displayBox.SetImg(texture2Ds[index], true);
            }
            else
            {
                // 没有图片，隐藏图片框
                displayBox.SetImg(null, false);
                // 或者如果DisplayBox有专门的隐藏图片方法，可以调用：
                // displayBox.HideImage();
            }

            // 设置文本内容（无论图片是否存在都会显示）
            string text = textsInfo[index];
            displayBox.SetText(0, textsInfo[index]);

            index++;

            // 检查是否到达数组末尾
            if (index >= texture2Ds.Count)
            {
                index = 0;
                isWaitingAfterCycle = true; // 设置为等待状态
            }
        }
    }

    private Texture2D LoadTexture(string filePath)
    {
        try
        {
            byte[] fileData = System.IO.File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            return texture;
        }
        catch (Exception e)
        {
            Debug.LogError($"加载图片失败: {filePath}, 错误: {e.Message}");
            return null;
        }
    }
}