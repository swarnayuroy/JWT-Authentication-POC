using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace web.Utils.FeatureFlag
{
    public enum AdminFlags
    {
        PRESENTATION,
        CREATE,
        EDIT,
        DELETE
    }

    public class AdminFeatureFlag
    {
        private string _devType;
        public AdminFlags Flag { get; set; }
        public string DevType { get{ return _devType; } set{ _devType = value == "MODAL" ? value : null; } }
        public bool IsEnabled { get; set; }
    }

    public class FlagList
    {
        public IList<AdminFeatureFlag> adminFeatureFlag = new List<AdminFeatureFlag>()
        {
            new AdminFeatureFlag() { Flag = AdminFlags.PRESENTATION, DevType = "MODAL", IsEnabled = true },
            new AdminFeatureFlag() { Flag = AdminFlags.CREATE, DevType = "MODAL", IsEnabled = false },
            new AdminFeatureFlag() { Flag = AdminFlags.EDIT, DevType = "MODAL", IsEnabled = false },
            new AdminFeatureFlag() { Flag = AdminFlags.DELETE, DevType = "MODAL", IsEnabled = false }
        };
    }
}