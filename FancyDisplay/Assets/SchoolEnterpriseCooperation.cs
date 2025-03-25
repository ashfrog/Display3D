using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using TMPro;
using UnityEngine;

/// <summary>
/// 校企合作
/// </summary>
public class SchoolEnterpriseCooperation : MonoBehaviour
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
    DisplayBox[] displayBoxes;

    [SerializeField]
    int _pageIndex;

    [SerializeField]
    float revealSpeed = 0.1f;

    // 跟踪当前正在播放的媒体项
    private int currentPlayingItemIndex = -1;

    private void Start()
    {
        dataSet = ExcelReader.ReadExcel("校企合作.xlsx");
        dataTable = dataSet.Tables[0];
        foreach (DataRow row in dataTable.Rows)
        {
            try
            {
                string file = Path.Combine(Application.streamingAssetsPath, ExcelReader.dataFolder, row[0].ToString());
                if (FileUtils.IsImgFile(file))
                {
                    Texture2D texture2D = LoadTexture(file);
                    texture2Ds.Add(texture2D);
                }
                else
                {
                    Debug.Log(file + "不存在");
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
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
                    // 更新下一篇
                    SetBoxDisplays(_pageIndex, displayBoxes.Length);
                    _pageIndex++;
                }
            }

            // 等待一小段时间再检查
            yield return new WaitForSeconds(0.1f);
        }
    }


    //展示一页图片
    private void SetBoxDisplays(int pageIndex, int everyPageCount)
    {
        int totalPageCount = (int)Math.Ceiling((double)texture2Ds.Count / everyPageCount);
        if (pageIndex < 0 || pageIndex > totalPageCount)
        {
            pageIndex = 0;
        }
        int index = pageIndex * everyPageCount;

        for (int itemid = 0; itemid < everyPageCount; itemid++)
        {
            DisplayBox displayBox = displayBoxes[itemid];

            if (index < texture2Ds.Count)
            {

                displayBox.frontRenderer.gameObject.SetActive(true);

                Texture2D imageTexture = texture2Ds[index];

                displayBox.SetImg(displayBox, imageTexture, true);

            }
            else
            {
                //隐藏meshrender
                displayBox.frontRenderer.gameObject.SetActive(false);
            }
            index++;
            index = index >= texture2Ds.Count ? 0 : index;
        }
    }
}
