using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class ToggleGroupListener : MonoBehaviour
{
    [Header("ToggleGroup,切换相机target")]
    public ToggleGroup toggleGroup;

    [Header("看模块的时候组合位置")]
    public AniSwitch aniSwitch;
    [SerializeField]
    public Transform[] targets;

    public Transform target;

    public SmoothOrbitManipulator smoothOrbitManipulator;

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

    public void SetOrbitCameraDefaultTarget()
    {
        smoothOrbitManipulator.target = target;
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
                smoothOrbitManipulator.target = targets[0];
                aniSwitch.Close();
                break;
            default:
                SetOrbitCameraDefaultTarget();
                aniSwitch.Close();
                break;
        }
    }
}