using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// ToggleGroup 中的 Toggle 标签样式控制器
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

    void Start()
    {
        toggleGroup = GetComponent<ToggleGroup>();

        if (toggleGroup != null)
            toggleGroup.allowSwitchOff = true;

        toggles.AddRange(GetComponentsInChildren<Toggle>());

        // 添加监听器
        foreach (var toggle in toggles)
        {
            toggle.onValueChanged.AddListener((value) => OnToggleValueChanged(toggle, value));
        }

        UpdateAllToggleLabels();
    }



    void OnToggleValueChanged(Toggle toggled, bool value)
    {
        // 初始化阶段不处理
        UpdateAllToggleLabels();
    }

    public void UpdateAllToggleLabels()
    {
        // 临时允许全部不选
        toggleGroup.allowSwitchOff = true;
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