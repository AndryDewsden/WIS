using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WIS
{
    public static class AssetImageHelper
    {
        private const string DefaultAssetImage = "/SystemImages/asset_type_default.png";

        private static readonly Dictionary<int, string> AssetTypeImages = new Dictionary<int, string>
        {
            {1, "/SystemImages/asset_type_PC.png"},
            {2, "/SystemImages/asset_type_monitor.png"},
            {3, "/SystemImages/asset_type_system_block.png"},
            {4, "/SystemImages/asset_type_printer.png"},
            {5, "/SystemImages/asset_type_xerox.png"},
            {6, "/SystemImages/asset_type_scanner.png"}
        };

        public static string GetImagePath(int assetTypeId)
        {
            if (AssetTypeImages.TryGetValue(assetTypeId, out string path))
            {
                return path;
            }

            return DefaultAssetImage;
        }
    }
}
