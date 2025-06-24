// HonorWallManager.cs
using UnityEngine;
using System.Collections.Generic;
using RenderHeads.Media.AVProVideo;
using System.Data;
using System.IO;
using ExcelDataReader;
using System;
using System.Collections;
using DG.Tweening;

public class HonorWallManager : MonoBehaviour
{
    [Header("Display Settings")]
    public float spacing = 2f;
    public float depth = 1f;
    public float scrollSpeed = 2f;

    [Header("Animation Settings")]
    public float animationDuration = 0.8f;
    public float animationDelay = 0.1f;
    public float slideDistance = 10f;
    public Ease animationEase = Ease.OutQuart;

    [Header("References")]
    public GameObject displayPrefab;
    public GameObject displayTitlePrefab;
    public Transform displayContainer;
    public Transform displayTitleContainer;
    public MediaPlayer mediaPlayerPrefab;
    public Camera mainCamera;

    [SerializeField]
    AudioSource audioSource;

    // 核心状态
    private DataSet dataSet;
    private Vector3 initialCameraPos;
    private Vector3 currentCameraTarget;
    private int currentSheetIndex = 0;
    private bool isTransitioning = false;
    private List<GameObject> activeDisplays = new List<GameObject>();

    private void Start()
    {
        Initialize();
        audioSource.Play();
    }

    private void Initialize()
    {
        // 初始化设置
        scrollSpeed = Settings.ini.Graphics.ScrollSpeed;
        initialCameraPos = mainCamera.transform.position;
        currentCameraTarget = initialCameraPos;

        // 加载数据
        dataSet = ExcelReader.ReadExcel("团队展示.xlsx");

        // 预制体设置
        SetupPrefabs();

        // 开始第一个sheet
        StartSheet(0);
    }

    private void SetupPrefabs()
    {
        displayPrefab.SetActive(false);
        displayTitlePrefab.SetActive(false);
    }

    private void Update()
    {
        if (!isTransitioning)
        {
            UpdateCameraMovement();
            CheckSheetComplete();
        }

        // 调试快捷键
        if (Input.GetKeyDown(KeyCode.Return))
        {
            TransitionToNextSheet();
        }
    }

    private void UpdateCameraMovement()
    {
        // 更新目标位置
        currentCameraTarget.x += Time.deltaTime * scrollSpeed * spacing;
        currentCameraTarget.z += Time.deltaTime * scrollSpeed * depth;

        // 应用相机位置
        mainCamera.transform.position = new Vector3(
            currentCameraTarget.x,
            mainCamera.transform.position.y,
            currentCameraTarget.z
        );
    }

    private void CheckSheetComplete()
    {
        if (dataSet?.Tables[currentSheetIndex] != null)
        {
            int rowCount = dataSet.Tables[currentSheetIndex].Rows.Count;
            if (currentCameraTarget.x >= rowCount * spacing)
            {
                TransitionToNextSheet();
            }
        }
    }

    private void TransitionToNextSheet()
    {
        if (isTransitioning || dataSet == null) return;

        isTransitioning = true;

        // 播放退出动画，完成后立即跳转到新sheet
        PlayExitAnimation(() =>
        {
            // 立即重置并开始新sheet，保持动画状态
            ResetForNewSheet();
            int nextIndex = (currentSheetIndex + 1) % dataSet.Tables.Count;
            StartSheet(nextIndex);
            audioSource.Play();
        });
    }

    private void ResetForNewSheet()
    {
        // 立即重置相机位置
        currentCameraTarget = initialCameraPos;
        mainCamera.transform.position = new Vector3(
            initialCameraPos.x,
            mainCamera.transform.position.y,
            initialCameraPos.z
        );

        // 清理旧显示
        ClearDisplays();
    }

    private void StartSheet(int sheetIndex)
    {
        currentSheetIndex = sheetIndex;
        // 开始时设置为动画状态，防止相机移动
        isTransitioning = true;
        StartCoroutine(CreateAndAnimateSheet(sheetIndex));
    }

    private IEnumerator CreateAndAnimateSheet(int sheetIndex)
    {
        if (dataSet?.Tables[sheetIndex] == null) yield break;

        DataTable table = dataSet.Tables[sheetIndex];

        // 创建标题
        CreateTitleDisplay(table.TableName);
        yield return null;

        // 创建数据显示
        for (int i = 0; i < table.Rows.Count; i++)
        {
            CreateDataDisplay(i, table.Rows[i]);
            yield return null;
        }

        // 立即播放进入动画，无需等待
        PlayEnterAnimation(() =>
        {
            isTransitioning = false; // 动画完成后允许相机移动
        });
    }

