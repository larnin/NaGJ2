using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameGridView : MonoBehaviour
{
    GameLevel m_level;

    ResizableMatrix3<GameObject> m_grid = new ResizableMatrix3<GameObject>(1, 1, 1);

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<GameSetCurrentLevelEvent>.Subscriber(SetLevel));
        m_subscriberList.Add(new Event<GameResetEvent>.Subscriber(Reset));
        m_subscriberList.Add(new Event<GameLoadEvent>.Subscriber(AfterLoad));
        m_subscriberList.Add(new Event<BlockUpdateEvent>.Subscriber(OnBlockUpdate));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void SetLevel(GameSetCurrentLevelEvent e)
    {
        m_level = e.level;
    }

    void Reset(GameResetEvent e)
    {
        ResetData();
    }

    void ResetData()
    {
        for (int i = m_grid.MinX(); i <= m_grid.MaxX(); i++)
        {
            for (int j = m_grid.MinY(); j <= m_grid.MaxY(); j++)
            {
                for (int k = m_grid.MinZ(); k <= m_grid.MaxZ(); k++)
                {
                    var obj = m_grid.Get(i, j, k);
                    if (obj != null)
                        Destroy(obj);
                }
            }
        }

        m_grid.Reset(1, 1, 1);
    }

    void AfterLoad(GameLoadEvent e)
    {
        ResetData();

        var bounds = m_level.grid.Bounds();
        var pos = bounds.position;

        NearMatrix3<SimpleBlock> nearMatrix = new NearMatrix3<SimpleBlock>();

        m_grid.Reset(bounds.size.x, bounds.size.y, bounds.size.z, -pos.x, -pos.y, -pos.z);
        for (int i = m_grid.MinX(); i <= m_grid.MaxX(); i++)
        {
            for (int j = m_grid.MinY(); j <= m_grid.MaxY(); j++)
            {
                for (int k = m_grid.MinZ(); k <= m_grid.MaxZ(); k++)
                {
                    m_level.grid.GetNearBlocks(new Vector3Int(i, j, k), nearMatrix);

                    var type = nearMatrix.Get(0, 0, 0);
                    if (type.id == BlockType.air)
                        continue;

                    GameObject prefab;
                    Quaternion rot;
                    BlockDataEx.GetBlockInfo(type.id, type.data, nearMatrix, out prefab, out rot);

                    if (prefab == null)
                        continue;

                    var obj = InstantiateAndPlaceBlock(prefab, rot, new Vector3Int(i, j, k));
                    m_grid.Set(obj, i, j, k);
                }
            }
        }
    }

    void OnBlockUpdate(BlockUpdateEvent e)
    {
        NearMatrix3<SimpleBlock> nearMatrix = new NearMatrix3<SimpleBlock>();
        m_level.grid.GetNearBlocks(e.pos, nearMatrix);

        NearMatrix3<SimpleBlock> otherMat = new NearMatrix3<SimpleBlock>();

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    Vector3Int pos = new Vector3Int(e.pos.x + i, e.pos.y + j, e.pos.z + k);

                    var oldObj = m_grid.Get(pos.x, pos.y, pos.z);
                    if (oldObj != null)
                        Destroy(oldObj);
                    m_grid.Set(null, pos.x, pos.y, pos.z);

                    var current = nearMatrix.Get(i, j, k);
                    if (current.id == BlockType.air)
                        continue;

                    GameObject prefab;
                    Quaternion rot;
                    if (i == 0 && j == 0 && k == 0)
                        BlockDataEx.GetBlockInfo(current.id, current.data, nearMatrix, out prefab, out rot);
                    else
                    {
                        m_level.grid.GetNearBlocks(pos, otherMat);
                        BlockDataEx.GetBlockInfo(current.id, current.data, otherMat, out prefab, out rot);
                    }

                    if (prefab == null)
                        continue;

                    var obj = InstantiateAndPlaceBlock(prefab, rot, pos);
                    m_grid.Set(obj, pos.x, pos.y, pos.z);
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
}
