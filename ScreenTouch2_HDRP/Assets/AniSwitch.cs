using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AniSwitch : MonoBehaviour
{
    [Header("动画Controller")]
    [SerializeField]
    private Animator _animator;

    public int State;

    void Start()
    {
        // 初始化状态（确保参数与默认状态一致）
        State = 0;
        _animator.SetInteger("Statue", State);
    }

    public void Open()
    {
        State = 1;
        _animator.SetInteger("Statue", State);
    }
    public void Close()
    {
        State = 0;
        _animator.SetInteger("Statue", State);
    }

    public void SetAniStatue(int statue)
    {
        State = statue;
        _animator.SetInteger("Statue", State);
    }
}