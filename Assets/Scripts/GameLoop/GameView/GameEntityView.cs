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

    private void Update()
    {
        var pos = m_entity.GetViewPos();

        float moveDir = m_entity.GetPath().GetMoveDir();

        transform.localPosition = pos;
        transform.rotation = Quaternion.Euler(0, moveDir * Mathf.Rad2Deg, 0);
    }
}
