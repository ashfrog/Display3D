// ReflectionPlane.cs
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ReflectionPlane : MonoBehaviour
{
    public float reflectionStrength = 0.5f;
    private Material material;

    private void Start()
    {
        material = GetComponent<MeshRenderer>().material;
        material.SetFloat("_ReflectionStrength", reflectionStrength);
    }
}