using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Lykke.Backoffice.Common.Extensions.BrowserExtension
{
    internal class MatchExpression
    {
        public List<Regex> Regexes { get; set; }
        public Action<Match, object> Action { get; set; }
    }
}
