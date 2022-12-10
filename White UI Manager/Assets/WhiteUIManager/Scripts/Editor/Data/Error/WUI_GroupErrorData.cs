using System.Collections.Generic;

namespace WUI.Editor.Data.Error
{
    using Elements;
    
    public class WUI_GroupErrorData
    {
        public WUI_ErrorData ErrorData { get; set; }
        
        public List<WUI_Group> Groups { get; set; }

        public WUI_GroupErrorData()
        {
            ErrorData = new WUI_ErrorData();
            Groups = new List<WUI_Group>();
        }
    }
}
