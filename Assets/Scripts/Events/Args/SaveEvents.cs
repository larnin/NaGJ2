using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SaveEvent
{
    public JsonDocument document; 

    public SaveEvent(JsonDocument _document)
    {
        document = _document;
    }
}

public class LoadEvent
{
    public JsonDocument document;

    public LoadEvent(JsonDocument _document)
    {
        document = _document;
    }
}