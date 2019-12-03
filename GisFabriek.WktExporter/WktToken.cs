using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace GisFabriek.WktExporter
{
    internal class WktToken
    {
        private readonly string _text;
        internal WktToken(string text)
        {
            _text = StripGeometryName(text);
        }

        internal IEnumerable<WktToken> PointArrays
        {
            get
            {
                var parts = new List<string>();
                if (_text.Contains(")),((")) // It is a MULTIPOLYGON
                {
                    string[] separator = {")),(("};
                    var split = _text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    parts.AddRange(split);
                }
                else
                {
                    parts.Add(_text);
                }
                var tokenStrings = new List<string>();
                foreach (var part in parts)
                {
                    if (part.Contains("),(")) // There a multiple parts
                    {
                        string[] separator = { "),(" };
                        var split = part.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var item in split)
                        {
                            var tokenString = CleanUpBraces(item);
                            tokenStrings.Add(tokenString);
                        }
                    }
                    else
                    {
                        var tokenString = CleanUpBraces(part);
                        tokenStrings.Add(tokenString);
                    }
                }

                foreach (var tokenString in tokenStrings)
                {
                    yield return new WktToken(tokenString);
                }
            }
        }

        private string CleanUpBraces(string item)
        {
            var temp = item;
            temp = temp.Replace("(", string.Empty);
            temp = temp.Replace(")", string.Empty);
            return temp;
        }

        internal  IEnumerable<WktToken> PointGroups
        {
            get
            {
                if ((_text.Contains(" ")))
                {
                    string[] separator = { "," };
                    var split = _text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in split)
                    {
                        yield return new WktToken(item);
                    }
                }
                else
                {
                    throw new InvalidDataException("Wrong Token level");
                }
            }
        }

        internal IEnumerable<double> Coords
        {
            get
            {
                if (_text.Contains(")(") || _text.Contains(","))
                {
                    throw new InvalidDataException("Wrong Token level");
                }
                string[] separator = { " " };
                var split = _text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in split)
                {
                    yield return Convert.ToDouble(item, CultureInfo.InvariantCulture);
                }
            }
        }

        private string StripGeometryName(string text)
        {
            var index = text.IndexOf("(", StringComparison.InvariantCulture);
            if (index >= 0)
            {
                return text.Substring(index);
            }
            return text;
        }
    }
}