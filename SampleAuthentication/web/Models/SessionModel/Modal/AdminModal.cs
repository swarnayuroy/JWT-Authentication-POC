using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using web.Utils.FeatureFlag;

namespace web.Models.SessionModel.Modal
{
    public class AdminModal
    {
        public AdminResult ModalData { get; set; }
        public IList<AdminFeatureFlag> Flag { get; set; } = new FlagList().adminFeatureFlag
            .Where(f => f.DevType == "MODAL").ToList<AdminFeatureFlag>();
    }
}