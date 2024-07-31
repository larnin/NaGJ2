using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DebugPopupData
{
    public string name;
    public bool selected;

    public DebugPopupData(string _name, bool _selected = false)
    {
        name = _name;
        selected = _selected;
    }
}

public static class DebugLayout
{
    //return new selected state
    public static int DrawPopup(Rect rect, ref bool selected, string label, DebugPopupData[] datas)
    {
        int returnValue = -1;

        const float buttonSpacing = 2;
        const float buttonHeight = 20;

        if (GUI.Button(rect, label))
            selected = !selected;

        if (selected)
        {
            float height = datas.Length * buttonHeight + buttonSpacing + buttonSpacing;
            GUI.Box(new Rect(rect.x, rect.y + rect.height, rect.width, height), "");

            for (int i = 0; i < datas.Length; i++)
            {
                float y = buttonSpacing + i * (buttonSpacing + buttonHeight);
                if (GUI.Button(new Rect(rect.x + buttonSpacing, rect.y + rect.height + y, rect.width - 2 * buttonSpacing, buttonHeight), datas[i].name))
                {
                    selected = false;
                    returnValue = i;
                }

                if (datas[i].selected)
                {
                    GUI.Label(new Rect(rect.x + rect.width - buttonSpacing - buttonHeight, rect.y + rect.height + y, buttonHeight, buttonHeight), "✓");
                }
            }
        }

        return returnValue;
    }
}
