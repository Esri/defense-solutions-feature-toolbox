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
# TestSymbolDictionary.py
# Description: Automatic test of template configuration
# -----------------------------------------------------------------------------

#import arcpy
import os
import sys
import traceback
import TestUtilities

try:
#    arcpy.AddMessage("Starting Test: TestSymbolDictionary")

    sys.path.append('../../../toolboxes/scripts') 
    import MilitaryUtilities
    
    symbolDictionary = MilitaryUtilities.symbolDictionary
    
    sic2Check = "GHMPOGL-----USG"
    
    name = symbolDictionary.symbolIdToName(sic2Check)    
    geoType = symbolDictionary.symbolIdToGeometryType(sic2Check)
    
    print "SIC: " + sic2Check + ", returned Name: " + name + ", GeoType: " + geoType
            
    print "Test Successful"    

except:
    # Get the traceback object
    tb = sys.exc_info()[2]
    tbinfo = traceback.format_tb(tb)[0]

    # Concatenate information together concerning the error into a message string
    pymsg = "PYTHON ERRORS:\nTraceback info:\n" + tbinfo + "\nError Info:\n" + str(sys.exc_info()[1])

    # Print Python error messages for use in Python / Python Window
    print pymsg + "\n"
    
    # return a system error code  
    sys.exit(-1)
