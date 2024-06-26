using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameLevel
{
    GameGrid m_grid;
    public GameGrid grid { get { return m_grid; } }

    GameBuildingList m_buildingList;
    public GameBuildingList buildingList { get { return m_buildingList; } }

    GameBeltSystem m_beltSystem;
    public GameBeltSystem beltSystem { get { return m_beltSystem; } }

    bool m_active;
    public bool active { get { return m_active; } set { m_active = value; } }

    public GameLevel()
    {
        m_grid = new GameGrid(this);
        m_buildingList = new GameBuildingList(this);
        m_beltSystem = new GameBeltSystem(this);
    }

    public void Load(JsonDocument doc)
    {
        m_grid.Load(doc);
        m_buildingList.Load(doc);
        m_beltSystem.Load(doc);

        m_beltSystem.AfterLoad();
    }

    public void Save(JsonDocument doc)
    {
        m_grid.Save(doc);
        m_buildingList.Save(doc);
        m_beltSystem.Save(doc);
    }

    public void Reset()
    {
        m_grid.Reset();
        m_buildingList.Reset();
        m_beltSystem.Reset();
    }

    public void Process(float deltaTime)
    {
        if (Gamestate.instance.paused)
            return;

        m_buildingList.Process(deltaTime);
        m_beltSystem.Process(deltaTime);
    }

    public void OnBuildingUpdate(int buildingID, ElementUpdateType type)
    {
        m_beltSystem.OnBuildingUpdate(buildingID, type);

        if (m_active)
            Event<BuildingUpdateEvent>.Broadcast(new BuildingUpdateEvent(buildingID, type));
    }

    public void OnResourceUpdate(int resourceID, ElementUpdateType type)
    {
        if (m_active)
            Event<ResourceUpdateEvent>.Broadcast(new ResourceUpdateEvent(resourceID, type));
    }

    public void OnBlockUpdate(Vector3Int pos)
    {
        if (m_active)
            Event<BlockUpdateEvent>.Broadcast(new BlockUpdateEvent(pos));
    }
}