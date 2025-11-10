using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Components;

public class PageLanguageController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] Toggle languageToggle;          // true: English, false: Chinese
    [SerializeField] Transform pageRoot;             // 该页面根节点(不填默认=本对象)

    [Header("Fonts")]
    [SerializeField] TMP_FontAsset englishFont;
    [SerializeField] TMP_FontAsset chineseFont;

    [Header("Identity")]
    [SerializeField] string pageId;                  // 每个页面唯一ID，未填则自动用 SceneName/ObjectName

    Locale zhLocale;
    Locale enLocale;

    string PrefKey => $"selected_locale_{pageId}";

    void Awake()
    {
        if (pageRoot == null) pageRoot = transform;
        if (string.IsNullOrEmpty(pageId))
        {
            var sceneName = gameObject.scene.IsValid() ? gameObject.scene.name : "NoScene";
            pageId = $"{sceneName}/{gameObject.name}";
        }
    }

    void Start()
    {
        StartCoroutine(InitAndApply());
    }

    IEnumerator InitAndApply()
    {
        // 等待本地化系统初始化
        yield return LocalizationSettings.InitializationOperation;

        var locales = LocalizationSettings.AvailableLocales;
        // 尽量精确匹配，找不到再用 StartsWith
        enLocale = locales.GetLocale("en") ?? locales.Locales.Find(l => l.Identifier.Code.StartsWith("en"));
        zhLocale = locales.GetLocale("zh") ?? locales.Locales.Find(l => l.Identifier.Code.StartsWith("zh"));

        if (enLocale == null || zhLocale == null)
        {
            Debug.LogWarning("[PageLanguageController] Missing locales for 'en' or 'zh'. Check AvailableLocales.");
        }

        // 读取该页面上次选择
        var saved = PlayerPrefs.GetString(PrefKey, "en"); // 默认英文
        bool isEnglish = saved.StartsWith("en");

        // 设置 Toggle 而不触发回调
        if (languageToggle != null)
            languageToggle.SetIsOnWithoutNotify(isEnglish);

        // 应用到本页面
        ApplyLocaleToPage(isEnglish ? enLocale : zhLocale);

        if (languageToggle != null)
        {
            languageToggle.onValueChanged.AddListener(OnToggleChanged);
        }
    }

    void OnDestroy()
    {
        if (languageToggle != null)
            languageToggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    void OnToggleChanged(bool isOn)
    {
        var target = isOn ? enLocale : zhLocale;
        ApplyLocaleToPage(target);

        if (target != null)
        {
            PlayerPrefs.SetString(PrefKey, target.Identifier.Code);
            PlayerPrefs.Save();
        }
    }

    void ApplyLocaleToPage(Locale locale)
    {
        if (locale == null) return;

        // 1) 仅对本页面的 LocalizeStringEvent 设置 LocaleOverride 并刷新
        var stringEvents = pageRoot.GetComponentsInChildren<LocalizeStringEvent>(true);
        foreach (var e in stringEvents)
        {
            if (e == null) continue;
            // 针对字符串的 Locale 覆盖
            e.StringReference.LocaleOverride = locale;
            e.RefreshString(); // 立即刷新显示
        }

        // 如需对其他资源（图片/音频/Prefab）做同样覆盖，可按需添加：
        // var spriteEvents = pageRoot.GetComponentsInChildren<LocalizeSpriteEvent>(true);
        // foreach (var s in spriteEvents) { s.LocaleOverride = locale; s.RefreshAsset(); }
        // 其他类型类似：LocalizeTextureEvent / LocalizeAudioClipEvent / LocalizeGameObjectEvent

        // 2) 更新本页面的 TMP 字体
        TMP_FontAsset targetFont = locale.Identifier.Code.StartsWith("zh") ? chineseFont : englishFont;
        if (targetFont != null)
        {
            foreach (var tmp in pageRoot.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (tmp == null) continue;
                tmp.font = targetFont;
                // 可选：若不同语言使用不同材质
                // tmp.fontMaterial = targetFont.material;
            }
        }
    }
}