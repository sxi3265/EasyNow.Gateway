using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace EasyNow.Gateway
{
    public class ForwardConfig
    {
        private static readonly Regex Regex = new Regex(@"{(\w+)}", RegexOptions.Compiled);
        private Regex _pathRegex;
        private string _path;

        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                var match=Regex.Match(Path);
                var pathRegexStr = Path;
                while (match.Success)
                {
                    pathRegexStr = pathRegexStr.Replace($"{{{match.Groups[1].Value}}}", $"(?<{match.Groups[1].Value}>[^/]+)");
                    match = match.NextMatch();
                }
                _pathRegex=new Regex($"^{pathRegexStr}$",RegexOptions.Compiled);
            }
        }

        public Uri Target { get; set; }
        public string TargetPath { get; set; }

        public bool Match(string path)
        {
            return _pathRegex.IsMatch(path);
        }

        public string TransferPath(string path)
        {
            var match = Regex.Match(TargetPath);
            if (!match.Success)
            {
                return TargetPath;
            }

            var matches = _pathRegex.Matches(path);
            var result = TargetPath;
            while (match.Success)
            {
                var name = match.Groups[1].Value;
                var match1 = matches.FirstOrDefault(e => e.Groups[1].Name == name);
                if (match1 != null)
                {
                    result = result.Replace($"{{{name}}}", match1.Groups[1].Value);
                    match = match.NextMatch();
                }
            }

            return result;
        }
    }
}