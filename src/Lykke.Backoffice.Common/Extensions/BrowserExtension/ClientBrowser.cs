using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lykke.Backoffice.Common.Extensions.BrowserExtension
{
    /// <summary>
    /// Client browser
    /// </summary>
    public class ClientBrowser
    {
        /// <summary>
        /// Major version
        /// </summary>
        public string Major { get; set; }
        /// <summary>
        /// Browser name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Browser full version
        /// </summary>
        public string Version { get; set; }
        private static Dictionary<string, string> _versionMap = new Dictionary<string, string>{
            {"/8","1.0" },
            { "/1","1.2"},
            { "/3","1.3"},
            { "/412","2.0"},
            { "/416","2.0.2"},
            { "/417","2.0.3"},
            { "/419","2.0.4"},
            {"?","/" }
        };
        private static List<MatchExpression> _matchs = new List<MatchExpression> {
            new MatchExpression{
                Regexes = new List<Regex>{
                    new Regex(@"(trident).+rv[:\s]([\w\.]+).+like\sgecko", RegexOptions.IgnoreCase | RegexOptions.Compiled)// IE11
                },
                Action = (Match match,Object obj) =>{
                    ClientBrowser current = obj as ClientBrowser;

                    current.Name = "IE";
                    current.Version = "11";
                }
            },
            new MatchExpression{
                Regexes = new List<Regex>{
                    new Regex(@"(edge)\/((\d+)?[\w\.]+)",RegexOptions.IgnoreCase | RegexOptions.Compiled),// Microsoft Edge
                },
                Action = NameVersionAction
            },
            new MatchExpression{
                Regexes = new List<Regex>{
                    new Regex(@"(chrome|omniweb|arora|[tizenoka]{5}\s?browser)\/v?([\w\.]+)",RegexOptions.IgnoreCase | RegexOptions.Compiled),// Chrome/OmniWeb/Arora/Tizen/Nokia
                },
                Action = NameVersionAction
            },
            new MatchExpression{
                Regexes = new List<Regex>{
                    new Regex(@"version\/([\w\.]+).+?(mobile\s?safari|safari)",RegexOptions.IgnoreCase | RegexOptions.Compiled)// Safari & Safari Mobile
                },
                Action = (Match match,Object obj) =>{
                    ClientBrowser current = obj as ClientBrowser;

                    var val = match.Value.Split('/');
                    var nameAndVersion = val[1].Split(" ");
                    current.Name = nameAndVersion[1];
                    current.Version = nameAndVersion[0];
                }
            },
            new MatchExpression{
                Regexes = new List<Regex>{
                    new Regex(@"webkit.+?(mobile\s?safari|safari)(\/[\w\.]+)",RegexOptions.IgnoreCase | RegexOptions.Compiled)// Safari < 3.0
                },
                Action = (Match match,Object obj) =>{
                    ClientBrowser current = obj as ClientBrowser;

                    var nameAndVersion = match.Value.Split('/');

                    current.Name = nameAndVersion[0];

                    var version = nameAndVersion[1];

                    current.Version = _versionMap.Keys.Any(m=>m==version)? _versionMap[version]:version;
                }
            },
            new MatchExpression{
                Regexes = new List<Regex>{
                    new Regex(@"(firefox|seamonkey|k-meleon|icecat|iceape|firebird|phoenix)\/([\w\.-]+)",RegexOptions.IgnoreCase | RegexOptions.Compiled),// Firefox/SeaMonkey/K-Meleon/IceCat/IceApe/Firebird/Phoenix
                    new Regex(@"(mozilla)\/([\w\.]+).+rv\:.+gecko\/\d+",RegexOptions.IgnoreCase | RegexOptions.Compiled),// Mozilla
                },
                Action = NameVersionAction
            },
        };
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="userAgent"></param>
        public ClientBrowser(string userAgent)
        {
            if (!string.IsNullOrEmpty(userAgent))
            {
                foreach (var matchItem in _matchs)
                {
                    foreach (var regexItem in matchItem.Regexes)
                    {
                        if (regexItem.IsMatch(userAgent))
                        {
                            var match = regexItem.Match(userAgent);

                            matchItem.Action(match, this);
                            if (!string.IsNullOrEmpty(this.Version))
                                this.Major = new Regex(@"\d*").Match(this.Version).Value;

                            return;
                        }
                    }
                }
            }
        }

        private static void NameVersionAction(Match match, Object obj)
        {
            if (!(obj is ClientBrowser current))
                return;

            current.Name = new Regex(@"^[a-zA-Z]+", RegexOptions.IgnoreCase).Match(match.Value).Value;
            if (match.Value.Length > current.Name.Length)
            {
                current.Version = match.Value.Substring(current.Name.Length + 1);
            }
        }
    }
}
