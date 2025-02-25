using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayBox : MonoBehaviour
{
    [SerializeField]
    public Renderer frontRenderer;

    public TextMeshPro[] xls_Texs;

    public void SetText(string name, string education, string school)
    {
        xls_Texs[0].text = name;
        xls_Texs[1].text = education;
        xls_Texs[2].text = school;
    }
}