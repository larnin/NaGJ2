using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameWorldBehaviour : MonoBehaviour
{
    [SerializeField] float m_threadUpdate = 0.1f;

    GameWorld m_behaviour;

    static GameWorldBehaviour m_instance;

    private void Awake()
    {
        if(m_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        m_instance = this;
        DontDestroyOnLoad(gameObject);

        m_behaviour.Init(m_threadUpdate);
    }

    private void Update()
    {
        m_behaviour.Update(Time.deltaTime);
    }

    private void OnDestroy()
    {
        m_behaviour.Destroy();
    }
}
