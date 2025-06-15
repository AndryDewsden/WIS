using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WIS.ApplicationData;

namespace WIS
{
    public class AssetViewModel
    {
        public WIS_Assets Asset { get; set; }
        public bool IsAvailable { get; set; }

        public string DisplayName => Asset.asset_name + (IsAvailable ? "" : " (занято)");
    }
}
