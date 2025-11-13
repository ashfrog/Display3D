using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 环绕相机
/// 结构：本脚本挂在camera的父物体上，target是被控物体的子物体。
/// 加入“软限制 + 回弹”效果：当上下角度超出 angleYClamp 时，拖动会变得“费劲”（阻尼增大，需拖动很多才动一点），
/// 松开手指/鼠标后会快速回弹到限制边界。
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

    [Header("上下旋转软限制")]
    [Tooltip("允许在硬限制外的最大越界角度（越界不会立刻被锁死）")]
    public float softOvershoot = 15f;
    [Tooltip("越界时的拖动阻尼强度，越大越难拖动")]
    [Range(0.01f, 20f)]
    public float rubberStrength = 10f;
    [Tooltip("越界后松手回弹到夹角边界所用时间(秒)，越小回弹越快")]
    public float springReturnTime = 0.06f;

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

    // 软限制/回弹相关
    private float softClampVelY = 0f;       // 回弹用速度缓存（SmoothDamp）
    private bool isYInputActive = false;    // 本帧是否对上下角度产生了主动输入（用于判断是否要回弹）

    public Text posText;

    void Start()
    {
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

    // 对上下角度输入做“橡皮筋”处理：越过限制后阻尼增大，最多允许 softOvershoot 的越界
    private void AddVerticalRotationInput(float rawDeltaY)
    {
        isYInputActive = true;

        float min = angleYClamp.x;
        float max = angleYClamp.y;
        float y = desiredOrbitAngles.y;

        // 当前越界量（在限制内为 0，低于下限为负，高于上限为正）
        float excess = 0f;
        if (y < min) excess = y - min;
        else if (y > max) excess = y - max;

        // 原始输入（注意原脚本为向上拖动使角度减少）
        float applied = rawDeltaY * rotationSpeed;

        // 越界则衰减输入，让拖动变得“费劲”
        if (!Mathf.Approximately(excess, 0f))
        {
            // 1 / (1 + k * (|越界| / softOvershoot))，越界越大，系数越小
            float factor = 1f / (1f + rubberStrength * Mathf.Clamp01(Mathf.Abs(excess) / Mathf.Max(0.0001f, softOvershoot)));
            // 保底（避免完全锁死）
            factor = Mathf.Max(0.05f, factor);
            applied *= factor;
        }

        // 应用输入（注意方向）
        desiredOrbitAngles.y -= applied;

        // 最多只允许在硬限制外 softOvershoot 的越界
        float minSoft = min - softOvershoot;
        float maxSoft = max + softOvershoot;
        desiredOrbitAngles.y = Mathf.Clamp(desiredOrbitAngles.y, minSoft, maxSoft);
    }

    void Update()
    {
        isYInputActive = false; // 每帧开始重置，只有本帧对 Y 角做了输入才会置为 true

        // 触控 —— 已根据 leftEnable & screenWidth 过滤
        if (Input.touchSupported && Input.touchCount > 0)
        {
            posText.text = Input.touches[0].position.x.ToString();
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
                    AddVerticalRotationInput(delta.y); // 使用软限制输入
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

                    // 旋转（双指间的角度变化） —— 只作用于水平角（X/yaw），不改动 pitch（Y）
                    Vector2 prevDir = (lastTouchPos1 - lastTouchPos0).normalized;
                    Vector2 curDir = (curTouchPos1 - curTouchPos0).normalized;
                    float angle = Vector2.SignedAngle(prevDir, curDir);
                    desiredOrbitAngles.x += angle;

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
                    AddVerticalRotationInput(delta.y); // 使用软限制输入
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

        // 松手回弹：当没有对 Y 角做主动输入时，若越界则快速回弹至边界
        if (!isYInputActive)
        {
            float min = angleYClamp.x;
            float max = angleYClamp.y;
            float clamped = Mathf.Clamp(desiredOrbitAngles.y, min, max);

            if (!Mathf.Approximately(desiredOrbitAngles.y, clamped))
            {
                desiredOrbitAngles.y = Mathf.SmoothDamp(desiredOrbitAngles.y, clamped, ref softClampVelY, springReturnTime);
            }
            else
            {
                // 在边界内时，清空回弹速度，避免下次进入造成不必要的惯性
                softClampVelY = 0f;
            }
        }

        float autoSmoothTime = smoothTime; // 旋转平滑时间
        const float MaxAngleDelta = 90f;   // 超过多少度就直接跳过去，避免“乱转”
        if (Mathf.Abs(Mathf.DeltaAngle(orbitAngles.x, desiredOrbitAngles.x)) > MaxAngleDelta)
        {
            autoSmoothTime = 0.05f;
        }

        if (Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, desiredOrbitAngles.y)) > MaxAngleDelta)
        {
            autoSmoothTime = 0.05f;
        }

        orbitAngles.x = Mathf.SmoothDampAngle(orbitAngles.x, desiredOrbitAngles.x, ref smoothOrbitVel.x, autoSmoothTime);
        orbitAngles.y = Mathf.SmoothDampAngle(orbitAngles.y, desiredOrbitAngles.y, ref smoothOrbitVel.y, autoSmoothTime);
        // 平滑插值角度和距离（保留原逻辑）
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