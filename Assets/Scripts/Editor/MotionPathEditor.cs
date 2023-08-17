using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(MotionPath))]
public class MotionPathEditor : Editor
{
    private ReorderableList _list;

    private void OnEnable()
    {
        Tools.hidden = true;

        _list = new ReorderableList(serializedObject, serializedObject.FindProperty("ControlPoints"), true, true, true, true);

        _list.elementHeight = 2 * EditorGUIUtility.singleLineHeight + 3 * EditorGUIUtility.standardVerticalSpacing;

        _list.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, "Control Points");
        };

        _list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = _list.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += EditorGUIUtility.standardVerticalSpacing;

            Rect line1Rect = new Rect(rect.x,
                                      rect.y,
                                      rect.width,
                                      EditorGUIUtility.singleLineHeight);

            Rect line2Rect = new Rect(rect.x,
                                      rect.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                                      rect.width,
                                      EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(line1Rect, element.FindPropertyRelative("Point"), new GUIContent("Point"));
            EditorGUI.PropertyField(line2Rect, element.FindPropertyRelative("Tangent"), new GUIContent("Tangent"));

            if (EditorGUI.EndChangeCheck())
            {
                ZoomToControlPoint(_list);
            }
        };
        _list.onChangedCallback = ZoomToControlPoint;
        _list.onSelectCallback = ZoomToControlPoint;
    }

    private void OnDisable()
    {
        Tools.hidden = false;
    }

    private void ZoomToControlPoint(ReorderableList list)
    {
        if (list == null || list.count == 0 || list.index < 0 || list.index >= list.count) return;

        Vector3 point = list.serializedProperty.GetArrayElementAtIndex(list.index).FindPropertyRelative("Point").vector3Value;
        Vector3 tangent = list.serializedProperty.GetArrayElementAtIndex(list.index).FindPropertyRelative("Tangent").vector3Value;

        float viewSize = tangent == Vector3.zero ? SceneView.lastActiveSceneView.size : tangent.magnitude * 5f;

        SceneView.lastActiveSceneView.LookAt(point, SceneView.lastActiveSceneView.rotation, viewSize);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("IsClosedPath"));
        _list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        MotionPath motionPath = target as MotionPath;

        if (motionPath == null || motionPath.ControlPoints == null) return;

        for (int i = 0; i < motionPath.ControlPoints.Count; i++)
        {
            Vector3 point = motionPath.ControlPoints[i].Point;
            Vector3 tangent = motionPath.ControlPoints[i].Tangent;
            Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, tangent);
            float magnitude = tangent.magnitude;

            Vector3 tangentPoint = point + tangent;
            Vector3 reverseTangentPoint = point - tangent;

            EditorGUI.BeginChangeCheck();
            Vector3 newPoint = Handles.FreeMoveHandle(point, Quaternion.identity, 0.05f * HandleUtility.GetHandleSize(point), Vector3.zero, Handles.RectangleHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(motionPath, "Move Control Point");
                motionPath.ControlPoints[i].Point = newPoint;
            }

            Handles.DrawDottedLines(new[] { reverseTangentPoint, point, point, tangentPoint }, 4);

            EditorGUI.BeginChangeCheck();
            Vector3 newTangentPoint = Handles.FreeMoveHandle(tangentPoint, Quaternion.identity, 0.05f * HandleUtility.GetHandleSize(tangentPoint), Vector3.zero, Handles.CircleHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(motionPath, "Change Control Point Tangent");
                motionPath.ControlPoints[i].Tangent = newTangentPoint - point;
            }

            EditorGUI.BeginChangeCheck();
            Vector3 newReverseTangentPoint = Handles.FreeMoveHandle(reverseTangentPoint, Quaternion.identity, 0.05f * HandleUtility.GetHandleSize(reverseTangentPoint), Vector3.zero, Handles.CircleHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(motionPath, "Change Control Point Tangent");
                motionPath.ControlPoints[i].Tangent = -(newReverseTangentPoint - point);
            }

            Rect guiRect = HandleUtility.WorldPointToSizedRect(point, new GUIContent(string.Format("P{0}", i)), EditorStyles.boldLabel);
            Handles.BeginGUI();
            GUILayout.BeginArea(guiRect);
            GUILayout.Label(string.Format("P{0}", i), EditorStyles.boldLabel);
            GUILayout.EndArea();
            Handles.EndGUI();
        }

        Handles.DrawAAPolyLine(motionPath.GetInterpolatedPoints(32).ToArray());
    }

    //[DrawGizmo(GizmoType.NonSelected)]
    private static void DrawGizmos(MotionPath motionPath, GizmoType gizmoType)
    {
        Color previous = Handles.color;
        Handles.color = new Color(1f, 1f, 1f, 0.5f);

        if (motionPath == null || motionPath.ControlPoints == null) return;

        for (int i = 0; i < motionPath.ControlPoints.Count; i++)
        {
            Vector3 point = motionPath.ControlPoints[i].Point;
            Vector3 tangent = motionPath.ControlPoints[i].Tangent;

            Vector3 tangentPoint = point + tangent;
            Vector3 reverseTangentPoint = point - tangent;

            Vector3 cameraVector = Camera.current.cameraToWorldMatrix.MultiplyVector(new Vector3(0, 0, 1));
            Quaternion cameraRotation = Quaternion.LookRotation(cameraVector, Vector3.up);

            Handles.RectangleHandleCap(i, point, cameraRotation, 0.05f * HandleUtility.GetHandleSize(point), EventType.Repaint);
            Handles.CircleHandleCap(i, tangentPoint, cameraRotation, 0.05f * HandleUtility.GetHandleSize(tangentPoint), EventType.Repaint);
            Handles.CircleHandleCap(i, reverseTangentPoint, cameraRotation, 0.05f * HandleUtility.GetHandleSize(reverseTangentPoint), EventType.Repaint);

            Handles.DrawDottedLines(new[] { reverseTangentPoint, point, point, tangentPoint }, 4);
        }

        Handles.DrawAAPolyLine(motionPath.GetInterpolatedPoints(32).ToArray());

        Handles.color = previous;
    }
}
