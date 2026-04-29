
using UnityEngine;

public class DualScreenManager : MonoBehaviour
{
    void Start()
    {
        // 检测可用的显示器数量
        Debug.Log("可用显示器数量: " + Display.displays.Length);

        // 激活所有显示器
        ActivateAllDisplays();
    }

    // 方法1: 激活所有显示器
    void ActivateAllDisplays()
    {
        for (int i = 0; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
            Debug.Log("激活显示器 " + i);
        }
    }


}