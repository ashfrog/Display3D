using System.Linq;
using UnityEngine;

/// <summary>
/// 环绕相机
/// 结构：本脚本挂在camera的父物体上，target是被控物体的子物体。
/// </summary>
public class SmoothOrbitManipulator : MonoBehaviour
{
    [Header("环绕中心")]
    public Transform target;                // 环绕中心
    public float distance = 5.0f;           // 初始距离
    public float minDistance = 1.0f;
    public float maxDistance = 10.0f;
    public float zoomSpeed = 0.5f;
    public float rotationSpeed = 0.2f;
    [Header("平滑时间")]
    public float smoothTime = 0.08f;        // 平滑时间

    // 用于指定哪个屏幕区域生效（基于 x 坐标 cutoff）
    // 当 leftEnable == true 时，只有 position.x <= screenWidth 的输入会生效（左侧区域）
    // 当 leftEnable == false 时，只有 position.x > screenWidth 的输入会生效（右侧区域）
    public int screenWidth = 1920;
    public bool leftEnable;
    [Header("指定缩放位置")]
    public float desiredDistance;
    [Header("指定角度")]
    public Vector2 desiredOrbitAngles;     // 目标角度
    [Header("限制上下旋转角度")]
    public Vector2 angleYClamp = new Vector2(-80, 80);


    [Header("中心点偏移")]
    public Vector3 desiredPanOffset = Vector3.zero;

    private Vector3 panOffset = Vector3.zero;
    private Vector3 currentPanOffset = Vector3.zero;

    private float smoothDistanceVel;
    private Vector2 orbitAngles;            // 当前角度

    private Vector2 smoothOrbitVel;

    private Vector3 lastTouchPos0, lastTouchPos1;
    private float lastTouchDistance;
    private bool isPinching = false;

    private Vector3 lastMousePos;
    private bool isAltScaling = false;
    private float lastAltMouseDistance;

    // 用来标记当前鼠标是否在区域内并处于拖拽状态（始于区域内）
    private bool isMouseDraggingInRegion = false;

    void Start()
    {
        //Vector3 toTarget = transform.position - target.position;
        distance = desiredDistance;
        // 直接使用相机父物体当前的世界旋转（transform.rotation），基于环绕中心target
        Vector3 worldAngles = transform.rotation.eulerAngles;
        orbitAngles = desiredOrbitAngles = new Vector2(worldAngles.y, worldAngles.x);
    }

    // 判断给定屏幕坐标是否在激活区域内
    private bool IsPointerInActiveRegion(Vector2 screenPos)
    {
        if (leftEnable)
            return screenPos.x <= (float)screenWidth;
        else
            return screenPos.x > (float)screenWidth;
    }

