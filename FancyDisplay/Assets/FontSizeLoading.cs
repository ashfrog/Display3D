using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FontSizeLoading : MonoBehaviour
{
    public TextMeshPro[] textMeshPros;

    private void Awake()
    {
        Debug.Log("Awake");
        //将TextMeshPro的字体大小设置为配置文件中的值
        string[] fontSizes = Settings.ini.Graphics.FontSize.Split(',');
        for (int i = 0; i < textMeshPros.Length; i++)
        {
            textMeshPros[i].fontSize = float.Parse(fontSizes[i]);
        }
        //将TextMeshPro的字体颜色设置为配置文件中的值
        string[] fontColors = Settings.ini.Graphics.FontColor.Split(',');
        for (int i = 0; i < textMeshPros.Length; i++)
        {
            textMeshPros[i].color = ColorUtility.TryParseHtmlString(fontColors[i], out Color color) ? color : Color.white;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        //按键F12 将textMeshPros的配置文件保存
        if (Input.GetKeyDown(KeyCode.F12))
        {
            string fontSizes = "";
            for (int i = 0; i < textMeshPros.Length; i++)
            {
                fontSizes += textMeshPros[i].fontSize + ",";
            }
            fontSizes = fontSizes.TrimEnd(',');
            Settings.ini.Graphics.FontSize = fontSizes;
            string fontColors = "";
            for (int i = 0; i < textMeshPros.Length; i++)
            {
                fontColors += "#" + ColorUtility.ToHtmlStringRGB(textMeshPros[i].color) + ",";
            }
            fontColors = fontColors.TrimEnd(',');
            Settings.ini.Graphics.FontColor = fontColors;
        }
    }
}