using AmazingAssets.AdvancedDissolve;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AniClip : MonoBehaviour
{
    [SerializeField]
    MeshRenderer frontRender;
    Material material;

    // Start is called before the first frame update
    void Start()
    {
        material = frontRender.material;
    }

    // Update is called once per frame
    void Update()
    {
        AdvancedDissolveProperties.Cutout.Standard.UpdateLocalProperty(material, AdvancedDissolveProperties.Cutout.Standard.Property.Clip, 0f);


    }




}
