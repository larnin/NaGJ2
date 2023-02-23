using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class DestroyAfter : MonoBehaviour
{
    [SerializeField] float m_duration = 1;

    private void Start()
    {
        Destroy(gameObject, m_duration);
    }
}