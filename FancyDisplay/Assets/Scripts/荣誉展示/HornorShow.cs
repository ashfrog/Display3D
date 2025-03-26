using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using TMPro;
using UnityEngine;

/// <summary>
/// 荣誉展示
/// </summary>
public class HornorShow : MonoBehaviour
{
    [SerializeField]
    PlaylistMediaPlayer playlistMediaPlayer;
    [SerializeField]
    TMP_Text tmpText;

    [SerializeField]
    TextFlyInEffect textFlyInEffect;

    DataSet dataSet;
    DataTable dataTable;

    /// <summary>
    /// 证书文件
    /// </summary>
    [SerializeField]
    List<Texture2D> texture2Ds_0 = new List<Texture2D>();

    /// <summary>
    /// 年
    /// </summary>
    [SerializeField]
    List<DateTime> textsDate_1 = new List<DateTime>();
    [SerializeField]
    List<string> textsInfo_2 = new List<string>();

    [SerializeField]
    List<string> textsInfo_3 = new List<string>();



    [SerializeField]
    DisplayBox displayBox;

    [SerializeField]
    int index;

    [SerializeField]
    float revealSpeed = 0.1f;

    // 跟踪当前正在播放的媒体项
    private int currentPlayingItemIndex = -1;

    private void Start()
    {
        dataSet = ExcelReader.ReadExcel("荣誉展示.xlsx");
        dataTable = dataSet.Tables[0];
        foreach (DataRow row in dataTable.Rows)
        {
            string file = Path.Combine(Application.streamingAssetsPath, ExcelReader.dataFolder, row[0].ToString());
            if (FileUtils.IsImgFile(file))
            {
                Texture2D texture2D = LoadTexture(file);
                texture2Ds_0.Add(texture2D);
                DateTime? dtime = ExcelReader.TryParseExcelDate(row[1]);
                string row2 = row[2].ToString();
                textsInfo_2.Add(row2);
                string row3 = row[3].ToString();
                textsInfo_3.Add(row3);
                if (dtime.HasValue)
                {
                    textsDate_1.Add(dtime.Value);
                }
                else
                {
                    Debug.Log("日期解析失败: " + row[1].ToString());
                }
            }
            else
            {
                Debug.Log(file + "不存在");
            }
        }

        // 开始检查视频变化的协程
        StartCoroutine(CheckPlaylistItemChange());
    }

    private Texture2D LoadTexture(string filePath)
    {
        byte[] fileData = System.IO.File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }

    // 持续检查播放列表项目是否发生变化的协程
    private IEnumerator CheckPlaylistItemChange()
    {
        while (true)
        {
            if (playlistMediaPlayer != null && playlistMediaPlayer.Playlist != null)
            {
                int playingItemIndex = playlistMediaPlayer.PlaylistIndex;

                // 如果播放项目发生变化
                if (playingItemIndex != currentPlayingItemIndex)
                {
                    currentPlayingItemIndex = playingItemIndex;
                    // 更新到下一个证书
                    AdvanceCertificate();
                }
            }

            // 等待一小段时间再检查
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void AdvanceCertificate()
    {
        // 更新显示
        SetBoxDisplay(index);
        index++;
        // 限制index范围
        if (index >= texture2Ds_0.Count)
        {
            index = 0;
        }
    }

    private void SetBoxDisplay(int curindex)
    {
        if (curindex < 0 || curindex >= texture2Ds_0.Count)
        {
            return;
        }
        displayBox.SetImg(texture2Ds_0[curindex], true);

        int year = textsDate_1[curindex].Year;
        float size = tmpText.fontSize;
        float size_1 = size * 2.6f;
        float size_2 = size * 1f;
        float size_3 = size * 3f;

        tmpText.text = $"<size={size_1}>{year}</size><size={size_1}>年</size>" +
            $" >>> <size={size_2}>{textsInfo_2[curindex]}</size>" +
            $"<br><size={size_3}>{textsInfo_3[curindex]}</size>";
        textFlyInEffect.StartFlyInEffect();
    }
}