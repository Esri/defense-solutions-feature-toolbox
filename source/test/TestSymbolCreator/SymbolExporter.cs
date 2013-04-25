using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;

using AppendMilitaryFeatures;

namespace TestSymbolCreator
{
    class SymbolExporter
    {
        SymbolCreator sc = new SymbolCreator();

        public SymbolExporter()
        {
            sc.Initialize();
        }

        public string GetDisplayNameFromSic(string sic)
        {
            return sc.GetRuleNameFromSidc(sic);
        }

        public Image CreateImageFromSic(string sic, int size)
        {
            ISymbol milFeatureSymbol = getMilitaryMarker(sic);

            Image image = ImageFromSymbol(milFeatureSymbol, size, size);

            return image;
        }

        public void CreateImageFileFromSic(string sic, int size, string fileName)
        {
            Image image = CreateImageFromSic(sic, size);

            if (image != null)
                image.Save(fileName);
        }

        private ISymbol getMilitaryMarker(string sic)
        {
            ISymbol symbol = sc.GetMarkerSymbolFromSIC(sic) as ISymbol;

            ((IMarkerSymbol)symbol).Size = 72.0;

            if (symbol == null)
            {
                Console.WriteLine("SIDC not found: " + sic);
                return null;
            }

            return symbol;
        }

        private Image ImageFromSymbol(ISymbol symbol, Int32 width, Int32 height)
        {
            if (symbol == null)
                return null;

            try
            {
                Bitmap bitmap = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(bitmap);
                IntPtr hdc = g.GetHdc();

                IGeometry geometry = null;
                if (symbol is IMarkerSymbol)
                {
                    IPoint point = new PointClass();
                    point.X = (width / 2);
                    point.Y = (height / 2);
                    geometry = point;
                }
                else if (symbol is ILineSymbol)
                {
                    IPolyline line = new PolylineClass();
                    IPoint ptFrom = new PointClass();
                    IPoint ptTo = new PointClass();
                    ptFrom.X = 3; ptFrom.Y = (height / 2);
                    ptTo.X = (width - 3); ptTo.Y = ptFrom.Y;
                    line.FromPoint = ptFrom;
                    line.ToPoint = ptTo;
                    geometry = line;
                }
                else
                {
                    IEnvelope bounds = new EnvelopeClass();
                    bounds.XMin = 1; bounds.XMax = width - 1;
                    bounds.YMin = 1; bounds.YMax = height - 1;
                    geometry = bounds;
                }

                symbol.SetupDC(hdc.ToInt32(), null);
                symbol.Draw(geometry);
                symbol.ResetDC();
                g.ReleaseHdc(hdc);
                g.Dispose();

                return bitmap as Image;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                return null;
            }
        }

        // For simple test
        private IMarkerSymbol getSimpleMarker()
        {
            IMarkerSymbol marker = new SimpleMarkerSymbolClass();
            IRgbColor Rgb = new RgbColorClass();
            Rgb.Red = 155; Rgb.Blue = 155; Rgb.Green = 155; Rgb.Transparency = 255;
            marker.Color = Rgb as IColor;
            marker.Size = 22.0;
            (marker as ISimpleMarkerSymbol).Style = esriSimpleMarkerStyle.esriSMSCircle;

            return marker;
        }
    }
}
