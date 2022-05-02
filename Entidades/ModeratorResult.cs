using System;
using System.Collections.Generic;
using System.Text;

namespace ContentModerator.Entidades
{
    public partial class ModeratorResult
    {
        public string OriginalText { get; set; }
        public string NormalizedText { get; set; }
        public string AutoCorrectedText { get; set; }
        public object Misrepresentation { get; set; }
        public string Language { get; set; }
        public List<Terms> Terms { get; set; }
        public Status Status { get; set; }
        public string TrackingId { get; set; }
    }
}