    private void CreateTitleDisplay(string title)
    {
        GameObject display = Instantiate(displayTitlePrefab, displayTitleContainer);
        display.SetActive(true);

        DisplayBox displayBox = display.GetComponent<DisplayBox>();
        displayBox.SetText(0, title);

        string imagePath = Path.Combine(Application.streamingAssetsPath,
            ExcelReader.dataFolder, title + ".jpg");
        if (File.Exists(imagePath))
        {
            displayBox.SetImgMov(imagePath, mediaPlayerPrefab);
        }

        // 创建时就设置到动画起始位置，避免闪烁
        SetDisplayToStartPosition(display);

        activeDisplays.Add(display);
    }

    private void CreateDataDisplay(int index, DataRow rowData)
    {
        GameObject display = Instantiate(displayPrefab, displayContainer);
        display.SetActive(true);

        // 设置最终位置
        Vector3 finalPosition = new Vector3(index * spacing, 0, index * depth);
        display.transform.localPosition = finalPosition;

        // 设置数据
        DisplayBox displayBox = display.GetComponent<DisplayBox>();
        displayBox.SetText(rowData);

        string mediaPath = Path.Combine(Application.streamingAssetsPath,
            ExcelReader.dataFolder, rowData[3].ToString());
        displayBox.SetImgMov(mediaPath, mediaPlayerPrefab);

        // 创建时就设置到动画起始位置，避免闪烁
        SetDisplayToStartPosition(display);

        activeDisplays.Add(display);
    }

    private void PlayExitAnimation(System.Action onComplete)
    {
        Sequence exitSequence = DOTween.Sequence();

        foreach (var display in activeDisplays)
        {
            if (display != null)
            {
                // 滑出和淡出
                exitSequence.Join(display.transform.DOLocalMoveX(
                    display.transform.localPosition.x - slideDistance,
                    animationDuration * 0.5f).SetEase(Ease.InQuart));

                var canvasGroup = GetOrAddCanvasGroup(display);
                exitSequence.Join(canvasGroup.DOFade(0f, animationDuration * 0.5f));
            }
        }

        // 滑出完成后立即执行回调，不需要额外延迟
        exitSequence.OnComplete(() => onComplete?.Invoke());
    }

    private void PlayEnterAnimation(System.Action onComplete)
    {
        Sequence enterSequence = DOTween.Sequence();

        for (int i = 0; i < activeDisplays.Count; i++)
        {
            GameObject display = activeDisplays[i];
            if (display == null) continue;

            var canvasGroup = GetOrAddCanvasGroup(display);

            // 重新计算最终位置（防止位置被意外修改）
            Vector3 finalPos;
            if (display.transform.parent == displayTitleContainer)
            {
                // 标题框的最终位置
                finalPos = Vector3.zero;
            }
            else
            {
                // 数据框的最终位置（基于索引）
                int displayIndex = i - 1; // 减去标题框
                if (displayIndex < 0) displayIndex = 0;
                finalPos = new Vector3(displayIndex * spacing, 0, displayIndex * depth);
            }

            float delay = i * animationDelay;

            Debug.Log($"动画 {display.name} 从 {display.transform.localPosition} 到 {finalPos}");

            // 添加动画（从当前的起始位置到最终位置）
            enterSequence.Insert(delay, display.transform.DOLocalMove(finalPos, animationDuration)
                .SetEase(animationEase));
            enterSequence.Insert(delay, display.transform.DOScale(Vector3.one, animationDuration)
                .SetEase(animationEase));
            enterSequence.Insert(delay, canvasGroup.DOFade(1f, animationDuration * 0.7f));
        }

        enterSequence.OnComplete(() => onComplete?.Invoke());
    }

    /// <summary>
    /// 设置显示对象到动画起始位置
    /// </summary>
    private void SetDisplayToStartPosition(GameObject display)
    {
        var canvasGroup = GetOrAddCanvasGroup(display);
        Vector3 finalPos = display.transform.localPosition;
        Vector3 startPos = finalPos + Vector3.right * slideDistance;

        // 立即设置到起始状态，避免闪烁
        display.transform.localPosition = startPos;
        //display.transform.localScale = Vector3.one * 0.8f;
        canvasGroup.alpha = 0f;

        // 调试信息
        Debug.Log($"设置 {display.name} 最终位置: {finalPos}, 起始位置: {startPos}");
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject obj)
    {
        var canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = obj.AddComponent<CanvasGroup>();
        }
        return canvasGroup;
    }

    private void ClearDisplays()
    {
        foreach (var display in activeDisplays)
        {
            if (display != null)
            {
                Destroy(display);
            }
        }
        activeDisplays.Clear();
    }

    private void OnDestroy()
    {
        DOTween.KillAll();
    }
}