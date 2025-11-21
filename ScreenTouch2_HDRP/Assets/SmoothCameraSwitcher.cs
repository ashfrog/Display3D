using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 设置相机缩放旋转偏移位置 通过参数
/// </summary>
public class SmoothCameraSwitcher : MonoBehaviour
{
    [SerializeField]
    SmoothOrbitManipulator smoothOrbitManipulator;

    [System.Serializable]
    public struct DesiredStatue
    {
        [Header("索引")]
        public string nameKey;
        [Header("状态位置")]
        public float distance;
        [Header("状态角度")]
        public Vector2 orbitAngles;
        [Header("位置偏移")]
        public Vector3 panOffset;
        [Header("目标点")]
        public Transform target;
    }

    [Header("当前 DesiredStatues Index")]
    [SerializeField]
    private int curIndex;
    public List<DesiredStatue> desiredStatues;

    private void Start()
    {
        Debug.Log("根据DesiredStatue配置初始化相机位置");
        SetToStatueIndex(0);
    }


    [ContextMenu("Open")]
    public void OpenPosRot()
    {
        SetToStatueIndex(1);
    }
    [ContextMenu("Close")]
    public void ClosePosRot()
    {
        SetToStatueIndex(0);
    }

    private void ResetStatue(DesiredStatue desiredStatue)
    {
        smoothOrbitManipulator.desiredDistance = desiredStatue.distance;
        smoothOrbitManipulator.desiredOrbitAngles = desiredStatue.orbitAngles;
        smoothOrbitManipulator.desiredPanOffset = desiredStatue.panOffset;
        smoothOrbitManipulator.target = desiredStatue.target;
    }
    public void SetToStatueByKey(string key)
    {
        for (int i = 0; i < desiredStatues.Count; i++)
        {
            if (string.Equals(key, desiredStatues[i].nameKey, System.StringComparison.OrdinalIgnoreCase))
            {
                curIndex = i;
            }
        }
        DesiredStatue desiredStatue = desiredStatues[curIndex];
        ResetStatue(desiredStatue);
    }
    public void SetToStatueIndex(int index)
    {
        if (index < 0 || index >= desiredStatues.Count)
        {
            Debug.LogError("索引超出范围");
            return;
        }
        DesiredStatue desiredStatue = desiredStatues[index];
        ResetStatue(desiredStatue);
        curIndex = index;
    }
}