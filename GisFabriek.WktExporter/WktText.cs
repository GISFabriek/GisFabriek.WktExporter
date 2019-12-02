using System;

namespace GisFabriek.WktExporter
{
    internal class WktText
    {
        internal WktText(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new ArgumentException("WKT is empty");
            }
            ParsePrefix(s);
        }

        internal bool HasZ { get; private set; }

        internal bool HasM { get; private set; }

        internal int CoordinateCount { get; private set; }

        internal WktType Type { get; private set; }

        internal WktToken Token { get; private set; }

        internal string ZmText => (HasZ ? "Z" : "") + (HasM ? "M" : "");

        private void ParsePrefix(string s)
        {
            var startIndex = s.IndexOf('(');
            var prefix = startIndex == -1 ? s : s.Substring(0, startIndex);

            HasZ = false;

            HasM = false;

            CoordinateCount = 2;

            prefix = prefix.Trim().ToLower();

            if (prefix.EndsWith(" z"))
            {
                HasZ = true;
                CoordinateCount = 3;
            }

            if (prefix.EndsWith(" m"))
            {
                HasM = true;
                CoordinateCount = 3;
            }

            if (prefix.EndsWith(" zm"))
            {
                HasM = true;
                HasZ = true;
                CoordinateCount = 4;
            }

            Type = WktType.None;
            if (prefix.StartsWith("point"))
            {
                Type = WktType.Point;
            }
            if (prefix.StartsWith("linestring"))

            {
                Type = WktType.LineString;
            }

            if (prefix.StartsWith("polygon"))
            {
                Type = WktType.Polygon;
            }

            if (prefix.StartsWith("polyhedralsurface"))
            {
                Type = WktType.PolyhedralSurface;
            }

            if (prefix.StartsWith("triangle"))
            {
                Type = WktType.Triangle;
            }

            if (prefix.StartsWith("tin"))
            {
                Type = WktType.Tin;
            }

            if (prefix.StartsWith("multipoint"))
            {
                s = NormalizeForMultiPoint(s);
                Type = WktType.MultiPoint;
            }

            if (prefix.StartsWith("multilinestring"))
            {
                Type = WktType.MultiLineString;
            }

            if (prefix.StartsWith("multipolygon"))
            {
                Type = WktType.MultiPolygon;
            }

            if (prefix.StartsWith("geometrycollection"))
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