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
using System.Linq;
using System.Text;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessing;
using System.Xml.Serialization;
using System.IO;

namespace AppendMilitaryFeatures
{
    class MilitaryFeatureAppender
    {
        // *********** Command Line Error Codes & Meaning *************
        public Dictionary<int, string> ErrorCodesToMeaning = 
            new Dictionary<int, string>()
            {
                {0, "No Error"},
                {1, "Failed to load dependent data files"},
                {2, "Input Dataset does not exist/can't be opened"},
                {3, "No military features found in input"},
                {4, "Output GDB does not exist/can't be opened"},
                {5, "Exclusive Schema Lock could not be obtained on Output GDB"},
                {6, "No [SIDC] field in input data"},
                {99, "Other/Unknown"}
            };

        public int LastErrorCode
        {
            get { return lastErrorCode; }
        }
        private int lastErrorCode = 0;

        public string DetailedErrorMessage
        {
            get { return detailedErrorMessage; }
        }
        private string detailedErrorMessage;
        // *********** Command Line Error Codes & Meaning *************

        /// <summary>
        /// Copies feature from inputFeatureClassString to Military Feature destinationGeodatabase
        /// Set the Representation Rule ID based on sidcFieldName field
        /// </summary>
        /// <returns>Success: True/False</returns>
        public bool Process(string inputFeatureClassString,
                             string destinationGeodatabase,
                             string sidcFieldName)
        {
            bool success = false;

            string installPath = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName;
            string sidcToFeatureClassRulesDataFile = System.IO.Path.Combine(installPath, @"Data\SIDCToFeatureClassRules.xml");

            string fieldMappingDataFile = System.IO.Path.Combine(installPath, @"Data\FieldMapping.xml");
            InitializeFieldMapping(fieldMappingDataFile);

            mapper = new SicToFeatureClassMapper(sidcToFeatureClassRulesDataFile);
            
            if (!mapper.Initialized)
            {
                lastErrorCode = 0; detailedErrorMessage = "Failed to load mapping data file " + sidcToFeatureClassRulesDataFile;
                Console.WriteLine(detailedErrorMessage);
                return false;
            }

            IFeatureClass inputFeatureClass = getFeatureClassFromName(inputFeatureClassString);
            if (inputFeatureClass == null)
            {
                lastErrorCode = 2; detailedErrorMessage = "Could not open Input Feature Class: " + inputFeatureClassString;
                Console.WriteLine(detailedErrorMessage);
                return false;
            }

            if (inputFeatureClass.FindField(sidcFieldName) < 0)
            {
                lastErrorCode = 6; detailedErrorMessage = "Could not find [SIDC] field: " + sidcFieldName + " in Input Feature Class: " + inputFeatureClassString;
                Console.WriteLine(detailedErrorMessage);
                return false;
            }

            HashSet<string> matchedRules = getMatchedRules(inputFeatureClass, sidcFieldName);

            if (matchedRules.Count <= 0)
            {
                lastErrorCode = 3; detailedErrorMessage = "No matching military features found in input: " + inputFeatureClassString;
                Console.WriteLine(detailedErrorMessage);
                return false;
            }

            militaryFeatures = new MilitaryFeatureClassHelper();
            militaryFeatures.FullWorkspacePath = destinationGeodatabase;
            IFeatureWorkspace ws = militaryFeatures.Workspace;

            if (!militaryFeatures.Initialized)
            {
                lastErrorCode = 4; detailedErrorMessage = "Output Workspace could not be found/opened: " + destinationGeodatabase;
                Console.WriteLine(detailedErrorMessage);
                return false;
            }

            symbolCreator = new SymbolCreator();

            // where the main work is done of updating the output
            success = updateMatchedRules(matchedRules, inputFeatureClass, sidcFieldName);

            return success;
        }

