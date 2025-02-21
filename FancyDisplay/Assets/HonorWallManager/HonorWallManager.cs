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
    public bool autoScroll = false;    // 自动循环移动开关
    public float autoScrollInterval = 2f; // 自动循环移动间隔时间

    [Header("References")]
    public GameObject displayPrefab;    // 展示框预制体

    public Transform displayContainer;  // 展示框容器
    public MediaPlayer mediaPlayerPrefab; // AVPro视频播放器预制体

    private List<GameObject> displays = new List<GameObject>();
    private Vector2 targetPosition = new Vector2(0, 0);
    private int currentIndex = 0;
    private float autoScrollTimer = 0f;

    private void Start()
    {
        InitializeDisplays();
    }

    private void Update()
    {
        HandleInput();
        UpdateDisplayPositions();
        HandleAutoScroll();
    }

    private void InitializeDisplays()
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
            Renderer renderer = display.GetComponent<Renderer>();
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
            else
            {
                MediaPlayer mediaPlayer = Instantiate(mediaPlayerPrefab, display.transform);
                ApplyToMaterial applyToMaterial = mediaPlayer.GetComponent<ApplyToMaterial>();
                applyToMaterial.Material = renderer.material;
                string videoPath = file + ".mp4";
                mediaPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, videoPath, true);
            }
        }
    }

    private void HandleInput()
    {
        // 处理输入控制
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ScrollLeft();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ScrollRight();
        }
    }

    private void ScrollLeft()
    {
        if (currentIndex < displays.Count - 1)
        {
            currentIndex++;
            targetPosition.x -= spacing;
            targetPosition.y -= depth; // 修改为左后方
        }
    }

    private void ScrollRight()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            targetPosition.x += spacing;
            targetPosition.y += depth; // 修改为左后方
        }
    }

    private void UpdateDisplayPositions()
    {
        // 平滑更新展示框位置
        Vector3 currentPos = displayContainer.localPosition;
        float newX = Mathf.Lerp(currentPos.x, targetPosition.x, Time.deltaTime * scrollSpeed);
        float newZ = Mathf.Lerp(currentPos.z, targetPosition.y, Time.deltaTime * scrollSpeed); // 修改为左后方
        displayContainer.localPosition = new Vector3(newX, currentPos.y, newZ);
    }

    private void HandleAutoScroll()
    {
        if (autoScroll)
        {
            autoScrollTimer += Time.deltaTime;
            if (autoScrollTimer >= autoScrollInterval)
            {
                autoScrollTimer = 0f;
                ScrollLeft();

                // 循环移动展示框
                if (currentIndex >= displays.Count - 1)
                {
                    currentIndex = 0;
                    displayContainer.localPosition = Vector3.zero;
                    targetPosition = Vector2.zero;
                }
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