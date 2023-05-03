using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NRand;

public class MainMenu : MonoBehaviour
{
    [SerializeField] List<GameObject> m_towers = new List<GameObject>();
    [SerializeField] Transform m_towerPosition;
    [SerializeField] GameObject m_towerForcedTarget;
    [SerializeField] GameObject m_PlayButton;

    Tower m_tower;

    private void Start()
    {
        if (m_towers != null && m_towers.Count > 0)
        {
            StaticRandomGenerator<MT19937> rand = new StaticRandomGenerator<MT19937>();
            UniformIntDistribution d = new UniformIntDistribution(0, m_towers.Count);

            int index = d.Next(rand);

            var tower = Instantiate(m_towers[index]);
            m_tower = tower.GetComponent<Tower>();
            m_tower.AllowFire(false);
            m_tower.OverrideRange(1000);
            m_tower.ForceTarget(m_towerForcedTarget);

            if (m_towerPosition != null)
                m_tower.transform.position = m_towerPosition.position;
        }

        if(m_PlayButton != null)
        {
            var button = m_PlayButton.GetComponent<ButtonHit>();
            button?.onClick.AddListener(OnPlayClick);
        }
    }

    private void Update()
    {
        if (m_tower != null)
        {
            if (Input.GetMouseButtonDown(0))
                m_tower.AllowFire(true);
            if (!Input.GetMouseButton(0))
                m_tower.AllowFire(false);
        }

    }

    void OnPlayClick()
    {

    }
}