        /// <summary>
        /// Set the Representation Rule ID based on sidc field of a 
        /// Military Feature destinationGeodatabase/featurelayer
        /// </summary>
        /// <returns>Success: True/False</returns>
        public bool CalculateRepRulesFromSidc(string outputMilitaryFeatureClassString, string sidcFieldName)
        {
            bool success = false;

            MilitaryFeatureClassHelper.SIDC_FIELD_NAME2 = sidcFieldName;

            symbolCreator = new SymbolCreator();

            militaryFeatures = new MilitaryFeatureClassHelper();
            IFeatureClass outputFeatureClass = militaryFeatures.GetFeatureClassByName(outputMilitaryFeatureClassString);

            if (!militaryFeatures.Initialized || (outputFeatureClass == null))
            {
                lastErrorCode = 4; detailedErrorMessage = "Output FeatureClass could not be found/opened: " + outputMilitaryFeatureClassString;
                Console.WriteLine(detailedErrorMessage);
                return false;
            }

            if (!militaryFeatures.IsFeatureClassLockable(outputFeatureClass))
            {
                // if a schema lock can't be obtained for feature class then bail on them all
                lastErrorCode = 5; detailedErrorMessage = string.Format("Exclusive Schema Lock can not be obtained for output feature class found for Rule:{0}", outputMilitaryFeatureClassString);
                Console.WriteLine(detailedErrorMessage);
                return false;
            }

            ////////////////////////////////////////////////////////////
            // Initialization/Verification complete, now do processing

            repRulesWereAdded = false;

            ///////////////////////////////////////////////////////////////////
            // DEBUG SWITCH: allows testing without writing the output to the feature class
            const bool DEBUG_DONT_WRITE_OUTPUT = false; //  true;
            ///////////////////////////////////////////////////////////////////

            ////////////////////////////////////////////////////////////
            // TRICKY: Handle the 2 different output SIC/SIDC names in Military Features
            int sicFieldIndex = outputFeatureClass.Fields.FindField(MilitaryFeatureClassHelper.SIDC_FIELD_NAME1);
            if (sicFieldIndex < 0)
            {
                sicFieldIndex = outputFeatureClass.Fields.FindField(MilitaryFeatureClassHelper.SIDC_FIELD_NAME2);
                if (sicFieldIndex < 0)
                {
                    lastErrorCode = 6; detailedErrorMessage = string.Format("ABORTING: Could not find SIDC field in output");
                    Console.WriteLine(detailedErrorMessage);
                    return false;
                }
            }
            ////////////////////////////////////////////////////////////

            // Start Editing
            IWorkspaceEdit workspaceEdit = militaryFeatures.Workspace as IWorkspaceEdit;
            if (workspaceEdit == null)
            {
                lastErrorCode = 5; detailedErrorMessage = string.Format("Exclusive Schema Lock can not be obtained for output feature class found for Rule:{0}", outputMilitaryFeatureClassString);
                Console.WriteLine(detailedErrorMessage);
                return false;
            }

            workspaceEdit.StartEditing(false);
            workspaceEdit.StartEditOperation();

            IRepresentationClass repClass = militaryFeatures.GetRepresentationClassForFeatureClass(outputFeatureClass);
            if (repClass == null)
            {
                Console.WriteLine("ABORTING: RepresentationClass not found in output");
                return false;
            }

            // setup insert cursor 
            IFeatureCursor featureCursor = outputFeatureClass.Update(null, true);
            IFeature currentFeature = featureCursor.NextFeature();

            int featureCount = 0;

            while (currentFeature != null)
            {
                string sidc = currentFeature.get_Value(sicFieldIndex) as string;

                if (!symbolCreator.IsValidSic(sidc))
                {
                    if (string.IsNullOrEmpty(sidc) || (sidc.Length <= 0))
                        Console.WriteLine("Skipping empty SIDC");
                    else
                        Console.WriteLine("Skipping invalid SIDC: " + sidc);

                    currentFeature = featureCursor.NextFeature();
                    continue;
                }

                featureCount++;

                IFeatureBuffer featureBuffer = currentFeature as IFeatureBuffer;

                processSidc(repClass, featureBuffer, sidc);

                if (!DEBUG_DONT_WRITE_OUTPUT)
                {
                    featureCursor.UpdateFeature(currentFeature);
                }

                currentFeature = featureCursor.NextFeature();
            }

            if (!DEBUG_DONT_WRITE_OUTPUT)
            {
                featureCursor.Flush();
            }

            workspaceEdit.StopEditOperation();
            workspaceEdit.StopEditing(true);

            // Release the cursors to remove the lock on the data.
            System.Runtime.InteropServices.Marshal.ReleaseComObject(featureCursor);
            featureCursor = null;

            if (!DEBUG_DONT_WRITE_OUTPUT)
            {
                // looks totally nonsensical - but this forces any new rules to written
                if (repRulesWereAdded)
                    repClass.RepresentationRules = repClass.RepresentationRules;
            }

            success = true;

            return success;
        }

