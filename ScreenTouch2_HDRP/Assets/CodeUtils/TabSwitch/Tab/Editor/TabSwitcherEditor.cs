using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

[CustomEditor(typeof(TabSwitcher))]
public class TabSwitcherEditor : Editor
{
    SerializedProperty allTabTypesProp;
    SerializedProperty tabPageGroupsProp;
    SerializedProperty tabSelectablesProp;
    SerializedProperty currentTabIndexProp;
    SerializedProperty initTabPagesProp;
    ReorderableList reorderableList;

    // 拖拽区域相关
    private Rect dragDropArea;
    private const float DRAG_DROP_AREA_HEIGHT = 40f;

    // 初始化标志
    private bool isInitialized = false;

    void OnEnable()
    {
        InitializeEditor();
    }

    private void InitializeEditor()
    {
        if (target == null || serializedObject == null)
        {
            return;
        }

        try
        {
            allTabTypesProp = serializedObject.FindProperty("allTabTypes");
            tabPageGroupsProp = serializedObject.FindProperty("tabPageGroups");
            tabSelectablesProp = serializedObject.FindProperty("tabSelectables");
            currentTabIndexProp = serializedObject.FindProperty("currentTabIndex");
            initTabPagesProp = serializedObject.FindProperty("initTabPages");

            if (allTabTypesProp == null || tabPageGroupsProp == null || tabSelectablesProp == null)
            {
                Debug.LogError("TabSwitcherEditor: 无法找到必要的序列化属性，请检查 TabSwitcher 脚本");
                return;
            }

            InitializeReorderableList();
            isInitialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TabSwitcherEditor 初始化失败: {e.Message}");
            isInitialized = false;
        }
    }

    private void InitializeReorderableList()
    {
        reorderableList = new ReorderableList(serializedObject, tabPageGroupsProp, true, true, true, true);

        reorderableList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Tab页面组列表 (可拖拽排序)");
        };

        reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            if (tabPageGroupsProp == null || index >= tabPageGroupsProp.arraySize)
                return;

            var element = tabPageGroupsProp.GetArrayElementAtIndex(index);
            var pagesProp = element.FindPropertyRelative("pages");
            var tabTypeProp = element.FindPropertyRelative("tabType");

            rect.y += 2;
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float currentY = rect.y;

            List<string> tabTypes = new List<string>();
            if (allTabTypesProp != null)
            {
                for (int j = 0; j < allTabTypesProp.arraySize; j++)
                {
                    tabTypes.Add(allTabTypesProp.GetArrayElementAtIndex(j).stringValue);
                }
            }

            EditorGUI.LabelField(new Rect(rect.x, currentY, 100, lineHeight), "Tab " + index + ":");

            if (tabTypes.Count > 0)
            {
                int selectedIdx = Mathf.Max(0, tabTypes.IndexOf(tabTypeProp.stringValue));
                int newIdx = EditorGUI.Popup(new Rect(rect.x + 60, currentY, rect.width - 60, lineHeight),
                                           selectedIdx, tabTypes.ToArray());
                if (newIdx >= 0 && newIdx < tabTypes.Count)
                {
                    tabTypeProp.stringValue = tabTypes[newIdx];
                }
            }
            else
            {
                EditorGUI.LabelField(new Rect(rect.x + 60, currentY, rect.width - 60, lineHeight), "请先添加Tab类型");
                tabTypeProp.stringValue = "";
            }

