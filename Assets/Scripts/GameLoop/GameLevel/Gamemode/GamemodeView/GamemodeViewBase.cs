using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class GamemodeViewBase
{
    protected Transform m_parent = null;

    public void SetParent(Transform parent)
    {
        m_parent = parent;
    }

    public abstract void OnGui(Vector2 position);

    public abstract GamemodeBase GetGamemode();

    public abstract void Init();
    public abstract void OnDestroy();
}