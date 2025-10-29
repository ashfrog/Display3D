using System.Linq;
using UnityEngine;

/// <summary>
/// 让物体旋转、缩放的手势和鼠标
/// 结构：本脚本挂在target的父物体上，target是被控物体的子物体。
/// 父物体控制 pitch(x轴)，子物体控制 yaw(y轴)，缩放作用于 target。
/// </summary>
public class SmoothTargetManipulator : MonoBehaviour
{
    public Transform target;                // 展示物体
    public float minScale = 0.2f;
    public float maxScale = 3.0f;
    public float zoomSpeed = 0.5f;
    public float rotationSpeed = 0.2f;
    public float smoothTime = 0.08f;

    // 屏幕区域支持
    public int display1Width = 1920;
    public bool isDisplay1 = true;

    // 父节点旋转（pitch，x轴），目标本体旋转（yaw，y轴）
    private float desiredPitch;      // x轴旋转
    private float desiredYaw;        // y轴旋转
    private float smoothPitchVel;
    private float smoothYawVel;
    private float currentPitch;
    private float currentYaw;

    private float desiredScale = 1f;
    private float smoothScaleVel;

    private Vector3 lastTouchPos0, lastTouchPos1;
    private float lastTouchDistance;
    private bool isPinching = false;

    private Vector3 lastMousePos;
    private bool isAltScaling = false;
    private float lastAltMouseDistance;
    private bool isMouseDraggingInRegion = false;

    void Start()
    {
        // 读取初始旋转
        Vector3 parentEuler = transform.localEulerAngles;
        Vector3 targetEuler = target.localEulerAngles;
        desiredPitch = currentPitch = parentEuler.x;
        desiredYaw = currentYaw = targetEuler.y;
        desiredScale = target.localScale.x;
    }

    private bool IsPointerInActiveRegion(Vector2 screenPos)
    {
        if (isDisplay1)
            return screenPos.x <= (float)display1Width;
        else
            return screenPos.x > (float)display1Width;
    }

    void Update()
    {
        // 触控
        if (Input.touchSupported && Input.touchCount > 0)
        {
            Touch[] touches;
            if (isDisplay1)
                touches = Input.touches.Where(t => t.position.x <= (float)display1Width).ToArray();
            else
                touches = Input.touches.Where(t => t.position.x > (float)display1Width).ToArray();

            if (touches.Length == 1)
            {
                if (touches[0].phase == TouchPhase.Moved)
                {
                    Vector2 delta = touches[0].deltaPosition;
                    desiredYaw -= delta.x * rotationSpeed;     // 左右滑动-》y轴，反向
                    desiredPitch += delta.y * rotationSpeed;   // 上下滑动-》x轴，反向
                    desiredPitch = Mathf.Clamp(desiredPitch, -80, 80); // 防止过度仰俯
                }
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
                    // 缩放（正号方向）
                    float deltaDistance = curTouchDistance - lastTouchDistance;
                    desiredScale += deltaDistance * zoomSpeed * 0.01f;
                    desiredScale = Mathf.Clamp(desiredScale, minScale, maxScale);

                    // 双指旋转（以夹角变化控制yaw）
                    Vector2 prevDir = (lastTouchPos1 - lastTouchPos0).normalized;
                    Vector2 curDir = (curTouchPos1 - curTouchPos0).normalized;
                    float angle = Vector2.SignedAngle(prevDir, curDir);
                    desiredYaw -= angle; // 保持与Orbit方向一致

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
        else
        {
            Vector2 mousePos = Input.mousePosition;

            // 鼠标滚轮缩放（正号方向）
            if (IsPointerInActiveRegion(mousePos))
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    desiredScale += scroll * zoomSpeed;
                    desiredScale = Mathf.Clamp(desiredScale, minScale, maxScale);
                }
            }

            // 鼠标左键旋转
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
                    desiredYaw -= delta.x * rotationSpeed;        // 左右拖动 = y轴（反向）
                    desiredPitch += delta.y * rotationSpeed;      // 上下拖动 = x轴（反向）
                    desiredPitch = Mathf.Clamp(desiredPitch, -80, 80);
                    lastMousePos = Input.mousePosition;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                isMouseDraggingInRegion = false;
            }

            // Alt+左键缩放（正号方向）
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
                    desiredScale += deltaY * zoomSpeed * 0.01f;
                    desiredScale = Mathf.Clamp(desiredScale, minScale, maxScale);
                    lastAltMouseDistance = Input.mousePosition.y;
                }

                if (Input.GetMouseButtonUp(0))
                {
                    isAltScaling = false;
                }
            }
            else
            {
                isAltScaling = false;
            }
        }

        // 平滑插值角度和缩放
        currentYaw = Mathf.SmoothDampAngle(currentYaw, desiredYaw, ref smoothYawVel, smoothTime);
        currentPitch = Mathf.SmoothDampAngle(currentPitch, desiredPitch, ref smoothPitchVel, smoothTime);
        float scale = Mathf.SmoothDamp(target.localScale.x, desiredScale, ref smoothScaleVel, smoothTime);

        // 应用旋转和平移
        // 父对象控制pitch（x），目标控制yaw（y）
        transform.localRotation = Quaternion.Euler(currentPitch, 0, 0);
        target.localRotation = Quaternion.Euler(0, currentYaw, 0);

        target.localScale = Vector3.one * scale;
    }
}