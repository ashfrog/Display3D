using RenderHeads.Media.AVProVideo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 根据视频时间调节动画
/// </summary>
public class MediaTime : MonoBehaviour
{
    public MediaPlayer mediaPlayer;
    public float framerate = 25; // 帧率
    public string frameTime;

    [Range(0, 1)]
    public float progressBar; // Progress bar for controlling playback time

    private bool isDragging = false;

    void Update()
    {
        if (mediaPlayer != null && mediaPlayer.Control != null)
        {
            // Get the current playback time in milliseconds
            float currentTimeMs = mediaPlayer.Control.GetCurrentTimeMs();
            float currentTimeSec = currentTimeMs / 1000f; // 秒

            // Calculate the video duration in seconds
            float videoDurationSec = mediaPlayer.Info.GetDurationMs() / 1000f;

            // Update the progress bar if it is not being dragged
            if (!isDragging)
            {
                progressBar = currentTimeSec / videoDurationSec;
            }

            int hours = Mathf.FloorToInt(currentTimeSec / 3600f);
            int minutes = Mathf.FloorToInt((currentTimeSec % 3600f) / 60f);
            int seconds = Mathf.FloorToInt(currentTimeSec % 60f);
            int frames = Mathf.FloorToInt((currentTimeSec - Mathf.Floor(currentTimeSec)) * framerate); // 帧数

            // Format the time string to "HH:MM:SS:FF"
            frameTime = string.Format("{0:D2}:{1:D2}:{2:D2}:{3:D2}", hours, minutes, seconds, frames);
        }
    }

    void OnValidate()
    {
        if (mediaPlayer != null && mediaPlayer.Control != null && !isDragging)
        {
            // Calculate the video duration in seconds
            float videoDurationSec = mediaPlayer.Info.GetDurationMs() / 1000f;

            // Set the video playback time based on the progress bar value
            mediaPlayer.Control.Seek(progressBar * videoDurationSec * 1000f); // Convert seconds to milliseconds

            mediaPlayer.Control.Pause();
        }
    }

    public void OnPointerDown()
    {
        isDragging = true;
    }

    public void OnPointerUp()
    {
        isDragging = false;
        OnValidate();
    }
}