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

            if (args.Length < 2)
            {
                // If no arguments given, use defaults
                Console.WriteLine("Usage: AppendMilitaryFeatures InputFeatureClass DestinationGDB SymbolIdField");
                Console.WriteLine("--> WARNING: Missing Arguments, Using Default Values");
            }
            else if (args.Length == 2)
            {
                // if exactly 2 arguments supplied, assumes we want to calculate the rep rules only 
                // on the input feature class
                CALCULATE_REP_RULES_ONLY = true;

                inputFeatureClassString = args[0];
                sidcFieldName = args[1];

                // For testing: inputFeatureClassString = System.IO.Path.Combine(dataPath, @"geodatabases\test_outputs.gdb\FriendlyOperations\FriendlyUnits");
                Console.WriteLine("*** Mode set to 'CalculateRepRules Only' ***");
                Console.WriteLine("--> Running CalculateRepRules on Military FeatureClass" + inputFeatureClassString);
            }
            else // >= 3
            {
                inputFeatureClassString = args[0];
                destinationGeodatabase = args[1];
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
                Console.WriteLine("SymbolIdField(2): " + sidcFieldName);
                success = appender.CalculateRepRulesFromSidc(inputFeatureClassString, sidcFieldName);
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

            if (lastErrorCode != 0) // if there is a meaningful error code returned
            {
                Console.WriteLine("**********************************************************");
                Console.WriteLine("ERROR:");
                Console.WriteLine("Exiting with ERROR:");
                Console.WriteLine("Error Code:" + lastErrorCode);
                Console.WriteLine("Generic Error:" + genericLastError);
                Console.WriteLine("Detailed Error:" + detailedLastError);
                Console.WriteLine("**********************************************************");
            }

            return lastErrorCode;
        }
    }
}
