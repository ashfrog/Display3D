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
    MediaPlayer mediaPlayer;
    [SerializeField]
    TextMeshPro textMesh;

    private float lastCheckTime = -1f;
    private float debounceTime = 0.5f; // 防抖时间间隔，单位为秒

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

    [SerializeField] int index;
    [SerializeField]
    float revealSpeed = 0.1f;

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
        //第一次加载
        SetBoxDisplay();
    }

    private Texture2D LoadTexture(string filePath)
    {
        byte[] fileData = System.IO.File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }

    // Update is called once per frame
    void Update()
    {
        if (mediaPlayer != null && mediaPlayer.Control != null)
        {
            if (mediaPlayer.Control.GetCurrentTime() >= mediaPlayer.Info.GetDuration())
            {
                if (Time.time - lastCheckTime < debounceTime)
                {
                    return; // 如果距离上次检查的时间小于防抖时间间隔，则直接返回
                }
                lastCheckTime = Time.time; // 更新上次检查时间
                Debug.Log("循环视频开始播放");

                index++;
                //限制index范围
                if (index >= texture2Ds.Count)
                {
                    index = 0;
                }

                // meshRenderer材质的Emission贴图 展示证书
                SetBoxDisplay();
            }
        }
    }

    private void SetBoxDisplay()
    {
        meshRenderer.material.SetTexture("_EmissionMap", texture2Ds[index]);
        textMesh.text = texts[index];
    }
}
