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
# TestAppendMilitaryFeatures.py
# Description: Automatic Test of GP script/toolbox
# Requirements: ArcGIS Desktop Standard
# -----------------------------------------------------------------------------

import arcpy
import os
import sys
import traceback

import TestUtilities

def deleteTemp() :
    try :   
        arcpy.Delete_management(TestUtilities.outputGDBTemp)
    except:    
        print "Delete: " + TestUtilities.outputGDBTemp + " failed (may not exist yet)"
        
def RunTest():
    try:
        arcpy.AddMessage("Starting Test: TestAppendMilitaryFeatures")
                    
        # Prior to this, run TestTemplateConfig.py to verify the expected configuration exists

        # Delete Temp GDB (in case it already exists, for instance if prior run crashed)
        deleteTemp()
        
        inputPointsFC = os.path.join(TestUtilities.inputGDBNonMilitaryFeatures, r"FriendlyForces")

        outputPointsFC = os.path.join(TestUtilities.outputGDBTemp, r"FriendlyOperations/FriendlyUnits")
                                        
        toolbox = TestUtilities.toolbox
               
        # Set environment settings
        print "Running from: " + str(TestUtilities.currentPath)
        print "Geodatabase path: " + str(TestUtilities.geodatabasePath)
                
        arcpy.env.overwriteOutput = True
        arcpy.ImportToolbox(toolbox, "MFT")
                     
        # Copy Blank Workspace to Temp GDB
        arcpy.Copy_management(TestUtilities.blankMilFeaturesGDB, TestUtilities.outputGDBTemp)
                        
        outputWorkspace = TestUtilities.outputGDBTemp
        symbolIdField = "Symbol_ID"
        standard = "2525"
        
        ########################################################
        # Execute the Model under test:           
        toolOutput = arcpy.AppendMilitaryFeatures_MFT(inputPointsFC, outputWorkspace, symbolIdField, standard)
        ########################################################

        # Verify the results
        
        # 1: Check the expected return value
        returnedValue = toolOutput.getOutput(0)        
        if (returnedValue <> TestUtilities.outputGDBTemp) :
            print "Unexpected Return Value: " + str(returnedValue)
            print "Expected: " + str(TestUtilities.outputGDBTemp)
            raise Exception("Test Failed")
        
        # 2: Check that the output feature class contains some values
        outputFeatureCount = int(arcpy.GetCount_management(outputPointsFC).getOutput(0))
        
        print "Output Feature Count: " + str(outputFeatureCount)
                
        if (outputFeatureCount <= 0) :
            print "Invalid Output Feature Count: exiting..."
            raise Exception("Test Failed")
                   
        # Delete Temp GDB
        deleteTemp()      
        
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