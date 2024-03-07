using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingPortList : MonoBehaviour
{
    [SerializeField] List<BuildingContainer> m_containers = new List<BuildingContainer>();
    [SerializeField] List<BuildingOnePortData> m_portList = new List<BuildingOnePortData>();

    SubscriberList m_subsciberList = new SubscriberList();

    private void Awake()
    {
        m_subsciberList.Add(new Event<GetBuildingPortsEvent>.LocalSubscriber(GetPorts, gameObject));

        m_subsciberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subsciberList.Unsubscribe();
    }

    void GetPorts(GetBuildingPortsEvent e)
    {
        e.ports = GetPorts();
        e.containers = GetContainers();
    }

    List<BuildingContainer> GetContainers()
    {
        GetBuildingInstanceIDEvent idData = new GetBuildingInstanceIDEvent();
        Event<GetBuildingInstanceIDEvent>.Broadcast(idData, gameObject);

        List<BuildingContainer> containers = new List<BuildingContainer>();
        for(int i = 0; i < m_containers.Count; i++)
        {
            var c = new BuildingContainer();
            c.direction = m_containers[i].direction;
            c.id = idData.ID;
            c.index = i;
            c.filter = m_containers[i].filter.Copy();
            c.count = m_containers[i].count;

            containers.Add(c);
        }

        return containers;
    }

    List<BuildingOnePortData> GetPorts()
    {
        GetBuildingInstanceIDEvent idData = new GetBuildingInstanceIDEvent();
        Event<GetBuildingInstanceIDEvent>.Broadcast(idData, gameObject);

        Rotation rot = Rotation.rot_0;
        Vector3Int pos = Vector3Int.zero; 
        if(idData.ID == 0)
        {
            rot = RotationEx.FromVector(transform.forward);

            var localPos = transform.position;
            var scale = Global.instance.allBlocks.blockSize;

            localPos = new Vector3(localPos.x / scale.x, localPos.y / scale.y, localPos.z / scale.z);

            pos.x = Mathf.RoundToInt(localPos.x);
            pos.y = Mathf.RoundToInt(localPos.y);
            pos.z = Mathf.RoundToInt(localPos.z);
        }
        else
        {
            GetBuildingEvent buildingData = new GetBuildingEvent(idData.ID);
            Event<GetBuildingEvent>.Broadcast(buildingData);

            rot = buildingData.element.rotation;
            var scale = Global.instance.allBlocks.blockSize;
            pos = buildingData.element.pos; new Vector3(buildingData.element.pos.x * scale.x, buildingData.element.pos.y * scale.y, buildingData.element.pos.z * scale.z);
        }

        List<BuildingOnePortData> ports = new List<BuildingOnePortData>();
        foreach (var p in m_portList)
        {
            BuildingOnePortData data = new BuildingOnePortData();
            data.containerIndex = p.containerIndex;
            data.rotation = RotationEx.Add(rot, p.rotation);
            data.direction = p.direction;
            data.pos = pos + p.pos;

            ports.Add(data);
        }

        return ports;
    }

    void OnDrawGizmosSelected()
    {
        DrawPorts();
    }

    void DrawPorts()
    {
        var scale = Global.instance.allBlocks.blockSize;

        var ports = GetPorts();

        foreach(var p in ports)
        {
            Vector3 pos = new Vector3(p.pos.x * scale.x, (p.pos.y - 0.5f) * scale.y, p.pos.z * scale.z);
            Color c = Color.white;
            if (p.direction == BuildingPortDirection.input)
                c = Color.green;
            else if (p.direction == BuildingPortDirection.output)
                c = new Color(1, 0.5f, 0);

            DebugDraw.Sphere(pos, 0.25f, c);

            Vector3 dir = RotationEx.ToVector3(p.rotation);
            Vector3 orthodir = new Vector3(dir.z, dir.y, -dir.x);
            DebugDraw.Line(pos, pos + dir, c);
            DebugDraw.Line(pos + dir, pos + dir * 0.8f + orthodir * 0.2f, c);
            DebugDraw.Line(pos + dir, pos + dir * 0.8f - orthodir * 0.2f, c);
            DebugDraw.Line(pos + dir * 0.8f + orthodir * 0.2f, pos + dir * 0.8f - orthodir * 0.2f, c);
        }
    }
}
