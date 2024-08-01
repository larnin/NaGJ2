using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class EditorBlock
{
    public string name;
    public BlockType type;
    public int data;
}

[Serializable]
public class EditorElements
{
    public List<EditorBlock> blocks = new List<EditorBlock>();
}
