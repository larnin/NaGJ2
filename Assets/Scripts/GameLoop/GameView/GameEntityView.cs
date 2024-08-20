using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameEntityView : MonoBehaviour
{
    GameEntity m_entity;

    public void SetEntity(GameEntity entity)
    {
        m_entity = entity;
    }
    public GameEntity GetEntity()
    {
        return m_entity;
    }
}
