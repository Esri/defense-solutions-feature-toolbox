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
# TestWriteMilitaryFeatureFromMessage.py
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
        arcpy.AddMessage("Starting Test: TestWriteMilitaryFeatureFromMessage")
                    
        # Prior to this, run TestTemplateConfig.py to verify the expected configuration exists

        inputMessageFile =  os.path.join(TestUtilities.outputMessagePath, r"FriendlyUnitsMessages.xml")       
        inputMessageFileGeoMsg = os.path.join(TestUtilities.outputMessagePath, r"GeoMessageSmall.xml")

        outputPointsFC = os.path.join(TestUtilities.outputGDB, r"FriendlyOperations/FriendlyUnits")
                                        
        toolbox = TestUtilities.toolbox
               
        # Set environment settings
        print "Running from: " + str(TestUtilities.currentPath)
        print "Geodatabase path: " + str(TestUtilities.geodatabasePath)
        print "Message File path: " + str(TestUtilities.outputMessagePath)
                
        startRecordCount = int(arcpy.GetCount_management(outputPointsFC).getOutput(0))
                
        arcpy.env.overwriteOutput = True
        arcpy.ImportToolbox(toolbox, "MFT")
                     
        ########################################################
        # Execute the Model under test:   
        # Test 1: (Runtime Message Output)
        toolOutput = arcpy.WriteMilitaryFeatureFromMessageFile_MFT(inputMessageFile, outputPointsFC)
        ########################################################
        
        # Verify the results
        # 1a: Check the expected return value (Test 1)
        returnedValue = toolOutput.getOutput(0)        
        if (returnedValue <> outputPointsFC) :
            print "Unexpected Return Value: " + str(returnedValue)
            print "Expected: " + str(outputPointsFC)
            raise Exception("Test Failed")  
        
        # 2a: Check that Output Record Count is larger that the previous count 
        #     ie. that it did get appended to
        endRecordCount = int(arcpy.GetCount_management(outputPointsFC).getOutput(0))
        print "Record Count Before: " + str(startRecordCount) + ", After: " + str(endRecordCount)
        
        if (endRecordCount <= startRecordCount) :
            print "Expected record count did not increase (was not added to)" 
            raise Exception("Test Failed")  
                
        # reset this for the next test
        startRecordCount = endRecordCount
        
        ########################################################
        # Execute the Model under test:   
        # Test 2: (Geo Message Output)    
        messageFormat = "ARCGIS_GEOMESSAGE"
                        
        toolOutput = arcpy.WriteMilitaryFeatureFromMessageFile_MFT(inputMessageFileGeoMsg, outputPointsFC, messageFormat)
        ########################################################
              
        # Verify the results
        # 1b: Check the expected return value
        returnedValue = toolOutput.getOutput(0)        
        if (returnedValue <> outputPointsFC) :
            print "Unexpected Return Value: " + str(returnedValue)
            print "Expected: " + str(outputPointsFC)
            raise Exception("Test Failed")  
        
        # 2b: Check that Output Record Count is larger that the previous count 
        #     ie. that it did get appended to
        endRecordCount = int(arcpy.GetCount_management(outputPointsFC).getOutput(0))
        print "Record Count Before: " + str(startRecordCount) + ", After: " + str(endRecordCount)
        
        if (endRecordCount <= startRecordCount) :
            print "Expected record count did not increase (was not added to)" 
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