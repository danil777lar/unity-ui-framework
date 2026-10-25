using System;
using System.Collections.Generic;
using Larje.UI;
using UnityEditor;
using UnityEngine;

namespace Larje.UI.Editor
{
    [CustomEditor(typeof(PDWPanel))]
    public sealed class PDWPanelEditor : UnityEditor.Editor
    {
        private Dictionary<int, bool> foldoutStates = new Dictionary<int, bool>();

        public override void OnInspectorGUI()
        {
            PDWPanel panel = (PDWPanel)target;

            EditorGUILayout.LabelField("Colors");
            SerializedProperty fillColorProp = serializedObject.FindProperty("fillColor");
            fillColorProp.colorValue = EditorGUILayout.ColorField("Fill Color", fillColorProp.colorValue);
            SerializedProperty borderColorProp = serializedObject.FindProperty("borderColor");
            borderColorProp.colorValue = EditorGUILayout.ColorField("Border Color", borderColorProp.colorValue);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shape");
            SerializedProperty borderPxProp = serializedObject.FindProperty("borderPx");
            borderPxProp.floatValue = Mathf.Max(0f, EditorGUILayout.FloatField("Border (px)", borderPxProp.floatValue));
            SerializedProperty softnessPxProp = serializedObject.FindProperty("softnessPx");
            softnessPxProp.floatValue = Mathf.Max(0f, EditorGUILayout.FloatField("Softness (px)", softnessPxProp.floatValue));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Corners");

            int index = 0;
            SerializedProperty cornerOptionsProp = serializedObject.FindProperty("cornerOptions");
            foreach (SerializedProperty cornerProp in cornerOptionsProp)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal("box");

                SerializedProperty cornersProp = cornerProp.FindPropertyRelative("corners");
                foreach (PDWPanel.CornerType corner in Enum.GetValues(typeof(PDWPanel.CornerType)))
                {
                    bool cornerSelected = false;
                    for (int i = 0; i < cornersProp.arraySize; i++)
                    {
                        if ((PDWPanel.CornerType)cornersProp.GetArrayElementAtIndex(i).enumValueIndex == corner)
                        {
                            cornerSelected = true;
                            break;
                        }
                    }

                    if (GUILayout.Button(corner.ToString(), cornerSelected ? EditorStyles.miniButton : EditorStyles.label))
                    {
                        if (cornerSelected)
                        {
                            for (int i = 0; i < cornersProp.arraySize; i++)
                            {
                                if ((PDWPanel.CornerType)cornersProp.GetArrayElementAtIndex(i).enumValueIndex == corner)
                                {
                                    cornersProp.DeleteArrayElementAtIndex(i);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            cornersProp.InsertArrayElementAtIndex(cornersProp.arraySize);
                            cornersProp.GetArrayElementAtIndex(cornersProp.arraySize - 1).enumValueIndex = (int)corner;

                            for (int i = 0; i < cornerOptionsProp.arraySize; i++)
                            {
                                if (i != index)
                                {
                                    SerializedProperty otherCornersProp = cornerOptionsProp.GetArrayElementAtIndex(i).FindPropertyRelative("corners");
                                    for (int j = 0; j < otherCornersProp.arraySize; j++)
                                    {
                                        if ((PDWPanel.CornerType)otherCornersProp.GetArrayElementAtIndex(j).enumValueIndex == corner)
                                        {
                                            otherCornersProp.DeleteArrayElementAtIndex(j);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (index > 0)
                {
                    if (GUILayout.Button("X", EditorStyles.miniButtonRight))
                    {
                        cornerOptionsProp.DeleteArrayElementAtIndex(index);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        break;
                    }
                }

                EditorGUILayout.EndVertical();

                SerializedProperty radiusProp = cornerProp.FindPropertyRelative("radiusPx");
                radiusProp.floatValue = EditorGUILayout.FloatField("Radius (px)", radiusProp.floatValue);

                EditorGUILayout.EndVertical();

                index++;
            }

            if (GUILayout.Button("Add Corner Option"))
            {
                cornerOptionsProp.InsertArrayElementAtIndex(cornerOptionsProp.arraySize);
                cornerOptionsProp.GetArrayElementAtIndex(cornerOptionsProp.arraySize - 1).FindPropertyRelative("corners").ClearArray();
                cornerOptionsProp.GetArrayElementAtIndex(cornerOptionsProp.arraySize - 1).FindPropertyRelative("radiusPx").floatValue = 0f;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
