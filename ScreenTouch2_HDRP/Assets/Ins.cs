using RenderHeads.Media.AVProVideo;
using System.Collections.Generic;
using UnityEngine;

public class Ins : MonoBehaviour
{
    /// <summary>
    /// 底层UI
    /// </summary>
    public Camera UICamera;

    /// <summary>
    /// 顶层UI
    /// </summary>
    public Camera UICamera_Top;

    /// <summary>
    /// 3D相机
    /// </summary>
    public Camera mainCamera;

    /// <summary>
    /// 生成实例
    /// </summary>
    public GameObject objChair;

    public List<Canvas> uiCanvas;

    public SmoothOrbitManipulator SmoothOrbitManipulator;

    /// <summary>
    /// balance 左右声道设置
    /// </summary>
    public LitVCR litVCR;
}