        private bool updateMatchedRules(HashSet<string> matchedRules, IFeatureClass inputFeatureClass, string sidcFieldName)
        {
            if ((mapper == null) || (inputFeatureClass == null) || (militaryFeatures == null)
                || (!mapper.Initialized) || (!militaryFeatures.Initialized))
                return false;

            foreach (string rule in matchedRules)
            {                
                string featureDatasetName, featureClassName, geometry;
                mapper.MapRuleNameToFeatureType(rule, out featureDatasetName, out featureClassName, out geometry);

                Console.WriteLine("Processing Rule:{0}, FDS:{1}, FC:{2}, Geometry:{3}", rule, featureDatasetName, featureClassName, geometry);

                IFeatureClass outputFeatureClass = militaryFeatures.GetFeatureClassByName(featureDatasetName, featureClassName);

                if (outputFeatureClass == null)
                {
                    Console.WriteLine("No output feature class found for Rule:{0}, FDS:{1}, FC:{2}", rule, featureDatasetName, featureClassName);
                    continue;
                }

                if (!militaryFeatures.IsFeatureClassLockable(outputFeatureClass))
                {
                    // if a schema lock can't be obtained for any 1 feature class then bail on them all
                    lastErrorCode = 5; detailedErrorMessage = string.Format("Exclusive Schema Lock can not be obtained for output feature class found for Rule:{0}, FDS:{1}, FC:{2}", rule, featureDatasetName, featureClassName);
                    Console.WriteLine(detailedErrorMessage);
                    return false;                    
                }

                if (!doesGeometryTypeEqualGeometryString(outputFeatureClass.ShapeType, geometry))
                {
                    Console.WriteLine("Geometry does not match Output feature class for Rule:{0}, FDS:{1}, FC:{2}, Geometry:{3}", rule, featureDatasetName, featureClassName, geometry);
                    continue;
                }

                bool success = processFeatureClass(rule, geometry, inputFeatureClass, outputFeatureClass, sidcFieldName);

            }

            return true;
        }

