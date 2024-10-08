﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CursorSelect : CursorBase
{
    [SerializeField] GameObject m_cursorPrefab;

    enum SelectedType
    {
        building,
    }

    class SelectedElement
    {
        public SelectedType selectedType;
        public int selectedID = 0;
        public GameObject cursor = null;
    }

    class NewSelectedElement
    {
        public SelectedType selectedType;
        public int selectedID = 0;
        public Vector3 selectionScale = Vector3.zero;
        public bool alreadyInSelection = false;
    }

    List<SelectedElement> m_selection = new List<SelectedElement>();

    bool m_startedSelection = false;
    bool m_deleteMode = false;

    Vector3 m_startMousePos;
    Vector3 m_endMousePos;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            StartSelect();
        if (Input.GetMouseButtonDown(1))
            StartDelete();
        if (m_startedSelection)
        {
            if (Input.GetMouseButtonUp(0))
                EndSelect();
            if (Input.GetMouseButtonUp(1))
                EndDelete();
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
                ProcessSelect();
            if (Input.GetKeyUp(KeyCode.Delete))
                EndDelete();
        }

        UpdateSelectionBox();
    }

    void StartSelect()
    {
        ClearSelection();

        m_startMousePos = Input.mousePosition;
        m_endMousePos = Input.mousePosition;

        m_deleteMode = false;
        m_startedSelection = true;

        UpdateSelection();
    }

    void ProcessSelect()
    {
        m_endMousePos = Input.mousePosition;

        UpdateSelection();
    }

    void EndSelect()
    {
        Event<HideSelectionRectangleEvent>.Broadcast(new HideSelectionRectangleEvent());
    }

    void StartDelete()
    {
        ClearSelection();

        m_startMousePos = Input.mousePosition;
        m_endMousePos = Input.mousePosition;

        m_deleteMode = true;
        m_startedSelection = true;

        UpdateSelection();
    }

    void EndDelete()
    {
        Event<HideSelectionRectangleEvent>.Broadcast(new HideSelectionRectangleEvent());

        GameGetCurrentLevelEvent level = new GameGetCurrentLevelEvent();
        Event<GameGetCurrentLevelEvent>.Broadcast(level);

        if (level.level != null)
        {
            foreach (var s in m_selection)
            {
                if (s.selectedType == SelectedType.building)
                    level.level.buildingList.Remove(s.selectedID);
            }
        }

        ClearSelection();
    }

    static Color selectColor = new Color(1, 1, 1, 0.2f);
    static Color deleteColor = new Color(1, 0, 0, 0.15f);

    void UpdateSelection()
    {
        Color c = m_deleteMode ? deleteColor : selectColor;

        Event<DisplaySelectionRectangleEvent>.Broadcast(new DisplaySelectionRectangleEvent(m_startMousePos, m_endMousePos, c));

        Shape s = GetMouseSelectionShape();
        Collisions.DrawShape(s, Vector3.one, Color.cyan);

        GameGetCurrentLevelEvent level = new GameGetCurrentLevelEvent();
        Event<GameGetCurrentLevelEvent>.Broadcast(level);
        if (level.level == null)
            return;

        int buildingNb = level.level.buildingList.GetBuildingNb();

        var size = Global.instance.allBlocks.blockSize;
        
        List<NewSelectedElement> newSelectedElements = new List<NewSelectedElement>();

        for(int i = 0; i < buildingNb; i++)
        {
            var building = level.level.buildingList.GetBuildingFromIndex(i);
            if (building == null)
                continue;
            var infos = building.GetInfos();

            var boundsInt = BuildingDataEx.GetBuildingBounds(infos.buildingType, infos.pos, infos.rotation, infos.level);
            
            var sizeInt = boundsInt.size;
            var posInt = boundsInt.center;

            Bounds bounds = new Bounds(new Vector3((posInt.x - 0.5f) * size.x, (posInt.y - 1) * size.y, (posInt.z - 0.5f) * size.z), new Vector3(sizeInt.x * size.x, sizeInt.y * size.y, sizeInt.z * size.z));

            Shape s2 = Collisions.GetFullShape(bounds);

            Collisions.DrawShape(s2, Vector3.one, Color.red);

            if (Collisions.Intersect(s, s2))
            {
                var element = new NewSelectedElement();
                var scale = BuildingDataEx.GetBuildingSize(infos.buildingType, infos.level);
                element.selectionScale = new Vector3(scale.x, scale.y, scale.z);
                element.selectedType = SelectedType.building;
                element.selectedID = building.GetID();
                newSelectedElements.Add(element);
            }
        }

        for(int i = 0; i < newSelectedElements.Count; i++)
        {
            foreach(var selection in m_selection)
            {
                if (selection.selectedID == newSelectedElements[i].selectedID)
                    newSelectedElements[i].alreadyInSelection = true;
            }
        }

        for(int i = 0; i < m_selection.Count; i++)
        {
            bool found = false;
            foreach (var e in newSelectedElements)
            {
                if (e.selectedID == m_selection[i].selectedID)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                m_selection[i].selectedID = 0;
                Destroy(m_selection[i].cursor);
                m_selection.RemoveAt(i);
                i--;
            }
        }

        for (int i = 0; i < newSelectedElements.Count; i++)
        {
            if (newSelectedElements[i].alreadyInSelection)
                continue;

            var newSelect = new SelectedElement();
            newSelect.cursor = Instantiate(m_cursorPrefab);

            newSelect.cursor.transform.parent = transform;
            newSelect.cursor.transform.localScale = newSelectedElements[i].selectionScale;
            newSelect.selectedType = newSelectedElements[i].selectedType;
            newSelect.selectedID = newSelectedElements[i].selectedID;

            m_selection.Add(newSelect);
        }
    }

    Shape GetMouseSelectionShape()
    {
        float minX = Mathf.Min(m_startMousePos.x, m_endMousePos.x);
        float maxX = Mathf.Max(m_startMousePos.x, m_endMousePos.x);
        float minY = Mathf.Min(m_startMousePos.y, m_endMousePos.y);
        float maxY = Mathf.Max(m_startMousePos.y, m_endMousePos.y);

        var topLeftRay = m_camera.ScreenPointToRay(new Vector3(minX, minY, 0));
        var topRightRay = m_camera.ScreenPointToRay(new Vector3(maxX, minY, 0));
        var bottomLeftRay = m_camera.ScreenPointToRay(new Vector3(minX, maxY, 0));
        var bottomRightRay = m_camera.ScreenPointToRay(new Vector3(maxX, maxY, 0));

        float near = m_camera.nearClipPlane;
        float far = m_camera.farClipPlane;

        var topLeftNear = topLeftRay.GetPoint(near);
        var topLeftFar = topLeftRay.GetPoint(far);
        var topRightNear = topRightRay.GetPoint(near);
        var topRightFar = topRightRay.GetPoint(far);
        var bottomLeftNear = bottomLeftRay.GetPoint(near);
        var bottomLeftFar = bottomLeftRay.GetPoint(far);
        var bottomRightNear = bottomRightRay.GetPoint(near);
        var bottomRightFar = bottomRightRay.GetPoint(far);

        Shape s = new Shape();
        s.points.Add(topLeftNear);
        s.points.Add(topLeftFar);
        s.points.Add(topRightNear);
        s.points.Add(topRightFar);
        s.points.Add(bottomLeftNear);
        s.points.Add(bottomLeftFar);
        s.points.Add(bottomRightNear);
        s.points.Add(bottomRightFar);

        s.indexs.Add(0); s.indexs.Add(2); s.indexs.Add(1);
        s.indexs.Add(3); s.indexs.Add(1); s.indexs.Add(2);

        s.indexs.Add(4); s.indexs.Add(5); s.indexs.Add(6);
        s.indexs.Add(5); s.indexs.Add(7); s.indexs.Add(6);

        s.indexs.Add(0); s.indexs.Add(1); s.indexs.Add(4);
        s.indexs.Add(5); s.indexs.Add(4); s.indexs.Add(1);

        s.indexs.Add(2); s.indexs.Add(6); s.indexs.Add(3);
        s.indexs.Add(3); s.indexs.Add(6); s.indexs.Add(7);

        s.indexs.Add(2); s.indexs.Add(6); s.indexs.Add(3);
        s.indexs.Add(3); s.indexs.Add(6); s.indexs.Add(7);

        s.indexs.Add(0); s.indexs.Add(4); s.indexs.Add(2);
        s.indexs.Add(2); s.indexs.Add(4); s.indexs.Add(6);

        return s;
    }
    
    void UpdateSelectionBox()
    {
        GameGetCurrentLevelEvent level = new GameGetCurrentLevelEvent();
        Event<GameGetCurrentLevelEvent>.Broadcast(level);

        var size = Global.instance.allBlocks.blockSize;

        foreach (var e in m_selection)
        {
            var building = level.level.buildingList.GetBuilding(e.selectedID);
            if (building == null)
                continue;
            var infos = building.GetInfos();

            Vector3 pos = new Vector3(infos.pos.x * size.x, infos.pos.y * size.y, infos.pos.z * size.z);

            var buildingbounds = BuildingDataEx.GetBuildingBounds(infos.buildingType, infos.pos, infos.rotation, infos.level);
            if (buildingbounds.x % 2 != 0)
                pos.x += size.x / 2;
            if (buildingbounds.z % 2 != 0)
                pos.y += size.z / 2;

            e.cursor.transform.position = pos;
            e.cursor.transform.rotation = RotationEx.ToQuaternion(infos.rotation);
        }
    }

    void ClearSelection()
    {
        foreach(var s in m_selection)
            Destroy(s.cursor);

        m_selection.Clear();
    }

    private void OnEnable()
    {
        ClearSelection();

        m_startMousePos = Input.mousePosition;
        m_endMousePos = Input.mousePosition;
        m_deleteMode = false;
    }

    private void OnDisable()
    {
        ClearSelection();
        m_startedSelection = false;
    }
}
