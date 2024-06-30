using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameBuildingListView : MonoBehaviour
{
    GameLevel m_level;

    Dictionary<int, GameObject> m_buildings = new Dictionary<int, GameObject>();

    SubscriberList m_subscriberList = new SubscriberList();

    NearMatrix3<SimpleBlock> m_tempNearBlocks = new NearMatrix3<SimpleBlock>();
    NearMatrix3<SimpleBeltInfos> m_tempNearBelts = new NearMatrix3<SimpleBeltInfos>();
    NearMatrix3<bool> m_tempNearPipes = new NearMatrix3<bool>();

    private void Awake()
    {
        m_subscriberList.Add(new Event<GameSetCurrentLevelEvent>.Subscriber(SetLevel));
        m_subscriberList.Add(new Event<GameResetEvent>.Subscriber(Reset));
        m_subscriberList.Add(new Event<GameLoadEvent>.Subscriber(AfterLoad));
        m_subscriberList.Add(new Event<BuildingUpdateEvent>.Subscriber(OnBuildingUpdate));
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

        int buildingNb = m_level.buildingList.GetBuildingNb();
        for(int i = 0; i < buildingNb; i++)
            UpdateRender(i);
    }

    void ResetData()
    {
        foreach(var b in m_buildings)
        {
            if (b.Value != null)
                Destroy(b.Value);
        }

        m_buildings.Clear();
    }

    void AfterLoad(GameLoadEvent e)
    {
        ResetData();

        if (m_level == null)
            return;

        int nbBuilding = m_level.buildingList.GetBuildingNb();

        for(int i = 0; i < nbBuilding; i++)
            UpdateRender(i);
    }

    void OnBuildingUpdate(BuildingUpdateEvent e)
    {
        GameObject oldInstance = null;
        m_buildings.TryGetValue(e.ID, out oldInstance);
        BuildingBase oldBuilding = null;
        if (oldInstance != null)
            oldBuilding = oldInstance.GetComponent<GameBuildingView>()?.GetBuildng();
        var oldInfos = oldBuilding == null ? null : oldBuilding.GetInfos();

        BuildingBase newBuilding = null;
        if (e.type != ElementUpdateType.removed)
            newBuilding = m_level.buildingList.GetBuilding(e.ID);
        var newInfos = newBuilding == null ? null : newBuilding.GetInfos();

        if (oldInfos == null && newInfos == null)
            return;

        Vector3Int pos = Vector3Int.zero;
        if (newInfos != null)
            pos = newInfos.pos;
        else if (oldInfos != null)
            pos = oldInfos.pos;

        if ((oldInfos != null && (oldInfos.buildingType == BuildingType.Belt || oldInfos.buildingType == BuildingType.Pipe))
            || (newInfos != null && (newInfos.buildingType == BuildingType.Belt || newInfos.buildingType == BuildingType.Pipe)))
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    for (int k = -1; k <= 1; k++)
                    {
                        if (Mathf.Abs(i) + Mathf.Abs(j) + Mathf.Abs(k) != 1)
                            continue;

                        Vector3Int currentPos = new Vector3Int(pos.x + i, pos.y + j, pos.z + k);
                        UpdateRender(m_level.buildingList.GetBuildingIndexAt(currentPos));
                    }
                }
            }
        }

        if (e.type == ElementUpdateType.removed)
            RemoveRender(e.ID);
        else UpdateRender(m_level.buildingList.GetBuildingIndexAt(pos));
    }


    void RemoveRender(int ID)
    {
        GameObject oldInstance = null;
        m_buildings.TryGetValue(ID, out oldInstance);

        if (oldInstance != null)
            Destroy(oldInstance);

        m_buildings.Remove(ID);
    }

    void UpdateRender(int buildingIndex)
    {
        if (buildingIndex < 0)
            return;

        var b = m_level.buildingList.GetBuildingFromIndex(buildingIndex);
        var infos = b?.GetInfos();

        GameObject instance = null;

        if (infos != null)
        {
            if (infos.buildingType == BuildingType.Belt)
            {
                m_level.grid.GetNearBlocks(infos.pos, m_tempNearBlocks);
                m_level.buildingList.GetNearBelts(infos.pos, m_tempNearBelts);

                BeltDirection dir;
                instance = BuildingDataEx.InstantiateBelt(infos.pos, m_tempNearBlocks, m_tempNearBelts, out dir, transform);
            }
            else if (infos.buildingType == BuildingType.Pipe)
            {
                m_level.grid.GetNearBlocks(infos.pos, m_tempNearBlocks);
                m_level.buildingList.GetNearPipes(infos.pos, m_tempNearPipes);

                instance = BuildingDataEx.InstantiatePipe(infos.pos, m_tempNearBlocks, m_tempNearPipes, transform);
            }
            else
            {
                var prefab = BuildingDataEx.GetBaseBuildingPrefab(infos.buildingType, infos.level);
                if (prefab == null)
                    return;
                instance = Instantiate(prefab);

                var size = Global.instance.allBlocks.blockSize;
                Vector3 offset = new Vector3(size.x * infos.pos.x, size.y * infos.pos.y, size.z * infos.pos.z);
                instance.transform.parent = transform;
                instance.transform.localPosition = offset;
                instance.transform.localRotation = RotationEx.ToQuaternion(infos.rotation);
            }

            SetBuilding(instance, b);
        }

        GameObject oldInstance = null;
        m_buildings.TryGetValue(b.GetID(), out oldInstance);
        if (oldInstance != null)
            Destroy(oldInstance);

        if (instance == null)
            m_buildings.Remove(b.GetID());
        else m_buildings[b.GetID()] = instance;
    }

    void SetBuilding(GameObject instance, BuildingBase building)
    {
        var view = instance.AddComponent<GameBuildingView>();
        view.SetBuilding(building);
    }
}