            currentY += lineHeight + 2;
            float remainingHeight = rect.height - (currentY - rect.y);
            EditorGUI.PropertyField(new Rect(rect.x, currentY, rect.width, remainingHeight),
                                  pagesProp, new GUIContent("Pages"), true);
        };

        reorderableList.onAddCallback = (ReorderableList list) =>
        {
            if (tabPageGroupsProp == null || allTabTypesProp == null) return;

            tabPageGroupsProp.InsertArrayElementAtIndex(tabPageGroupsProp.arraySize);
            var newGroup = tabPageGroupsProp.GetArrayElementAtIndex(tabPageGroupsProp.arraySize - 1);
            newGroup.FindPropertyRelative("tabType").stringValue = (allTabTypesProp.arraySize > 0)
                ? allTabTypesProp.GetArrayElementAtIndex(0).stringValue : "";
            newGroup.FindPropertyRelative("pages").ClearArray();
        };

        reorderableList.elementHeightCallback = (int index) =>
        {
            if (tabPageGroupsProp == null || index >= tabPageGroupsProp.arraySize)
                return EditorGUIUtility.singleLineHeight;

            var element = tabPageGroupsProp.GetArrayElementAtIndex(index);
            var pagesProp = element.FindPropertyRelative("pages");

            float height = EditorGUIUtility.singleLineHeight + 4;
            height += EditorGUI.GetPropertyHeight(pagesProp, true) + 4;
            return height;
        };
    }

    public override void OnInspectorGUI()
    {
        if (!isInitialized)
        {
            if (target == null)
            {
                EditorGUILayout.HelpBox("目标对象为空，请选择一个 TabSwitcher 组件", MessageType.Error);
                return;
            }

            EditorGUILayout.HelpBox("编辑器正在初始化...", MessageType.Info);
            InitializeEditor();
            return;
        }

        if (serializedObject == null)
        {
            EditorGUILayout.HelpBox("序列化对象无效", MessageType.Error);
            return;
        }

        serializedObject.Update();

        EditorGUILayout.Space(5);

        if (allTabTypesProp != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(allTabTypesProp, new GUIContent("自定义Tab类型名"), true);

            if (GUILayout.Button("拷贝", GUILayout.Width(50)))
            {
                CopyTabTypesToClipboard();
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(10);

        DrawTabSelectablesField();

        EditorGUILayout.Space(5);

        if (currentTabIndexProp != null)
        {
            EditorGUILayout.PropertyField(currentTabIndexProp);
        }

        if (initTabPagesProp != null)
        {
            EditorGUILayout.PropertyField(initTabPagesProp);
        }

        EditorGUILayout.Space(10);

        DrawDragDropArea();

        EditorGUILayout.Space(10);

        if (reorderableList != null)
        {
            reorderableList.DoLayoutList();
        }

        EditorGUILayout.Space(10);

        if (tabPageGroupsProp != null)
        {
            if (tabPageGroupsProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("添加Tab页面组的方法：\n" +
                                      "• 点击上方列表的 '+' 按钮手动添加\n" +
                                      "• 直接拖入多个Toggle或Button到Tab Selectables数组中自动创建\n" +
                                      "• 拖拽多个GameObject到上方拖拽区域自动创建", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("可以拖拽左侧的 '≡' 图标来重新排序Tab页面组", MessageType.Info);
            }
        }

        if (serializedObject.ApplyModifiedProperties())
        {
            Repaint();
        }
    }

    private void DrawDragDropArea()
    {
        Rect dropArea = GUILayoutUtility.GetRect(0, DRAG_DROP_AREA_HEIGHT, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(dropArea, new Color(0.5f, 0.5f, 0.5f, 0.1f));
        Event evt = Event.current;
        bool isDragging = evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform;
        bool isInDragArea = dropArea.Contains(evt.mousePosition);

        if (isDragging && isInDragArea)
        {
            EditorGUI.DrawRect(dropArea, new Color(0.0f, 1.0f, 0.0f, 0.2f));
        }

        GUIStyle centeredStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Italic
        };

        EditorGUI.LabelField(dropArea, "拖拽多个GameObject到此处自动创建Tab页面组", centeredStyle);

        HandleDragAndDrop(evt, isInDragArea);
    }

    private void HandleDragAndDrop(Event evt, bool isInDragArea)
    {
        if (!isInDragArea) return;

        switch (evt.type)
        {
            case EventType.DragUpdated:
                bool hasValidObjects = DragAndDrop.objectReferences.Any(obj => obj is GameObject);
                DragAndDrop.visualMode = hasValidObjects ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
                evt.Use();
                break;

            case EventType.DragPerform:
                DragAndDrop.AcceptDrag();

                var draggedObjects = DragAndDrop.objectReferences
                    .OfType<GameObject>()
                    .Where(go => go != null)
                    .ToList();

                if (draggedObjects.Count > 0)
                {
                    CreateTabPageGroupsFromDraggedObjects(draggedObjects);
                }

                evt.Use();
                break;
        }
    }

    private void CreateTabPageGroupsFromDraggedObjects(List<GameObject> draggedObjects)
    {
        if (tabPageGroupsProp == null) return;

        for (int i = 0; i < draggedObjects.Count; i++)
        {
            var obj = draggedObjects[i];
            tabPageGroupsProp.InsertArrayElementAtIndex(tabPageGroupsProp.arraySize);
            var newGroup = tabPageGroupsProp.GetArrayElementAtIndex(tabPageGroupsProp.arraySize - 1);
            string tabTypeName = GetOrCreateTabTypeName(obj.name, tabPageGroupsProp.arraySize - 1);
            newGroup.FindPropertyRelative("tabType").stringValue = tabTypeName;
            var pagesProp = newGroup.FindPropertyRelative("pages");
            pagesProp.ClearArray();
            pagesProp.InsertArrayElementAtIndex(0);
            pagesProp.GetArrayElementAtIndex(0).objectReferenceValue = obj;

            Debug.Log($"从拖拽对象自动创建Tab页面组 '{tabTypeName}'，添加页面: '{obj.name}'");
        }

        Debug.Log($"成功从 {draggedObjects.Count} 个拖拽对象创建了 {draggedObjects.Count} 个Tab页面组");
    }

    private string GetOrCreateTabTypeName(string objectName, int index)
    {
        if (allTabTypesProp == null) return $"Tab{index + 1}";

        string baseTypeName = objectName;

        if (baseTypeName.EndsWith("Panel", System.StringComparison.OrdinalIgnoreCase))
            baseTypeName = baseTypeName.Substring(0, baseTypeName.Length - 5);
        if (baseTypeName.EndsWith("Page", System.StringComparison.OrdinalIgnoreCase))
            baseTypeName = baseTypeName.Substring(0, baseTypeName.Length - 4);
        if (baseTypeName.StartsWith("Tab", System.StringComparison.OrdinalIgnoreCase))
            baseTypeName = baseTypeName.Substring(3);

        if (string.IsNullOrEmpty(baseTypeName.Trim()))
            baseTypeName = $"Tab{index + 1}";

        string finalTypeName = baseTypeName;
        int counter = 1;
        while (IsTabTypeNameExists(finalTypeName))
        {
            finalTypeName = $"{baseTypeName}{counter}";
            counter++;
        }

        allTabTypesProp.InsertArrayElementAtIndex(allTabTypesProp.arraySize);
        allTabTypesProp.GetArrayElementAtIndex(allTabTypesProp.arraySize - 1).stringValue = finalTypeName;

        return finalTypeName;
    }

    private bool IsTabTypeNameExists(string typeName)
    {
        if (allTabTypesProp == null) return false;

        for (int i = 0; i < allTabTypesProp.arraySize; i++)
        {
            if (allTabTypesProp.GetArrayElementAtIndex(i).stringValue == typeName)
                return true;
        }
        return false;
    }

    private void DrawTabSelectablesField()
    {
        if (tabSelectablesProp == null) return;

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(tabSelectablesProp, new GUIContent("Tab Selectables数组 (Toggle/Button) 作为Tab按钮"), true);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            CheckAndCreateTabPageGroups();
        }
    }

    private void CheckAndCreateTabPageGroups()
    {
        if (tabSelectablesProp == null || tabPageGroupsProp == null) return;

        int tabSelectableCount = tabSelectablesProp.arraySize;
        int tabPageGroupCount = tabPageGroupsProp.arraySize;

        if (tabSelectableCount > tabPageGroupCount)
        {
            for (int i = tabPageGroupCount; i < tabSelectableCount; i++)
            {
                tabPageGroupsProp.InsertArrayElementAtIndex(tabPageGroupsProp.arraySize);
                var newGroup = tabPageGroupsProp.GetArrayElementAtIndex(tabPageGroupsProp.arraySize - 1);
                string tabTypeName = GetAvailableTabTypeName(i);
                newGroup.FindPropertyRelative("tabType").stringValue = tabTypeName;
                var pagesProp = newGroup.FindPropertyRelative("pages");
                pagesProp.ClearArray();

                var selectableProp = tabSelectablesProp.GetArrayElementAtIndex(i);
                if (selectableProp.objectReferenceValue != null)
                {
                    var selectable = selectableProp.objectReferenceValue as Selectable;
                    if (selectable != null)
                    {
                        pagesProp.InsertArrayElementAtIndex(0);
                        pagesProp.GetArrayElementAtIndex(0).objectReferenceValue = selectable.gameObject;

                        Debug.Log($"自动创建Tab页面组 '{tabTypeName}'，并将Selectable '{selectable.name}' 添加为第一个页面");
                    }
                }
            }
        }
        else if (tabSelectableCount < tabPageGroupCount)
        {
            Debug.LogWarning($"Tab Selectables数量({tabSelectableCount})少于页面组数量({tabPageGroupCount})，请手动调整页面组");
        }
    }

    private string GetAvailableTabTypeName(int index)
    {
        if (allTabTypesProp == null) return $"Tab{index + 1}";

        if (index < allTabTypesProp.arraySize)
        {
            string existingTypeName = allTabTypesProp.GetArrayElementAtIndex(index).stringValue;
            if (!string.IsNullOrEmpty(existingTypeName))
            {
                return existingTypeName;
            }
        }

        string newTypeName = $"Tab{index + 1}";

        while (allTabTypesProp.arraySize <= index)
        {
            allTabTypesProp.InsertArrayElementAtIndex(allTabTypesProp.arraySize);
            allTabTypesProp.GetArrayElementAtIndex(allTabTypesProp.arraySize - 1).stringValue = $"Tab{allTabTypesProp.arraySize}";
        }

        if (string.IsNullOrEmpty(allTabTypesProp.GetArrayElementAtIndex(index).stringValue))
        {
            allTabTypesProp.GetArrayElementAtIndex(index).stringValue = newTypeName;
        }

        return allTabTypesProp.GetArrayElementAtIndex(index).stringValue;
    }

    private void CopyTabTypesToClipboard()
    {
        if (allTabTypesProp == null || allTabTypesProp.arraySize == 0)
        {
            Debug.LogWarning("没有Tab类型可以拷贝");
            return;
        }

        List<string> tabTypes = new List<string>();
        for (int i = 0; i < allTabTypesProp.arraySize; i++)
        {
            string tabType = allTabTypesProp.GetArrayElementAtIndex(i).stringValue;
            if (!string.IsNullOrEmpty(tabType))
            {
                tabTypes.Add(tabType);
            }
        }

        if (tabTypes.Count == 0)
        {
            Debug.LogWarning("没有有效的Tab类型可以拷贝");
            return;
        }

        string result = string.Join(", ", tabTypes);
        EditorGUIUtility.systemCopyBuffer = result;
        Debug.Log($"已拷贝 {tabTypes.Count} 个Tab类型名: {result}");
    }
}