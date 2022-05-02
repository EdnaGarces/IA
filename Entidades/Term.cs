using System;
using System.Collections.Generic;
using System.Text;

namespace ContentModerator.Entidades
{
    public partial class Terms
    {
        public long Index { get; set; }
        public long OriginalIndex { get; set; }
        public long ListId { get; set; }
        public string Term { get; set; }
    }
}
