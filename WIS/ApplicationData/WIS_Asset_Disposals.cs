//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WIS.ApplicationData
{
    using System;
    using System.Collections.Generic;
    
    public partial class WIS_Asset_Disposals
    {
        public int ID_disposal { get; set; }
        public int disposal_asset_ID { get; set; }
        public System.DateTime disposal_date { get; set; }
        public string disposal_reason { get; set; }
        public int disposal_user_ID { get; set; }
    
        public virtual WIS_Assets WIS_Assets { get; set; }
        public virtual WIS_Users WIS_Users { get; set; }
    }
}
