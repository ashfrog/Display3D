// HonorWallManager.cs
using UnityEngine;
using System.Collections.Generic;
using RenderHeads.Media.AVProVideo;
using System.Data;
using System.IO;
using ExcelDataReader;
using System;

public class HonorWallManager : MonoBehaviour
{
    [Header("Display Settings")]
    public float spacing = 2f;         // 展示框之间的间距

    public float depth = 1f;           // 展示框的深度偏移
    public float scrollSpeed = 2f;     // 滑动速度
    public int displayCount = 5;       // 同时显示的展示框数量

    [Header("References")]
    public GameObject displayPrefab;    // 展示框预制体

    public Transform displayContainer;  // 展示框容器
    public MediaPlayer mediaPlayerPrefab; // AVPro视频播放器预制体
    public Camera mainCamera;           // 主摄像机

    private List<GameObject> displays = new List<GameObject>();
    private Vector3 targetPosition = new Vector3(0, 0, -10); // 摄像机初始位置

    private bool reSetPos = false;

    [SerializeField]
    private string excelFileName = "data.xlsx"; // Excel文件名

    private void Start()
    {
        InitializeDisplays();
        displayPrefab.SetActive(false); // 启动程序后将 displayPrefab 设为不可见
    }

    private void UpdateCameraPosition()
    {
        // 平滑更新摄像机位置
        Vector3 currentPos = mainCamera.transform.position;
        float newX = Mathf.Lerp(currentPos.x, targetPosition.x, Time.deltaTime * scrollSpeed);
        float newZ = Mathf.Lerp(currentPos.z, targetPosition.z, Time.deltaTime * scrollSpeed); // 修改为左后方
        if (reSetPos)
        {
            mainCamera.transform.position = new Vector3(targetPosition.x, currentPos.y, targetPosition.z);
            reSetPos = false;
        }
        else
        {
            mainCamera.transform.position = new Vector3(newX, currentPos.y, newZ);
        }

        // 匀速移动相机
        targetPosition.x += Time.deltaTime * scrollSpeed * spacing;
        targetPosition.z += Time.deltaTime * scrollSpeed * depth;
        if (targetPosition.x >= displays.Count * spacing)
        {
            targetPosition.x = 0;
            targetPosition.z = -10;
            reSetPos = true;
        }
    }

    private void Update()
    {
        UpdateCameraPosition();
    }

    public void InitializeDisplays()
    {
        DataSet dataSet = ExcelReader.ReadExcel();
        if (dataSet == null || dataSet.Tables.Count == 0)
        {
            Debug.LogError("没有找到Excel数据");
            return;
        }

        // 遍历所有工作表
        for (int i = 0; i < dataSet.Tables.Count; i++)
        {
            DataTable table = dataSet.Tables[i];
            Debug.Log($"处理工作表: {table.TableName}");

            // 遍历所有行
            for (int j = 0; j < table.Rows.Count; j++)
            {
                DataRow row = table.Rows[j];
                // 示例：打印每一行的数据
                string rowData = "";
                for (int k = 0; k < table.Columns.Count; k++)
                {
                    rowData += $"{table.Columns[k].ColumnName}: {row[k]}, ";
                }
                Debug.Log(rowData);

                // 在这里处理你的数据
                // 例如: 创建游戏对象、更新UI等
            }
        }

        // 初始化展示框
        for (int i = 0; i < displayCount; i++)
        {
            CreateDisplayBox(i, null);
        }
    }

    private void CreateDisplayBox(int row, DataRow dataRow)
    {
        GameObject display = Instantiate(displayPrefab, displayContainer);
        float xPos = row * spacing;
        float zPos = row * depth;
        display.transform.localPosition = new Vector3(xPos, 0, zPos);
        displays.Add(display);

        DisplayBox displayBox = display.GetComponent<DisplayBox>();
        // 动态生成material
        Renderer renderer = displayBox.frontRenderer;
        if (renderer != null)
        {
            renderer.material = new Material(renderer.material);
        }
        string file = $@"M:\GitHub\Display3D\FancyDisplay\Assets\StreamingAssets\AVProVideoSamples\{row + 1}";
        // 在展示框上添加AVPro视频播放器或图片
        if (FileUtils.IsImgFile(file + ".png"))
        {
            renderer.material.mainTexture = LoadTexture(file + ".png");
        }
        else if (FileUtils.IsMovFile(file + ".mp4"))
        {
            MediaPlayer mediaPlayer = Instantiate(mediaPlayerPrefab, display.transform);
            ApplyToMaterial applyToMaterial = mediaPlayer.GetComponent<ApplyToMaterial>();
            applyToMaterial.Material = renderer.material;
            string videoPath = file + ".mp4";
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
}