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
# TestToolsApp6.py
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

        arcpy.AddMessage("Starting Test: TestToolsApp6")
                                                            
        toolbox = TestUtilities.toolbox
        
        currentPath = os.path.dirname(__file__)
        dataPath = os.path.normpath(os.path.join(currentPath, r"../../../data/app6b/testdata/"))
        shapefilePath = os.path.normpath(os.path.join(dataPath, r"shapefiles/"))
        geodatabasePath = os.path.normpath(os.path.join(dataPath, r"geodatabases/"))
        
        # Show environment settings
        print "Running from: " + str(currentPath)        
        print "Shapefile path: " + str(shapefilePath)
        print "Geodatabase path: " + str(geodatabasePath)
                        
        arcpy.env.overwriteOutput = True
        arcpy.ImportToolbox(toolbox, "MFT")
                
        sourceGeodatabaseApp6 = os.path.join(geodatabasePath, "Blank-Military_Overlay-APP6.gdb")
        outputGeodatabaseApp6 = os.path.join(geodatabasePath, "Military_Overlay-APP6-Output.gdb")        

        print "Copying geodatabase: " + str(sourceGeodatabaseApp6) + " to " + str(outputGeodatabaseApp6)
        arcpy.Copy_management(sourceGeodatabaseApp6, outputGeodatabaseApp6)

        #############################################################################

        print "Test 1: AppendMilitaryFeatures with APP6"
        
        inputPointsNonMilitaryFeaturesFC = os.path.join(shapefilePath, "TGPoints.shp")
        symbolIdField = "Symbol_ID"
        standard = "APP6"
        
        toolOutput = arcpy.AppendMilitaryFeatures_MFT(inputPointsNonMilitaryFeaturesFC, outputGeodatabaseApp6, symbolIdField, standard) 

        # 1-1: Check the expected return value
        returnedValue = toolOutput.getOutput(0)     
        if (returnedValue <> outputGeodatabaseApp6) :
            print "Unexpected Return Value: " + str(returnedValue)
            print "Expected: " + str(outputGeodatabaseApp6)
            raise Exception("Test Failed")

        outputPointsFC = os.path.join(outputGeodatabaseApp6, r'UnknownOperations\UnknownOperationsP')
                        
        # 1-2: Check that the output feature class contains some values
        outputFeatureCount = int(arcpy.GetCount_management(outputPointsFC).getOutput(0))
        
        print "Output Feature Count: " + str(outputFeatureCount)
                
        if (outputFeatureCount <= 0) :
            print "Invalid Output Feature Count: exiting..."
            raise Exception("Test Failed")        
        
        print "Test 1 Successful" 
        
        #############################################################################
        
        print "Test 2: Calculate SIDC with APP6"        
        
        inputPointsFC = outputPointsFC
        
        sidcField = "sic"
        echelonField = "#" 
        affiliation = "#"
                     
        # Zero out the SIDC/SIC field first
        print "Clearing SIDC Values from " + str(inputPointsFC)
        arcpy.CalculateField_management(inputPointsFC, sidcField, '""')
                      
        print "Running CalcSIDCField_MFT..."                      
        ########################################################
        # Execute the Model under test:   
        toolOutput = arcpy.CalcSymbolIDField_MFT(inputPointsFC, sidcField, standard, echelonField, affiliation)
        ########################################################
        
        # Verify the results
         
        # 2-1: Check the expected return value
        returnedValue = toolOutput.getOutput(0)        
        if (returnedValue <> inputPointsFC) :
            print "Unexpected Return Value: " + str(returnedValue)
            print "Expected: " + str(inputPointsFC)
            raise Exception("Test Failed")
                
        # 2-2: That there are no blank or "Default Unknown/SUGPU----------" SIDC values
        outputSidcsLayer = "SidcNull_layer"             
        
        arcpy.MakeFeatureLayer_management(inputPointsFC, outputSidcsLayer)
        query = '(' + sidcField + ' is NULL)' + ' or (' + sidcField + ' = \'SUGPU----------\')'
        
        arcpy.SelectLayerByAttribute_management(outputSidcsLayer, "NEW_SELECTION", query)
        
        nullSidcCount = int(arcpy.GetCount_management(outputSidcsLayer).getOutput(0))
        print "Number of Null SIDC Records is: " + str(nullSidcCount)
        
        if (nullSidcCount > 0) :
            print "Invalid Null SIDC Field Feature Count: " +  str(nullSidcCount)
            raise Exception("Test Failed")         

        print "Test 2 Successful" 

        #############################################################################
        
        print "Test 3: WriteMessageFileFromMilitaryFeatures with APP6"            
        
        print "Message File path: " + str(dataPath)
        
        outputMessageFile =  os.path.join(dataPath, r"Test-WriteMessageFileFromMilitaryFeatures.xml")       
                
        messageTypeField = "#"
        orderBy = "#"
        disableGeoTransform = "#"
           
        toolOutput = arcpy.WriteMessageFileFromMilitaryFeatures_MFT(inputPointsFC, outputMessageFile, standard, messageTypeField, orderBy, disableGeoTransform)
                
        # Verify the results        
        # 3-1: Check the expected return value
        returnedValue = toolOutput.getOutput(0)        
        if (returnedValue <> outputMessageFile) :
            print "Unexpected Return Value: " + str(returnedValue)
            print "Expected: " + str(outputMessageFile)
            raise Exception("Test Failed")
        
        # 3-2: Check Output File Exists        
        if not (arcpy.Exists(outputMessageFile)) :
            print "Expected output file does not exist: " +  outputMessageFile
            raise Exception("Test Failed")
        
        print "Test 3 Successful" 
                        
        print "All APP6 Tests Successful" 
        
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
