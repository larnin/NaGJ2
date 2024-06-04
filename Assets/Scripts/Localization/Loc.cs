using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace NLocalization
{
    public static class Loc
    {
        class LocData
        {
            public string textID;
            public string text;

            public LocData(string _textID, string _text)
            {
                textID = _textID;
                text = _text;
            }
        }

        static Dictionary<int, LocData> m_texts = null;

        public static string Tr(int id)
        {
            if (m_texts == null)
                Load();

            LocData data = null;
            m_texts.TryGetValue(id, out data);

            if (data == null)
                return "TEXT NOT FOUND";

            return data.text;
        }

        public static string Tr(int id, params object[] args)
        {
            var str = Tr(id);

            for (int i = 0; i < args.Length; i++)
            {
                string key = "{" + i + "}";
                str = str.Replace(key, args[i].ToString());
            }

            return str;
        }

        public static string Tr(string textID)
        {
            if (m_texts == null)
                Load();

            int id = GetID(textID);
            return Tr(id);
        }

        public static string Tr(string textID, params object[] args)
        {
            var str = Tr(textID);

            for (int i = 0; i < args.Length; i++)
            {
                string key = "{" + i + "}";
                str = str.Replace(key, args[i].ToString());
            }

            return str;
        }

        public static int GetTextID(string textID)
        {
            return GetID(textID);
        }

        static int GetID(string textID)
        {
            if (m_texts == null)
                Load();

            foreach (var t in m_texts)
            {
                if (t.Value.textID == textID)
                    return t.Key;
            }

            return LocTable.invalidID;
        }

        public static void Load()
        {
            if (m_texts == null)
                m_texts = new Dictionary<int, LocData>();
            m_texts.Clear();

            string currentLang = Settings.instance.language;

            var list = new LocList();
            list.Load();

            var table = list.GetTable();
            if (table == null)
            {
                Debug.LogError("No localization table found");
                return;
            }

            var lang = list.GetLanguage(currentLang);
            if (lang == null)
                list.GetLanguage(table.defaultLanguageID);
            if (lang == null)
            {
                if (list.GetNbLang() > 0)
                {
                    lang = list.GetLanguage(0);
                    Debug.LogWarning("Default or selected langage not found, using the first langage found : " + lang.languageID);
                }
                else
                {
                    Debug.LogError("No langage found");
                    return;
                }
            }

            int nbText = table.Count();
            for (int i = 0; i < nbText; i++)
            {
                int id = table.GetIdAt(i);
                string textID = table.Get(id);
                string text = lang.GetText(id);

                m_texts.Add(id, new LocData(textID, text));
            }

            Debug.Log("Localization loaded : " + lang.languageID);
        }

        public static bool ProcessFilter(string text, string filter)
        {
            if (filter.Length <= 0)
                return true;

            bool startWithMinus = filter[0] == '-';
            if (startWithMinus && filter.Length <= 1)
                return true;

            if (!startWithMinus)
                return text.Contains(filter, StringComparison.CurrentCultureIgnoreCase);

            string subFilter = filter.Substring(1);

            return !text.Contains(subFilter, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}