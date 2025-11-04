using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
/// <summary>
/// 根据选中模块展示设置相机位置旋转缩放
/// </summary>
public class ToggleGroupListener : MonoBehaviour
{
    [Header("ToggleGroup,切换相机target")]
    public ToggleGroup toggleGroup;

    [Header("看模块的时候组合位置")]
    public AniSwitch aniSwitch;
    [SerializeField]
    public Transform[] targets;

    public Transform target;

    public SmoothCameraSwitcher smoothCameraSwitcher;

    void Awake()
    {
        if (toggleGroup == null)
        {
            toggleGroup = GetComponent<ToggleGroup>();
        }

        // 获取 ToggleGroup 下的所有 Toggle 并添加监听
        foreach (var toggle in toggleGroup.GetComponentsInChildren<Toggle>(true))
        {
            toggle.onValueChanged.AddListener((isOn) => OnToggleValueChanged(toggle, isOn));
        }
    }

    /// <summary>
    /// 当任意Toggle变化（被选中时）触发
    /// </summary>
    private void OnToggleValueChanged(Toggle changedToggle, bool isOn)
    {
        if (!isOn) return; // 只关心被选中
        Debug.Log($"被选中的Toggle: {changedToggle.name}");

        // 你可以在这里根据不同toggle做出不同逻辑分支
        switch (changedToggle.name)
        {
            case "ECU":
                aniSwitch.Close();
                smoothCameraSwitcher.SetToStatueIndex(2);//第三个位置缩放数据为看ECU
                break;
            default:
                aniSwitch.Close();
                smoothCameraSwitcher.SetToStatueIndex(0);//默认的那个位置缩放数据
                break;
        }
    }
}