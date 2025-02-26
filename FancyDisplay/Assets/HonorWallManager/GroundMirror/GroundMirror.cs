using UnityEngine;

[ExecuteInEditMode]
public class GroundMirror : MonoBehaviour
{
    public bool m_DisablePixelLights = true;
    public int m_TextureSize = 1024;
    public float m_ClipPlaneOffset = 0.07f;
    public LayerMask m_ReflectLayers = -1;

    private Camera m_ReflectionCamera;
    private RenderTexture m_ReflectionTexture;
    private Material m_SharedMaterial;
    private static bool s_InsideRendering;

    private void OnEnable()
    {
        SetupReflection();
    }

    private void OnDisable()
    {
        CleanupReflection();
    }

    private void Update()
    {
        if (!m_SharedMaterial)
        {
            var renderer = GetComponent<Renderer>();
            m_SharedMaterial = renderer.sharedMaterial;
        }
    }

    private void OnWillRenderObject()
    {
        if (!enabled || !m_SharedMaterial || s_InsideRendering)
            return;

        Camera cam = Camera.current;
        if (!cam)
            return;

        s_InsideRendering = true;

        CreateMirrorObjects(cam);
        UpdateCameraProperties(cam, m_ReflectionCamera);

        float d = -Vector3.Dot(transform.up, transform.position) - m_ClipPlaneOffset;
        Vector4 reflectionPlane = new Vector4(transform.up.x, transform.up.y, transform.up.z, d);

        Matrix4x4 reflection = Matrix4x4.zero;
        CalculateReflectionMatrix(ref reflection, reflectionPlane);

        m_ReflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;

        Vector4 clipPlane = CameraSpacePlane(m_ReflectionCamera, transform.position, transform.up, 1.0f);
        Matrix4x4 projection = cam.projectionMatrix;
        CalculateObliqueMatrix(ref projection, clipPlane);
        m_ReflectionCamera.projectionMatrix = projection;

        m_ReflectionCamera.cullingMask = ~(1 << 4) & m_ReflectLayers.value;
        m_ReflectionCamera.targetTexture = m_ReflectionTexture;

        GL.invertCulling = true;

        if (!cam.orthographic)
        {
            m_ReflectionCamera.Render();
        }
        GL.invertCulling = false;

        m_SharedMaterial.SetTexture("_ReflectionTex", m_ReflectionTexture);
        s_InsideRendering = false;
    }

    private void SetupReflection()
    {
        if (m_ReflectionTexture == null)
        {
            m_ReflectionTexture = new RenderTexture(m_TextureSize, m_TextureSize, 16);
            m_ReflectionTexture.name = "__MirrorReflection" + GetInstanceID();
            m_ReflectionTexture.hideFlags = HideFlags.DontSave;
        }
    }

    private void CleanupReflection()
    {
        if (m_ReflectionCamera)
        {
            DestroyImmediate(m_ReflectionCamera.gameObject);
            m_ReflectionCamera = null;
        }
        if (m_ReflectionTexture)
        {
            DestroyImmediate(m_ReflectionTexture);
            m_ReflectionTexture = null;
        }
    }

    private void CreateMirrorObjects(Camera currentCamera)
    {
        if (!m_ReflectionCamera)
        {
            GameObject go = new GameObject("Mirror Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera));
            m_ReflectionCamera = go.GetComponent<Camera>();
            m_ReflectionCamera.enabled = false;
            m_ReflectionCamera.transform.position = transform.position;
            m_ReflectionCamera.transform.rotation = transform.rotation;
            go.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
        }
    }

    private static void UpdateCameraProperties(Camera src, Camera dest)
    {
        if (dest == null) return;
        dest.clearFlags = src.clearFlags;
        dest.backgroundColor = src.backgroundColor;
        dest.farClipPlane = src.farClipPlane;
        dest.nearClipPlane = src.nearClipPlane;
        dest.orthographic = src.orthographic;
        dest.fieldOfView = src.fieldOfView;
        dest.aspect = src.aspect;
        dest.orthographicSize = src.orthographicSize;
    }

    private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
    {
        reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
        reflectionMat.m01 = (-2F * plane[0] * plane[1]);
        reflectionMat.m02 = (-2F * plane[0] * plane[2]);
        reflectionMat.m03 = (-2F * plane[3] * plane[0]);

        reflectionMat.m10 = (-2F * plane[1] * plane[0]);
        reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
        reflectionMat.m12 = (-2F * plane[1] * plane[2]);
        reflectionMat.m13 = (-2F * plane[3] * plane[1]);

        reflectionMat.m20 = (-2F * plane[2] * plane[0]);
        reflectionMat.m21 = (-2F * plane[2] * plane[1]);
        reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
        reflectionMat.m23 = (-2F * plane[3] * plane[2]);

        reflectionMat.m30 = 0F;
        reflectionMat.m31 = 0F;
        reflectionMat.m32 = 0F;
        reflectionMat.m33 = 1F;
    }

    private static Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        Vector3 offsetPos = pos + normal * 0.07f;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

    private static void CalculateObliqueMatrix(ref Matrix4x4 projection, Vector4 clipPlane)
    {
        Vector4 q = projection.inverse * new Vector4(
            Mathf.Sign(clipPlane.x),
            Mathf.Sign(clipPlane.y),
            1.0f,
            1.0f
        );
        Vector4 c = clipPlane * (2.0F / (Vector4.Dot(clipPlane, q)));
        projection[2] = c.x - projection[3];
        projection[6] = c.y - projection[7];
        projection[10] = c.z - projection[11];
        projection[14] = c.w - projection[15];
    }
}