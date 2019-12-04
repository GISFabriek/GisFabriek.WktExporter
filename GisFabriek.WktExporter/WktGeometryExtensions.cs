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

using ArcGIS.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Envelope = ArcGIS.Core.Geometry.Envelope;
using Geometry = ArcGIS.Core.Geometry.Geometry;
using Multipoint = ArcGIS.Core.Geometry.Multipoint;
using Polygon = ArcGIS.Core.Geometry.Polygon;
using Polyline = ArcGIS.Core.Geometry.Polyline;
using SpatialReference = ArcGIS.Core.Geometry.SpatialReference;

namespace GisFabriek.WktExporter
{
    /// <summary>
    /// Transform Esri geometries to and from the WKT representation, as defined in
    /// section 7 of http://www.opengeospatial.org/standards/sfa
    /// </summary>

    public static class WktGeometryExtensions
    {
        private static int _webMercatorWkId = 3857;

        public static async Task<string> ToWellKnownText(this Geometry geometry)
        {
            return await BuildWellKnownText(geometry);
        }

        public static async Task<Geometry> ToGeometry(this string wkt, bool simplified)
        {
            var spatialReference = await CreateDefaultSpatialReference();
            return await BuildGeometry(wkt, spatialReference, simplified);
        }

        public static async Task<Geometry> ToGeometry(this string wkt, SpatialReference spatialReference, bool simplified)
        {
            return await BuildGeometry(wkt, spatialReference, simplified);
        }

        public static async Task<Geometry> ToGeometry(this string wkt, int wkId, bool simplified)
        {
            var spatialReference = await CreateSpatialReference(wkId);
            return await BuildGeometry(wkt, spatialReference, simplified);
        }

        private static async Task<string> BuildWellKnownText(Geometry geometry)
        {
            if (geometry is MapPoint point)
            {
                return BuildWellKnownText(point);
            }

            if (geometry is Multipoint multipoint)
            {
                return BuildWellKnownText(multipoint);
            }

            if (geometry is Polyline polyline)
            {
                return BuildWellKnownText(polyline);
            }

            if (geometry is Polygon polygon)
            {
                return await BuildWellKnownText(polygon);
            }

            if (geometry is GeometryBag)
            {
                throw new NotImplementedException(Localization.Resources.GeometryBagNotSupportedMessage);
            }

            return string.Empty;
        }

        private static string BuildWellKnownText(MapPoint point)
        {
            var suffix = string.Empty;
            if (point.HasZ)
            {
                suffix += "Z";
            }

            if (point.HasM)
            {
                suffix += "M";
            }
            if (suffix.Length > 0)
            {
                suffix += " ";
            }
            return string.Format(CultureInfo.InvariantCulture, "POINT {0}({1} {2})", suffix, point.X, point.Y);
        }

        private static string BuildWellKnownText(Multipoint points)
        {
            //Example - MULTIPOINT (10 40, 40 30, 20 20, 30 10)
            var suffix = string.Empty;
            if (points.HasZ)
            {
                suffix += "Z";
            }

            if (points.HasM)
            {
                suffix += "M";
            }
            if (suffix.Length > 0)
            {
                suffix += " ";
            }
            return "MULTIPOINT "+ suffix + "(" + BuildWellKnownText(points.Points) + ")";
        }

        private static string BuildWellKnownText(Polyline polyline)
        {
            //Example - LINESTRING (30 10, 10 30, 40 40)
            //Example - MULTILINESTRING ((10 10, 20 20, 10 40),(40 40, 30 30, 40 20, 30 10))

            if (polyline.Parts == null)
            {
                return string.Empty;
            }

            var partCount = polyline.PartCount;
            if (partCount == 0)
            {
                return string.Empty;
            }
            var suffix = string.Empty;
            if (polyline.HasZ)
            {
                suffix += "Z";
            }

            if (polyline.HasM)
            {
                suffix += "M";
            }
            if (suffix.Length > 0)
            {
                suffix += " ";
            }

            if (partCount == 1)
            {
                return "LINESTRING "+ suffix + "(" + BuildWellKnownText(polyline.Points) + ")";
            }

            return "MULTILINESTRING " + suffix + BuildWellKnownText(polyline.Parts, false);
        }

