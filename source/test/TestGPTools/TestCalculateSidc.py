#------------------------------------------------------------------------------
# Copyright 2013 Esri
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
#------------------------------------------------------------------------------
# TestCalculateSidc.py
# Description: Automatic Test of GP script/toolbox
# Requirements: ArcGIS Desktop Standard
# -----------------------------------------------------------------------------

import arcpy
import os
import sys
import traceback

import TestUtilities

def RunTest():
    try:
        arcpy.AddMessage("Starting Test: TestCalculateSidc")
                    
        # Prior to this, run TestTemplateConfig.py to verify the expected configuration exists

        inputPointsFC = os.path.join(TestUtilities.inputGDB, "FriendlyOperations\FriendlyUnits")
                                        
        toolbox = TestUtilities.toolbox
               
        # Set environment settings
        print "Running from: " + str(TestUtilities.currentPath)
        print "Geodatabase path: " + str(TestUtilities.geodatabasePath)
        print "Running CalcSIDCField on: " + inputPointsFC
                
        arcpy.env.overwriteOutput = True
        arcpy.ImportToolbox(toolbox, "MFT")
        
        sidcField = "sic"
        standard = "2525"
        echelonField = "echelon" 
        affiliation = "FRIENDLY"
                     
        # Zero out the SIDC/SIC field first
        arcpy.CalculateField_management(inputPointsFC, sidcField, '""')
                      
        ########################################################
        # Execute the Model under test:   
        toolOutput = arcpy.CalcSIDCField_MFT(inputPointsFC, sidcField, standard, echelonField, affiliation)
        ########################################################
        
        # Verify the results
         
        # 1: Check the expected return value
        returnedValue = toolOutput.getOutput(0)        
        if (returnedValue <> inputPointsFC) :
            print "Unexpected Return Value: " + str(returnedValue)
            print "Expected: " + str(inputPointsFC)
            raise Exception("Test Failed")
                
        # 2: That there are no blank or "Default Unknown/SUGPU----------" SIDC values
        outputSidcsLayer = "SidcNull_layer"             
        
        arcpy.MakeFeatureLayer_management(inputPointsFC, outputSidcsLayer)
        query = '(' + sidcField + ' is NULL)' + ' or (' + sidcField + ' = \'SUGPU----------\')'
        
        arcpy.SelectLayerByAttribute_management(outputSidcsLayer, "NEW_SELECTION", query)
        
        nullSidcCount = int(arcpy.GetCount_management(outputSidcsLayer).getOutput(0))
        print "Number of Null SIDC Records is: " + str(nullSidcCount)
        
        if (nullSidcCount > 0) :
            print "Invalid Null SIDC Field Feature Count: " +  str(nullSidcCount)
            raise Exception("Test Failed")         
        
        print "Test Successful"        
                
    except arcpy.ExecuteError: 
        # Get the tool error messages 
        msgs = arcpy.GetMessages() 
        arcpy.AddError(msgs) 
    
        # return a system error code
        sys.exit(-1)
        
    except Exception as e:
        # Get the traceback object
        tb = sys.exc_info()[2]
        tbinfo = traceback.format_tb(tb)[0]
    
        # Concatenate information together concerning the error into a message string
        pymsg = "PYTHON ERRORS:\nTraceback info:\n" + tbinfo + "\nError Info:\n" + str(sys.exc_info()[1])
        msgs = "ArcPy ERRORS:\n" + arcpy.GetMessages() + "\n"
    
        # Return python error messages for use in script tool or Python Window
        arcpy.AddError(pymsg)
        arcpy.AddError(msgs)
    
        # return a system error code
        sys.exit(-1)

RunTest()