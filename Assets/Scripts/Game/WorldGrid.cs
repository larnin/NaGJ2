using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

struct OneBlock
{
    public SimpleBlock block;
    public GameObject instance;
}

public class WorldGrid : MonoBehaviour
{
    ResizableMatrix3<OneBlock> m_grid = new ResizableMatrix3<OneBlock>(1, 1, 1);

    SubscriberList m_subscriberList = new SubscriberList();

    NearMatrix3<SimpleBlock> m_tempMatrix = new NearMatrix3<SimpleBlock>();

    private void Awake()
    {
        m_subscriberList.Add(new Event<SetBlockEvent>.Subscriber(SetBlock));
        m_subscriberList.Add(new Event<GetBlockEvent>.Subscriber(GetBlock));
        m_subscriberList.Add(new Event<SaveEvent>.Subscriber(OnSave));
        m_subscriberList.Add(new Event<LoadEvent>.Subscriber(OnLoad));
        m_subscriberList.Add(new Event<NewLevelEvent>.Subscriber(OnNew));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void SetBlock(SetBlockEvent e)
    {
        SetBlock(e.pos, e.type, e.data);
    }

    void GetBlock(GetBlockEvent e)
    {
        if(!m_grid.IsInGrid(e.pos.x, e.pos.y, e.pos.z))
        {
            e.type = BlockType.air;
            e.data = 0;
            return;
        }

        var b = m_grid.Get(e.pos.x, e.pos.y, e.pos.z);

        e.type = b.block.id;
        e.data = b.block.data;
    }

    void SetBlock(Vector3Int pos, BlockType type, byte data)
    {
        OneBlock block = new OneBlock();
        if (m_grid.IsInGrid(pos.x, pos.y, pos.z))
            block = m_grid.Get(pos.x, pos.y, pos.z);
        block.block.id = type;
        block.block.data = data;

        m_grid.Set(block, pos.x, pos.y, pos.z);

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    Vector3Int tempPos = new Vector3Int(pos.x + i, pos.y + j, pos.z + k);
                    UpdateBlock(tempPos);
                }
            }
        }
    }

    void UpdateBlock(Vector3Int pos)
    {
        if (!m_grid.IsInGrid(pos.x, pos.y, pos.z))
            return;

        var element = m_grid.Get(pos.x, pos.y, pos.z);

        if(element.instance != null)
            Destroy(element.instance);

        if(element.block.id == BlockType.air)
            return;

        SetNearMatrix(pos, m_tempMatrix);

        GameObject prefab;
        Quaternion rot;
        BlockDataEx.GetBlockInfo(element.block.id, element.block.data, m_tempMatrix, out prefab, out rot);

        if (prefab == null)
            return;

        var obj = InstantiateAndPlaceBlock(prefab, rot, pos);

        element.instance = obj;
        m_grid.Set(element, pos.x, pos.y, pos.z);
    }

    void SetNearMatrix(Vector3Int pos, NearMatrix3<SimpleBlock> matrix)
    {
        matrix.Reset(SimpleBlock.defaultValue);

        for(int i = -1; i <= 1; i++)
        {
            for(int j = -1; j <= 1; j++)
            {
                for(int k = -1; k <= 1; k++)
                {
                    int x = pos.x + i;
                    int y = pos.y + j;
                    int z = pos.z + k;

                    if (!m_grid.IsInGrid(x, y, z))
                        matrix.Set(SimpleBlock.defaultValue, i, j, k);
                    else
                    {
                        var element = m_grid.Get(x, y, z);
                        matrix.Set(element.block, i, j, k);
                    }
                }
            }
        }
    }

    GameObject InstantiateAndPlaceBlock(GameObject prefab, Quaternion rot, Vector3Int pos)
    {
        Vector3 size = Global.instance.allBlocks.blockSize;

        Vector3 offset = new Vector3(size.x * pos.x, size.y * pos.y, size.z * pos.z);

        var obj = Instantiate(prefab);
        obj.transform.parent = transform;
        obj.transform.localPosition = offset;
        obj.transform.localRotation = rot;

        return obj;
    }

    void OnNew(NewLevelEvent e)
    {
        for (int i = m_grid.MinX(); i <= m_grid.MaxX(); i++)
        {
            for (int j = m_grid.MinY(); j <= m_grid.MaxY(); j++)
            {
                for (int k = m_grid.MinZ(); k <= m_grid.MaxZ(); k++)
                {
                    var item = m_grid.Get(i, j, k);
                    if (item.instance != null)
                        Destroy(item.instance);
                }
            }
        }

        m_grid.Reset(1, 1, 1);
    }

    void OnSave(SaveEvent e)
    {
        var root = e.document.GetRoot();
        if (root != null && root.IsJsonObject())
        {
            var rootObject = root.JsonObject();

            var worldObject = new JsonObject();
            rootObject.AddElement("World", worldObject);

            worldObject.AddElement("Size", Json.FromVector3Int(new Vector3Int(m_grid.Width(), m_grid.Height(), m_grid.Depth())));
            worldObject.AddElement("Origin", Json.FromVector3Int(new Vector3Int(m_grid.OriginX(), m_grid.OriginY(), m_grid.OriginZ())));

            var dataArray = new JsonArray();
            worldObject.AddElement("Data", dataArray);
            for (int i = m_grid.MinX(); i <= m_grid.MaxX(); i++)
            {
                for (int j = m_grid.MinY(); j <= m_grid.MaxY(); j++)
                {
                    for (int k = m_grid.MinZ(); k <= m_grid.MaxZ(); k++)
                    {
                        var item = m_grid.Get(i, j, k);

                        var elemObject = new JsonObject();
                        dataArray.Add(elemObject);
                        elemObject.AddElement("T", new JsonString(item.block.id.ToString()));
                        elemObject.AddElement("D", new JsonNumber(item.block.data));

                    }
                }
            }
        }
    }

    void OnLoad(LoadEvent e)
    {
        for (int i = m_grid.MinX(); i <= m_grid.MaxX(); i++)
        {
            for (int j = m_grid.MinY(); j <= m_grid.MaxY(); j++)
            {
                for (int k = m_grid.MinZ(); k <= m_grid.MaxZ(); k++)
                {
                    var item = m_grid.Get(i, j, k);
                    if (item.instance != null)
                        Destroy(item.instance);
                }
            }
        }

        var root = e.document.GetRoot();
        if(root != null && root.IsJsonObject())
        {
            var rootObject = root.JsonObject();

            var worldObject = rootObject.GetElement("World")?.JsonObject();
            if(worldObject != null)
            {
                var size = Json.ToVector3Int(worldObject.GetElement("Size")?.JsonArray(), Vector3Int.one);

                var origin = Json.ToVector3Int(worldObject.GetElement("Origin")?.JsonArray(), Vector3Int.zero);

                m_grid.Reset(size.x, size.y, size.z, origin.x, origin.y, origin.z);

                var dataArray = worldObject.GetElement("Data")?.JsonArray();
                if (dataArray != null && dataArray.Size() == size.x * size.y * size.z)
                {
                    for (int i = m_grid.MinX(); i <= m_grid.MaxX(); i++)
                    {
                        for (int j = m_grid.MinY(); j <= m_grid.MaxY(); j++)
                        {
                            for (int k = m_grid.MinZ(); k <= m_grid.MaxZ(); k++)
                            {
                                int index = (k - m_grid.MinZ()) + (j - m_grid.MinY()) * size.z + (i - m_grid.MinX()) * size.y * size.z;

                                var elemObject = dataArray[index].JsonObject();
                                if(elemObject != null)
                                {
                                    var item = new OneBlock();

                                    string name = elemObject.GetElement("T")?.String();
                                    if (name != null)
                                        Enum.TryParse<BlockType>(name, out item.block.id);
                                    item.block.data = (byte)(elemObject.GetElement("D")?.Int() ?? 0);

                                    m_grid.Set(item, i, j, k);
                                }
                            }
                        }
                    }
                }
            }
        }

        for (int i = m_grid.MinX(); i <= m_grid.MaxX(); i++)
        {
            for (int j = m_grid.MinY(); j <= m_grid.MaxY(); j++)
            {
                for (int k = m_grid.MinZ(); k <= m_grid.MaxZ(); k++)
                {
                    UpdateBlock(new Vector3Int(i, j, k));
                }
            }
        }
    }
}
