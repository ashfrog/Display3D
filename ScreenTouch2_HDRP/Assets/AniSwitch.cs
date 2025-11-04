using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AniSwitch : MonoBehaviour
{
    [Header("拆开动画")]
    [SerializeField]
    private Animator _animator;
    private bool _isOpen = false;

    void Start()
    {
        // 初始化状态（确保参数与默认状态一致）
        _animator.SetBool("IsOpen", _isOpen);
    }

    public void Open()
    {
        _isOpen = true;
        _animator.SetBool("IsOpen", _isOpen);
    }
    public void Close()
    {
        _isOpen = false;
        _animator.SetBool("IsOpen", _isOpen);
    }

    void Update()
    {
        // 按 E 键直接切换状态，无需锁定（中途可反向）
        if (Input.GetKeyDown(KeyCode.E))
        {
            _isOpen = !_isOpen;
            _animator.SetBool("IsOpen", _isOpen); // 实时更新参数，触发反向过渡
        }
    }
}