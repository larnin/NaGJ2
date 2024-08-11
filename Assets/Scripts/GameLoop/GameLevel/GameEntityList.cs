using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GameEntityList
{
    List<GameEntity> m_entities = new List<GameEntity>();
    Dictionary<int, int> m_idDictionary = new Dictionary<int, int>();

    List<int> m_toRemoveIndexs = new List<int>();

    int m_nextID = 1;

    GameLevel m_level;

    public GameEntityList(GameLevel level)
    {
        m_level = level;
    }

    public void Load(JsonDocument doc)
    {
        Reset();


        var root = doc.GetRoot();
        if (root != null && root.IsJsonObject())
        {
            var rootObject = root.JsonObject();

            var entitiesObject = rootObject.GetElement("Entities")?.JsonObject();
            if (entitiesObject != null)
            {
                m_nextID = entitiesObject.GetElement("NextID")?.Int() ?? 0;

                var dataArray = entitiesObject.GetElement("List")?.JsonArray();
                if (dataArray != null)
                {
                    foreach (var elem in dataArray)
                    {
                        var entityObject = elem.JsonObject();
                        if (entityObject != null)
                        {
                            string str = entityObject.GetElement("Type")?.String();
                            EntityType type = default;
                            if (str != null)
                            {
                                if (Enum.TryParse(str, out type))
                                {
                                    var entity = new GameEntity(type, m_level);
                                    entity.Load(entityObject);
                                    m_entities.Add(entity);
                                }
                            }
                        }
                    }
                }
            }
        }

        CreateDictionaties();
    }

    void CreateDictionaties()
    {
        m_idDictionary.Clear();

        for (int x = 0; x < m_entities.Count; x++)
            m_idDictionary.Add(m_entities[x].GetID(), x);
    }

    public void Save(JsonDocument doc)
    {
        var root = doc.GetRoot();
        if (root != null && root.IsJsonObject())
        {
            var rootObject = root.JsonObject();

            var worldObject = new JsonObject();
            rootObject.AddElement("Entities", worldObject);

            var entitiesArray = new JsonArray();
            worldObject.AddElement("List", entitiesArray);
            worldObject.AddElement("NextID", new JsonNumber(m_nextID));

            foreach (var e in m_entities)
            {
                var entityObject = new JsonObject();
                entitiesArray.Add(entityObject);
                e.Save(entityObject);
            }
        }
    }

    public void Reset()
    {
        m_entities.Clear();
        m_idDictionary.Clear();

        m_nextID = 1;
    }

    public void Process(float deltaTime)
    {
        for (int i = 0; i < m_entities.Count; i++)
            m_entities[i].Process(deltaTime);

        ProcessRemove();
    }

    public bool Add(GameEntity entity)
    {
        return false;
    }

    public bool Remove(int ID)
    {
        int index = -1;
        if (!m_idDictionary.TryGetValue(ID, out index))
            return false;

        if (m_toRemoveIndexs.Contains(index))
            return false;

        m_toRemoveIndexs.Add(index);
        return true;
    }

    void ProcessRemove()
    {
        for (int i = 0; i < m_toRemoveIndexs.Count; i++)
        {
            RemoveReal(m_toRemoveIndexs[i]);

            for (int j = i + 1; j < m_toRemoveIndexs.Count; j++)
            {
                if (m_toRemoveIndexs[j] > m_toRemoveIndexs[i])
                    m_toRemoveIndexs[j]--;
            }
        }

        m_toRemoveIndexs.Clear();
    }

    void RemoveReal(int index)
    {
        //todo
        
    }

    public int GetEntityIndex(int ID)
    {
        int index = -1;

        if (!m_idDictionary.TryGetValue(ID, out index))
            return -1;

        if (m_toRemoveIndexs.Contains(index))
            return -1;

        return index;
    }

    public GameEntity GetEntity(int ID)
    {
        int index = -1;

        if (!m_idDictionary.TryGetValue(ID, out index))
            return null;

        if (m_toRemoveIndexs.Contains(index))
            return null;

        return m_entities[index];
    }

    public int GetBuildingNb()
    {
        return m_entities.Count;
    }

    public GameEntity GetEntityFromIndex(int index)
    {
        if (index < 0 || index >= m_entities.Count)
            return null;

        if (m_toRemoveIndexs.Contains(index))
            return null;

        return m_entities[index];
    }
}
