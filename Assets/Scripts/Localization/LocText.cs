using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NLocalization
{
    [Serializable]
    public class LocText
    {
        [HideInInspector]
        [SerializeField] int m_id = LocTable.invalidID;

        [HideInInspector]
        [SerializeField] string m_textID = "";

#if UNITY_EDITOR
        Rect m_setupRect;
#endif

        public void DrawInspectorGUI()
        {
            OnInspectorGUI();
        }

        [OnInspectorGUI]
        private void OnInspectorGUI()
        {
#if UNITY_EDITOR
            if (m_id == LocTable.invalidID)
            {
                EditorGUILayout.HelpBox("ID not set", MessageType.Warning);

                GUILayout.BeginHorizontal();
            }
            else
            {
                var table = LocList.GetEditorList().GetTable();
                if (!table.Contains(m_id))
                {
                    if (!table.Contains(m_textID))
                        EditorGUILayout.HelpBox("Invalid ID " + m_textID + " (" + m_id + ")", MessageType.Error);
                    else m_id = table.Get(m_textID);
                }
                m_textID = table.Get(m_id);

                GUILayout.BeginHorizontal();

                GUILayout.Label("TextID: " + m_textID + " (" + m_id + ")");
            }

            if(GUILayout.Button("Setup", GUILayout.Width(100)))
            {
                PopupWindow.Show(m_setupRect, new LocSelectionPopup(this));
            }
            if (Event.current.type == EventType.Repaint)
                m_setupRect = GUILayoutUtility.GetLastRect();

            GUILayout.EndHorizontal();

            //string[] options = new string[] { "Cube", "Sphere", "Plane" };
            //EditorGUILayout.Popup(0, options);
#endif
        }

        public string GetText()
        {
            return Loc.Tr(m_id);
        }

        public int GetTextID()
        {
            return m_id;
        }

        public void SetText(int id)
        {
#if UNITY_EDITOR
            m_id = id;
            var table = LocList.GetEditorList().GetTable();
            m_textID = table.Get(id);
            EditorUtility.SetDirty(Selection.activeObject);
#endif
        }
    }
}
