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

    [Header("References")]
    public GameObject displayPrefab;    // 展示框预制体

    public GameObject displayTitlePrefab;    // 固定展示框预制体

    public Transform displayContainer;  // 展示框容器

    public Transform displayTitleContainer;  // 固定展示框容器
    public MediaPlayer mediaPlayerPrefab; // AVPro视频播放器预制体
    public Camera mainCamera;           // 主摄像机

    private Vector3 targetPosition = new Vector3(0, 0, -10); // 摄像机初始位置

    private bool reSetPos = false;
    private DataSet dataSet;//xlsx数据
    private int curSheetIndex = 0;

    private void Start()
    {
        Debug.Log("Start");
        dataSet = ExcelReader.ReadExcel();

        SetPrefabInactive(displayPrefab);
        SetPrefabInactive(displayTitlePrefab);

        scrollSpeed = Settings.ini.Graphics.ScrollSpeed;
        InitializeDisplays(curSheetIndex);
    }

    private void SetPrefabInactive(GameObject prefab)
    {
        prefab.SetActive(false);
        prefab.transform.SetParent(null);
        prefab.transform.position = Vector3.zero;
        prefab.transform.rotation = Quaternion.identity;
        prefab.transform.localScale = Vector3.one;
    }

    private void UpdateCameraPosition()
    {
        // 平滑更新摄像机位置
        Vector3 currentPos = mainCamera.transform.position;
        //float newX = Mathf.Lerp(currentPos.x, targetPosition.x, Time.deltaTime * scrollSpeed); //允许移动不用加lerp
        //float newZ = Mathf.Lerp(currentPos.z, targetPosition.z, Time.deltaTime * scrollSpeed); // 修改为左后方

        float newX = targetPosition.x;
        float newZ = targetPosition.z;

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
        if (targetPosition.x >= displayContainer.childCount * spacing)
        {
            targetPosition.x = 0;
            targetPosition.z = -10;
            reSetPos = true;
            curSheetIndex = (curSheetIndex + 1) % dataSet.Tables.Count;
            if (curSheetIndex < dataSet.Tables.Count)
            {
                DestroyChildrens(displayTitleContainer);
                DestroyChildrens(displayContainer);
                InitializeDisplays(curSheetIndex);
            }
        }
    }

    private void DestroyChildrens(Transform transform)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private void Update()
    {
        UpdateCameraPosition();
        if (Input.GetKeyDown(KeyCode.F12))
        {
            Settings.ini.Graphics.ScrollSpeed = scrollSpeed;
        }
    }

    public void InitializeDisplays(int sheetindex)
    {
        if (dataSet == null || dataSet.Tables.Count == 0)
        {
            Debug.LogError("没有找到Excel数据");
            return;
        }

        DataTable table = dataSet.Tables[sheetindex];
        Debug.Log($"处理工作表: {table.TableName}");
        CreateDisplayTitleBox(table.TableName);
        // 遍历所有行
        for (int j = 0; j < table.Rows.Count; j++)
        {
            DataRow row = table.Rows[j];

            CreateDisplayBox(j, row);

            //// 示例：打印每一行的数据
            //string rowData = "";
            //for (int k = 0; k < table.Columns.Count; k++)
            //{
            //    rowData += $"{table.Columns[k].ColumnName}: {row[k]}, ";
            //}
            //Debug.Log(rowData);

            // 在这里处理你的数据
            // 例如: 创建游戏对象、更新UI等
        }
    }

    private void CreateDisplayBox(int rowindex, DataRow rowdata)
    {
        GameObject display = Instantiate(displayPrefab, displayContainer);
        display.SetActive(true);
        float xPos = rowindex * spacing;
        float zPos = rowindex * depth;
        display.transform.localPosition = new Vector3(xPos, 0, zPos);
        DisplayBox displayBox = display.GetComponent<DisplayBox>();
        displayBox.SetText(rowdata);
        string mediafilepath = Path.Combine(Application.streamingAssetsPath, ExcelReader.dataFolder, rowdata[3].ToString());
        displayBox.SetImgMov(mediaPlayerPrefab, displayBox, mediafilepath);
    }

    /// <summary>
    /// 团队展示 固定位置
    /// </summary>
    /// <param name="title"></param>
    private void CreateDisplayTitleBox(string title)
    {
        GameObject display = Instantiate(displayTitlePrefab, displayTitleContainer);
        display.SetActive(true);
        DisplayBox displayBox = display.GetComponent<DisplayBox>();
        displayBox.SetText(0, title);
        string mediafilepath = Path.Combine(Application.streamingAssetsPath, ExcelReader.dataFolder, title.ToString(), ".jpg");
        if (File.Exists(mediafilepath))
        {
            displayBox.SetImgMov(mediaPlayerPrefab, displayBox, mediafilepath);
        }
    }
}