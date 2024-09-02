﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GamemodeViewWaves : GamemodeViewBase
{
    List<bool> m_openList = new List<bool>();
    List<GUIDropdown> m_groupsDropdown = new List<GUIDropdown>();
    int m_currentDropdownIndex = 0;

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
        m_currentDropdownIndex = 0;
        GUILayout.BeginHorizontal();
        GUILayout.Label("Spawn points :");
        if (GUILayout.Button("Add", GUILayout.MaxWidth(100)))
            AddNewPoint();
        GUILayout.EndHorizontal();

        for(int i = 0; i < m_mode.GetPointNb(); i++)
        {
            var point = m_mode.GetPointFromIndex(i);
            if (DrawOnePointHeader(point, i))
            {
                GUILayout.BeginHorizontal();
                GUIEx.DrawVerticalLine(Color.white, 2, true);
                GUILayout.BeginVertical();
                DrawPoint(point, position);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
                
        }
    }

    void DrawPoint(GamemodeWaves.GamemodeWavesInfos point, Vector2 position)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Pos: ", GUILayout.MaxWidth(30));
        point.spawnPoint = GUIEx.Vector3IntField(point.spawnPoint);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Rot: ", GUILayout.MaxWidth(30));
        point.spawnPointRot = GUIEx.FloatField(point.spawnPointRot);
        GUILayout.Space(8);
        GUILayout.Label("Radius: ", GUILayout.MaxWidth(50));
        point.spawnRadius = GUIEx.FloatField(point.spawnRadius);
        GUILayout.Space(8);
        GUILayout.Label("Start:", GUILayout.MaxWidth(40));
        point.startIndex = GUIEx.IntField(point.startIndex);
        if (point.startIndex < 0)
            point.startIndex = 0;
        GUILayout.EndHorizontal();

        for(int i = 0; i < point.waves.Count; i++)
        {
            bool drawWave = true;
            var wave = point.waves[i];
            GUILayout.BeginHorizontal();
            GUILayout.Label("Wave " + (i + point.startIndex).ToString());
            if(GUILayout.Button("X", GUILayout.MaxWidth(20)))
            {
                drawWave = false;
                point.waves.RemoveAt(i);
            }
            GUILayout.EndHorizontal();

            if (drawWave)
            {
                GUILayout.BeginHorizontal();
                GUIEx.DrawVerticalLine(Color.white, 2);
                GUILayout.BeginVertical();
                DrawOneWave(wave, position);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }
       
        if(GUILayout.Button("Add Wave"))
        {
            point.waves.Add(new GameEntityWave());
        }
    }

    void DrawOneWave(GameEntityWave wave, Vector2 position)
    {
        for(int i = 0; i < wave.groups.Count; i++)
        {
            var dropDown = GetGroupDropdown(m_currentDropdownIndex);
            m_currentDropdownIndex++;

            dropDown.SetLabel(wave.groups[i].type.ToString());

            bool draw = true;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Type:", GUILayout.MaxWidth(50));
            int select = dropDown.OnGUI(position);
            if (select >= 0)
                wave.groups[i].type = (EntityType)select;
            if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
            {
                draw = false;
                wave.groups.RemoveAt(i);
            }
            GUILayout.EndHorizontal();

            if (draw)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Count:", GUILayout.MaxWidth(40));
                wave.groups[i].count = GUIEx.IntField(wave.groups[i].count);
                GUILayout.Label("Delay:", GUILayout.MaxWidth(40));
                wave.groups[i].delay = GUIEx.FloatField(wave.groups[i].delay);
                GUILayout.Label("Spacing:", GUILayout.MaxWidth(55));
                wave.groups[i].timeSpacing = GUIEx.FloatField(wave.groups[i].timeSpacing);
                GUILayout.EndHorizontal();
            }
        }

        if (GUILayout.Button("Add group"))
        {
            wave.groups.Add(new GameEntityGroup());
        }
    }

    GUIDropdown GetGroupDropdown(int index)
    {
        while(index >= m_groupsDropdown.Count)
        {
            var dropdown = new GUIDropdown();
            List<string> names = new List<string>();
            int nbNames = Enum.GetValues(typeof(EntityType)).Length;
            for (int i = 0; i < nbNames; i++)
                names.Add(((EntityType)i).ToString());
            dropdown.SetDatas(names);
            m_groupsDropdown.Add(dropdown);
        }

        return m_groupsDropdown[index];
    }

    bool DrawOnePointHeader(GamemodeWaves.GamemodeWavesInfos point, int index)
    {
        string label = index.ToString() + ": " + point.spawnPoint.ToString();

        while (m_openList.Count <= index)
            m_openList.Add(false);

        bool opened = m_openList[index];

        var style = new GUIStyle(GUI.skin.button);
        style.alignment = TextAnchor.MiddleLeft;

        GUILayout.BeginHorizontal();

        if (GUILayout.Button(label, style))
        {
            opened = !opened;
            m_openList[index] = opened;
        }

        if (GUILayout.Button("X", GUILayout.Width(25)))
        {
            m_mode.RemovePointAt(index);
            opened = false;
        }

        GUILayout.EndHorizontal();

        return opened;
    }

    void AddNewPoint()
    {
        //todo place point on an empty pos
        m_mode.AddPoint(new GamemodeWaves.GamemodeWavesInfos());
    }
}
