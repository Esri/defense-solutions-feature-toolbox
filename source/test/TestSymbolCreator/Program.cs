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

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

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
                IntPtr h = Process.GetCurrentProcess().MainWindowHandle;
                ShowWindow(h, 0);

                // if no command line arguments, run as Form App
                Application.Run(new Form1());
            }
            else
            {
                // if command line arguments, run as Consolse App

                // WORKAROUND: Kludgey way to get a Windows App also to write to the console 
                // when run as a console app 
                // AttachConsole(ATTACH_PARENT_PROCESS);

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