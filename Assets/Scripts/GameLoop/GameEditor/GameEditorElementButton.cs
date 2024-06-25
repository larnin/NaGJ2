using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class GameEditorElementButton : MonoBehaviour
{
    enum ElementType
    {
        Block,
        Building,
    }

    [SerializeField] ElementType m_buttonType;

    [ShowIf("m_buttonType", ElementType.Block)]
    [SerializeField] BlockType m_blockType;
    [ShowIf("m_buttonType", ElementType.Block)]
    [SerializeField] int m_blockData;

    [ShowIf("m_buttonType", ElementType.Building)]
    [SerializeField] BuildingType m_buildingType;
    [ShowIf("m_buttonType", ElementType.Building)]
    [SerializeField] int m_buildingLevel;

    private void Awake()
    {
        var renderer = transform.Find("Sprite")?.GetComponent<SpriteRenderer>();
        if(renderer != null)
        {
            Sprite s = null;
            if (m_buttonType == ElementType.Block)
                s = BlockDataEx.GetSprite(m_blockType, m_blockData);
            else if (m_buttonType == ElementType.Building)
                s = BuildingDataEx.GetSprite(m_buildingType, m_buildingLevel);

            if (s != null)
                renderer.sprite = s;
        }
    }

    public void OnClick()
    {
        if (m_buttonType == ElementType.Block)
            Event<EditorSelectBlockTypeEvent>.Broadcast(new EditorSelectBlockTypeEvent(m_blockType, m_blockData));
        else if (m_buttonType == ElementType.Building)
            Event<EditorSelectBuildingTypeEvent>.Broadcast(new EditorSelectBuildingTypeEvent(m_buildingType, m_buildingLevel));
    }

}
