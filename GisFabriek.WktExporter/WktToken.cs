/*
    MIT License

    Copyright (c) 2019 De GISFabriek

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

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