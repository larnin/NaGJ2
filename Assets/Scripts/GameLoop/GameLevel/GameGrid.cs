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

    public void SetBlock(Vector3Int pos, BlockType id, byte data = 0)
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
        Reset();

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

    public bool RaycastWorld(Vector3 pos, Vector3 dir, out Vector3 hit, out Vector3 normal)
    {
        hit = Vector3.zero;
        normal = Vector3.up;

        var size = Global.instance.allBlocks.blockSize;

        pos.x /= size.x;
        pos.y /= size.y;
        pos.z /= size.z;

        dir.x /= size.x;
        dir.y /= size.y;
        dir.z /= size.z;

        bool result = RaycastLocal(pos, dir, out hit, out normal);

        if (!result)
            return result;

        hit.x *= size.x;
        hit.y *= size.y;
        hit.z *= size.z;

        normal.x *= size.x;
        normal.y *= size.y;
        normal.z *= size.z;

        return result;
    }

    public bool RaycastLocal(Vector3 pos, Vector3 dir, out Vector3 hit, out Vector3 normal)
    {
        hit = Vector3.zero;
        normal = Vector3.zero;

        dir = dir.normalized;

        Vector3 min = new Vector3(m_grid.MinX() - 0.5f, m_grid.MinY() - 1.0f, m_grid.MinZ() - 0.5f);
        Vector3 size = new Vector3(m_grid.Width(), m_grid.Height(), m_grid.Depth());
        Bounds gridBounds = new Bounds(min + size / 2, size);

        Vector3 gridHitStart, gridHitEnd, gridNormal;
        var gridShape = Collisions.GetFullShape(gridBounds);
        bool hitGrid = Collisions.Raycast(gridShape, pos, dir, out gridHitStart, out gridNormal);
        if (!hitGrid)
            return false;
        
        Vector3 endPos = gridHitStart + dir * (gridBounds.size.x + gridBounds.size.y + gridBounds.size.z);
        hitGrid = Collisions.Raycast(gridShape, endPos, -dir, out gridHitEnd, out gridNormal);
        if (!hitGrid) //must not happen
            gridHitEnd = endPos;

        float length = (gridHitEnd - gridHitStart).magnitude;
        const float maxStep = 6;
        int nbStep = Mathf.CeilToInt(length / maxStep);
        float step = length / nbStep;

        for(int i = 0; i < nbStep; i++)
        {
            Vector3 start = gridHitStart + dir * step * i / nbStep;
            Vector3 end = gridHitStart + dir * step * (i + 1) / nbStep;

            bool localHit = RaycastStep(pos, start, end, out hit, out normal);
            if (localHit)
                return true;
        }

        return false;
    }

    bool RaycastStep(Vector3 realStart, Vector3 start, Vector3 end, out Vector3 hit, out Vector3 normal)
    {
        hit = Vector3.zero;
        normal = Vector3.zero;

        Vector3Int min = new Vector3Int(Mathf.FloorToInt(start.x), Mathf.FloorToInt(start.y), Mathf.FloorToInt(start.z));
        Vector3Int max = new Vector3Int(Mathf.CeilToInt(start.x), Mathf.CeilToInt(start.y), Mathf.CeilToInt(start.z));

        if (end.x < start.x) min.x = Mathf.FloorToInt(end.x);
        else max.x = Mathf.CeilToInt(end.x);
        if (end.y < start.y) min.y = Mathf.FloorToInt(end.y);
        else max.y = Mathf.CeilToInt(end.y);
        if (end.z < start.z) min.z = Mathf.FloorToInt(end.z);
        else max.z = Mathf.CeilToInt(end.z);

        Vector3 dir = (end - start).normalized;

        bool oneHit = false;
        float hitDistance = 0;

        for(int i = min.x; i <= max.x; i++)
        {
            for(int j = min.y; j <= max.y; j++)
            {
                for(int k = min.z; k <= max.z; k++)
                {
                    if (m_grid.Get(i, j, k).id == BlockType.air)
                        continue;

                    var b = GetBounds(new Vector3Int(i, j, k));

                    Vector3 localHit, localNormal;
                    bool haveHit = Collisions.Raycast(Collisions.GetFullShape(b), realStart, dir, out localHit, out localNormal);
                    if(haveHit)
                    {
                        float d = (localHit - start).sqrMagnitude;
                        if(!oneHit || d < hitDistance)
                        {
                            hitDistance = d;
                            hit = localHit;
                            normal = localNormal;
                            oneHit = true;
                        }
                    }
                }
            }
        }

        return oneHit;
    }

    Bounds GetBounds(Vector3Int pos)
    {
        Bounds b = new Bounds();
        b.center = new Vector3(pos.x, pos.y - 0.5f, pos.z);
        b.size = Vector3.one;
        return b;
    }
}
