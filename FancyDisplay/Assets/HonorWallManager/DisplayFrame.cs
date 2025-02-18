// DisplayFrame.cs
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class DisplayFrame : MonoBehaviour
{
    private Material material;

    private void Start()
    {
        material = GetComponent<MeshRenderer>().material;
    }

    public void SetContent(Texture2D content)
    {
        material.mainTexture = content;
    }
}