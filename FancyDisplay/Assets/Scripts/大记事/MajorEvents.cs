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
/// 授权证书展示 - 优化版本，支持按需加载和内存管理
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
    /// 图片文件路径列表
    /// </summary>
    List<string> imagePaths = new List<string>();

    /// <summary>
    /// 图片缓存字典 - 只缓存当前和相邻的几张图片
    /// </summary>
    Dictionary<int, Texture2D> textureCache = new Dictionary<int, Texture2D>();

    /// <summary>
    /// 默认图片（当指定图片不存在时使用）
    /// </summary>
    [SerializeField]
    Texture2D defaultTexture;

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

    /// <summary>
    /// 缓存大小 - 同时在内存中保持的图片数量
    /// </summary>
    [SerializeField]
    int cacheSize = 5;

    /// <summary>
    /// 预加载距离 - 提前加载前后几张图片
    /// </summary>
    [SerializeField]
    int preloadDistance = 2;

    // 跟踪当前正在播放的媒体项
    private int currentPlayingItemIndex = -1;

    public bool en;

    public LimitTMPLinesExpandWidth limitTMPLinesExpandWidth;

    private void Start()
    {
        LoadDataFromExcel();

        // 预加载初始图片
        PreloadTextures(0);

        // 开始检查视频变化的协程
        StartCoroutine(CheckPlaylistItemChange());
    }

    private void LoadDataFromExcel()
    {
        dataSet = ExcelReader.ReadExcel("大事记.xlsx");
        dataTable = dataSet.Tables[0];

        foreach (DataRow row in dataTable.Rows)
        {
            string file = Path.Combine(Application.streamingAssetsPath, ExcelReader.dataFolder, row[0].ToString());
            imagePaths.Add(file);

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
                textsDate.Add(DateTime.Now);
            }
        }
    }

    /// <summary>
    /// 按需加载纹理
    /// </summary>
    /// <param name="textureIndex">纹理索引</param>
    /// <returns>加载的纹理</returns>
    private Texture2D LoadTextureOnDemand(int textureIndex)
    {
        if (textureIndex < 0 || textureIndex >= imagePaths.Count)
            return GetDefaultTexture();

        // 如果已经在缓存中，直接返回
        if (textureCache.ContainsKey(textureIndex))
        {
            return textureCache[textureIndex];
        }

        string filePath = imagePaths[textureIndex];
        Texture2D texture;

        if (FileUtils.IsImgFile(filePath) && File.Exists(filePath))
        {
            texture = LoadTexture(filePath);
        }
        else
        {
            Debug.Log(filePath + "不存在，使用默认图片");
            texture = GetDefaultTexture();
        }

        // 添加到缓存
        textureCache[textureIndex] = texture;

        // 清理缓存，保持缓存大小
        CleanupCache(textureIndex);

        return texture;
    }

    /// <summary>
    /// 预加载纹理 - 加载当前索引附近的图片
    /// </summary>
    /// <param name="centerIndex">中心索引</param>
    private void PreloadTextures(int centerIndex)
    {
        // 异步预加载周围的图片
        StartCoroutine(PreloadTexturesCoroutine(centerIndex));
    }

    private IEnumerator PreloadTexturesCoroutine(int centerIndex)
    {
        for (int offset = -preloadDistance; offset <= preloadDistance; offset++)
        {
            int targetIndex = centerIndex + offset;

            // 处理循环索引
            if (targetIndex < 0)
                targetIndex = imagePaths.Count + targetIndex;
            else if (targetIndex >= imagePaths.Count)
                targetIndex = targetIndex - imagePaths.Count;

            // 如果不在缓存中，则加载
            if (!textureCache.ContainsKey(targetIndex))
            {
                LoadTextureOnDemand(targetIndex);

                // 每加载一张图片后等待一帧，避免卡顿
                yield return null;
            }
        }
    }

    /// <summary>
    /// 清理缓存，移除距离当前索引较远的纹理
    /// </summary>
    /// <param name="currentIndex">当前索引</param>
    private void CleanupCache(int currentIndex)
    {
        if (textureCache.Count <= cacheSize)
            return;

        List<int> keysToRemove = new List<int>();

        foreach (var kvp in textureCache)
        {
            int distance = Mathf.Min(
                Mathf.Abs(kvp.Key - currentIndex),
                Mathf.Abs(kvp.Key - currentIndex + imagePaths.Count),
                Mathf.Abs(kvp.Key - currentIndex - imagePaths.Count)
            );

            if (distance > preloadDistance)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        // 移除最远的纹理直到缓存大小符合要求
        keysToRemove.Sort((a, b) =>
        {
            int distanceA = Mathf.Min(
                Mathf.Abs(a - currentIndex),
                Mathf.Abs(a - currentIndex + imagePaths.Count),
                Mathf.Abs(a - currentIndex - imagePaths.Count)
            );
            int distanceB = Mathf.Min(
                Mathf.Abs(b - currentIndex),
                Mathf.Abs(b - currentIndex + imagePaths.Count),
                Mathf.Abs(b - currentIndex - imagePaths.Count)
            );
            return distanceB.CompareTo(distanceA);
        });

        int removeCount = textureCache.Count - cacheSize;
        for (int i = 0; i < removeCount && i < keysToRemove.Count; i++)
        {
            int keyToRemove = keysToRemove[i];
            if (textureCache.ContainsKey(keyToRemove))
            {
                // 销毁纹理以释放内存
                if (textureCache[keyToRemove] != defaultTexture)
                {
                    DestroyImmediate(textureCache[keyToRemove]);
                }
                textureCache.Remove(keyToRemove);
                Debug.Log($"从缓存中移除纹理索引: {keyToRemove}");
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

    /// <summary>
    /// 获取默认纹理
    /// </summary>
    /// <returns></returns>
    private Texture2D GetDefaultTexture()
    {
        if (defaultTexture != null)
            return defaultTexture;

        return CreateDefaultTexture();
    }

    /// <summary>
    /// 创建一个默认的纯色纹理
    /// </summary>
    /// <returns></returns>
    private Texture2D CreateDefaultTexture()
    {
        Texture2D defaultTex = new Texture2D(512, 512);
        Color[] colors = new Color[512 * 512];

        // 创建一个灰色的默认纹理
        Color defaultColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = defaultColor;
        }

        defaultTex.SetPixels(colors);
        defaultTex.Apply();
        return defaultTex;
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
        if (index >= imagePaths.Count)
        {
            index = 0;
        }

        // 预加载下一组图片
        PreloadTextures(index);
    }

    private void SetBoxDisplay(int curindex)
    {
        if (curindex < 0 || curindex >= imagePaths.Count)
        {
            return;
        }

        // 按需加载当前纹理
        Texture2D currentTexture = LoadTextureOnDemand(curindex);
        meshRenderer.material.SetTexture("_EmissionMap", currentTexture);

        int year = textsDate[curindex].Year;
        int month = textsDate[curindex].Month;
        int day = textsDate[curindex].Day;
        float size = tmpText.fontSize;
        float bigsize = size * 3f;
        float normalsize = size * 2.6f;
        float minsize = size * 1.6f;

        if (!en)
        {
            tmpText.text = $"<size={bigsize}>{year}</size><size={normalsize}>年</size>" +
                $"<size={minsize}>{month}</size><size={minsize}>月</size>" +
                $"<size={minsize}>{day}</size><size={minsize}>日</size>" +
                $"<br>{textsInfo[curindex]}";

        }
        else
        {
            // 获取英文月份
            string monthName = System.Globalization.CultureInfo.GetCultureInfo("en-US").DateTimeFormat.GetMonthName(month);
            // 英文日期格式：Month Day, Year
            tmpText.text = $"<size={minsize}>{monthName}</size> " +
               $"<size={minsize}>{day}, </size>" +
               $"<size={normalsize}>{year}</size>" +
               $"<br>{textsInfo[curindex]}";
        }

        if (limitTMPLinesExpandWidth != null)
        {
            limitTMPLinesExpandWidth.SetTextWithLineLimit(tmpText.text);
        }

        textFlyInEffect.StartFlyInEffect();
    }

    /// <summary>
    /// 清理所有缓存的纹理
    /// </summary>
    private void OnDestroy()
    {
        foreach (var kvp in textureCache)
        {
            if (kvp.Value != null && kvp.Value != defaultTexture)
            {
                DestroyImmediate(kvp.Value);
            }
        }
        textureCache.Clear();
    }

    /// <summary>
    /// 获取缓存状态信息 - 用于调试
    /// </summary>
    /// <returns></returns>
    public string GetCacheInfo()
    {
        return $"缓存大小: {textureCache.Count}/{cacheSize}, 当前索引: {index}";
    }
}