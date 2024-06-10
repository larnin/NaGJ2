using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameGrid
{
    GameLevel m_level;

    ResizableMatrix3<SimpleBlock> m_grid = new ResizableMatrix3<SimpleBlock>(1, 1, 1);

    public GameGrid(GameLevel level)
    {
        m_level = level;
    }

    public void Reset()
    {
        m_grid.Reset(1, 1, 1);
    }

    public void SetBlock(Vector3Int pos, BlockType id, byte data)
    {
        SetBlock(pos, new SimpleBlock(id, data));
    }

    public void SetBlock(Vector3Int pos, SimpleBlock block)
    {
        m_grid.Set(block, pos.x, pos.y, pos.z);
        m_level.OnBlockUpdate(pos);
    }

    public SimpleBlock GetBlock(Vector3Int pos)
    {
        if (!m_grid.IsInGrid(pos.x, pos.y, pos.z))
        {
            return SimpleBlock.defaultValue;
        }

        return m_grid.Get(pos.x, pos.y, pos.z);
    }

    public BlockType GetBlockID(Vector3Int pos)
    {
        return GetBlock(pos).id;
    }

    public byte GetBlockData(Vector3Int pos)
    {
        return GetBlock(pos).data;
    }

    public NearMatrix3<SimpleBlock> GetNearBlocks(Vector3Int pos)
    {
        NearMatrix3<SimpleBlock> matrix = new NearMatrix3<SimpleBlock>();
        GetNearBlocks(pos, matrix);
        return matrix;
    }

    public void GetNearBlocks(Vector3Int pos, NearMatrix3<SimpleBlock> matrix)
    {
        matrix.Reset(SimpleBlock.defaultValue);

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    int x = pos.x + i;
                    int y = pos.y + j;
                    int z = pos.z + k;

                    if (!m_grid.IsInGrid(x, y, z))
                        matrix.Set(SimpleBlock.defaultValue, i, j, k);
                    else
                    {
                        var element = m_grid.Get(x, y, z);
                        matrix.Set(element, i, j, k);
                    }
                }
            }
        }
    }

    public void Load(JsonDocument doc)
    {
        var root = doc.GetRoot();
        if (root != null && root.IsJsonObject())
        {
            var rootObject = root.JsonObject();

            var worldObject = rootObject.GetElement("World")?.JsonObject();
            if (worldObject != null)
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
                                if (elemObject != null)
                                {
                                    var item = new SimpleBlock();

                                    string name = elemObject.GetElement("T")?.String();
                                    if (name != null)
                                        Enum.TryParse<BlockType>(name, out item.id);
                                    item.data = (byte)(elemObject.GetElement("D")?.Int() ?? 0);

                                    m_grid.Set(item, i, j, k);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void Save(JsonDocument doc)
    {
        var root = doc.GetRoot();
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
                        elemObject.AddElement("T", new JsonString(item.id.ToString()));
                        elemObject.AddElement("D", new JsonNumber(item.data));

                    }
                }
            }
        }
    }

    public int MinX() { return m_grid.MinX(); }

    public int MinY() { return m_grid.MinY(); }

    public int MinZ() { return m_grid.MinZ(); }

    public int MaxX() { return m_grid.MaxX(); }

    public int MaxY() { return m_grid.MaxY(); }

    public int MaxZ() { return m_grid.MaxZ(); }

    public BoundsInt Bounds()
    {
        Vector3Int pos = new Vector3Int(MinX(), MinY(), MinZ());
        Vector3Int size = new Vector3Int(m_grid.Width(), m_grid.Height(), m_grid.Depth());

        return new BoundsInt(pos, size);
    }
}
