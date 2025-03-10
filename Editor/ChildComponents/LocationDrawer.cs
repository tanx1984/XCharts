﻿
using UnityEditor;
using UnityEngine;
using XCharts.Runtime;

namespace XCharts.Editor
{
    [CustomPropertyDrawer(typeof(Location), true)]
    public class LocationDrawer : BasePropertyDrawer
    {
        public override string ClassName { get { return "Location"; } }
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            base.OnGUI(pos, prop, label);
            if (MakeComponentFoldout(prop, "m_Align", true))
            {
                ++EditorGUI.indentLevel;
                PropertyField(prop, "m_Top");
                PropertyField(prop, "m_Bottom");
                PropertyField(prop, "m_Left");
                PropertyField(prop, "m_Right");
                --EditorGUI.indentLevel;
            }
        }
    }
}