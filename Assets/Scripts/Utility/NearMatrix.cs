using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NearMatrix
{
    bool[] m_data = new bool[9];

    public void Set(bool value, int x, int y)
    {
        if (x < -1 || x > 1)
            return;
        if (y < -1 || y > 1)
            return;

        m_data[PosToIndex(x, y)] = value;
    }

    public bool Get(int x, int y)
    {
        if (x < -1 || x > 1)
            return false;
        if (y < -1 || y > 1)
            return false;

        return m_data[PosToIndex(x, y)];
    }

    int PosToIndex(int x, int y)
    {
        return x + 1 + (y + 1) * 3;
    }
}