    void Update()
    {
        // 触控 —— 已根据 leftEnable & screenWidth 过滤
        if (Input.touchSupported && Input.touchCount > 0)
        {
            Touch[] touches;
            if (leftEnable)
            {
                touches = Input.touches.Where(t => t.position.x <= (float)screenWidth).ToArray();
            }
            else
            {
                touches = Input.touches.Where(t => t.position.x > (float)screenWidth).ToArray();
            }

            if (touches.Length == 1)
            {
                // 单指旋转
                if (touches[0].phase == TouchPhase.Moved)
                {
                    Vector2 delta = touches[0].deltaPosition;
                    desiredOrbitAngles.x += delta.x * rotationSpeed;
                    desiredOrbitAngles.y -= delta.y * rotationSpeed;
                    desiredOrbitAngles.y = Mathf.Clamp(desiredOrbitAngles.y, angleYClamp.x, angleYClamp.y);
                }

                // 当触摸点从多点减为单点时，停止捏合状态
                isPinching = false;
            }
            else if (touches != null && touches.Length >= 2)
            {
                Touch t0 = touches[0];
                Touch t1 = touches[1];
                Vector2 curTouchPos0 = t0.position;
                Vector2 curTouchPos1 = t1.position;
                float curTouchDistance = Vector2.Distance(curTouchPos0, curTouchPos1);

                if (!isPinching)
                {
                    lastTouchPos0 = curTouchPos0;
                    lastTouchPos1 = curTouchPos1;
                    lastTouchDistance = curTouchDistance;
                    isPinching = true;
                }
                else
                {
                    // 缩放（双指捏合）
                    float deltaDistance = curTouchDistance - lastTouchDistance;
                    desiredDistance -= deltaDistance * zoomSpeed * 0.02f;
                    desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);

                    // 旋转（双指间的角度变化）
                    Vector2 prevDir = (lastTouchPos1 - lastTouchPos0).normalized;
                    Vector2 curDir = (curTouchPos1 - curTouchPos0).normalized;
                    float angle = Vector2.SignedAngle(prevDir, curDir);
                    desiredOrbitAngles.x += angle;
                    desiredOrbitAngles.y = Mathf.Clamp(desiredOrbitAngles.y, angleYClamp.x, angleYClamp.y);

                    lastTouchPos0 = curTouchPos0;
                    lastTouchPos1 = curTouchPos1;
                    lastTouchDistance = curTouchDistance;
                }
            }
            else
            {
                isPinching = false;
            }
        }
        // 鼠标 —— 现在也会根据 leftEnable & screenWidth 限制，使鼠标操作只在指定屏幕区域生效
        else
        {
            Vector2 mousePos = Input.mousePosition;

            // 滚轮缩放：仅当鼠标在激活区域内才响应滚轮
            if (IsPointerInActiveRegion(mousePos))
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    desiredDistance -= scroll * zoomSpeed * 5f;
                    desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
                }
            }

            // 鼠标左键环绕（拖拽）：只有当拖拽始于激活区域时才生效
            if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))
            {
                if (IsPointerInActiveRegion(mousePos))
                {
                    lastMousePos = mousePos;
                    isMouseDraggingInRegion = true;
                }
                else
                {
                    isMouseDraggingInRegion = false;
                }
            }

            if (Input.GetMouseButton(0) && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))
            {
                if (isMouseDraggingInRegion)
                {
                    Vector3 delta = (Vector3)Input.mousePosition - lastMousePos;
                    desiredOrbitAngles.x += delta.x * rotationSpeed;
                    desiredOrbitAngles.y -= delta.y * rotationSpeed;
                    desiredOrbitAngles.y = Mathf.Clamp(desiredOrbitAngles.y, angleYClamp.x, angleYClamp.y);
                    lastMousePos = Input.mousePosition;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                isMouseDraggingInRegion = false;
            }

            // Alt+左键模拟双指缩放：仅当按下时鼠标在激活区域才开始，并且仅在区域内拖动时生效
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (IsPointerInActiveRegion(mousePos))
                    {
                        lastAltMouseDistance = mousePos.y;
                        isAltScaling = true;
                    }
                    else
                    {
                        isAltScaling = false;
                    }
                }

                if (Input.GetMouseButton(0) && isAltScaling)
                {
                    // 如果鼠标移出区域后应该继续缩放还是停止，取决于需求。
                    // 这里我们只要缩放始于区域内，就允许持续缩放（即不再每帧重新判断区域）。
                    float deltaY = Input.mousePosition.y - lastAltMouseDistance;
                    desiredDistance -= deltaY * zoomSpeed * 0.02f;
                    desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
                    lastAltMouseDistance = Input.mousePosition.y;
                }

                if (Input.GetMouseButtonUp(0))
                {
                    isAltScaling = false;
                }
            }
            else
            {
                // 如果 Alt 被抬起，确保状态复位
                isAltScaling = false;
            }
        }

        float autoSmoothTime = smoothTime; //旋转平滑时间
        const float MaxAngleDelta = 90f; // 超过多少度就直接跳过去，避免“乱转”
        if (Mathf.Abs(Mathf.DeltaAngle(orbitAngles.x, desiredOrbitAngles.x)) > MaxAngleDelta)
        {
            autoSmoothTime = 0.05f;
            //orbitAngles.x = desiredOrbitAngles.x;
        }

        if (Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, desiredOrbitAngles.y)) > MaxAngleDelta)
        {
            autoSmoothTime = 0.05f;
            //orbitAngles.y = desiredOrbitAngles.y;
        }

        orbitAngles.x = Mathf.SmoothDampAngle(orbitAngles.x, desiredOrbitAngles.x, ref smoothOrbitVel.x, autoSmoothTime);
        orbitAngles.y = Mathf.SmoothDampAngle(orbitAngles.y, desiredOrbitAngles.y, ref smoothOrbitVel.y, autoSmoothTime);
        // 平滑插值角度和距离
        orbitAngles.x = Mathf.SmoothDampAngle(orbitAngles.x, desiredOrbitAngles.x, ref smoothOrbitVel.x, smoothTime);
        orbitAngles.y = Mathf.SmoothDampAngle(orbitAngles.y, desiredOrbitAngles.y, ref smoothOrbitVel.y, smoothTime);
        distance = Mathf.SmoothDamp(distance, desiredDistance, ref smoothDistanceVel, smoothTime);

        // 计算新位置
        Quaternion rot = Quaternion.Euler(orbitAngles.y, orbitAngles.x, 0);
        Vector3 offset = rot * new Vector3(0, 0, -distance);

        currentPanOffset = Vector3.SmoothDamp(currentPanOffset, desiredPanOffset, ref panOffset, smoothTime);
        transform.position = target.position + offset + currentPanOffset;
        transform.rotation = rot;
    }
}