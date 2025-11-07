using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// ToggleGroup 中的 Toggle 标签样式和选中控制器
/// </summary>
public class ToggleGroupTMPLabelStyler : MonoBehaviour
{
    [Header("选中与未选中状态的颜色和样式")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.green;
    public FontStyles normalStyle = FontStyles.Normal;
    public FontStyles selectedStyle = FontStyles.Bold;

    private ToggleGroup toggleGroup;
    private List<Toggle> toggles = new List<Toggle>();

    void Awake()
    {
        // 获取 ToggleGroup
        toggleGroup = GetComponent<ToggleGroup>();
        toggles.Clear();
        toggles.AddRange(GetComponentsInChildren<Toggle>(true));

    }

    void Start()
    {
        foreach (var toggle in toggles)
        {
            // 移除之前的监听，防止重复绑定
            toggle.onValueChanged.RemoveListener(OnAnyToggleValueChanged);
            toggle.onValueChanged.AddListener(OnAnyToggleValueChanged);
        }

        UpdateAllToggleLabels();
    }

    void OnEnable()
    {
        StartCoroutine(delayDisAllowSwitchOff());
    }
    IEnumerator delayDisAllowSwitchOff()
    {
        yield return null; //必须等一帧allowSwitchOff才不会被系统默认选中一个
        toggleGroup.allowSwitchOff = false;
    }

    private void OnAnyToggleValueChanged(bool value)
    {
        // 只负责更新视觉，不控制业务逻辑
        UpdateAllToggleLabels();
    }

    /// <summary>
    /// 更新所有Toggle的Label样式
    /// </summary>
    public void UpdateAllToggleLabels()
    {
        foreach (var toggle in toggles)
        {
            TMP_Text label = toggle.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                if (toggle.isOn)
                {
                    label.color = selectedColor;
                    label.fontStyle = selectedStyle;
                }
                else
                {
                    label.color = normalColor;
                    label.fontStyle = normalStyle;
                }
            }
        }
    }

    /// <summary>
    /// 彻底取消所有选中，无论当前限制（用于初始化和外部业务控制）
    /// </summary>
    public void ForceUnselectAll()
    {
        if (toggleGroup != null)
            toggleGroup.allowSwitchOff = true;


        foreach (var toggle in toggles)
        {
            toggle.isOn = false;
        }
        UpdateAllToggleLabels();
    }

}