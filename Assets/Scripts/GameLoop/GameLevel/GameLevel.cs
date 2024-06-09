using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GameLevel
{
    GameGrid m_grid;
    public GameGrid grid { get { return m_grid; } }

    GameBuildingList m_buildingList;
    public GameBuildingList buildingList { get { return m_buildingList; } }

    GameBeltSystem m_beltSystem;
    public GameBeltSystem beltSystem { get { return m_beltSystem; } }

    public GameLevel()
    {
        m_grid = new GameGrid();
        m_buildingList = new GameBuildingList(this);
        m_beltSystem = new GameBeltSystem(this);
    }

    public void Load(JsonDocument doc)
    {
        m_grid.Load(doc);
        m_buildingList.Load(doc);

        m_beltSystem.AfterLoad();
    }

    public void Save(JsonDocument doc)
    {
        m_grid.Save(doc);
        m_buildingList.Save(doc);
    }

    public void Process(float deltaTime)
    {
        if (Gamestate.instance.paused)
            return;

        m_buildingList.Process(deltaTime);
        m_beltSystem.Process(deltaTime);
    }

    public void OnBuildingUpdate(int buildingID, BuildingUpdateType type)
    {
        m_beltSystem.OnBuildingUpdate(buildingID, type);
    }
}