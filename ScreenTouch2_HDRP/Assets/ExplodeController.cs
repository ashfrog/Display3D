using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExplodeController : MonoBehaviour
{
    public float explodeRadius = 3f;         // 炸开时的半径
    public float animationDuration = 1f;     // 动画时长（秒）
    public bool useSphere = false;           // 是否球面分布，否则为圆形
    public Vector3 planeNormal = Vector3.up; // 如果是平面，决定圆的法线方向

    private List<Transform> children = new List<Transform>();
    private List<Vector3> originalPositions = new List<Vector3>();
    private Coroutine animCoroutine;
    private bool exploded = false;

    void Start()
    {
        foreach (Transform child in transform)
        {
            children.Add(child);
            originalPositions.Add(child.localPosition);
        }
    }

    [ContextMenu("Explode")]
    public void Explode()
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateExplode(true));
    }

    [ContextMenu("Reset")]
    public void ResetAll()
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateExplode(false));
    }

    IEnumerator AnimateExplode(bool toExplode)
    {
        float time = 0;
        List<Vector3> startPositions = new List<Vector3>();
        List<Vector3> endPositions = new List<Vector3>();
        int count = children.Count;
        Vector3 center = Vector3.zero;
        for (int i = 0; i < count; i++) center += originalPositions[i];
        center /= count;

        for (int i = 0; i < count; i++)
        {
            startPositions.Add(children[i].localPosition);

            if (toExplode)
            {
                Vector3 dir;
                if (useSphere)
                {
                    // 均匀分布在球面
                    float phi = Mathf.Acos(1 - 2 * (i + 0.5f) / count);
                    float theta = Mathf.PI * (1 + Mathf.Pow(5, 0.5f)) * (i + 0.5f);
                    dir = new Vector3(Mathf.Sin(phi) * Mathf.Cos(theta), Mathf.Cos(phi), Mathf.Sin(phi) * Mathf.Sin(theta));
                }
                else
                {
                    // 均匀分布在圆周
                    float angle = i * Mathf.PI * 2f / count;
                    Quaternion rot = Quaternion.FromToRotation(Vector3.up, planeNormal.normalized);
                    dir = rot * new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                }
                endPositions.Add(center + dir * explodeRadius);
            }
            else
            {
                endPositions.Add(originalPositions[i]);
            }
        }

        while (time < animationDuration)
        {
            float t = time / animationDuration;
            t = Mathf.SmoothStep(0, 1, t);
            for (int i = 0; i < count; i++)
            {
                children[i].localPosition = Vector3.Lerp(startPositions[i], endPositions[i], t);
            }
            time += Time.deltaTime;
            yield return null;
        }
        for (int i = 0; i < count; i++)
        {
            children[i].localPosition = endPositions[i];
        }
        exploded = toExplode;
        animCoroutine = null;
    }
}