using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 根据选中模块展示设置相机位置旋转缩放
/// </summary>
public class ToggleGroupListener : MonoBehaviour
{
    [Header("ToggleGroup,切换相机target")]
    [SerializeField]
    private ToggleGroup toggleGroup;

    [SerializeField]
    private ToggleGroupTMPLabelStyler toggleGroupTMPLabelStyler;

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
        面套 = 10,
        泡沫 = 20,
        骨架 = 30,
        骨架展开 = 31,
        舒适系统 = 40,
        ECU = 50,
    }

    void Awake()
    {
        // 不做操作，推荐逻辑交由TMPLabelStyler处理
    }

    void Start()
    {
        if (toggleGroup == null)
        {
            toggleGroup = GetComponent<ToggleGroup>();
        }

        // 可以不在这里绑定Toggle的事件，由TMPLabelStyler统一处理
        // 如果有特殊逻辑需要，依然可自定义监听
        foreach (var toggle in toggleGroup.GetComponentsInChildren<Toggle>(true))
        {
            toggle.onValueChanged.AddListener((isOn) => OnToggleValueChanged(toggle, isOn));
        }
    }

    void OnEnable()
    {
        // 进入页面都取消全选
        if (toggleGroupTMPLabelStyler != null)
        {
            toggleGroupTMPLabelStyler.ForceUnselectAll();
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
                aniSwitch.SetAniStatue((int)AniStatue.骨架);
                break;
            case "舒适系统":
                aniSwitch.SetAniStatue(3);
                smoothCameraSwitcher.SetToStatueIndex(0);
                aniSwitch.SetAniStatue((int)AniStatue.舒适系统);
                break;
            case "ECU":
                aniSwitch.Close();
                smoothCameraSwitcher.SetToStatueIndex(2);//相机位置缩放数据
                aniSwitch.SetAniStatue((int)AniStatue.ECU);
                break;
            default:
                aniSwitch.Close();
                smoothCameraSwitcher.SetToStatueIndex(0);//相机位置缩放数据
                break;
        }
    }

    /// <summary>
    /// 对外暴露——全部取消Toggle选中（如外部按钮等调用），一律交给TMPLabelStyler处理
    /// </summary>
    public void UnselectAllToggles()
    {
        if (toggleGroupTMPLabelStyler != null)
        {
            toggleGroupTMPLabelStyler.ForceUnselectAll();
            aniSwitch.Close();
            smoothCameraSwitcher.SetToStatueIndex(0);
            Debug.Log("已取消所有Toggle的选中状态。");
        }
    }
}