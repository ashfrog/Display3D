using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CertificateShowcase : MonoBehaviour
{
    [System.Serializable]
    public class Certificate
    {
        public GameObject certificateObject;
        public string certificateName;
        [TextArea(1, 3)]
        public string description;
    }

    [Header("证书设置")]
    public List<Certificate> certificates = new List<Certificate>();

    [Header("动画设置")]
    public Transform startPosition;    // 起始位置（在相机后方）
    public Transform displayPosition;  // 展示位置（在相机前方中央）
    public Transform endPosition;      // 结束位置（远处消失点）

    [Header("时间设置")]
    public float flyInDuration = 1.5f;    // 飞入时间
    public float displayDuration = 5.0f;  // 展示停留时间
    public float flyOutDuration = 2.0f;   // 飞出时间
    public float delayBetweenCertificates = 1.0f; // 证书间隔时间

    [Header("动画曲线")]
    public AnimationCurve flyInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve flyOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private int currentIndex = 0;
    private bool isAnimating = false;

    void Start()
    {
        // 确保所有证书初始状态为隐藏
        foreach (var cert in certificates)
        {
            if (cert.certificateObject != null)
            {
                cert.certificateObject.SetActive(false);
            }
        }

        // 如果有证书，开始展示
        if (certificates.Count > 0)
        {
            StartCoroutine(ShowcaseCertificates());
        }
        else
        {
            Debug.LogWarning("没有设置证书对象！");
        }
    }

    IEnumerator ShowcaseCertificates()
    {
        while (true) // 无限循环展示
        {
            // 确保索引在有效范围内
            if (currentIndex >= certificates.Count)
            {
                currentIndex = 0;
            }

            // 获取当前要展示的证书
            Certificate currentCert = certificates[currentIndex];

            if (currentCert.certificateObject != null)
            {
                yield return StartCoroutine(AnimateCertificate(currentCert));

                // 等待指定的时间再展示下一个证书
                yield return new WaitForSeconds(delayBetweenCertificates);
            }

            // 移动到下一个证书
            currentIndex++;
        }
    }

    IEnumerator AnimateCertificate(Certificate cert)
    {
        isAnimating = true;
        GameObject certObj = cert.certificateObject;

        // 设置初始位置并激活对象
        certObj.transform.position = startPosition.position;
        certObj.transform.rotation = startPosition.rotation;
        certObj.SetActive(true);

        // 飞入动画
        float elapsed = 0;
        while (elapsed < flyInDuration)
        {
            float t = flyInCurve.Evaluate(elapsed / flyInDuration);
            certObj.transform.position = Vector3.Lerp(startPosition.position, displayPosition.position, t);
            certObj.transform.rotation = Quaternion.Slerp(startPosition.rotation, displayPosition.rotation, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 确保精确到达展示位置
        certObj.transform.position = displayPosition.position;
        certObj.transform.rotation = displayPosition.rotation;

        // 展示停留时间
        yield return new WaitForSeconds(displayDuration);

        // 飞出动画
        elapsed = 0;
        while (elapsed < flyOutDuration)
        {
            float t = flyOutCurve.Evaluate(elapsed / flyOutDuration);
            certObj.transform.position = Vector3.Lerp(displayPosition.position, endPosition.position, t);
            certObj.transform.rotation = Quaternion.Slerp(displayPosition.rotation, endPosition.rotation, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 隐藏证书对象
        certObj.SetActive(false);
        isAnimating = false;
    }

    // 用于外部控制的方法
    public void PauseShowcase()
    {
        StopAllCoroutines();
        isAnimating = false;
    }

    public void ResumeShowcase()
    {
        if (!isAnimating)
        {
            StartCoroutine(ShowcaseCertificates());
        }
    }

    public void ShowSpecificCertificate(int index)
    {
        if (index >= 0 && index < certificates.Count)
        {
            StopAllCoroutines();
            currentIndex = index;
            StartCoroutine(ShowcaseCertificates());
        }
    }
}