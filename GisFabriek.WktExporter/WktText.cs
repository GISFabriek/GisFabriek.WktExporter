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

namespace GisFabriek.WktExporter
{
    internal class WktText
    {
        internal WktText(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new ArgumentException(Localization.Resources.WktIsEmptyErrorMessage, nameof(s));
            }
            ParsePrefix(s);
        }

        internal bool HasZ { get; private set; }

        internal bool HasM { get; private set; }

        internal int CoordinateCount { get; private set; }

        internal WktType Type { get; private set; }

        internal WktToken Token { get; private set; }

        private void ParsePrefix(string s)
        {
            var startIndex = s.IndexOf('(');
            var prefix = startIndex == -1 ? s : s.Substring(0, startIndex);

            HasZ = false;

            HasM = false;

            CoordinateCount = 2;

            prefix = prefix.Trim().ToUpper();

            if (prefix.EndsWith(" Z"))
            {
                HasZ = true;
                CoordinateCount = 3;
            }

            if (prefix.EndsWith(" M"))
            {
                HasM = true;
                CoordinateCount = 3;
            }

            if (prefix.EndsWith(" ZM"))
            {
                HasM = true;
                HasZ = true;
                CoordinateCount = 4;
            }

            Type = WktType.None;
            if (prefix.StartsWith("POINT"))
            {
                Type = WktType.Point;
            }
            if (prefix.StartsWith("LINESTRING"))

            {
                Type = WktType.LineString;
            }

            if (prefix.StartsWith("POLYGON"))
            {
                Type = WktType.Polygon;
            }

            if (prefix.StartsWith("POLYHEDRALSURFACE"))
            {
                Type = WktType.PolyhedralSurface;
            }

            if (prefix.StartsWith("TRIANGLE"))
            {
                Type = WktType.Triangle;
            }

            if (prefix.StartsWith("TIN"))
            {
                Type = WktType.Tin;
            }

            if (prefix.StartsWith("MULTIPOINT"))
            {
                s = NormalizeForMultiPoint(s);
                Type = WktType.MultiPoint;
            }

            if (prefix.StartsWith("MULTILINESTRING"))
            {
                Type = WktType.MultiLineString;
            }

            if (prefix.StartsWith("MULTIPOLYGON"))
            {
                Type = WktType.MultiPolygon;
            }

            if (prefix.StartsWith("GEOMETRYCOLLECTION"))
            {
                Type = WktType.GeometryCollection;
            }

            Token = new WktToken(s);
        }

        private string NormalizeForMultiPoint(string s)
        {
            if (s.Contains("(("))
            {
                var first = s.IndexOf('(');
                var prefix = s.Substring(0, first);
                var rest = s.Substring(first);
                rest = rest.Replace("(", string.Empty);
                rest = rest.Replace(")", string.Empty);
                s = prefix + "(" + rest + ")";
            }

            return s;
        }
    }
}