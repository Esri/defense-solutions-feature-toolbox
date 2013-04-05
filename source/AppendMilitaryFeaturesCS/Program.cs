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
            // default values for stand-alone test
            // assumes current path currentPath: military-feature-toolbox\application
            string currentPath = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName;
            string dataPath =  System.IO.Path.Combine(currentPath, @"..\data");

            if (!System.IO.Directory.Exists(dataPath))
            {
                // Might also be run from: military-feature-toolbox\source\AppendMilitaryFeaturesCS\bin
                dataPath = System.IO.Path.Combine(currentPath, @"..\..\..\..\data");

                if (!System.IO.Directory.Exists(dataPath))
                {
                    Console.WriteLine("--> WARNING: Could not field expected test data folder");
                }
            }

            string inputFeatureClassString = System.IO.Path.Combine(dataPath, @"shapefiles\FriendlyForcesSmall.shp");
            string destinationGeodatabase = System.IO.Path.Combine(dataPath, @"geodatabases\test_outputs.gdb");
            string sidcFieldName = "Symbol_ID";

            bool CALCULATE_REP_RULES_ONLY = false;
            bool success = false;

            if (args.Length < 1)
            {
                // If no arguments given, use defaults
                Console.WriteLine("Usage: AppendMilitaryFeatures InputFeatureClass DestinationGDB [SymbolIdField]");
                Console.WriteLine("--> WARNING: Missing Arguments, Using Default Values");
            }
            else if (args.Length < 2)
            {
                // if only 1 argument supplied, assumes this argument is a Military Feature Class
                // to just calculate the rep rules on
                CALCULATE_REP_RULES_ONLY = true;

                inputFeatureClassString = args[0];

                // For testing: inputFeatureClassString = System.IO.Path.Combine(dataPath, @"geodatabases\test_outputs.gdb\FriendlyOperations\FriendlyUnits");
                Console.WriteLine("*** Mode set to 'CalculateRepRules Only' ***");
                Console.WriteLine("--> Running CalculateRepRules on Military FeatureClass" + inputFeatureClassString);
            }
            else
            {
                inputFeatureClassString = args[0];
                destinationGeodatabase = args[1];

                // use default SIDC if none provided
                if (args.Length < 3)
                    Console.WriteLine("Using default input [SymbolIdField] value: " + sidcFieldName);
                else
                    sidcFieldName = args[2];
            }

            //ESRI License Initializer generated code.
            m_AOLicenseInitializer.InitializeApplication(new esriLicenseProductCode[] 
                { esriLicenseProductCode.esriLicenseProductCodeAdvanced },
            new esriLicenseExtensionCode[] { });

            Console.WriteLine("AppendMilitaryFeatures Parameters:");
            Console.WriteLine("InputFeatureClass(1): " + inputFeatureClassString);

            MilitaryFeatureAppender appender = new MilitaryFeatureAppender();
            if (CALCULATE_REP_RULES_ONLY)
            {
                success = appender.CalculateRepRulesFromSidc(inputFeatureClassString);
            }
            else
            {
                Console.WriteLine("DestinationGDB(2): " + destinationGeodatabase);
                Console.WriteLine("SymbolIdField(3): " + sidcFieldName);
                success = appender.Process(inputFeatureClassString,
                                 destinationGeodatabase,
                                 sidcFieldName);
            }
            
            //ESRI License Initializer generated code.
            //Do not make any call to ArcObjects after ShutDownApplication()
            m_AOLicenseInitializer.ShutdownApplication();

            if (success)
            {
                Console.WriteLine("AppendMilitaryFeatures.exe Succeeded.");
                return 0;
            }

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
