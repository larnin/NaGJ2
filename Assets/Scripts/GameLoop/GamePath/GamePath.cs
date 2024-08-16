using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GamePath
{
    const float offsetToTarget = 0.3f;

    List<Vector3> m_path = new List<Vector3>();
    int m_currentIndex = 0;

    public void SetPath(List<Vector3> path)
    {
        m_path = path.ToList();
        m_currentIndex = 0;
    }

    public void Reset()
    {
        m_path.Clear();
        m_currentIndex = 0;
    }

    public Vector3 GetNextPos(Vector3 current)
    {
        if (m_path.Count == 0)
            return current;

        if (m_currentIndex >= m_path.Count - 1 || m_currentIndex < 0)
            return m_path[m_path.Count - 1];

        float sqrDist = (m_path[m_currentIndex] - current).sqrMagnitude;

        if (sqrDist < offsetToTarget * offsetToTarget)
            m_currentIndex++;

        return m_path[m_currentIndex];
    }
}
