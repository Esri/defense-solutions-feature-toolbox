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
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace TestSymbolCreator
{
    static class Program
    {
        private static LicenseInitializer m_AOLicenseInitializer = new TestSymbolCreator.LicenseInitializer();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
             
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //ESRI License Initializer generated code.
            m_AOLicenseInitializer.InitializeApplication(new esriLicenseProductCode[] { esriLicenseProductCode.esriLicenseProductCodeAdvanced },
            new esriLicenseExtensionCode[] { });
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length < 1)
            {
                ///////////////////////////////////////////////////////////////////////////////////////
                // WORKAROUND: Somewhat Kludgey way to get a Windows App also to work as a console app
                // Set it to Console App in project settings, but then hide the Command Window here:
                IntPtr h = Process.GetCurrentProcess().MainWindowHandle;
                ShowWindow(h, 0);
                ///////////////////////////////////////////////////////////////////////////////////////

                // if no command line arguments, run as Form App
                Application.Run(new Form1());
            }
            else
            {
                // if command line arguments, run as Consolse App
                Console.WriteLine("Usage: TestSymbolCreator.exe <Symbol ID Code>");

                string sic = args[0];

                string exportFilename = sic + ".png";
                exportFilename = exportFilename.Replace('*', '-'); // just in case some *'s in sic

                const int size = 256;

                SymbolExporter symbolExporter = new SymbolExporter();
                symbolExporter.CreateImageFileFromSic(sic, size, exportFilename);
            }

            //ESRI License Initializer generated code.
            //Do not make any call to ArcObjects after ShutDownApplication()
            m_AOLicenseInitializer.ShutdownApplication();
        }
    }
}