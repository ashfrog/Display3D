using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StairUnfoldController : MonoBehaviour
{
    public float unfoldOffset = 2f; // 每一级展开时的距离
    public float animationDuration = 1f; // 动画时长（秒）
    public Vector3 unfoldDirection = Vector3.right; // 展开方向

    private List<Transform> children = new List<Transform>();
    private List<Vector3> originalPositions = new List<Vector3>();
    private bool isUnfolded = false;
    private Coroutine animCoroutine;

    void Start()
    {
        // 记录所有子物体及其原始本地位置
        foreach (Transform child in transform)
        {
            children.Add(child);
            originalPositions.Add(child.localPosition);
        }
    }

    [ContextMenu("Unfold")]
    public void Unfold()
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateToUnfold(true));
    }

    [ContextMenu("Fold")]
    public void Fold()
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateToUnfold(false));
    }

    IEnumerator AnimateToUnfold(bool toUnfold)
    {
        float time = 0;
        List<Vector3> startPositions = new List<Vector3>();
        List<Vector3> endPositions = new List<Vector3>();

        // 先计算目标展开位置
        List<Vector3> unfoldPositions = new List<Vector3>();
        if (toUnfold)
        {
            for (int i = 0; i < children.Count; i++)
            {
                unfoldPositions.Add(originalPositions[i] + unfoldDirection * unfoldOffset * i);
            }
            // 计算中心点
            Vector3 center = Vector3.zero;
            for (int i = 0; i < unfoldPositions.Count; i++)
            {
                center += unfoldPositions[i];
            }
            center /= unfoldPositions.Count;
            // 目标位置减去中心点
            for (int i = 0; i < unfoldPositions.Count; i++)
            {
                unfoldPositions[i] -= center;
            }
        }

        for (int i = 0; i < children.Count; i++)
        {
            startPositions.Add(children[i].localPosition);
            if (toUnfold)
                endPositions.Add(unfoldPositions[i]);
            else
                endPositions.Add(originalPositions[i]);
        }

        while (time < animationDuration)
        {
            float t = time / animationDuration;
            t = Mathf.SmoothStep(0, 1, t);
            for (int i = 0; i < children.Count; i++)
            {
                children[i].localPosition = Vector3.Lerp(startPositions[i], endPositions[i], t);
            }
            time += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < children.Count; i++)
        {
            children[i].localPosition = endPositions[i];
        }

        isUnfolded = toUnfold;
        animCoroutine = null;
    }
}