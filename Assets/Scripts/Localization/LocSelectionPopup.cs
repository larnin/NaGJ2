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

#if UNITY_EDITOR
    class LocSelectionPopup : PopupWindowContent
    {
        string m_filter = "";
        LocText m_text = null;

        Vector2 m_scrollPos = Vector2.zero;

        public LocSelectionPopup(LocText text)
        {
            m_text = text;
        }

        public override void OnGUI(Rect rect)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Filter:", GUILayout.Width(40));
            m_filter = GUILayout.TextField(m_filter);
            GUILayout.EndHorizontal();

            var table = LocList.GetEditorList().GetTable();

            m_scrollPos = GUILayout.BeginScrollView(m_scrollPos);
            int nbText = table.Count();
            for(int i = 0; i < nbText; i++)
            {
                int id = table.GetIdAt(i);
                string textID = table.Get(id);
                if (!Loc.ProcessFilter(textID, m_filter))
                    continue;

                if (GUILayout.Button(textID))
                {
                    m_text.SetText(id);
                    editorWindow.Close();
                }
            }
            GUILayout.EndScrollView();
        }
    }
#endif
}
