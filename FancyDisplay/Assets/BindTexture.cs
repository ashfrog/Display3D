using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BindTexture : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer meshRenderer;
    [SerializeField]
    private Camera displayCamera;
    // Start is called before the first frame update
    void Start()
    {
        displayCamera.targetTexture = meshRenderer.material.mainTexture as RenderTexture;

    }

    // Update is called once per frame
    void Update()
    {

    }
}
