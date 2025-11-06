using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ToggleGroupTMPLabelStyler : MonoBehaviour
{
    [Header("选中与未选中状态的颜色和样式")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.green;
    public FontStyles normalStyle = FontStyles.Normal;
    public FontStyles selectedStyle = FontStyles.Bold;

    private ToggleGroup toggleGroup;
    private List<Toggle> toggles = new List<Toggle>();
    private bool initializing = true;

    void Start()
    {
        toggleGroup = GetComponent<ToggleGroup>();
        if (toggleGroup != null)
            toggleGroup.allowSwitchOff = true;

        toggles.AddRange(GetComponentsInChildren<Toggle>());

        // 全部置为未选中
        foreach (var toggle in toggles)
            toggle.isOn = false;

        // 添加监听器
        foreach (var toggle in toggles)
        {
            toggle.onValueChanged.AddListener((value) => OnToggleValueChanged(toggle, value));
        }

        initializing = false;
        UpdateAllToggleLabels();
    }

    void OnToggleValueChanged(Toggle toggled, bool value)
    {
        // 初始化阶段不处理
        if (initializing) return;
        if (!value)
        {
            // 检查是否所有 Toggle 都未选中（只允许初始出现这种情况）
            bool anyOn = false;
            foreach (var t in toggles)
            {
                if (t.isOn) { anyOn = true; break; }
            }
            // 如果本次操作后所有toggle都未选中（点自己的取消），强制设回选中
            if (!anyOn)
            {
                toggled.isOn = true; // 重新选中自己
                return;
            }
        }
        UpdateAllToggleLabels();
    }

    void UpdateAllToggleLabels()
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
}