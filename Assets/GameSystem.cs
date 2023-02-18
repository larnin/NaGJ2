using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class GameSystem : MonoBehaviour
{
    bool m_initialized = false;

    private void Update()
    {
        if(!m_initialized)
            SetInitialState();

    }   
    
    void SetInitialState()
    {
        if (WorldHolder.Instance() == null)
            return;

        m_initialized = true;

        WorldHolder.Instance().SetGround(GroundType.normal, 0, 0);
    }
}
