using RenderHeads.Media.AVProVideo;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

/// <summary>
/// 授权证书展示
/// </summary>
public class CertificateDisplay : MonoBehaviour
{
    [SerializeField]
    MediaPlayer mediaPlayer;

    private float lastCheckTime = -1f;
    private float debounceTime = 0.5f; // 防抖时间间隔，单位为秒

    DataSet dataSet;
    private void Start()
    {
        dataSet = ExcelReader.ReadExcel("授权证书.xlsx");
    }

    // Update is called once per frame
    void Update()
    {

        if (mediaPlayer != null && mediaPlayer.Control != null)
        {
            if (mediaPlayer.Control.GetCurrentTimeMs() >= mediaPlayer.Info.GetDurationMs())
            {
                if (Time.time - lastCheckTime < debounceTime)
                {
                    return; // 如果距离上次检查的时间小于防抖时间间隔，则直接返回
                }
                lastCheckTime = Time.time; // 更新上次检查时间
                Debug.Log("循环视频开始播放");
            }
        }
    }
}