        private static async Task<string> BuildWellKnownText(Polygon polygon)
        {
            //Example - POLYGON ((30 10, 10 20, 20 40, 40 40, 30 10))
            //Example - POLYGON ((35 10, 10 20, 15 40, 45 45, 35 10),(20 30, 35 35, 30 20, 20 30))
            //Example - MULTIPOLYGON (((30 20, 10 40, 45 40, 30 20)),((15 5, 40 10, 10 20, 5 10, 15 5)))
            //Example - MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)),((20 35, 45 20, 30 5, 10 10, 10 30, 20 35),(30 20, 20 25, 20 15, 30 20)))

            if (polygon == null)
            {
                return string.Empty;
            }

            var partCount = polygon.PartCount;

            if (partCount == 0)
            {
                return string.Empty;
            }

            var outerPartCount = 0;
            foreach (var part in polygon.Parts)
            {
                if (IsOuterRing(part))
                {
                    outerPartCount++;
                }
            }

            if (outerPartCount == 0)
            {
                return string.Empty;
            }
            var suffix = string.Empty;
            if (polygon.HasZ)
            {
                suffix += "Z";
            }

            if (polygon.HasM)
            {
                suffix += "M";
            }
            if (suffix.Length > 0)
            {
                suffix += " ";
            }
            if (outerPartCount > 1)
            {
                var polygons = await MultipartToSinglePart(polygon);
                return "MULTIPOLYGON " + suffix + BuildWellKnownText(polygons);
            }

            return "POLYGON " + suffix + BuildWellKnownText(polygon.Parts, true);
        }

        private static bool IsOuterRing(ReadOnlySegmentCollection part)
        {
            var ringCoordinates = new List<Coordinate2D>();

            foreach (var segment in part)
            {
                ringCoordinates.Add(segment.StartCoordinate);
                ringCoordinates.Add(segment.EndCoordinate);
            }

            var signedArea = 0.0;

            for (var i = 0; i < ringCoordinates.Count - 1; i++)
            {
                var x1 = ringCoordinates[i].X;
                var y1 = ringCoordinates[i].Y;

                double x2, y2;

                if (i == ringCoordinates.Count - 2)
                {
                    x2 = ringCoordinates[0].X;
                    y2 = ringCoordinates[0].Y;
                }
                else
                {
                    x2 = ringCoordinates[i + 1].X;
                    y2 = ringCoordinates[i + 1].Y;
                }

                signedArea += ((x1 * y2) - (x2 * y1));
            }

            // if signedArea is a negative number => indicates an outer ring 
            // if signedArea is a positive number => indicates an inner ring
            return signedArea < 0;
        }

