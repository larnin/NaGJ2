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
    public class LocLanguage : SerializedScriptableObject
    {
        class LocText
        {
            public int id;
            public string text;
            public bool dirty;

            public LocText(int _id, string _text = "", bool _dirty = true)
            {
                id = _id;
                text = _text;
                dirty = _dirty;
            }
        }

        [HideInInspector]
        [SerializeField] string m_languageID;
        public string languageID { get { return m_languageID; } set { m_languageID = value; } }

        [HideInInspector]
        [SerializeField] string m_languageName;
        public string languageName { get { return m_languageName; } set { m_languageName = value; } }

        [HideInInspector]
        [SerializeField] List<LocText> m_texts = new List<LocText>();

        public bool HaveText(int id)
        {
            return GetInternal(id) != null;
        }

        public void SetText(int id)
        {
            SetText(id, "", true);
        }

        public void SetText(int id, string text, bool newDirtyValue = false)
        {
            var t = GetInternal(id);

            if (t == null)
            {
                t = new LocText(id, text, newDirtyValue);
                m_texts.Add(t);

                return;
            }

            t.text = text;
            t.dirty = newDirtyValue;


#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }

        public string GetText(int id, string defaultValue = null)
        {
            var t = GetInternal(id);
            if (t != null)
                return t.text;
            return defaultValue;
        }

        public void Remove(int id)
        {
            for (int i = 0; i < m_texts.Count; i++)
            {
                if (m_texts[i].id == id)
                {
                    m_texts.RemoveAt(i);
                    return;
                }
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }

        public void SetDirty(int id, bool dirty = true)
        {
            var t = GetInternal(id);
            if (t != null)
                t.dirty = dirty;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }

        public bool GetDirty(int id, bool defaultValue = false)
        {
            var t = GetInternal(id);
            if (t != null)
                return t.dirty;
            return defaultValue;
        }

        public int GetTextCount()
        {
            return m_texts.Count;
        }

        public int GetTextIdAt(int index)
        {
            if (index < 0 || index >= m_texts.Count)
                return LocTable.invalidID;
            return m_texts[index].id;
        }

        LocText GetInternal(int id)
        {
            foreach (var t in m_texts)
            {
                if (t.id == id)
                    return t;
            }

            return null;
        }
    }
}