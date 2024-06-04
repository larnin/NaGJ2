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
    public class LocTable : SerializedScriptableObject
    {
        class LocCategory
        {
            public int id;
            public string name;

            public LocCategory(int _id, string _name)
            {
                id = _id;
                name = _name;
            }
        }

        class LocElement
        {
            public int id;
            public int category;
            public string textID;
            public string remark;

            public LocElement(int _id, string _textID, int _category = LocTable.invalidID)
            {
                id = _id;
                textID = _textID;
                category = _category;
                remark = "";
            }
        }

        public const int invalidID = -1;

        [HideInInspector]
        [SerializeField] int m_nextID = 0;

        [HideInInspector]
        [SerializeField] int m_nextCategory = 0;

        [HideInInspector] [SerializeField]
        List<LocElement> m_locs = new List<LocElement>();

        [HideInInspector] [SerializeField]
        List<LocCategory> m_categories = new List<LocCategory>();

        [HideInInspector]
        [SerializeField] string m_defaultLanguageID;
        public string defaultLanguageID { get { return m_defaultLanguageID; } set { m_defaultLanguageID = value; } }

        public int Add(string textID, int category = invalidID)
        {
            if (Contains(textID))
                return invalidID;

            ValidateNextID();

            if (category != invalidID && !ContainsCategory(category))
                category = invalidID;

            var element = new LocElement(m_nextID, textID, category);

            m_locs.Add(element);
            m_nextID++;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif

            return element.id;
        }

        public string Add(int id, int category = invalidID)
        {
            var element = GetInternal(id);
            if (element != null)
                return element.textID;

            if (category != invalidID && !ContainsCategory(category))
                category = invalidID;

            if (m_nextID <= id)
                m_nextID = id + 1;

            string label = "ID_";
            string text = label + id;

            for(int i = 0; i <= m_nextID; i++)
            {
                element = GetInternal(text);
                if (element == null)
                {
                    m_locs.Add(new LocElement(id, text));

#if UNITY_EDITOR
                    EditorUtility.SetDirty(this);
                    AssetDatabase.SaveAssets();
#endif

                    return text;
                }
                text = label + id;
            }

            return null;
        }

        public bool ForceAdd(int id, string textID, int category = invalidID)
        {
            var element = GetInternal(id);
            var textElement = GetInternal(textID);

            if(element != null && textElement != null)
                return element == textElement;

            if (category != invalidID && !ContainsCategory(category))
                category = invalidID;

            if (element != null)
            {
                element.textID = textID;

#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
#endif

                return true;
            }

            if(textElement != null)
            {
                textElement.id = id;

#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
#endif

                return true;
            }

            if (m_nextID <= id)
                m_nextID = id + 1;

            m_locs.Add(new LocElement(id, textID, category));

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif

            return true;
        }

        public void Remove(int id)
        {
            for (int i = 0; i < m_locs.Count; i++)
            {
                if (m_locs[i].id == id)
                {
                    m_locs.RemoveAt(i);

#if UNITY_EDITOR
                    EditorUtility.SetDirty(this);
                    AssetDatabase.SaveAssets();
#endif

                    return;
                }
            }
        }

        public void Remove(string textID)
        {
            for (int i = 0; i < m_locs.Count; i++)
            {
                if (m_locs[i].textID == textID)
                {
                    m_locs.RemoveAt(i);

#if UNITY_EDITOR
                    EditorUtility.SetDirty(this);
                    AssetDatabase.SaveAssets();
#endif

                    return;
                }
            }
        }

        public int AddCategory(string name)
        {

            ValidateNextID();

            int id = m_nextCategory;
            m_nextCategory++;

            m_categories.Add(new LocCategory(id, name));

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif

            return id;
        }

        public bool RemoveCategory(int id)
        {
            for(int i = 0; i < m_categories.Count; i++)
            {
                if(m_categories[i].id == id)
                {
                    m_categories.RemoveAt(i);

                    foreach(var l in m_locs)
                    {
                        if (l.category == id)
                            l.category = invalidID;
                    }

#if UNITY_EDITOR
                    EditorUtility.SetDirty(this);
                    AssetDatabase.SaveAssets();
#endif

                    return true;
                }
            }

            return false;
        }

        public bool Contains(int id)
        {
            return GetInternal(id) != null;
        }

        public bool Contains(string textID)
        {
            return GetInternal(textID) != null;
        }

        public bool ContainsCategory(int id)
        {
            return GetCategoryInternal(id) != null;
        }

        public bool ContainsCategory(string name)
        {
            return GetCategoryInternal(name) != null;
        }

        public string Get(int id)
        {
            var l = GetInternal(id);
            if (l != null)
                return l.textID;
            return null;
        }

        public int Get(string textID)
        {
            var l = GetInternal(textID);
            if (l != null)
                return l.id;
            return invalidID;
        }

        public bool Set(int id, string textID)
        {
            if (Contains(textID))
                return false;

            var l = GetInternal(id);
            if (l != null)
            {
                l.textID = textID;

#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
#endif

                return true;
            }
            return false;
        }

       public void SetRemark(int id, string remark)
        {
            var l = GetInternal(id);

            if (l != null)
            {
                l.remark = remark;

#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
#endif
            }
        }

        public string GetRemark(int id)
        {
            var l = GetInternal(id);
            if (l != null)
                return l.remark;
            return null;
        }

        public void SetCategory(int id, int categoryID)
        {
            var l = GetInternal(id);
            var c = GetCategoryInternal(categoryID);

            if (l != null && c != null)
            {
                l.category = categoryID;

#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
#endif
            }
        }

        public int GetCategory(int id)
        {
            var l = GetInternal(id);

            if (l != null)
                return l.category;
            return invalidID;
        }

        public void SetCategoryName(int id, string name)
        {
            var c = GetCategoryInternal(id);
            if(c != null)
            {
                c.name = name;

#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
#endif
            }
        }

        public string GetCategoryName(int id)
        {
            var c = GetCategoryInternal(id);
            if (c != null)
                return c.name;
            return null;
        }

        public int Count()
        {
            return m_locs.Count;
        }

        public int GetIdAt(int index)
        {
            if (index < 0 || index >= m_locs.Count)
                return invalidID;
            return m_locs[index].id;
        }

        public int CategoryCount()
        {
            return m_categories.Count;
        }

        public int GetCategoryIdAt(int index)
        {
            if (index < 0 || index >= m_categories.Count)
                return invalidID;
            return m_categories[index].id;
        }

        LocElement GetInternal(int id)
        {
            foreach (var l in m_locs)
                if (l.id == id)
                    return l;
            return null;
        }

        LocElement GetInternal(string textID)
        {
            foreach (var l in m_locs)
                if (l.textID == textID)
                    return l;
            return null;
        }

        LocCategory GetCategoryInternal(int id)
        {
            foreach (var c in m_categories)
                if (c.id == id)
                    return c;
            return null;
        }

        LocCategory GetCategoryInternal(string name)
        {
            foreach (var c in m_categories)
                if (c.name == name)
                    return c;
            return null;
        }

        void ValidateNextID()
        {
            foreach (var l in m_locs)
            {
                if (m_nextID <= l.id)
                {
                    m_nextID = l.id + 1;
#if UNITY_EDITOR
                    EditorUtility.SetDirty(this);
#endif
                }
            }

            foreach(var c in m_categories)
            {
                if(m_nextCategory < c.id)
                {
                    m_nextCategory = c.id + 1;
#if UNITY_EDITOR
                    EditorUtility.SetDirty(this);
#endif
                }
            }

#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
#endif
        }
    }
}