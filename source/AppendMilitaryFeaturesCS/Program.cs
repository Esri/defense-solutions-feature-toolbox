using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.esriSystem;

namespace AppendMilitaryFeatures
{
    class Program
    {
        private static LicenseInitializer m_AOLicenseInitializer = new AppendMilitaryFeatures.LicenseInitializer();
    
        [STAThread()]
        static int Main(string[] args)
        {
            // default values
            string inputFeatureClassString = @"C:\DefenseTemplates\MilitaryFeatures\AppendMilitaryFeatures\Deployment\TestData\FriendlyForcesSmall.shp";
            string destinationGeodatabase = @"C:\DefenseTemplates\MilitaryFeatures\AppendMilitaryFeatures\Deployment\TestOutput\Default.gdb";
            string sidcFieldName = "Symbol_ID";

            if (args.Length < 2)
            {
                Console.WriteLine("Usage: AppendMilitaryFeatures InputFeatureClass DestinationGDB [SymbolIdField]");
                Console.WriteLine("--> Missing Arguments, Using Default Values");
            }
            else
            {
                inputFeatureClassString = args[0];
                destinationGeodatabase  = args[1];

                // use default SIDC if none provided
                if (args.Length < 3)
                    Console.WriteLine("Using default [SymbolIdField] value: " + sidcFieldName);
                else
                    sidcFieldName = args[2];
            }

            //ESRI License Initializer generated code.
            m_AOLicenseInitializer.InitializeApplication(new esriLicenseProductCode[] 
                { esriLicenseProductCode.esriLicenseProductCodeAdvanced },
            new esriLicenseExtensionCode[] { });

            Console.WriteLine("AppendMilitaryFeatures Parameters:");
            Console.WriteLine("InputFeatureClass(1): " + inputFeatureClassString);
            Console.WriteLine("DestinationGDB(2): " + destinationGeodatabase);
            Console.WriteLine("SymbolIdField(3): " + sidcFieldName);

            MilitaryFeatureAppender appender = new MilitaryFeatureAppender();
            bool success = appender.Process(inputFeatureClassString,
                             destinationGeodatabase,
                             sidcFieldName);
            
            //ESRI License Initializer generated code.
            //Do not make any call to ArcObjects after ShutDownApplication()
            m_AOLicenseInitializer.ShutdownApplication();

            if (success)
                return 0;

            ////////////////////////////////////////////////////
            // IMPORTANT: ERROR CODES RETURNED
            // See: MilitaryFeatureAppender.ErrorCodesToMeaning
            // Get MilitaryFeatureAppender errors 
            // (somewhat complicated because of command line/return codes)
            // The Return Code:
            int lastErrorCode = appender.LastErrorCode;
            // The meaning:
            string detailedLastError = appender.DetailedErrorMessage;            
            string genericLastError = string.Empty;
            if (appender.ErrorCodesToMeaning.ContainsKey(lastErrorCode))
                genericLastError = appender.ErrorCodesToMeaning[lastErrorCode];

            Console.WriteLine("**********************************************************");
            Console.WriteLine("ERROR:");
            Console.WriteLine("Exiting with ERROR:");
            Console.WriteLine("Error Code:" + lastErrorCode);
            Console.WriteLine("Generic Error:" + genericLastError);
            Console.WriteLine("Detailed Error:" + detailedLastError);
            Console.WriteLine("**********************************************************");

            return lastErrorCode;
        }
    }
}
