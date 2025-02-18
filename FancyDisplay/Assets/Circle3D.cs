using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 一个用于展示荣誉墙的示例脚本：
/// 1. 可将多张奖状或荣誉图片对象在Inspector中进行序列化设置。
/// 2. 在Update中使用简单的旋转或动画来制造“炫酷”效果。
/// 3. 可在实际项目中根据需要，结合Unity的UI、实例化特效、Shader等进行更复杂的可视化处理。
/// </summary>
public class Circle3D : MonoBehaviour
{
    [Tooltip("所有荣誉项的容器，可以是UI Image或者3D平面等")]
    public List<GameObject> honorItems;

    [Tooltip("整体旋转速度")]
    public float rotationSpeed = 20f;

    [Tooltip("展开时的半径（如果是3D平面环绕，可以考虑环绕半径）")]
    public float radius = 5f;

    // 内部缓存：将每个荣誉项均匀分布在圆周上
    private void Start()
    {
        string abc = null + "abc";
        Debug.Log(abc);
        if (honorItems == null || honorItems.Count == 0) return;

        float angleStep = 360f / honorItems.Count;
        for (int i = 0; i < honorItems.Count; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            // 按圆周分布
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            honorItems[i].transform.localPosition = pos;
            // 面向圆心，避免图像偏离
            honorItems[i].transform.LookAt(transform);
        }
    }

    // 在Update中实现旋转或其他动画效果
    private void Update()
    {
        // 让整个物体绕Y轴旋转
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}