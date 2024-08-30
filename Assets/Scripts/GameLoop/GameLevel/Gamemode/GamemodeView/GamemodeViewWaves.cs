using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GamemodeViewWaves : GamemodeViewBase
{
    Vector2 m_scrollPos = Vector2.zero;
    List<bool> m_openList = new List<bool>();

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
        m_scrollPos = GUILayout.BeginScrollView(m_scrollPos);

        for(int i = 0; i < m_mode.GetPointNb(); i++)
        {

        }

        GUILayout.EndScrollView();
    }

    void DrawWave(GamemodeWaves.GamemodeWavesInfos wave)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Pos: ");
        wave.spawnPoint = GUIEx.Vector3IntField(wave.spawnPoint);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Rot: ");
        wave.spawnPointRot = GUIEx.FloatField(wave.spawnPointRot);
        GUIEx.DrawVerticalLine(Color.white, 1, false);
        GUILayout.Label("Radius: ");
        wave.spawnRadius = GUIEx.FloatField(wave.spawnRadius);
        GUILayout.EndHorizontal();
    }
}
