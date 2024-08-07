using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class GamePathMaker
{
    const float buildingWeight = 1;

    struct PointInfo
    {
        public Vector3Int pos;
        public float weight;
        public int lastIndex;

        public PointInfo(Vector3Int _pos, float _weight, int _lastIndex)
        {
            pos = _pos;
            weight = _weight;
            lastIndex = _lastIndex;
        }
    }

    static List<Vector3Int> m_offsets = new List<Vector3Int>();
    static List<Vector3Int> m_diagonalOffsets = new List<Vector3Int>();

    static List<PointInfo> m_visitedPlaces = new List<PointInfo>();
    static List<PointInfo> m_nextPlaces = new List<PointInfo>();
    static Dictionary<ulong, bool> m_checkedPlaces = new Dictionary<ulong, bool>();

    static NearMatrix3<float> m_neighborsMatrix = new NearMatrix3<float>();

    public static List<Vector3Int> MakeGroundPath(GameLevel level, Vector3Int start, Vector3Int end, int width = 1, int height = 1)
    {
        return MakePath(level, start, end, width, height, false);
    }

    public static List<Vector3Int> MakeWaterPath(GameLevel level, Vector3Int start, Vector3Int end, int width = 1, int height = 1)
    {
        return MakePath(level, start, end, width, height, false);
    }

    static List<Vector3Int> MakePath(GameLevel level, Vector3Int start, Vector3Int end, int width, int height, bool onGround)
    {
        if(start == end)
        {
            var tempPath = new List<Vector3Int>();
            tempPath.Add(start);
            return tempPath;
        }

        m_visitedPlaces.Clear();
        m_nextPlaces.Clear();
        m_checkedPlaces.Clear();

        CreateOffsets(onGround);

        m_nextPlaces.Add(new PointInfo(start, 0, -1));
        m_checkedPlaces[Utility.PosToID(start)] = true;

        while(m_nextPlaces.Count > 0)
        {
            int currentIndex = GetMinNextIndex(end);
            if (currentIndex < 0)
                break;

            PointInfo current = m_nextPlaces[currentIndex];
            m_nextPlaces.RemoveAt(currentIndex);

            currentIndex = m_visitedPlaces.Count();
            m_visitedPlaces.Add(current);

            VisitNeighbors(level, current.pos, width, height, onGround);

            bool needBreak = false;

            for(int i = -1; i <= 1; i++)
            {
                for(int j = -1; j <= 1; j++)
                {
                    for(int k = -1; k <= 1; k++)
                    {
                        float weight = m_neighborsMatrix.Get(i, j, k);
                        if (weight < 0)
                            continue;

                        var next = new PointInfo(current.pos + new Vector3Int(i, j, k), current.weight + weight, currentIndex);

                        if(next.pos == end)
                        {
                            m_visitedPlaces.Add(next);
                            needBreak = true;
                            break;
                        }

                        m_checkedPlaces[Utility.PosToID(next.pos)] = true;
                        m_nextPlaces.Add(next);
                    }
                    if (needBreak)
                        break;
                }
                if (needBreak)
                    break;
            }
            if (needBreak)
                break;
        }

        List<Vector3Int> path = new List<Vector3Int>();

        int index = m_visitedPlaces.Count - 1;

        //find the nearest pos
        if (m_visitedPlaces[index].pos != end)
        {
            float bestDistance = -1;
            int bestIndex = -1;

            for(int i = 0; i < m_visitedPlaces.Count; i++)
            {
                float distance = (m_visitedPlaces[i].pos - end).sqrMagnitude;
                if(distance < bestDistance || bestIndex < 0)
                {
                    bestIndex = i;
                    bestDistance = distance;
                }
            }

            if (bestIndex >= 0)
                index = bestIndex;
        }

        path.Add(m_visitedPlaces[index].pos);

        while(m_visitedPlaces[index].lastIndex >= 0)
        {
            index = m_visitedPlaces[index].lastIndex;
            path.Add(m_visitedPlaces[index].pos);
        }

        path.Reverse();

        return path;
    }

    static int GetMinNextIndex(Vector3Int end)
    {
        int minIndex = -1;
        float minWeight = -1;

        for(int i =0 ; i < m_nextPlaces.Count; i++)
        {
            float dist = (m_nextPlaces[i].pos - end).magnitude;
            float weight = dist + m_nextPlaces[i].weight;

            if(weight < minWeight || minIndex < 0)
            {
                minIndex = i;
                minWeight = weight;
            }
        }

        return minIndex;
    }

    static void VisitNeighbors(GameLevel level, Vector3Int currentPos, int width, int height, bool onGround)
    {
        m_neighborsMatrix.Reset(-1);

        foreach(var offset in m_offsets)
        {
            Vector3Int next = currentPos + offset;
            float weight = IsVisitable(level, next, currentPos, width, height, onGround);

            m_neighborsMatrix.Set(weight, offset.x, offset.y, offset.z);
        }

        foreach(var offset in m_diagonalOffsets)
        {
            if(m_neighborsMatrix.Get(0, offset.y, offset.z) > 0 && m_neighborsMatrix.Get(offset.x, offset.y, 0) > 0)
            {
                Vector3Int next = currentPos + offset;
                float weight = IsVisitable(level, next, currentPos, width, height, onGround);

                m_neighborsMatrix.Set(weight, offset.x, offset.y, offset.z);
            }
        }
    }

    static float IsVisitable(GameLevel level, Vector3Int nextPos, Vector3Int currentPos, int width, int height, bool onGround)
    {
        //first test if alreadyVisited
        var ID = Utility.PosToID(nextPos);
        if (m_checkedPlaces.ContainsKey(ID))
            return -1;

        Vector3Int dir = nextPos - currentPos;

        //next check if center ground is valid
        var ground = level.grid.GetBlock(new Vector3Int(nextPos.x, nextPos.y - 1, nextPos.z));
        if (onGround)
        {
            if (ground.id != BlockType.groundSlope && ground.id != BlockType.ground)
                return -1;

            if (nextPos.y < currentPos.y)
            {
                var startGround = level.grid.GetBlock(new Vector3Int(currentPos.x, currentPos.y - 1, currentPos.z));
                if (startGround.id != BlockType.groundSlope)
                    return -1;
                var rot = RotationEx.FromVector(dir);
                var blockRot = BlockDataEx.ExtractDataRotation(ground.data);

                if (rot != blockRot)
                    return -1;
            }
            else if (ground.id == BlockType.groundSlope)
            {
                if (dir.x != 0 && dir.z != 0)
                    return -1;

                var rot = RotationEx.FromVector(dir);
                var blockRot = BlockDataEx.ExtractDataRotation(ground.data);

                if (nextPos.y > currentPos.y && rot != RotationEx.Add(blockRot, Rotation.rot_180))
                    return -1;
                if (nextPos.y == currentPos.y && rot != blockRot)
                    return -1;
            }
        }
        else
        {
            if (nextPos.y != currentPos.y)
                return -1;
            if (ground.id != BlockType.lake && ground.id != BlockType.river)
                return -1;
        }

        //next check entity volume on air or building 
        bool isOnBuilding = false;
        for(int i = -width + 1; i < width; i++)
        {
            for(int k = -width + 1; k < width; k++)
            {
                for(int j = 0; j <= height; j++)
                {
                    var pos = nextPos + new Vector3Int(i, j, k);
                    var collideBlock = level.grid.GetBlock(pos);
                    if (collideBlock.id != BlockType.air)
                        return -1;

                    if (!isOnBuilding)
                    {
                        var building = level.buildingList.GetBuildingAt(pos);
                        if (building != null)
                            isOnBuilding = true;
                    }
                }
            }
        }

        float weight = isOnBuilding ? buildingWeight : 0;
        weight += (nextPos - currentPos).magnitude;
        return weight;
    }

    static void CreateOffsets(bool onGround)
    {
        m_offsets.Clear();
        m_diagonalOffsets.Clear();

        m_offsets.Add(new Vector3Int(0, 0, 1));
        m_offsets.Add(new Vector3Int(0, 0, -1));
        m_offsets.Add(new Vector3Int(1, 0, 0));
        m_offsets.Add(new Vector3Int(-1, 0, 0));

        if(onGround)
        {
            m_offsets.Add(new Vector3Int(0, 1, 1));
            m_offsets.Add(new Vector3Int(0, 1, -1));
            m_offsets.Add(new Vector3Int(1, 1, 0));
            m_offsets.Add(new Vector3Int(-1, 1, 0));

            m_offsets.Add(new Vector3Int(0, 1, 1));
            m_offsets.Add(new Vector3Int(0, 1, -1));
            m_offsets.Add(new Vector3Int(1, 1, 0));
            m_offsets.Add(new Vector3Int(-1, 1, 0));
        }

        m_diagonalOffsets.Add(new Vector3Int(1, 0, 1));
        m_diagonalOffsets.Add(new Vector3Int(1, 0, -1));
        m_diagonalOffsets.Add(new Vector3Int(-1, 0, 1));
        m_diagonalOffsets.Add(new Vector3Int(-1, 0, 1));
    }
}
