using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GamemodeViewWaves : GamemodeViewBase
{
    GamemodeWaves m_mode;

    public GamemodeViewWaves(GamemodeWaves mode)
    {
        m_mode = mode;
    }

    public override GamemodeBase GetGamemode()
    {
        return m_mode;
    }

    public override void Init()
    {

    }

    public override void OnDestroy()
    {

    }

    public override void OnGui(Vector2 position)
    {
        GUILayout.Label("Prout !");
    }
}
