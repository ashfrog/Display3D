using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AniSwitch : MonoBehaviour
{
    [Header("动画Controller")]
    [SerializeField]
    private Animator _animator;

    void Start()
    {
        // 初始化状态（确保参数与默认状态一致）
        _animator.SetInteger("Statue", 0);
    }

    public void Open()
    {
        _animator.SetInteger("Statue", 1);
    }
    public void Close()
    {
        _animator.SetInteger("Statue", 2);
    }

    public void Open3()
    {
        _animator.SetInteger("Statue", 3);
    }
}