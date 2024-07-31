using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameEditorWindowFile : GameEditorWindowBase
{
    string m_currentPath = "";

    public override void OnGUI()
    {
        GUILayout.Label(GetFileName());

        if (GUILayout.Button("New"))
            OnNew();

        if (GUILayout.Button("Save"))
            OnSave();

        if (GUILayout.Button("Save As ..."))
            OnSaveAs();

        if (GUILayout.Button("Load"))
            OnLoad();

        if (GUILayout.Button("Quit"))
            OnQuit();
    }

    string GetFileName()
    {
        if (m_currentPath.Length == 0)
            return "New Level";

        string filename = "";
        int posSlash = m_currentPath.LastIndexOfAny(new char[] { '/', '\\' });
        if (posSlash >= 0)
            filename = m_currentPath.Substring(posSlash + 1);
        else String.Copy(m_currentPath);

        int posDot = filename.LastIndexOf('.');
        if (posDot > 0)
            filename = filename.Substring(0, posDot);

        return filename;
    }

    void OnNew()
    {

    }

    void OnSave()
    {

    }

    void OnSaveAs()
    {

    }

    void OnLoad()
    {

    }

    void OnQuit()
    {

    }
}
