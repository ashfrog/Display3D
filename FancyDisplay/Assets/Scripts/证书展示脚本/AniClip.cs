using AmazingAssets.AdvancedDissolve;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 通过指定clip控制shader渐变进度
/// </summary>
public class AniClip : MonoBehaviour
{
    [SerializeField]
    MeshRenderer frontRender;
    Material material;

    public float clip = 0f;

    public float A = 1f;

    // Start is called before the first frame update
    void Start()
    {
        material = frontRender.material;
    }

    // Update is called once per frame
    void Update()
    {
        //应用溶解进度
        AdvancedDissolveProperties.Cutout.Standard.UpdateLocalProperty(material, AdvancedDissolveProperties.Cutout.Standard.Property.Clip, clip);

        // 应用透明度
        Color color = material.color;
        color.a = A;
        material.color = color;
    }
}
