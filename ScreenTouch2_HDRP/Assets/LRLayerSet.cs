using UnityEngine;

/// <summary>
/// 实例化一个Ins到Display2中
/// </summary>
public class LRLayerSet : MonoBehaviour
{
    public Ins defaultIns;
    private Ins rightIns;
    public LayerMask rightLayerMask;
    public int rightLayer = 11;
    public int rightDisplay = 1;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        defaultIns.litVCR.SetBalance(0);
        if (Settings.ini.Game.DoubleScreen)
        {
            rightIns = Instantiate(defaultIns);
            rightIns.SmoothOrbitManipulator.leftEnable = false;
            rightIns.mediaPlayerUI.leftEnable = false;
            SetLayerRecursively(rightIns.objChair, rightLayer);
            SetLayerRecursively(rightIns.objLogo, rightLayer);
            rightIns.UICamera.targetDisplay = rightDisplay;
            rightIns.UICamera_Top.targetDisplay = rightIns.UICamera.targetDisplay;
            rightIns.mainCamera.targetDisplay = rightDisplay;
            rightIns.mainCamera.cullingMask = rightLayerMask;

            defaultIns.litVCR.SetBalance(-1);
            rightIns.litVCR.SetBalance(1);
        }

    }

    /// <summary>
    /// 递归设置物体及其子物体的Layer
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="layer"></param>
    static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
