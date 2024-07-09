using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GameLevelResources
{
    class ResourceInfo
    {
        public ResourceType type;
        public float count;
    }

    List<ResourceInfo> m_resources = new List<ResourceInfo>();

    public void Reset()
    {
        m_resources.Clear();
    }

    public void Save(JsonDocument doc)
    {
        var root = doc.GetRoot();
        if (root != null && root.IsJsonObject())
        {
            var rootObject = root.JsonObject();

            var resourcesObject = new JsonObject();
            rootObject.AddElement("Resources", resourcesObject);

            var resourcesArray = new JsonArray();
            resourcesObject.AddElement("List", resourcesArray);

            foreach (var r in m_resources)
            {
                var resourceObject = new JsonObject();
                resourcesArray.Add(resourceObject);
                resourceObject.AddElement("Type", r.type.ToString());
                resourceObject.AddElement("Count", r.count);
            }
        }
    }

    public void Load(JsonDocument doc)
    {
        Reset();

        var root = doc.GetRoot();
        if (root != null && root.IsJsonObject())
        {
            var rootObject = root.JsonObject();

            var resourcesObject = rootObject.GetElement("Resources")?.JsonObject();
            if (resourcesObject != null)
            {
                var dataArray = resourcesObject.GetElement("List")?.JsonArray();
                if (dataArray != null)
                {
                    foreach (var elem in dataArray)
                    {
                        var resourceObject = elem.JsonObject();
                        if (resourceObject != null)
                        {
                            ResourceInfo r = new ResourceInfo();

                            string str = resourceObject.GetElement("Type")?.String();
                            if (str != null)
                                Enum.TryParse(str, out r.type);

                            r.count = resourceObject.GetElement("Count")?.Float() ?? 0;

                            if (r.count > 0)
                                m_resources.Add(r);
                        }
                    }
                }
            }
        }
    }

    public float GetNb(ResourceType type)
    {
        foreach(var r in m_resources)
        {
            if (r.type == type)
                return r.count;
        }
        return 0;
    }

    public int GetNbInt(ResourceType type)
    {
        return (int)GetNb(type);
    }

    public void Add(ResourceType type, float nb)
    {
        if (nb <= 0)
            return;

        foreach(var r in m_resources)
        {
            if(r.type == type)
            {
                r.count += nb;
                return;
            }
        }

        ResourceInfo newR = new ResourceInfo();
        newR.type = type;
        newR.count = nb;
        m_resources.Add(newR);
    }

    public float Remove(ResourceType type, float nb)
    {
        if (nb <= 0)
            return 0;

        for(int i = 0; i < m_resources.Count; i++)
        {
            var r = m_resources[i];

            if(r.type == type)
            {
                if(nb >= r.count)
                {
                    float nbRemoved = r.count;
                    m_resources.RemoveAt(i);
                    return nbRemoved;
                }

                r.count -= nb;
                return nb;
            }
        }

        return 0;
    }

    public int Remove(ResourceType type, int nb)
    {
        if (nb <= 0)
            return 0;

        for (int i = 0; i < m_resources.Count; i++)
        {
            var r = m_resources[i];

            if (r.type == type)
            {
                int countInt = (int)r.count;

                if(countInt == r.count && nb >= countInt)
                {
                    int nbRemoved = countInt;
                    m_resources.RemoveAt(i);
                    return nbRemoved;
                }
                if(nb >= countInt)
                {
                    r.count -= countInt;
                    return countInt;
                }
                
                r.count -= nb;
                return nb;
            }
        }

        return 0;
    }
}
