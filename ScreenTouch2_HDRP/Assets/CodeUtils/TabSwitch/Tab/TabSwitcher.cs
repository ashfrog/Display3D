using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;

[System.Serializable]
public class TabPageGroup
{
    [Tooltip("该Tab对应的所有页面")]
    public GameObject[] pages;

    [Tooltip("Tab类型名（下拉选择）")]
    public string tabType;
}

public class TabSwitcher : MonoBehaviour
{
    [Tooltip("可拖入Toggle或Button作为Tab")]
    public Selectable[] tabSelectables; // 允许Toggle或Button
    public int currentTabIndex = -1;

    [Header("Tab类型名（类似enum，可增删）")]
    public List<string> allTabTypes = new List<string> { "key1", "key2" };

    [Header("每个Tab下挂载的页面组")]
    public List<TabPageGroup> tabPageGroups = new List<TabPageGroup>();

    public bool initTabPages;

    // 防止递归事件触发
    private bool _suppressToggleCallback = false;

    private void Start()
    {
        for (int i = 0; i < tabSelectables.Length; i++)
        {
            int index = i;
            if (tabSelectables[i] is Toggle toggle)
            {
                toggle.onValueChanged.AddListener((isOn) =>
                {
                    if (_suppressToggleCallback) return;
                    if (isOn)
                    {
                        SwitchTab(index);
                    }
                });
            }
            else if (tabSelectables[i] is Button button)
            {
                button.onClick.AddListener(() =>
                {
                    SwitchTab(index);
                });
            }
        }
        if (initTabPages)
        {
            InitTabPages();
        }
        UpdateTabPages();
        // 设置Toggle选中状态（如果有有效的currentTabIndex）
        if (currentTabIndex >= 0 && currentTabIndex < tabSelectables.Length)
        {
            _suppressToggleCallback = true;
            for (int i = 0; i < tabSelectables.Length; i++)
            {
                if (tabSelectables[i] is Toggle toggle)
                    toggle.isOn = (i == currentTabIndex);
            }
            _suppressToggleCallback = false;
        }
    }

    public void SwitchTab(int index)
    {
        if (index == currentTabIndex)
            return; // 避免重复切换

        currentTabIndex = index;
        // 只让当前index对应的Toggle为On，其余为Off
        _suppressToggleCallback = true;
        for (int i = 0; i < tabSelectables.Length; i++)
        {
            if (tabSelectables[i] is Toggle toggle)
                toggle.isOn = (i == index);
            // Button一般不需要设置选中高亮，如需处理可扩展
        }
        _suppressToggleCallback = false;
        UpdateTabPages();
    }

    public void SwitchTab(Enum label)
    {
        SwitchTab(label.ToString());
        Debug.Log("切换页面:" + label.ToString());
    }

    public void Hide()
    {
        InitTabPages(false);
    }

    /// <summary>
    /// 通过Tab类型名切换
    /// </summary>
    public void SwitchTab(string tabTypeName)
    {
        for (int i = 0; i < tabPageGroups.Count; i++)
        {
            if (tabPageGroups[i].tabType == tabTypeName)
            {
                SwitchTab(i);
                return;
            }
        }
        Debug.LogWarning("TabType " + tabTypeName + " not found in tabPageGroups.");
    }

    /// <summary>
    /// 获取当前Tab的名称
    /// </summary>
    /// <returns>当前Tab的类型名，如果索引无效则返回空字符串</returns>
    public string GetCurrentTabName()
    {
        if (currentTabIndex >= 0 && currentTabIndex < tabPageGroups.Count)
        {
            return tabPageGroups[currentTabIndex].tabType;
        }
        return string.Empty;
    }

    private bool _updatingTabPages = false;

    private void UpdateTabPages()
    {
        if (_updatingTabPages) return;
        _updatingTabPages = true;

        try
        {
            HashSet<GameObject> currentActivePages = new HashSet<GameObject>();
            if (currentTabIndex >= 0 && currentTabIndex < tabPageGroups.Count)
            {
                var currentGroup = tabPageGroups[currentTabIndex];
                if (currentGroup.pages != null)
                {
                    foreach (var page in currentGroup.pages)
                    {
                        if (page != null)
                            currentActivePages.Add(page);
                    }
                }
            }

            HashSet<GameObject> allPages = new HashSet<GameObject>();
            foreach (var group in tabPageGroups)
            {
                if (group.pages != null)
                {
                    foreach (var page in group.pages)
                    {
                        if (page != null)
                            allPages.Add(page);
                    }
                }
            }

            foreach (var page in allPages)
            {
                bool shouldBeActive = currentActivePages.Contains(page);
                page.SetActive(shouldBeActive);
            }
        }
        finally
        {
            _updatingTabPages = false;
        }
    }

    private void InitTabPages(bool enable = true)
    {
        foreach (var group in tabPageGroups)
        {
            if (group.pages != null)
            {
                foreach (var page in group.pages)
                {
                    if (page != null)
                        page.SetActive(enable);
                }
            }
        }
    }
}