        private bool processFeatureClass(string rule, string geometry, IFeatureClass inputFeatureClass, IFeatureClass outputFeatureClass, string sidcFieldName)
        {
            repRulesWereAdded = false;

            // allows testing without writing the output to the feature
            const bool DEBUG_DONT_WRITE_OUTPUT = false; //  true;

            if ((mapper == null) || (inputFeatureClass == null) || (outputFeatureClass == null)
                || (militaryFeatures == null) || (symbolCreator == null)
                || (!mapper.Initialized) || (!militaryFeatures.Initialized))
                return false;

            bool success = false;

            int sicFieldIndex = inputFeatureClass.Fields.FindField(sidcFieldName);

            if (sicFieldIndex < 0)
            {
                Console.WriteLine("SIDC field not found: {0} - ABORTING", sidcFieldName);
                return false;
            }

            doFieldMapping(inputFeatureClass, outputFeatureClass, sidcFieldName);

            // Start Editing
            IWorkspaceEdit workspaceEdit = militaryFeatures.Workspace as IWorkspaceEdit;
            if (workspaceEdit == null) return false;
            workspaceEdit.StartEditing(false);        
            workspaceEdit.StartEditOperation();

            IRepresentationClass repClass = militaryFeatures.GetRepresentationClassForFeatureClass(outputFeatureClass);
            if (repClass == null)
            {
                Console.WriteLine("RepresentationClass not found in output - ABORTING");
                return false;
            }

            // setup insert cursor 
            IFeatureBuffer targetFeatureBuffer = outputFeatureClass.CreateFeatureBuffer();
            IFeatureCursor targetFeatureCursor = outputFeatureClass.Insert(true);

            IFeatureCursor featureCursor = inputFeatureClass.Search(null, true);
            IFeature currentFeature = featureCursor.NextFeature();

            int matchingFeatureCount = 0;

            while (currentFeature != null)
            {
                string sidc = currentFeature.get_Value(sicFieldIndex) as string;

                string matchingRule = mapper.RuleNameFromSymbolIdAndGeometry(sidc, geometry);

                if (matchingRule != rule)
                {
                    currentFeature = featureCursor.NextFeature();
                    continue;
                }

                matchingFeatureCount++;

                Console.WriteLine("Processing Matching Feature: #:{0}, SIDC:{1}, Rule:{2}", matchingFeatureCount, sidc, rule);               

                targetFeatureBuffer.Shape = currentFeature.Shape;

                processFieldMapping(currentFeature, targetFeatureBuffer);

                processSidc(repClass, targetFeatureBuffer, sidc);

                processMiscellaneousFields(targetFeatureBuffer, sidc);

                if (!DEBUG_DONT_WRITE_OUTPUT)
                {
                    // insert new feature
                    targetFeatureCursor.InsertFeature(targetFeatureBuffer);
                }
                
                currentFeature = featureCursor.NextFeature();
            }

            if (!DEBUG_DONT_WRITE_OUTPUT)
            {
                targetFeatureCursor.Flush();
            }

            workspaceEdit.StopEditOperation();        
            workspaceEdit.StopEditing(true);

            // Release the cursors to remove the lock on the data.
            System.Runtime.InteropServices.Marshal.ReleaseComObject(targetFeatureCursor);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(featureCursor);
            targetFeatureCursor = null;
            featureCursor = null;

            if (!DEBUG_DONT_WRITE_OUTPUT)
            {
                // looks totally nonsensical - but this forces any new rules to written
                if (repRulesWereAdded)
                    repClass.RepresentationRules = repClass.RepresentationRules;
            }

            return success;
        }

        private bool doesGeometryTypeEqualGeometryString(esriGeometryType geoType, string geometryString)
        {
            if ((geometryString == SicToFeatureClassMapper.POINT_STRING) &&
                (geoType == esriGeometryType.esriGeometryPoint))
                return true;

            if ((geometryString == SicToFeatureClassMapper.LINE_STRING) &&
                ((geoType == esriGeometryType.esriGeometryPolyline) ||
                (geoType == esriGeometryType.esriGeometryMultipoint)))
                return true;

            if ((geometryString == SicToFeatureClassMapper.AREA_STRING) &&
                (geoType == esriGeometryType.esriGeometryPolygon))
                return true;

            return false;
        }