        private static async Task<List<Geometry>> MultipartToSinglePart(Geometry inputGeometry)
        {
            return await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                var singleParts = new List<Geometry>();

                if (inputGeometry == null || inputGeometry.IsEmpty)
                {
                    return singleParts;
                }

                switch (inputGeometry.GeometryType)
                {
                    case GeometryType.Envelope:
                        singleParts.Add(inputGeometry.Clone() as Envelope);
                        break;
                    case GeometryType.Multipatch:
                        singleParts.Add(inputGeometry.Clone() as Multipatch);
                        break;
                    case GeometryType.Multipoint:

                        if (inputGeometry is Multipoint multiPoint)
                            foreach (var point in multiPoint.Points)
                            {
                                singleParts.Add(point);
                            }

                        break;
                    case GeometryType.Point:
                        singleParts.Add(inputGeometry.Clone() as MapPoint);
                        break;
                    case GeometryType.Polygon:

                        if (inputGeometry is Polygon polygon)
                            foreach (var polygonPart in polygon.Parts)
                            {
                                var singlePart = PolygonBuilder.CreatePolygon(polygonPart);
                                singlePart = GeometryEngine.Instance.SimplifyAsFeature(singlePart, true) as Polygon;
                                singleParts.Add(singlePart);
                            }

                        break;
                    case GeometryType.Polyline:

                        if (inputGeometry is Polyline polyline)
                            foreach (var polylinePart in polyline.Parts)
                            {
                                var singlePart = PolylineBuilder.CreatePolyline(polylinePart);
                                singlePart = GeometryEngine.Instance.SimplifyAsFeature(singlePart, true) as Polyline;
                                singleParts.Add(singlePart);
                            }

                        break;
                    case GeometryType.Unknown:
                        break;
                }

                return singleParts;
            });
        }

        private static string BuildWellKnownText(List<Geometry> polygons)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("(");
            var first = true;
            foreach (var item in polygons)
            {
                if (item is Polygon polygon)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        stringBuilder.Append(",");
                    }
                    stringBuilder.Append(BuildWellKnownText(polygon.Parts, true));
                }
            }

            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }

        private static string BuildWellKnownText(ReadOnlyPartCollection parts, bool isPolygon)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("(");
            using (var segments = parts.GetEnumerator())
            {
                if (isPolygon)
                {
                    BuildPolygonText(segments, stringBuilder);
                }
                else
                {
                    BuildLinestringText(segments, stringBuilder);
                }
            }

            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }

        private static void BuildLinestringText(IEnumerator<ReadOnlySegmentCollection> segments,
            StringBuilder stringBuilder)
        {
            var start = true;
            while (segments.MoveNext())
            {
                var seg = segments.Current;
                if (seg != null)
                {
                    var line = new List<MapPoint>();
                    for (var i = 0; i < seg.Count; i++)
                    {
                        var item = seg[i];
                        line.Add(item.StartPoint);
                        if (i == seg.Count - 1)
                        {
                            line.Add(item.EndPoint);
                        }
                    }

                    if (start)
                    {
                        start = false;
                    }
                    else
                    {
                        stringBuilder.Append(",");
                    }
                    stringBuilder.AppendFormat("({0})", BuildWellKnownText(line));
                }
            }
        }

        private static void BuildPolygonText(IEnumerator<ReadOnlySegmentCollection> segments, StringBuilder stringBuilder)
        {
            var outerRing = new List<MapPoint>();
            var innerRings = new List<List<MapPoint>>();
            while (segments.MoveNext())
            {
                var seg = segments.Current;
                if (seg != null)
                {
                    if (IsOuterRing(seg))
                    {
                        for (var i = 0; i < seg.Count; i++)
                        {
                            var item = seg[i];
                            outerRing.Add(item.StartPoint);
                            if (i == seg.Count - 1)
                            {
                                outerRing.Add(item.EndPoint);
                            }
                        }
                    }
                    else
                    {
                        var innerRing = new List<MapPoint>();
                        for (var i = 0; i < seg.Count; i++)
                        {
                            var item = seg[i];
                            innerRing.Add(item.StartPoint);
                            if (i == seg.Count - 1)
                            {
                                innerRing.Add(item.EndPoint);
                            }
                        }

                        innerRings.Add(innerRing);
                    }
                }
            }

            outerRing.Reverse();
            stringBuilder.AppendFormat("({0})", BuildWellKnownText(outerRing));
            foreach (var innerRing in innerRings)
            {
                innerRing.Reverse();
                stringBuilder.AppendFormat(",({0})", BuildWellKnownText(innerRing));
            }
        }


        private static string BuildWellKnownText(ReadOnlyPointCollection points)
        {
            var items = points.Select(x => x).ToList();
            return BuildWellKnownText(items);
        }

        private static string BuildWellKnownText(List<MapPoint> points)
        {
            //Example - 10 40
            //Example - 10 40, 40 30, 20 20, 30 10
            var stringBuilder = new StringBuilder();
            var pointCount = points.Count;
            if (pointCount < 1)
            {
                return string.Empty;
            }

            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0} {1}", GetNumberAsString(points[0].X), GetNumberAsString(points[0].Y));
            if (points[0].HasZ)
            {
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " {0}", GetNumberAsString(points[0].Z));
            }

            if (points[0].HasM)
            {
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " {0}", GetNumberAsString(points[0].M));
            }
            for (var i = 1; i < pointCount; i++)
            {
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, ",{0} {1}", GetNumberAsString(points[i].X), GetNumberAsString(points[i].Y));
                if (points[i].HasZ)
                {
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " {0}",GetNumberAsString(points[i].Z));
                }

                if (points[i].HasM)
                {
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " {0}", GetNumberAsString(points[i].M));
                }
            }
            return stringBuilder.ToString();
        }

        private static string GetNumberAsString(double number)
        {
            return double.IsNaN(number) ? "0.0" : string.Format(CultureInfo.InvariantCulture, "{0}", Math.Round(number, 8));
        }

        private static async Task<Geometry> BuildGeometry(string wktString, SpatialReference spatialReference, bool simplified)
        {
            var wkt = new WktText(wktString);
            switch (wkt.Type)
            {
                case WktType.None:
                    return null;
                case WktType.Point:
                    return await BuildPoint(wkt, spatialReference);
                case WktType.LineString:
                    return await BuildPolyline(wkt, spatialReference, simplified);
                case WktType.Polygon:
                    return await BuildPolygon(wkt, spatialReference, simplified);
                case WktType.Triangle:
                    return await BuildPolygon(wkt, spatialReference, simplified);
                case WktType.PolyhedralSurface:
                    throw new NotImplementedException(Localization.Resources.PolyhedralSurfaceNotSupportedMessage);
                case WktType.Tin:
                    return await BuildPolygon(wkt, spatialReference, simplified);  // TODO is this OK?
                case WktType.MultiPoint:
                    return await BuildMultiPoint(wkt, spatialReference);
                case WktType.MultiLineString:
                    return await BuildMultiPolyline(wkt, spatialReference, simplified);
                case WktType.MultiPolygon:
                    return await BuildPolygon(wkt, spatialReference, simplified);
                 case WktType.GeometryCollection:
                     throw new NotImplementedException(Localization.Resources.GeometryCollectionNotSupportedMessage);
                default:
                    throw new ArgumentOutOfRangeException(nameof(wktString), Localization.Resources.UnsupportedGeometryType + @": " + wkt.Type);
            }
        }

        private static async Task<Geometry> BuildPoint(WktText wkt, SpatialReference spatialReference)
        {
            return await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() => BuildPoint(wkt.Token.PointArrays.FirstOrDefault()?.PointGroups.FirstOrDefault(), wkt, spatialReference));
        }

        private static async Task<Geometry> BuildMultiPoint(WktText wkt, SpatialReference spatialReference)
        {
            return await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                var points = GetMapPoints(wkt.Token.PointArrays.FirstOrDefault(), wkt, spatialReference);

                using (var polylineBuilder = new MultipointBuilder(points))
                {
                    polylineBuilder.SpatialReference = spatialReference;
                    return polylineBuilder.ToGeometry();
                }
            });
        }

        private static async Task<Geometry> BuildPolyline(WktText wkt, SpatialReference spatialReference, bool simplified)
        {
            return await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run( () =>
            {
                var path = GetMapPoints(wkt.Token.PointArrays.FirstOrDefault(), wkt, spatialReference);
                using (var polylineBuilder = new PolylineBuilder(path))
                {
                    polylineBuilder.SpatialReference = spatialReference;
                    var result = polylineBuilder.ToGeometry();
                    if (!simplified)
                    {
                        return result;
                    }
                    var simplePolyline = GeometryEngine.Instance.SimplifyAsFeature(result, true);
                    return simplePolyline;
                }
            });
        }

        private static async Task<Geometry> BuildMultiPolyline(WktText wkt, SpatialReference spatialReference, bool simplified)
        {
            return await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                using (var polylineBuilder = new PolylineBuilder(spatialReference))
                {
                    foreach (var lineString in wkt.Token.PointArrays)
                    {
                        var path = GetMapPoints(lineString, wkt, spatialReference);
                        polylineBuilder.AddPart(path);
                    }

                    polylineBuilder.SpatialReference = spatialReference;
                    var result = polylineBuilder.ToGeometry();
                    if (!simplified)
                    {
                        return result;
                    }
                    var simplePolyline = GeometryEngine.Instance.SimplifyAsFeature(result, true);
                    return simplePolyline;
                }
            });
        }

        private static async Task<Geometry> BuildPolygon(WktText wkt, SpatialReference spatialReference, bool simplified)
        {
            return await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                using (var polygonBuilder = new PolygonBuilder(spatialReference))
                {
                    foreach (var lineString in wkt.Token.PointArrays)
                    {
                        var path = GetMapPoints(lineString, wkt, spatialReference);
                        path.Reverse();
                        polygonBuilder.AddPart(path);
                    }
                    var result = polygonBuilder.ToGeometry();
                    if (!simplified)
                    {
                        return result;
                    }
                    var simplePolygon = GeometryEngine.Instance.SimplifyAsFeature(result, true);
                    return simplePolygon;
                }
            });
        }

        private static List<MapPoint> GetMapPoints(WktToken token, WktText wkt, SpatialReference spatialReference)
        {
            var points = new List<MapPoint>();
                if (token == null)
                {
                    return points;

                }
                foreach (var point in token.PointGroups)
                {
                    var mapPoint = BuildPoint(point, wkt, spatialReference);
                    points.Add(mapPoint);
                }
                return points;
        }

        private static MapPoint BuildPoint(WktToken token, WktText wkt, SpatialReference spatialReference)
        {
            if (token == null)
            {
                return null;
            }
            var coordinates = token.Coords.ToArray();
            var partCount = coordinates.Length;
            if (!wkt.HasZ && !wkt.HasM && partCount != 2)
            {
                throw new ArgumentException( Localization.Resources.MalformedWktWrongNumberOfElelementsFragment + " x y");
            }

            if (wkt.HasZ && !wkt.HasM && partCount != 3)
            {
                throw new ArgumentException(Localization.Resources.MalformedWktWrongNumberOfElelementsFragment + " x y z");
            }

            if (!wkt.HasZ && wkt.HasM && partCount != 3)
            {
                throw new ArgumentException(Localization.Resources.MalformedWktWrongNumberOfElelementsFragment + " x y m");
            }

            if (wkt.HasZ && wkt.HasM && partCount != 4)
            {
                throw new ArgumentException(Localization.Resources.MalformedWktWrongNumberOfElelementsFragment + " x y z m");
            }

            MapPoint mapPoint;
            if (partCount == 2)
            {
                mapPoint = MapPointBuilder.CreateMapPoint(coordinates[0], coordinates[1], spatialReference);
            }
            else if (partCount == 3)
            {
                if (wkt.HasZ)
                {
                    mapPoint = MapPointBuilder.CreateMapPoint(coordinates[0], coordinates[1], coordinates[2],
                        spatialReference);
                }
                else
                {
                    mapPoint = MapPointBuilder.CreateMapPoint(coordinates[0], coordinates[1], 0.0, coordinates[2],
                        spatialReference);
                }
            }
            else
            {
                mapPoint = MapPointBuilder.CreateMapPoint(coordinates[0], coordinates[1], coordinates[2],
                    coordinates[3],
                    spatialReference);
            }

            return mapPoint;
        }

        private static async Task<SpatialReference> CreateSpatialReference(int wkId)
        {
            return await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                SpatialReferenceBuilder.CreateSpatialReference(wkId));
        }

        private static async Task<SpatialReference> CreateDefaultSpatialReference()
        {
            return await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                SpatialReferenceBuilder.CreateSpatialReference(_webMercatorWkId));
        }
    }

}
