/* Copyright 2013 Esri
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
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
        public Form1(string standard)
        {
            InitializeComponent();

            symbolExporter = new SymbolExporter(standard);
        }

        SymbolExporter symbolExporter = null; 

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
            string sic = this.cbSymbolId.Text;
            if (!this.cbSymbolId.Items.Contains(sic))
                this.cbSymbolId.Items.Add(sic);

            const int size = 256;
            Image image = symbolExporter.CreateImageFromSic(sic, size);

            if (image == null)
            {
                this.labelStatus.Text = "Could not create SIDC: " + sic;
                this.labelName.Text = "";
                return;
            }

            this.labelStatus.Text = "Created SIDC: " + sic;
            this.labelName.Text = symbolExporter.GetDisplayNameFromSic(sic);
            updateImage(image);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.labelStatus.Text = "Using Symbology: " + symbolExporter.SymbologyStandard;
        }

        private void cbSymbolId_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1_Click(sender, e);
        }
    }
}