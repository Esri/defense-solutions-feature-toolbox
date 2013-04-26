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
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;

namespace AppendMilitaryFeatures
{
    class SicToFeatureClassMapper
    {
        public SicToFeatureClassMapper()
        {
            initialized = false;
        }

        public SicToFeatureClassMapper(string configFile)
        {
            Initialize(configFile);
        }

        public bool Initialize(string configFile)
        {
            initialized = false;

            if (System.IO.File.Exists(configFile))
            {
                XmlSerializer ruleSerializer = new XmlSerializer(typeof(MappingRules));

                System.IO.FileStream readFileStream = new System.IO.FileStream(configFile, FileMode.Open, FileAccess.Read, FileShare.Read);

                mappingRules = (MappingRules)ruleSerializer.Deserialize(readFileStream);

                readFileStream.Close();

                configurationFile = configFile;
                initialized = true;
            }

            return initialized;
        }

        public bool Initialized
        {
            get { return initialized; }
        }
        private bool initialized = false;

        public string ConfigurationFile
        {
            get { return configurationFile; }
        }
        private string configurationFile;

        public void MapRuleNameToFeatureType(string ruleName, out string featureDataSet, out string featureClassName, out string geometry)
        {
            featureDataSet   = NOT_FOUND;
            featureClassName = NOT_FOUND;
            geometry         = NO_GEOMETRY_STRING;

            if ((mappingRules == null) || (!initialized))
                return;

            foreach (MappingRulesMappingRule rule in mappingRules.MappingRule)
            {
                if (rule.Name == ruleName)
                {
                    featureDataSet   = rule.FeatureDataSet;
                    featureClassName = rule.FeatureClass;
                    geometry         = rule.GeometryType;
                    break;
                }
            }
        }

        public string RuleNameFromSymbolIdAndGeometry(string sidc, string geometry)
        {
            string matchingRuleName, featureDatasetName, featureClassName;
            MapSymbolId(sidc, geometry,
                out matchingRuleName, out featureDatasetName, out featureClassName);

            return matchingRuleName;
        }

        public void MapSymbolId(string sidc, string geometry, out string matchingRuleName, out string featureDataSet, out string featureClassName)
        {
            matchingRuleName = NOT_FOUND;
            featureDataSet = NOT_FOUND;
            featureClassName = NOT_FOUND;

            if ((mappingRules == null) || (!initialized))
                return;

            string sidcUpper = sidc.ToUpper();

            MappingRulesMappingRule[] mappingRule = mappingRules.MappingRule;

            int size = mappingRule.Length;

            bool match = true;
            MappingRulesMappingRule matchingRule = null;

            foreach (MappingRulesMappingRule rule in mappingRule)
            {
                match = (geometry == rule.GeometryType);

                if (!match) continue;

                foreach (string expression in rule.MatchingExpressions)
                {
                    match = Regex.IsMatch(sidcUpper, expression);

                    if (!match) break;
                }

                if (match)
                {
                    matchingRule = rule;
                    break; // break out on 1st rule match
                }
            }

            if ((match) && (matchingRule != null))
            {
                matchingRuleName = matchingRule.Name;
                featureDataSet   = matchingRule.FeatureDataSet;
                featureClassName = matchingRule.FeatureClass;
            }
        }

        public const string NOT_FOUND = "NOT FOUND";
        public const string POINT_STRING = "Point";
        public const string LINE_STRING  = "Line";
        public const string AREA_STRING  = "Area";
        public const string NO_GEOMETRY_STRING = "None";

        private MappingRules mappingRules;
    }


}
