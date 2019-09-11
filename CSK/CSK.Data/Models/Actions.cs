using System;
using System.Collections.Generic;

namespace CSK.Data.Models
{
    public partial class Actions
    {
        public string Id { get; set; }
        public string ActionContent { get; set; }
        public DateTime? Time { get; set; }
        public string UserId { get; set; }

        public virtual AspNetUsers User { get; set; }
    }
}
