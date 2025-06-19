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
/// 授权证书展示
/// </summary>
public class MajorEvents : MonoBehaviour
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

    [SerializeField]
    AudioSource audioSource;

    // 跟踪当前正在播放的媒体项
    private int currentPlayingItemIndex = -1;

    private void Start()
    {
        dataSet = ExcelReader.ReadExcel("大事记.xlsx");
        dataTable = dataSet.Tables[0];
        foreach (DataRow row in dataTable.Rows)
        {
            string file = Path.Combine(Application.streamingAssetsPath, ExcelReader.dataFolder, row[0].ToString());
            if (FileUtils.IsImgFile(file))
            {
                Texture2D texture2D = LoadTexture(file);
                texture2Ds.Add(texture2D);
                DateTime? dtime = ExcelReader.TryParseExcelDate(row[1]);
                string row2 = row[2].ToString();
                textsInfo.Add(row2);
                if (dtime.HasValue)
                {
                    textsDate.Add(dtime.Value);
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
                    audioSource.Play();
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
        if (index >= texture2Ds.Count)
        {
            index = 0;
        }
    }

    private void SetBoxDisplay(int curindex)
    {
        if (curindex < 0 || curindex >= texture2Ds.Count)
        {
            return;
        }
        meshRenderer.material.SetTexture("_EmissionMap", texture2Ds[curindex]);
        int year = textsDate[curindex].Year;
        int month = textsDate[curindex].Month;
        int day = textsDate[curindex].Day;
        float size = tmpText.fontSize;
        float bigsize = size * 3f;
        float normalsize = size * 2.6f;
        float minsize = size * 1.6f;
        tmpText.text = $"<size={bigsize}>{year}</size><size={normalsize}>年</size>" +
            $"<size={minsize}>{month}</size><size={minsize}>月</size>" +
            $"<size={minsize}>{day}</size><size={minsize}>日</size>" +
            $"<br>{textsInfo[curindex]}";
        textFlyInEffect.StartFlyInEffect();
    }
}