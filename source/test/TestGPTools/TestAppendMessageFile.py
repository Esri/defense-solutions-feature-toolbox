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
# TestAppendMessageFile.py
# Description: Automatic Test of GP script/toolbox
# Requirements: ArcGIS Desktop Standard
# -----------------------------------------------------------------------------

import arcpy
import os
import sys
import traceback
import shutil

import TestUtilities

def RunTest():
    try:
        arcpy.AddMessage("Starting Test: TestAppendMessageFile")
                    
        # Prior to this, run TestTemplateConfig.py to verify the expected configuration exists

        inputPointsFC = os.path.join(TestUtilities.inputGDB, r"FriendlyOperations/FriendlyUnits")

        copyMessageFile =  os.path.join(TestUtilities.outputMessagePath, r"GeoMessageSmall.xml")                   

        outputMessageFile =  os.path.join(TestUtilities.outputMessagePath, r"Test-AppendMessageFileFromMilitaryFeatures.xml")
        
        shutil.copyfile(copyMessageFile, outputMessageFile)  
        
        startOutputFileSize= os.path.getsize(outputMessageFile)
                                        
        toolbox = TestUtilities.toolbox
               
        # Set environment settings
        print "Running from: " + str(TestUtilities.currentPath)
        print "Geodatabase path: " + str(TestUtilities.geodatabasePath)
        print "Message File path: " + str(TestUtilities.outputMessagePath)
                
        arcpy.env.overwriteOutput = True
        arcpy.ImportToolbox(toolbox, "MFT")
        
        standard = "2525"
        
        ########################################################
        # Execute the Model under test:                      
        toolOutput = arcpy.AppendMessageFileFromMilitaryFeatures_MFT(inputPointsFC, outputMessageFile, standard)
        ########################################################
                        
        # Verify the results
        
        # 1: Check the expected return value
        returnedValue = toolOutput.getOutput(0)        
        if (returnedValue <> outputMessageFile) :
            print "Unexpected Return Value: " + str(returnedValue)
            print "Expected: " + str(outputMessageFile)
            raise Exception("Test Failed")
        
        #2: Check that Output File is larger that the previous version 
        #   ie. that it did get appended to
        finishOutputFileSize = os.path.getsize(outputMessageFile)
        print "File Before Append Size: " + str(startOutputFileSize) + ", After Append: " + str(finishOutputFileSize)
        
        if (finishOutputFileSize <= startOutputFileSize) :
            print "Expected output file did not increase (was not appended to)" 
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
