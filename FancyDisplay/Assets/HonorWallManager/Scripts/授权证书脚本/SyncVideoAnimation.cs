using UnityEngine;
// 如果使用的是 RenderHeads.Media.AVProVideo，请保证引入了对应命名空间
using RenderHeads.Media.AVProVideo;

public class SyncVideoAnimation : MonoBehaviour
{
    [Header("AVPro Components")]
    public MediaPlayer mediaPlayer;

    [Header("Animator Components")]
    public Animator animator;
    public string animationStateName;
    [Range(-1f, 1f)]
    public float offset = 0f;

    private void Update()
    {
        if (mediaPlayer == null || animator == null || mediaPlayer.Info == null) return;

        float duration = mediaPlayer.Info.GetDurationMs();
        if (duration <= 0) return;

        float normalized = Mathf.Repeat((float)(mediaPlayer.Control.GetCurrentTimeMs() / duration) + offset, 1f);

        if (animator.GetCurrentAnimatorStateInfo(0).IsName(animationStateName))
        {
            animator.Play(animationStateName, 0, normalized);
        }
    }
}
