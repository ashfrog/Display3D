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
    [SerializeField]
    private ToggleGroup toggleGroup;

    [Header("看模块的时候组合位置")]
    [SerializeField]
    private AniSwitch aniSwitch;

    [Header("模型TabSwitcher")]
    [SerializeField]
    private TabSwitcher objTabSwitcher;

    [SerializeField]
    public Transform[] targets;

    public Transform target;

    public SmoothCameraSwitcher smoothCameraSwitcher;


    enum AniStatue
    {
        默认 = 0,
        拆开 = 1,
        面套 = 2,
        舒适系统 = 3,
        泡沫 = 4,
        零重力展示 = 5
    }

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
        //objTabSwitcher.SwitchTab(changedToggle.name);
        // 你可以在这里根据不同toggle做出不同逻辑分支
        switch (changedToggle.name)
        {
            case "面套":
                smoothCameraSwitcher.SetToStatueIndex(0);
                aniSwitch.SetAniStatue((int)AniStatue.面套);
                break;
            case "泡沫":
                smoothCameraSwitcher.SetToStatueIndex(0);
                aniSwitch.SetAniStatue((int)AniStatue.泡沫);
                break;
            case "骨架":
                smoothCameraSwitcher.SetToStatueIndex(0);
                aniSwitch.SetAniStatue((int)AniStatue.零重力展示);
                break;
            case "舒适系统":
                aniSwitch.SetAniStatue(3);
                smoothCameraSwitcher.SetToStatueIndex(0);
                aniSwitch.SetAniStatue((int)AniStatue.舒适系统);
                break;
            case "ECU":
                aniSwitch.Close();
                smoothCameraSwitcher.SetToStatueIndex(2);//相机位置缩放数据
                break;
            default:
                aniSwitch.Close();
                smoothCameraSwitcher.SetToStatueIndex(0);//相机位置缩放数据
                break;

        }
    }
}