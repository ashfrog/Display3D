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

    void Awake()
    {
        toggleGroup = GetComponent<ToggleGroup>();
        toggles.AddRange(GetComponentsInChildren<Toggle>());

        foreach (var toggle in toggles)
        {
            toggle.onValueChanged.AddListener((_) => UpdateAllToggleLabels());
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