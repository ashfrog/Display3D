using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System;

public class TextFlyInEffect : MonoBehaviour
{
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private float characterDelay = 0.05f;
    [SerializeField] private float flyInDuration = 0.5f;
    [SerializeField] private float flyInDistance = 100f;
    [SerializeField] private Ease easeType = Ease.OutQuad;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private float randomDelayMax = 0.5f;
    [SerializeField][Range(0f, 1f)] private float randomnessIntensity = 0.5f;

    private string fullText;
    private Vector3[] originalPositions;
    private Color[] originalColors;
    private Sequence mainSequence;

    void Start()
    {
        if (playOnStart)
        {
            StartFlyInEffect();
        }
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            targetText.text = "时维九月，序属三秋。潦水尽而寒潭清，烟光凝而暮山紫。俨骖騑于上路，访风景于崇阿；临帝子之长洲，得天人之旧馆。层峦耸翠，上出重霄；飞阁流丹，下临无地。鹤汀凫渚，穷岛屿之萦回；桂殿兰宫，即冈峦之体势。\r\n\r\n披绣闼，俯雕甍，山原旷其盈视，川泽纡其骇瞩。闾阎扑地，钟鸣鼎食之家；舸舰弥津，青雀黄龙之舳。云销雨霁，彩彻区明。落霞与孤鹜齐飞，秋水共长天一色。渔舟唱晚，响穷彭蠡之滨；雁阵惊寒，声断衡阳之浦。\r\n\r\n遥襟甫畅，逸兴遄飞。爽籁发而清风生，纤歌凝而白云遏。睢园绿竹，气凌彭泽之樽；邺水朱华，光照临川之笔。四美具，二难并。穷睇眄于中天，极娱游于暇日。天高地迥，觉宇宙之无穷；兴尽悲来，识盈虚之有数。望长安于日下，目吴会于云间。地势极而南溟深，天柱高而北辰远。关山难越，谁悲失路之人？萍水相逢，尽是他乡之客。怀帝阍而不见，奉宣室以何年？";
            StartFlyInEffect();
        }
    }

    public void StartFlyInEffect()
    {
        if (targetText == null)
        {
            Debug.LogError("TextMeshPro component is not assigned!");
            return;
        }

        // Store original text and clear it
        fullText = targetText.text;
        targetText.text = fullText;
        targetText.ForceMeshUpdate();

        // Store original character positions and set alpha to 0
        TMP_TextInfo textInfo = targetText.textInfo;
        int characterCount = textInfo.characterCount;

        originalPositions = new Vector3[characterCount];
        originalColors = new Color[characterCount];

        // Create a new sequence
        mainSequence = DOTween.Sequence();

        for (int i = 0; i < characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;

            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
            int vertexIndex = textInfo.characterInfo[i].vertexIndex;

            // Store original position and color
            originalPositions[i] = (textInfo.meshInfo[materialIndex].vertices[vertexIndex] +
                                   textInfo.meshInfo[materialIndex].vertices[vertexIndex + 1] +
                                   textInfo.meshInfo[materialIndex].vertices[vertexIndex + 2] +
                                   textInfo.meshInfo[materialIndex].vertices[vertexIndex + 3]) / 4f;

            originalColors[i] = textInfo.characterInfo[i].color;

            // Make character invisible initially
            SetCharacterAlpha(i, 0f);

            // Position character in front of the screen (only changing Z axis)
            Vector3 offset = new Vector3(0, 0, -flyInDistance);
            OffsetCharacter(i, offset);
        }

        // Update the mesh
        targetText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);

        // Animate characters one by one from left to right with randomness
        System.Random random = new System.Random();
        float[] randomDelays = new float[characterCount];

        // Generate random delays for each character
        for (int i = 0; i < characterCount; i++)
        {
            randomDelays[i] = (float)random.NextDouble() * randomDelayMax * randomnessIntensity;
        }

        for (int i = 0; i < characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;

            // Create a separate sequence for each character
            Sequence charSequence = DOTween.Sequence();

            int charIndex = i; // Need to capture the current index for the closure

            // Animate character alpha
            charSequence.Append(DOTween.To(() => 0f, value => SetCharacterAlpha(charIndex, value), 1f, flyInDuration)
                        .SetEase(easeType));

            // Animate character position from front to original (only Z axis)
            charSequence.Join(DOTween.To(
                () => -flyInDistance,
                value => OffsetCharacter(charIndex, new Vector3(0, 0, value)),
                0f,
                flyInDuration
            ).SetEase(easeType));

            // Add this character's sequence to the main sequence with a delay based on character index + random delay
            mainSequence.Insert(characterDelay * i + randomDelays[i], charSequence);
        }

        // Play the sequence
        mainSequence.Play();
    }

    private void SetCharacterAlpha(int charIndex, float alpha)
    {
        if (targetText == null || charIndex >= targetText.textInfo.characterCount) return;

        TMP_TextInfo textInfo = targetText.textInfo;
        if (!textInfo.characterInfo[charIndex].isVisible) return;

        int materialIndex = textInfo.characterInfo[charIndex].materialReferenceIndex;
        int vertexIndex = textInfo.characterInfo[charIndex].vertexIndex;

        Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;

        vertexColors[vertexIndex + 0].a = (byte)(alpha * 255);
        vertexColors[vertexIndex + 1].a = (byte)(alpha * 255);
        vertexColors[vertexIndex + 2].a = (byte)(alpha * 255);
        vertexColors[vertexIndex + 3].a = (byte)(alpha * 255);

        targetText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    private void OffsetCharacter(int charIndex, Vector3 offset)
    {
        if (targetText == null || charIndex >= targetText.textInfo.characterCount) return;

        TMP_TextInfo textInfo = targetText.textInfo;
        if (!textInfo.characterInfo[charIndex].isVisible) return;

        int materialIndex = textInfo.characterInfo[charIndex].materialReferenceIndex;
        int vertexIndex = textInfo.characterInfo[charIndex].vertexIndex;

        Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

        Vector3 originalPos = originalPositions[charIndex];

        vertices[vertexIndex + 0] = textInfo.characterInfo[charIndex].bottomLeft + offset;
        vertices[vertexIndex + 1] = textInfo.characterInfo[charIndex].topLeft + offset;
        vertices[vertexIndex + 2] = textInfo.characterInfo[charIndex].topRight + offset;
        vertices[vertexIndex + 3] = textInfo.characterInfo[charIndex].bottomRight + offset;

        targetText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }

    public void StopEffect()
    {
        if (mainSequence != null)
        {
            mainSequence.Kill();
        }

        // Reset text to original state
        if (targetText != null)
        {
            targetText.text = fullText;
            targetText.ForceMeshUpdate();
        }
    }

    void OnDestroy()
    {
        StopEffect();
    }
}