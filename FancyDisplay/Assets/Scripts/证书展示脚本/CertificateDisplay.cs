using RenderHeads.Media.AVProVideo;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using TMPro;
using UnityEngine;

/// <summary>
/// 授权证书展示
/// </summary>
public class CertificateDisplay : MonoBehaviour
{
    [SerializeField]
    PlaylistMediaPlayer playlistMediaPlayer;
    [SerializeField]
    TextMeshPro textMesh;

    DataSet dataSet;
    DataTable dataTable;

    /// <summary>
    /// 证书文件
    /// </summary>
    [SerializeField]
    List<Texture2D> texture2Ds = new List<Texture2D>();

    /// <summary>
    /// 证书文字
    /// </summary>
    List<string> texts = new List<string>();

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
        dataSet = ExcelReader.ReadExcel("授权证书.xlsx");
        dataTable = dataSet.Tables[0];
        foreach (DataRow row in dataTable.Rows)
        {
            string file = Path.Combine(Application.streamingAssetsPath, ExcelReader.dataFolder, row[0].ToString());
            if (FileUtils.IsImgFile(file))
            {
                Texture2D texture2D = LoadTexture(file);
                texture2Ds.Add(texture2D);
                texts.Add(row[1].ToString());
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
                    audioSource.Play();
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
        textMesh.text = texts[curindex];
    }
}