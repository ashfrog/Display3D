using UnityEngine;
// 如果使用的是 RenderHeads.Media.AVProVideo，请保证引入了对应命名空间
using RenderHeads.Media.AVProVideo;

public class SyncVideoAnimation : MonoBehaviour
{
    [Header("AVPro Components")]
    public MediaPlayer mediaPlayer;    // 拖拽你的 AVPro MediaPlayer

    [Header("Animator Components")]
    public Animator animator;          // 拖拽你的 Animator
    public string animationStateName;  // 需要同步的动画状态名称
    [Range(0f, 1f)]
    public float offset = 0f;          // 可选：在同步基础上的偏移

    private void Update()
    {
        if (mediaPlayer == null || animator == null) return;

        // 确保视频已经加载，且有有效的持续时长
        if (mediaPlayer.Info != null && mediaPlayer.Info.GetDurationMs() > 0)
        {
            double currentTime = mediaPlayer.Control.GetCurrentTimeMs();
            double duration = mediaPlayer.Info.GetDurationMs();

            // 归一化进度，范围在 [0, 1]
            float normalized = (float)(currentTime / duration);

            // 可选：加上偏移，并取模以保证在 [0, 1] 内循环
            normalized = Mathf.Repeat(normalized + offset, 1f);

            // 需要同步的动画层索引（如果是默认层，可直接用 0）
            int layerIndex = 0;

            // 获取动画状态信息
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);

            // 如果当前 Animator 处于指定的状态，则设置 NormalizedTime(动画归一化时间)
            if (stateInfo.IsName(animationStateName))
            {
                // 用 normalized 赋值到动画上
                animator.Play(animationStateName, layerIndex, normalized);
            }
        }
    }
}