        private HashSet<string> getMatchedRules(IFeatureClass inputFeatureClass, string sidcFieldName)
        {
            HashSet<string> matchedRules = new HashSet<string>();

            if ((inputFeatureClass == null) || (mapper == null) || (!mapper.Initialized))
                return matchedRules; // = nothing

            IFeatureCursor featureCursor = inputFeatureClass.Search(null, true);
            if (featureCursor == null)
                return matchedRules; // = nothing

            IFeature currentFeature = featureCursor.NextFeature();
            if (currentFeature == null)
            {
                Console.WriteLine("No features in input Feature Class - ABORTING");
                return matchedRules; // = nothing
            }

            string geometryString = SicToFeatureClassMapper.POINT_STRING;

            if ((inputFeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline) ||
                (inputFeatureClass.ShapeType == esriGeometryType.esriGeometryMultipoint))
                geometryString = SicToFeatureClassMapper.LINE_STRING;
            else
                if (inputFeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon)
                    geometryString = SicToFeatureClassMapper.AREA_STRING;

            int sicFieldIndex = currentFeature.Fields.FindField(sidcFieldName);

            if (sicFieldIndex < 0)
            {
                Console.WriteLine("SIDC field not found: {0} - ABORTING", sidcFieldName);
                return matchedRules;
            }

            while (currentFeature != null)
            {
                string sidc = currentFeature.get_Value(sicFieldIndex) as string;

                string matchingRule = mapper.RuleNameFromSymbolIdAndGeometry(sidc, geometryString);

                if (!((matchingRule == SicToFeatureClassMapper.NOT_FOUND) || (matchedRules.Contains(matchingRule))))
                    matchedRules.Add(matchingRule);

                currentFeature = featureCursor.NextFeature();
            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(featureCursor);
            featureCursor = null;
            currentFeature = null;

            Console.WriteLine("Applicable Rules:");
            foreach (string rule in matchedRules)
            {
                Console.WriteLine("   Rules: " + rule);
            }

            return matchedRules;
        }

        private void doFieldMapping(IFeatureClass inputFeatureClass, IFeatureClass outputFeatureClass, string sidcFieldName)
        {
            try
            {
                // reset the field mapping on each new FeatureClass
                OutputFieldToInputField.Clear();

                int fieldCount = inputFeatureClass.Fields.FieldCount;

                for (int i = 0; i < fieldCount; i++)
                {
                    IField inputField = inputFeatureClass.Fields.get_Field(i);

                    if (!(inputField.Type == esriFieldType.esriFieldTypeOID || inputField.Type == esriFieldType.esriFieldTypeGeometry 
                        || inputField.Name.ToUpper().StartsWith("SHAPE_")))
                    {
                        string inputName = inputField.Name;

                        string mapToOutputFieldName = inputName; // default output to same as input name

                        if (mapToOutputFieldName == sidcFieldName)
                        {
                            // TRICKY: Handle the 2 different output SIC/SIDC names in Military Features
                            if (outputFeatureClass.Fields.FindField(
                                MilitaryFeatureClassHelper.SIDC_FIELD_NAME1) >= 0)
                                mapToOutputFieldName = MilitaryFeatureClassHelper.SIDC_FIELD_NAME1;
                            else 
                                if (outputFeatureClass.Fields.FindField(
                                    MilitaryFeatureClassHelper.SIDC_FIELD_NAME2) >= 0)
                                    mapToOutputFieldName = MilitaryFeatureClassHelper.SIDC_FIELD_NAME2;
                                else
                                    Console.WriteLine("WARNING: Could not find SIDC field in output");
                        }
                        else
                            if (InputFieldToOutputFieldAllDatasets.ContainsKey(inputName))
                                mapToOutputFieldName = InputFieldToOutputFieldAllDatasets[inputName];

                        if (!OutputFieldToInputField.ContainsKey(mapToOutputFieldName))
                        {
                            int outputFieldIndex = outputFeatureClass.Fields.FindField(mapToOutputFieldName);
                            if (outputFieldIndex >= 0)
                            {
                                IField outputField = outputFeatureClass.Fields.get_Field(outputFieldIndex);

                                if (inputField.Type == outputField.Type)
                                {
                                    Console.WriteLine("Mapping Input Field: " + inputName + " to Output Field: " + mapToOutputFieldName);
                                    OutputFieldToInputField.Add(mapToOutputFieldName, inputName);
                                }
                                else
                                {
                                    Console.WriteLine("Failed to Map (Types Don't Match) Input Field: " + inputName + " to Output Field: " + mapToOutputFieldName);
                                }
                            }
                        }
                    }
                }

                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void processFieldMapping(IFeature sourceFeature, IFeatureBuffer targetFeatureBuffer)
        {
            if ((sourceFeature == null) || (targetFeatureBuffer == null))
                return;

            try
            {
                foreach (string outputFieldName in OutputFieldToInputField.Keys)
                {
                    string inputFieldName = OutputFieldToInputField[outputFieldName];

                    if (!String.IsNullOrEmpty(outputFieldName) && !String.IsNullOrEmpty(inputFieldName))
                    {
                        int sourceIndex = sourceFeature.Fields.FindField(inputFieldName);
                        int targetIndex = targetFeatureBuffer.Fields.FindField(outputFieldName);

                        if (sourceIndex >= 0 && targetIndex >= 0)
                        {
                            IField sourceField = sourceFeature.Fields.get_Field(sourceIndex);

                            object sourceValue = sourceFeature.get_Value(sourceIndex);
                            if (sourceValue == null)
                            {
                                continue;
                            }
                            if (sourceField.Type == esriFieldType.esriFieldTypeString)
                            {
                                if (String.IsNullOrEmpty(sourceValue as string))
                                {
                                    continue;
                                }
                            }
                            IField targetField = targetFeatureBuffer.Fields.get_Field(targetIndex);
                            try
                            {
                                if (targetField.Editable)
                                {
                                    targetFeatureBuffer.set_Value(targetIndex, sourceValue);
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine("Field: " + targetField.Name + " not editable.");
                                }
                            }
                            catch (Exception ex2)
                            {
                                Console.WriteLine(ex2);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        // used by processMiscellaneousFields to determine if a featurebuffer has not changed
        private string lastUniqueDesignation = "NOT SET";
        private string lastCountryCode = "NOT SET";
        private int lastEchelon = -1;

        private void processMiscellaneousFields(IFeatureBuffer targetFeatureBuffer, string sidc)
        {
            if ((symbolCreator == null) || (targetFeatureBuffer == null))
                return;

            // Set required field UniqueDesignation if empty
            int indexUD = targetFeatureBuffer.Fields.FindField(MilitaryFeatureClassHelper.UNIQUE_ID_FIELD_NAME);
            if (indexUD >= 0)
            {
                object oUD = targetFeatureBuffer.get_Value(indexUD);
                string s = oUD.ToString();

                // if its null or hasn't changed, set it
                if ((lastUniqueDesignation == s) || (oUD == null) || (oUD == DBNull.Value) || (s.Length == 0))
                {
                    string newUniqueDesignation = symbolCreator.GetGenericSymbolName(sidc);

                    // Apparently if you set a string field to a size larger than allowed it throws an exception
                    // so don't let this happen
                    IField uidField = targetFeatureBuffer.Fields.get_Field(indexUD);
                    if (uidField != null)
                    {
                        int maxUidLength = uidField.Length;
                        if (newUniqueDesignation.Length > maxUidLength)
                            newUniqueDesignation = newUniqueDesignation.Substring(0, maxUidLength);
                    }

                    targetFeatureBuffer.set_Value(indexUD, newUniqueDesignation);
                    lastUniqueDesignation = newUniqueDesignation;
                }
            }

            // Set Echelon field 
            int indexEch = targetFeatureBuffer.Fields.FindField(MilitaryFeatureClassHelper.ECHELON_FIELD);
            if ((indexEch >= 0) && (symbolCreator.HasValidEchelon(sidc)))
            {
                object oEch = targetFeatureBuffer.get_Value(indexEch);
                if ((oEch == DBNull.Value) || (oEch.ToString().Length == 0)) // if it's empty
                {
                    int ech = symbolCreator.GetEchelonOrdinal(sidc);
                    oEch = (object)ech;
                    targetFeatureBuffer.set_Value(indexEch, oEch);
                    lastEchelon = ech;
                }
                else
                {
                    // if its not empty, it may be because of the previous value/state of the featurebuffer
                    // so we need to check the case when it hasn't changed
                    int ech = (int)oEch;
                    if (ech == lastEchelon)
                    {
                        ech = symbolCreator.GetEchelonOrdinal(sidc);
                        oEch = (object)ech;
                        targetFeatureBuffer.set_Value(indexEch, oEch);
                        lastEchelon = ech;
                    }
                }
            }

            // Set Country Code field 
            int indexCC = targetFeatureBuffer.Fields.FindField(MilitaryFeatureClassHelper.COUNTRY_FIELD);
            if (indexCC >= 0)
            {
                object oCC = targetFeatureBuffer.get_Value(indexCC);
                string s = oCC.ToString();

                // if its null or hasn't changed, set it
                if ((lastCountryCode == s) || (oCC == null) || (oCC == DBNull.Value) || (s.Length == 0))
                {
                    string countryCode = symbolCreator.GetCountryCode(sidc);
                    targetFeatureBuffer.set_Value(indexCC, countryCode);
                    lastCountryCode = countryCode;
                }
            }


        }

        private void processSidc(IRepresentationClass repClass, IFeatureBuffer targetFeatureBuffer, string sidc)
        {
            if ((symbolCreator == null) || (repClass == null) || (targetFeatureBuffer == null))
            {
                Console.WriteLine("Failed to initialize - could not create RepRule for SIDC: " + sidc);
                return;
            }

            symbolCreator.Initialize();

            int ruleIdIndex = targetFeatureBuffer.Fields.FindField(MilitaryFeatureClassHelper.RULE_FIELD_NAME1);
            if (ruleIdIndex < 0) // *2* different rule field names, need to check for both
                ruleIdIndex = targetFeatureBuffer.Fields.FindField(MilitaryFeatureClassHelper.RULE_FIELD_NAME2);

            if (ruleIdIndex < 0)
            {
                Console.WriteLine("Could not find field:{0}/{1} in output", 
                    MilitaryFeatureClassHelper.RULE_FIELD_NAME1,
                    MilitaryFeatureClassHelper.RULE_FIELD_NAME2);
                return;
            }

            int repRuleId = getRepRuleIdForSidc(repClass, sidc);
               
            if (repRuleId < 0)
                repRuleId = addRepRuleIdForSidc(repClass, sidc);

            if (repRuleId < 0)
            {
                Console.WriteLine("Could not create RepRule for SIDC: " + sidc);
                return;
            }

            object o = repRuleId;
            targetFeatureBuffer.set_Value(ruleIdIndex, o);
        }

        /// <summary>
        /// Check if a rep rule for the selected SymbolIDCode exists and if so returns it
        /// </summary>
        /// <returns>-1 if symbol not found/could not be added</returns>
        private int addRepRuleIdForSidc(IRepresentationClass repClass, string sidc)
        {
            if ((symbolCreator == null) || (repClass == null))
                return -1;

            int repRuleId = -1;

            IRepresentationRules repRules = repClass.RepresentationRules;

            IRepresentationRule newRule = new RepresentationRuleClass();

            ISymbol symbol = symbolCreator.GetMarkerSymbolFromSIC(sidc) as ISymbol;
            if (symbol == null)
            {
                Console.WriteLine("ERROR: Null Symbol returned for SIDC: " + sidc);
                return -1;
            }

            IMarkerSymbol markerSymbol = symbol as IMarkerSymbol;

            const double DEFAULT_MARKER_SIZE = 32.0;
            if (markerSymbol != null)
                markerSymbol.Size = DEFAULT_MARKER_SIZE;

            (newRule as IRepresentationRuleInit).InitWithSymbol(symbol);

            string symboName = symbolCreator.GetRuleNameFromSidc(sidc);
            if (string.IsNullOrEmpty(symboName))
                return -1;

            repRuleId = repRules.Add(newRule);
            repRules.set_Name(repRuleId, symboName);

            repRulesWereAdded = true;

            Console.WriteLine("Adding new RepRule for Name: {0}, SIDC:{1}", symboName, sidc);
                
            return repRuleId;
        }

        /// <summary>
        /// Check if a rep rule for the selected SymbolIDCode exists and if so returns it
        /// </summary>
        /// <returns>-1 if not found</returns>
        private int getRepRuleIdForSidc(IRepresentationClass repClass, string sidc)
        {
            int returnRepRuleId = -1;

            if ((symbolCreator == null) || (repClass == null))
                return -1;

            string symboName = symbolCreator.GetRuleNameFromSidc(sidc);
            if (string.IsNullOrEmpty(symboName))
            {
                Console.WriteLine("Empty Name returned for SIDC: " + sidc);
                return -1;
            }

            IRepresentationRules repRules = repClass.RepresentationRules;
            repRules.Reset();

            int ruleID = 0;
            IRepresentationRule rule;
            repRules.Next(out ruleID, out rule);

            while (rule != null)
            {
                if (rule != null)
                {
                    string ruleName = repRules.get_Name(ruleID);

                    if (ruleName == symboName)
                    {
                        returnRepRuleId = ruleID;
                        break;
                    }
                }

                repRules.Next(out ruleID, out rule);
            }

            if (returnRepRuleId == -1)
                System.Diagnostics.Debug.WriteLine("Existing Rule not found for " + symboName);

            return returnRepRuleId;
        }

        private IFeatureClass getFeatureClassFromName(string inputFeatureClassString)
        {
            IFeatureClass inputFeatureClass = null;

            try
            {
                inputFeatureClass = gpUtils.OpenFeatureClassFromString(inputFeatureClassString);

                if (inputFeatureClass != null)
                    Console.WriteLine("Successfully opened input feature class: " + inputFeatureClass.AliasName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to open input feature class: " + inputFeatureClassString);
                Console.WriteLine("Exception: " + ex.Message);

                inputFeatureClass = null;
            }

            return inputFeatureClass;
        }

        public void InitializeFieldMapping(string fieldMappingFile)
        {
            InputFieldToOutputFieldAllDatasets.Clear();

            if (!System.IO.File.Exists(fieldMappingFile))
            {
                Console.WriteLine("No Field Mapping Selected");

                // if no mapping file exists, set any hard coded values here:
                InputFieldToOutputFieldAllDatasets["Name"] = "uniquedesignation";

                return;
            }

            Console.WriteLine("Using Field Mapping File: " + fieldMappingFile);

            // Load mapping file
            XmlSerializer mappingSerializer = new XmlSerializer(typeof(FieldMapping));

            System.IO.FileStream readFileStream = new System.IO.FileStream(fieldMappingFile, FileMode.Open, FileAccess.Read, FileShare.Read);

            FieldMapping mapping = (FieldMapping)mappingSerializer.Deserialize(readFileStream);

            readFileStream.Close();

            foreach (FieldMappingField field in mapping.Items)
            {
                string keyInputField = field.InputName;
                string valueOutputField = field.OutputName;

                InputFieldToOutputFieldAllDatasets[keyInputField] = valueOutputField;
            }

        }

        // Private Members

        // Geoprocessing Helper to get FeatureClass from string
        IGPUtilities gpUtils = new GPUtilitiesClass();

        // Maps SIDCs to a Military Feature Class
        private SicToFeatureClassMapper mapper = null;

        // Helper to open the Military Feature Classes
        private MilitaryFeatureClassHelper militaryFeatures = null;

        // Creates ISymbols from SIDCs 
        private SymbolCreator symbolCreator = null;

        // Feature Class Field Mapping
        private Dictionary<string, string> InputFieldToOutputFieldAllDatasets = new Dictionary<string, string>();
        private Dictionary<string, string> OutputFieldToInputField = new Dictionary<string, string>();

        // Internal Flag to indicate that a Feature Class's RepRules need updated on close
        private bool repRulesWereAdded = false;
    }
}
           
