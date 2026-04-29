using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class SetLanguageSingleToggle : MonoBehaviour
{
    public Toggle languageToggle;            // 只有一个Toggle
    public TMP_FontAsset englishFont;
    public TMP_FontAsset chineseFont;

    Locale chineseLocale;
    Locale englishLocale;

    void Start()
    {
        StartCoroutine(SetupToggle());
    }

    IEnumerator SetupToggle()
    {
        yield return LocalizationSettings.InitializationOperation;

        var locales = LocalizationSettings.AvailableLocales.Locales;
        chineseLocale = locales.Find(l => l.Identifier.Code.StartsWith("zh"));
        englishLocale = locales.Find(l => l.Identifier.Code.StartsWith("en"));

        string savedLocale = PlayerPrefs.GetString("selected_locale", null);
        if (!string.IsNullOrEmpty(savedLocale))
        {
            var locale = locales.Find(l => l.Identifier.Code == savedLocale);
            if (locale != null)
                LocalizationSettings.SelectedLocale = locale;
        }

        // 设置Toggle状态但不触发回调
        if (LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("en"))
            languageToggle.isOn = true;
        else
            languageToggle.isOn = false;

        languageToggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        OnLocaleChanged(LocalizationSettings.SelectedLocale); // 初始化时设置一次字体
    }

    void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    void OnToggleValueChanged(bool isOn)
    {
        if (isOn && englishLocale != null)
        {
            LocalizationSettings.SelectedLocale = englishLocale;
            PlayerPrefs.SetString("selected_locale", englishLocale.Identifier.Code);
        }
        else if (!isOn && chineseLocale != null)
        {
            LocalizationSettings.SelectedLocale = chineseLocale;
            PlayerPrefs.SetString("selected_locale", chineseLocale.Identifier.Code);
        }
    }

    void OnLocaleChanged(Locale locale)
    {
        TMP_FontAsset targetFont = null;
        if (locale.Identifier.Code.StartsWith("zh"))
            targetFont = chineseFont;
        else if (locale.Identifier.Code.StartsWith("en"))
            targetFont = englishFont;

        if (targetFont != null)
        {
            foreach (var tmp in FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None))
            {
                tmp.font = targetFont;
            }
        }
    }
}