using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace NLocalization
{
    public class LocEditor : OdinMenuEditorWindow
    {
        [MenuItem("Game/Localization")]
        public static void OpenWindow()
        {
            GetWindow<LocEditor>().Show();
        }

        enum Tabs
        {
            Languages,
            Exports,
            Stats,
            Settings,
        }

        private int m_currentTab;

        public LocList locList { get { return LocList.GetEditorList(); } }

        protected override void OnImGUI()
        {
            int newTab = GUILayout.Toolbar(m_currentTab, Enum.GetNames(typeof(Tabs)));
            if (newTab != m_currentTab)
            {
                m_currentTab = newTab;
                ChangeTab();
            }

            base.OnImGUI();
        }

        public void ChangeTab()
        {
            ForceMenuTreeRebuild();
        }


        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();

            Tabs tab = (Tabs)m_currentTab;

            switch(tab)
            {
                case Tabs.Languages:
                    BuildMenuTreeLanguages(tree);
                    break;
                case Tabs.Exports:
                    BuildMenuTreeExports(tree);
                    break;
                case Tabs.Settings:
                    BuildMenuTreeSettings(tree);
                    break;
                case Tabs.Stats:
                    BuildMenuTreeStats(tree);
                    break;
                default:
                    Debug.LogError("No tree for " + tab.ToString() + " tab");
                    break;
            }

            return tree;
        }

        void BuildMenuTreeLanguages(OdinMenuTree tree)
        {
            tree.Add("All languages", new LocEditorLangsTab("", this));

            int nbLangs = locList.GetNbLang();
            for(int i = 0; i < nbLangs; i++)
            {
                var lang = locList.GetLanguage(i);
                tree.Add(lang.languageName, new LocEditorLangsTab(lang.languageID, this));
            }

            tree.Add("Add language", new LocEditorNewLangTab(this));
        }

        void BuildMenuTreeExports(OdinMenuTree tree)
        {
            tree.Add("Export", new LocEditorExportTab(this));
            tree.Add("Import", new LocEditorImportTab(this));
        }

        void BuildMenuTreeStats(OdinMenuTree tree)
        {

        }

        void BuildMenuTreeSettings(OdinMenuTree tree)
        {

        }
    }

    public class LocEditorLangsTab
    {
        string m_langID = "";
        LocEditor m_editor;
        string m_filter = "";

        Dictionary<int, bool> m_categoriesStatus = new Dictionary<int, bool>();
        string m_newTextID = "";
        string m_newCategoryID = "";

        int m_destroyTextID = LocTable.invalidID;
        int m_destroyCategoryID = LocTable.invalidID;

        public LocEditorLangsTab(string langID, LocEditor editor)
        {
            m_langID = langID;
            m_editor = editor;
        }

        [OnInspectorGUI]
        private void OnInspectorGUI()
        {
            GUILayout.Label("Filter");
            m_filter = GUILayout.TextField(m_filter);
            GUILayout.Space(5);

            DrawList();

            var list = m_editor.locList;
            var table = list.GetTable();

            if (m_destroyCategoryID != LocTable.invalidID)
            {
                string categoryName = table.GetCategoryName(m_destroyCategoryID);

                if (EditorUtility.DisplayDialog("Remove Category", "Are you sure to remove the category " + categoryName + "?\nRemaining texts will be moved to \"No category\"", "Yes", "No"))
                    table.RemoveCategory(m_destroyCategoryID);

                m_destroyCategoryID = LocTable.invalidID;
                GUIUtility.ExitGUI();
            }

            if(m_destroyTextID != LocTable.invalidID)
            {
                string textID = table.Get(m_destroyTextID);

                if (EditorUtility.DisplayDialog("Remove Text", "Are you sure to remove the text " + textID + "?", "Yes", "No"))
                    list.RemoveText(m_destroyTextID);

                m_destroyTextID = LocTable.invalidID;

                GUIUtility.ExitGUI();
            }
        }

        void DrawList()
        {
            var list = m_editor.locList;
            var table = list.GetTable();

            LocLanguage lang = null;
            if(m_langID.Length > 0)
            {
                lang = list.GetLanguage(m_langID);
                if (lang == null)
                {
                    EditorGUILayout.HelpBox("No asset for lang " + m_langID, MessageType.Error);
                    return;
                }
            }

            int nbCategories = table.CategoryCount();
            for(int i = 0; i <= nbCategories; i++)
            {
                int categoryID = i < nbCategories ? table.GetCategoryIdAt(i) : LocTable.invalidID;

                DrawCategory(categoryID, lang);
            }

            GUILayout.Space(5);
            bool isValid = !table.ContainsCategory(m_newCategoryID);
            if (!isValid)
                EditorGUILayout.HelpBox("This category id already exist", MessageType.Error);
            if (m_newCategoryID.Length <= 0)
                isValid = false;
            GUILayout.BeginHorizontal();
            GUILayout.Label("New category", GUILayout.Width(100));
            m_newCategoryID = GUILayout.TextField(m_newCategoryID);
            GUILayout.Space(5);
            GUI.enabled = isValid;
            if (GUILayout.Button("Create", GUILayout.Width(50)))
            {
                table.AddCategory(m_newCategoryID);
                m_newCategoryID = "";
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        void DrawCategory(int categoryID, LocLanguage lang)
        {
            var list = m_editor.locList;
            var table = list.GetTable();

            string categoryName = categoryID == LocTable.invalidID ? "No category" : table.GetCategoryName(categoryID);

            bool fold = true;
            m_categoriesStatus.TryGetValue(categoryID, out fold);

            SirenixEditorGUI.BeginBox();

            SirenixEditorGUI.BeginBoxHeader();
            GUILayout.BeginHorizontal();
            fold = EditorGUILayout.Foldout(fold, categoryName);
            m_categoriesStatus[categoryID] = fold;

            if (categoryID != LocTable.invalidID)
            {
                if (GUILayout.Button("X", GUILayout.Width(30), GUILayout.Height(15)))
                    m_destroyCategoryID = categoryID;
            }

            GUILayout.EndHorizontal();
            SirenixEditorGUI.EndBoxHeader();

            if (fold)
            {

                int nbText = table.Count();

                for (int j = 0; j < nbText; j++)
                {
                    int id = table.GetIdAt(j);
                    int textCategory = table.GetCategory(id);
                    if (textCategory != categoryID)
                        continue;

                    string textID = table.Get(id);
                    if (!Loc.ProcessFilter(textID, m_filter))
                        continue;

                    DrawText(id, lang);
                }

                GUILayout.Space(5);
                bool isValid = !table.Contains(m_newTextID);
                if (!isValid)
                    EditorGUILayout.HelpBox("This text id already exist", MessageType.Error);
                if (m_newTextID.Length <= 0)
                    isValid = false;
                GUILayout.BeginHorizontal();
                GUILayout.Label("New Text", GUILayout.Width(100));
                m_newTextID = GUILayout.TextField(m_newTextID);
                GUILayout.Space(5);
                GUI.enabled = isValid;
                if (GUILayout.Button("Create", GUILayout.Width(50)))
                {
                    list.AddText(m_newTextID, categoryID);
                    m_newTextID = "";
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }

            SirenixEditorGUI.EndBox();
        }

        void DrawText(int textID, LocLanguage lang)
        {
            if (lang == null)
                DrawTextAllLang(textID);
            else DrawTextOneLang(textID, lang);
        }

        void DrawTextOneLang(int id, LocLanguage lang)
        {
            SirenixEditorGUI.BeginBox();

            var list = m_editor.locList;
            var table = list.GetTable();

            DrawTextID(id);

            //dirty
            GUILayout.BeginHorizontal();
            GUILayout.Label("Dirty", GUILayout.Width(100));
            bool wasDirty = lang.GetDirty(id);
            bool newDirty = GUILayout.Toggle(wasDirty, "");
            if (wasDirty != newDirty)
                lang.SetDirty(id, newDirty);
            GUILayout.EndHorizontal();

            //text
            string text = lang.GetText(id);
            string newText = text;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Text", GUILayout.Width(100));
            newText = GUILayout.TextArea(text);
            if (newText != text)
                lang.SetText(id, newText);
            GUILayout.EndHorizontal();

            //comment
            text = table.GetRemark(id);
            newText = text;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Comment", GUILayout.Width(100));
            newText = GUILayout.TextArea(text);
            if (newText != text)
                table.SetRemark(id, newText);
            GUILayout.EndHorizontal();

            SirenixEditorGUI.EndBox();
        }

        void DrawTextAllLang(int id)
        {
            SirenixEditorGUI.BeginBox();

            var list = m_editor.locList;
            var table = list.GetTable();

            DrawTextID(id);

            string text = "";
            string newText = "";

            int nbLangs = list.GetNbLang();
            for(int i = 0; i < nbLangs; i++)
            {
                var lang = list.GetLanguage(i);

                GUILayout.BeginHorizontal();
                GUILayout.Label(lang.languageID, GUILayout.Width(100));
                bool wasDirty = lang.GetDirty(id);
                bool newDirty = GUILayout.Toggle(wasDirty, "", GUILayout.Width(20));
                if (wasDirty != newDirty)
                    lang.SetDirty(id, newDirty);

                text = lang.GetText(id);
                newText = text;
                newText = GUILayout.TextArea(text);
                if (newText != text)
                    lang.SetText(id, newText);
                GUILayout.EndHorizontal();
            }

            //comment
            text = table.GetRemark(id);
            newText = text;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Comment", GUILayout.Width(100));
            newText = GUILayout.TextArea(text);
            if (newText != text)
                table.SetRemark(id, newText);
            GUILayout.EndHorizontal();

            SirenixEditorGUI.EndBox();
        }

        void DrawUniquePopup(string textID)
        {
            var list = m_editor.locList;
            var table = list.GetTable();

            int nbText = table.Count();

            int nbFound = 0;
            for(int i = 0; i < nbText; i++)
            {
                int id = table.GetIdAt(i);
                if (table.Get(id) == textID)
                    nbFound++;
            }

            if(nbFound > 1)
                EditorGUILayout.HelpBox("This textID is not unique", MessageType.Error);
        }

        void DrawTextID(int id)
        {
            var grayStyle = new GUIStyle(GUI.skin.button);
            grayStyle.normal.textColor = Color.gray;

            var list = m_editor.locList;
            var table = list.GetTable();

            var textID = table.Get(id);

            DrawUniquePopup(textID);
            //textID
            GUILayout.BeginHorizontal();
            GUILayout.Label("ID", GUILayout.Width(100));
            string newText = textID;
            newText = GUILayout.TextField(textID);
            if (newText != textID)
                table.Set(id, newText);
            GUILayout.Space(5);
            GUILayout.Label(id.ToString(), grayStyle, GUILayout.Width(40));
            GUILayout.Space(5);
            if (GUILayout.Button("X", GUILayout.Width(30)))
                m_destroyTextID = id;
            GUILayout.EndHorizontal();
        }
    }

    public class LocEditorNewLangTab
    {
        LocEditor m_editor;
        string m_lang;
        string m_langID;
        bool m_langValid = false;
        bool m_langAlreadyExist = false;

        public LocEditorNewLangTab(LocEditor editor)
        {
            m_editor = editor;
        }

        [OnInspectorGUI]
        private void OnInspectorGUI()
        {
            GUILayout.Label("Display name");
            m_lang = GUILayout.TextField(m_lang);

            GUILayout.Space(5);
            GUILayout.Label("Language ID");
            if (!m_langValid)
                EditorGUILayout.HelpBox("This language id is not valid\nYou must follow the BCP47 standard.\nYou can found the supported names here\nhttps://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c", MessageType.Error);
            else if (m_langAlreadyExist)
                EditorGUILayout.HelpBox("This language already exist", MessageType.Error);
            else EditorGUILayout.HelpBox("Language " + GetDisplayLangID(m_langID), MessageType.Info);

            string newLangID = GUILayout.TextField(m_langID);
            if(newLangID != m_langID)
            {
                m_langID = newLangID;
                ValidateLangID();
            }

            if(GUILayout.Button("Create") && m_langValid)
            {
                var langs = m_editor.locList;
                langs.AddLang(m_langID, m_lang);
                m_editor.ChangeTab();
            }
        }

        void ValidateLangID()
        {
            m_langAlreadyExist = false;
            m_langValid = true;

            if(m_langID.Length <= 1)
            {
                m_langValid = false;
                return;
            }

            bool found = false;
            foreach (var ci in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
            {
                if(ci.Name == m_langID)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                m_langValid = false;
                return;
            }

            var loc = m_editor.locList;

            if (loc.GetLanguage(m_langID))
                m_langAlreadyExist = true;
        }

        string GetDisplayLangID(string id)
        {
            var info = CultureInfo.GetCultureInfo(id);
            if (info != null)
                return info.DisplayName;
            return "";
        }
    }

    public class LocEditorExportTab
    {
        LocEditor m_editor;

        public LocEditorExportTab(LocEditor editor)
        {
            m_editor = editor;
        }

        [OnInspectorGUI]
        private void OnInspectorGUI()
        {

        }
    }

    public class LocEditorImportTab
    {
        LocEditor m_editor;

        public LocEditorImportTab(LocEditor editor)
        {
            m_editor = editor;
        }


        [OnInspectorGUI]
        private void OnInspectorGUI()
        {

        }
    }
}

