// HonorWallManager.cs
using UnityEngine;
using System.Collections.Generic;
using RenderHeads.Media.AVProVideo;

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
        // 初始化展示框
        for (int i = 0; i < displayCount; i++)
        {
            GameObject display = Instantiate(displayPrefab, displayContainer);
            float xPos = i * spacing;
            float zPos = i * depth;
            display.transform.localPosition = new Vector3(xPos, 0, zPos);
            displays.Add(display);

            // 动态生成material
            Renderer renderer = display.GetComponent<DisplayBox>().frontRenderer;
            if (renderer != null)
            {
                renderer.material = new Material(renderer.material);
            }
            string file = $@"M:\GitHub\Display3D\FancyDisplay\Assets\StreamingAssets\AVProVideoSamples\{i + 1}";
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
    }

    private Texture2D LoadTexture(string filePath)
    {
        byte[] fileData = System.IO.File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }
}