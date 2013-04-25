using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        SymbolExporter symbolExporter = new SymbolExporter();

        //SymbolCreator sc = new SymbolCreator();

        //private IMarkerSymbol getSimpleMarker()
        //{
        //    IMarkerSymbol marker = new SimpleMarkerSymbolClass();
        //    IRgbColor Rgb = new RgbColorClass();
        //    Rgb.Red = 155; Rgb.Blue = 155; Rgb.Green = 155; Rgb.Transparency = 255;
        //    marker.Color = Rgb as IColor;
        //    marker.Size = 22.0;
        //    (marker as ISimpleMarkerSymbol).Style = esriSimpleMarkerStyle.esriSMSCircle;

        //    return marker;
        //}

        //void PointSymbolToImageFile(ISymbol symbol, int size, string fileName)
        //{
        //    Image image = ImageFromSymbol(symbol, size, size);
        //    pictureBoxExport.Image = image;

        //    image.Save(fileName);
        //    this.labelStatus.Text = "Exported to " + fileName;
        //}

        //public Image ImageFromSymbol(ISymbol symbol, Int32 width, Int32 height)
        //{
        //    try
        //    {
        //        Bitmap bitmap = new Bitmap(width, height);
        //        Graphics g = Graphics.FromImage(bitmap);
        //        IntPtr hdc = g.GetHdc();

        //        IGeometry geometry = null;
        //        if (symbol is IMarkerSymbol)
        //        {
        //            IPoint point = new PointClass();
        //            point.X = (width / 2);
        //            point.Y = (height / 2);
        //            geometry = point;
        //        }
        //        else if (symbol is ILineSymbol)
        //        {
        //            IPolyline line = new PolylineClass();
        //            IPoint ptFrom = new PointClass();
        //            IPoint ptTo = new PointClass();
        //            ptFrom.X = 3; ptFrom.Y = (height / 2);
        //            ptTo.X = (width - 3); ptTo.Y = ptFrom.Y;
        //            line.FromPoint = ptFrom;
        //            line.ToPoint = ptTo;
        //            geometry = line;
        //        }
        //        else
        //        {
        //            IEnvelope bounds = new EnvelopeClass();
        //            bounds.XMin = 1; bounds.XMax = width - 1;
        //            bounds.YMin = 1; bounds.YMax = height - 1;
        //            geometry = bounds;
        //        }

        //        symbol.SetupDC(hdc.ToInt32(), null);
        //        symbol.Draw(geometry);
        //        symbol.ResetDC();
        //        g.ReleaseHdc(hdc);
        //        g.Dispose();

        //        return bitmap as Image;
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Trace.WriteLine(ex.Message);
        //        return null;
        //    }
        //}

        private void updateImage(Image image)
        {
            if (image == null)
                return;

            pictureBoxExport.Image = image;

            if (cbExportToFile.Checked)
            {
                const string exportFileName = "Export.png";
                image.Save(exportFileName);
                this.labelStatus.Text = "Exported to " + exportFileName;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // sc.Initialize();

            string sic = this.cbSymbolId.Text;
            if (!this.cbSymbolId.Items.Contains(sic))
                this.cbSymbolId.Items.Add(sic);

            const int size = 256;
            Image image = symbolExporter.CreateImageFromSic(sic, size);

            if (image == null)
            {
                this.labelStatus.Text = "Could not create SIDC: " + sic;
                return;
            }

            this.labelStatus.Text = "Created SIDC: " + sic;
            this.labelName.Text = symbolExporter.GetDisplayNameFromSic(sic);
            updateImage(image);

            //ISymbol symbol = sc.GetMarkerSymbolFromSIC(sic) as ISymbol;
            //if (symbol == null)
            //{
            //    this.labelStatus.Text = "SIDC not found: " + sic;
            //    return;
            //}
            //else
            //{
            //    this.labelStatus.Text = "Exporting...";
            //}

            //((IMarkerSymbol)symbol).Size = 72.0;

            //PointSymbolToImageFile(symbol, 256, "Export.png");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void cbSymbolId_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1_Click(sender, e);
        }
    }
}