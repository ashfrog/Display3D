using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class LimitTMPLinesExpandWidth : MonoBehaviour
{
    int maxLines = 6;
    float widthIncrement = 0.2f;
    float maxWidth = 8f;

    private TMP_Text tmp;
    private RectTransform rectTransform;

    float startWidth;

    void Awake()
    {

        tmp = GetComponent<TMP_Text>();
        rectTransform = GetComponent<RectTransform>();

        // Reset to starting width if needed
        startWidth = rectTransform.sizeDelta.x;
    }

    public void SetTextWithLineLimit(string text)
    {
        tmp.text = text;
        tmp.ForceMeshUpdate();
        rectTransform.sizeDelta = new Vector2(startWidth, rectTransform.sizeDelta.y);
        Debug.Log(tmp.textInfo.lineCount);
        while (tmp.textInfo.lineCount > maxLines && rectTransform.sizeDelta.x < maxWidth)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x + widthIncrement, rectTransform.sizeDelta.y);
            // 强制刷新
            tmp.ForceMeshUpdate();
        }
        Debug.Log("limit 完成");
    }
}