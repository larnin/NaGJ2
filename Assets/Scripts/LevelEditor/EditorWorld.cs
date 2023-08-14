using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class EditorBlock
{
    public BlockType id = BlockType.air;
    public Rotation rot = Rotation.rot_0;
    public GameObject instance = null;
}

public class EditorWorld : MonoBehaviour
{
    [SerializeField] Vector3 m_blockSize = Vector3.one;

    ResizableMatrix3<EditorBlock> m_grid = new ResizableMatrix3<EditorBlock>(1, 1, 1);

    SubscriberList m_subscriberList = new SubscriberList();

    NearMatrix3<BlockType> m_tempMatrix = new NearMatrix3<BlockType>();

    private void Awake()
    {
        m_subscriberList.Add(new Event<EditorSetBlockEvent>.Subscriber(SetBlock));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void SetBlock(EditorSetBlockEvent e)
    {
        SetBlock(e.pos, e.type, e.rot);
    }

    void SetBlock(Vector3Int pos, BlockType type, Rotation rot)
    {
        EditorBlock block = new EditorBlock();
        block.id = type;
        block.rot = rot;

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

        if(element != null)
            Destroy(element.instance);

        if(element.id == BlockType.air && element.instance != null)
            return;

        SetNearMatrix(pos, m_tempMatrix);

        GameObject prefab;
        Quaternion rot;
        BlockDataEx.GetBlockInfo(element.id, element.rot, m_tempMatrix, out prefab, out rot);

        if (prefab == null)
            return;

        var obj = InstantiateAndPlaceBlock(prefab, rot, pos);

        element.instance = obj;
    }

    void SetNearMatrix(Vector3Int pos, NearMatrix3<BlockType> matrix)
    {
        matrix.Reset(BlockType.air);

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
                        matrix.Set(BlockType.air, i, j, k);
                    else
                    {
                        var element = m_grid.Get(x, y, z);
                        matrix.Set(element.id, i, j, k);
                    }
                }
            }
        }
    }

    GameObject InstantiateAndPlaceBlock(GameObject prefab, Quaternion rot, Vector3Int pos)
    {
        Vector3 offset = new Vector3(m_blockSize.x * pos.x, m_blockSize.y * pos.y, m_blockSize.z * pos.z);

        var obj = Instantiate(prefab);
        obj.transform.parent = transform;
        obj.transform.localPosition = offset;
        obj.transform.localRotation = rot;

        return obj;
    }
}
