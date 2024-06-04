using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Globalization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NLocalization
{
    public class LocList
    {
        string m_path = "";

        LocTable m_table;
        List<LocLanguage> m_languages = new List<LocLanguage>();

        public void Load(string path = "Localization") //path in /resource/ subdirectory
        {
            m_languages.Clear();
            m_table = null;

            m_path = path;

            var locs = Resources.LoadAll<ScriptableObject>(path);

            foreach (var i in locs)
            {
                LocTable table = i as LocTable;
                if (table != null)
                {
                    if (m_table != null)
                    {
                        Debug.LogError("Multiple table found in resource directory \"" + path + "\" - Loaded \"" + m_table.name + "\" - New \"" + table.name + "\"");
                        continue;
                    }
                    m_table = table;
                    continue;
                }

                LocLanguage lang = i as LocLanguage;
                if (lang != null)
                    m_languages.Add(lang);
            }

#if UNITY_EDITOR
            if (m_table == null)
                Initialize();

            ValidateTable();
#endif
        }

        void Initialize()
        {
#if UNITY_EDITOR
            m_table = ScriptableObjectEx.CreateAsset<LocTable>(m_path, "Table");

            if(m_languages.Count == 0)
            {
                CultureInfo ci = CultureInfo.InstalledUICulture;
                LocLanguage lang = ScriptableObjectEx.CreateAsset<LocLanguage>(m_path, ci.Name);
                lang.languageName = ci.DisplayName;
                lang.languageID = ci.Name;
                EditorUtility.SetDirty(lang);

                m_table.defaultLanguageID = lang.languageID;
                EditorUtility.SetDirty(m_table);

                m_languages.Add(lang);
            }

            AssetDatabase.SaveAssets();
#else
            DebugLogs.LogError("You can't initialize languages outside the editor");
#endif
        }

#if UNITY_EDITOR
        void ValidateTable()
        {
            foreach(var lang in m_languages)
            {
                int nbText = lang.GetTextCount();

                for (int i = 0; i < nbText; i++)
                {
                    int id = lang.GetTextIdAt(i);
                    if (!m_table.Contains(id))
                    {
                        m_table.Add(id);
                        EditorUtility.SetDirty(m_table);
                    }
                }

            }

            int nbLoc = m_table.Count();
            for(int i = 0; i < nbLoc; i++)
            {
                int id = m_table.GetIdAt(i);
                foreach(var lang in m_languages)
                {
                    if (!lang.HaveText(id))
                    {
                        lang.SetText(id);
                        EditorUtility.SetDirty(lang);
                    }
                }

                int nCategoryID = m_table.GetCategory(id);
                if(nCategoryID != LocTable.invalidID)
                {
                    if(!m_table.Contains(nCategoryID))
                    {
                        m_table.SetCategory(id, LocTable.invalidID);
                        EditorUtility.SetDirty(m_table);
                    }
                }
            }

            if(GetLanguage(m_table.defaultLanguageID) == null)
            {
                m_table.defaultLanguageID = m_languages[0].languageID;
                EditorUtility.SetDirty(m_table);
            }

            AssetDatabase.SaveAssets();
        }
#endif

        public LocTable GetTable()
        {
            return m_table;
        }

        public int GetNbLang()
        {
            return m_languages.Count();
        }

        public LocLanguage GetLanguage(int index)
        {
            if (index < 0 || index >= m_languages.Count)
                return null;
            return m_languages[index];
        }

        public LocLanguage GetLanguage(string langID)
        {
            foreach(var l in m_languages)
            {
                if (l.languageID == langID)
                    return l;
            }

            return null;
        }

        public bool AddLang(string langID, string langName)
        {
#if UNITY_EDITOR
            if (GetLanguage(langID) != null)
                return false;

            LocLanguage lang = ScriptableObjectEx.CreateAsset<LocLanguage>(m_path, langID);
            lang.languageID = langID;
            lang.languageName = langName;
            EditorUtility.SetDirty(lang);

            int nbText = m_table.Count();
            for(int i = 0; i < nbText; i++)
            {
                int id = m_table.GetIdAt(i);
                lang.SetText(id);
            }

            m_languages.Add(lang);

            AssetDatabase.SaveAssets();
#endif
            return true;
        }

        public int AddText(string textID, int categoryID = LocTable.invalidID)
        {
#if UNITY_EDITOR
            int id = m_table.Get(textID);
            if (id != LocTable.invalidID)
                return id;

            id = m_table.Add(textID, categoryID);
            EditorUtility.SetDirty(m_table);

            foreach (var l in m_languages)
            {
                l.SetText(id);
                EditorUtility.SetDirty(l);
            }

            AssetDatabase.SaveAssets();

            return id;
#else
            return LocTable.invalidID;
#endif
        }

        public void RemoveText(int id)
        {
            m_table.Remove(id);

            foreach(var l in m_languages)
                l.Remove(id);
        }


#if UNITY_EDITOR
        static LocList m_editorStaticList;

        static public LocList GetEditorList()
        {
            if(m_editorStaticList == null)
            {
                m_editorStaticList = new LocList();
                m_editorStaticList.Load();
            }

            return m_editorStaticList;
        }
#endif
    }
}