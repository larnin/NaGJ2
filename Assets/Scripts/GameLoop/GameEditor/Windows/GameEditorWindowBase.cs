using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class GameEditorWindowBase
{
    public virtual void Start() { }

    public virtual void OnEnable() { }

    public virtual void OnDisable() { }

    public virtual void OnDestroy() { }

    public virtual void Update() { }

    public virtual void AlwaysUpdate() { }

    public virtual void OnGUI(Vector2 position) { }
}
