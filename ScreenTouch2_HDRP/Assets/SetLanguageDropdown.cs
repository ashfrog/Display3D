using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using System.Collections;
using System.Collections.Generic;

public class SetLanguageDropdown : MonoBehaviour
{
    public Dropdown languageDropdown; // 关联你的 Dropdown 组件

    List<Locale> locales;

    void Start()
    {
        StartCoroutine(SetupDropdown());
    }

    IEnumerator SetupDropdown()
    {
        // 确保 LocalizationSettings 已初始化
        yield return LocalizationSettings.InitializationOperation;

        locales = LocalizationSettings.AvailableLocales.Locales;

        // 清空现有选项
        languageDropdown.ClearOptions();

        // 加入所有语言名称
        List<string> options = new List<string>();
        int currentLocaleIndex = 0;

        string savedLocale = PlayerPrefs.GetString("selected_locale", null);
        if (!string.IsNullOrEmpty(savedLocale))
        {
            var locale = locales.Find(l => l.Identifier.Code == savedLocale);
            if (locale != null)
                LocalizationSettings.SelectedLocale = locale;
        }

        for (int i = 0; i < locales.Count; i++)
        {
            options.Add(locales[i].Identifier.CultureInfo.NativeName);
            if (locales[i] == LocalizationSettings.SelectedLocale)
                currentLocaleIndex = i;
        }

        languageDropdown.AddOptions(options);
        languageDropdown.value = currentLocaleIndex;
        languageDropdown.RefreshShownValue();

        // 监听下拉选项变化
        languageDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    void OnDropdownValueChanged(int index)
    {
        LocalizationSettings.SelectedLocale = locales[index];
        PlayerPrefs.SetString("selected_locale", locales[index].Identifier.Code);
    }
}