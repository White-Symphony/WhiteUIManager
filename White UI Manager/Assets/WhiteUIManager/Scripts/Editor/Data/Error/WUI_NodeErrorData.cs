using System.Collections.Generic;

namespace WUI.Editor.Data.Error
{
    using Elements;
    
    public class WUI_NodeErrorData
    {
        public WUI_ErrorData ErrorData { get; }
        public List<WUI_Node> Nodes { get; }

        public WUI_NodeErrorData()
        {
            ErrorData = new WUI_ErrorData();
            Nodes = new List<WUI_Node>();
        }
    }
}