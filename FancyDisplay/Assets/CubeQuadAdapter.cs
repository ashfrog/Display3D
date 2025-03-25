using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CubeQuadAdapter : MonoBehaviour
{
    [Tooltip("Quad与Cube前表面之间的距离")]
    [Range(-1f, 1f)]
    public float offset = 0.01f;
    [Tooltip("使Quad在Cube前方而不是后方")]
    public bool isFrontFacing = true;
    private GameObject quadChild;

    void Start()
    {
        // 查找或创建子Quad
        SetupQuad();
        // 立即调整Quad大小和位置
        UpdateQuadTransform();
    }

    private void SetupQuad()
    {
        // 查找是否已有Quad子物体
        Transform existingQuad = transform.Find("FrontQuad");
        if (existingQuad != null)
        {
            quadChild = existingQuad.gameObject;
        }
        else
        {
            // 创建一个新的Quad作为子物体
            quadChild = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quadChild.name = "FrontQuad";
            quadChild.transform.SetParent(transform, false);
            // 可选：设置Quad的材质
            // Renderer quadRenderer = quadChild.GetComponent<Renderer>();
            // quadRenderer.material = yourMaterial;
        }
    }

    private void UpdateQuadTransform()
    {
        if (quadChild == null)
            return;

        // 获取当前Cube的尺寸
        Vector3 cubeScale = transform.localScale;
        Renderer cubeRenderer = GetComponent<Renderer>();
        if (cubeRenderer == null)
            return;

        // 获取Cube在局部坐标中的尺寸
        Vector3 localCubeSize = GetComponent<Collider>().bounds.size;
        localCubeSize = new Vector3(
            localCubeSize.x / transform.lossyScale.x,
            localCubeSize.y / transform.lossyScale.y,
            localCubeSize.z / transform.lossyScale.z
        );

        // 根据方向设置Z位置
        float zDirection = isFrontFacing ? 1 : -1;

        // 设置Quad在局部坐标中的位置
        quadChild.transform.localPosition = new Vector3(0, 0, zDirection * (localCubeSize.z / 2 + offset));

        // 设置Quad的旋转，确保它面向正确方向
        if (isFrontFacing)
        {
            // 面向Z轴正方向（前方），但翻转180度使正面朝向相机
            quadChild.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            // 面向Z轴负方向（后方）
            quadChild.transform.localRotation = Quaternion.identity;
        }

        // 设置Quad的尺寸匹配Cube的前/后面
        quadChild.transform.localScale = new Vector3(localCubeSize.x, localCubeSize.y, 1);
    }

    // 如果Cube的尺寸发生变化，可以调用此方法更新Quad
    public void RefreshQuadSize()
    {
        UpdateQuadTransform();
    }

    // 运行时如果Cube可能改变大小，取消注释这个方法
    /*
    void Update()
    {
        UpdateQuadTransform();
    }
    */

    // 在编辑器中实时更新
#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying)
            return;
        // 确保在编辑器中也能看到效果
        SetupQuad();
        UpdateQuadTransform();
    }

    [CustomEditor(typeof(CubeQuadAdapter))]
    public class CubeQuadAdapterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            CubeQuadAdapter script = (CubeQuadAdapter)target;
            EditorGUILayout.Space();
            if (GUILayout.Button("更新Quad"))
            {
                script.SetupQuad();
                script.UpdateQuadTransform();
            }
        }
    }
#endif
}