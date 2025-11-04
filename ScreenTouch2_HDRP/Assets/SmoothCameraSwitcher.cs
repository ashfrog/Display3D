using UnityEngine;

/// <summary>
/// 平滑切换相机Y位置 切换展开和还原的2个位置和角度
/// </summary>
public class SmoothCameraSwitcher : MonoBehaviour
{
    [SerializeField]
    SmoothOrbitManipulator smoothOrbitManipulator;

    [Header("拆开状态位置")]
    public float openDistance;
    [Header("拆开状态角度")]
    public Vector2 openOrbitAngles;
    [Header("拆开位置偏移")]
    public Vector3 openPanOffset = Vector3.zero;

    [Header("组装状态位置")]
    public float closeDistance;
    [Header("组装状态角度")]
    public Vector2 closeOrbitAngles;

    [Header("组装位置偏移")]
    public Vector3 closePanOffset = Vector3.zero;

    // 测试接口：你可以这样在别的脚本/Inspector调用
    [ContextMenu("Open")]
    public void OpenPosRot()
    {
        smoothOrbitManipulator.desiredDistance = openDistance;
        smoothOrbitManipulator.desiredOrbitAngles = openOrbitAngles;
        smoothOrbitManipulator.desiredPanOffset = openPanOffset;
    }

    [ContextMenu("Close")]
    public void ClosePosRot()
    {
        smoothOrbitManipulator.desiredDistance = closeDistance;
        smoothOrbitManipulator.desiredOrbitAngles = closeOrbitAngles;
        smoothOrbitManipulator.desiredPanOffset = closePanOffset;
    }
}