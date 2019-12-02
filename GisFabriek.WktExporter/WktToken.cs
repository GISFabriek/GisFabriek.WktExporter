using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace GisFabriek.WktExporter
{
    internal class WktToken
    {
        private string _text;
        internal WktToken(string text)
        {
            _text = StripGeometryName(text);
        }

        internal IEnumerable<WktToken> PointArrays
        {
            get
            {
                _text = StripLeadingBraces(_text);
                _text = StripTrailingBraces(_text);
                while (_text.Contains("(("))
                {
                    _text = _text.Replace("((", "(");
                }
                while (_text.Contains("))"))
                {
                    _text = _text.Replace("))", ")");
                }

                if (_text.Contains("),("))
                {
                   string[] separator = { "),(" };
                   var split = _text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                   foreach (var item in split)
                   {
                       yield return new WktToken(item);
                   }
                }
                else
                {
                    yield return new WktToken(_text);
                }
                
            }
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

        private string StripLeadingBraces(string text)
        {
            while (text.StartsWith("("))
            {
                text = text.Substring(1);
            }

            return text;
        }

        private string StripTrailingBraces(string text)
        {
            while (text.EndsWith(")"))
            {
                text = text.Substring(0, text.Length - 1);
            }

            return text;
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