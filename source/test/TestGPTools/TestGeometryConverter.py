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
# TestGeometryConverter.py
# Description: Stand-alone unit test of GeometryConverter class
# -----------------------------------------------------------------------------

import os
import sys
import math
import traceback
import TestUtilities
 
geometryConverter = None
FAIL_DELTA_TOLERANCE = 0.0000001 # used by tests to determine if expected/returned values are close enough
    
def TestGeoConversionProblem() :
    
    SymbolIdCodeVal = "GHMPOFG---****X"
    
    # -80.293990821,38.37637526; -80.103633941,38.438836112"
    controlPointsString = "-80.0,38.0;-81.0,39.0" 
    attributes = { } 
    
    requiresConversion = MilitaryUtilities.geoConverter.requiresConversion(SymbolIdCodeVal)
    if requiresConversion :                
        msg = "SIC: " + SymbolIdCodeVal + " requires conversion/translation"
        print msg
        
    transformedPoints, conversionNotes = \
        MilitaryUtilities.geoConverter.geometrytoControlPoints(SymbolIdCodeVal, controlPointsString, attributes)     
    
    print "Original Points: " + controlPointsString

    print "Transformed Points: " + transformedPoints
    
    print "Notes: " + conversionNotes

def TestRotateAndScale() :
    
    import GeometryConverter
       
    x0 = -80.0
    y0 = 40.0 
    x1 = -81.0
    y1 = 39.0 
    
    # rotate (-81,39) about (-80, 40) by 45 degrees
    rx, ry = GeometryConverter.rotate(45.0, x0, y0, x1, y1)    
    print "Rotate: ", rx, ry

    # (-81.0, 39.0) --> (-80.0, 38.5857864376)
    # http://www.wolframalpha.com/input/?i=rotate+%28-81%2C39%29+about+%28-80%2C+40%29+by+45+degrees# 
    
    expectedRx = -80.0
    expectedRy = 38.5857864376
    
    deltaX = math.fabs(expectedRx - rx)
    deltaY = math.fabs(expectedRy - ry)
    
    if ((deltaX > FAIL_DELTA_TOLERANCE) or (deltaY > FAIL_DELTA_TOLERANCE)) :
        raise Exception('Test Failed') 
                
    sx, sy = GeometryConverter.scale(2.0, x0, y0, x1, y1)
    print "Scale: ", sx, sy
        
    expectedSx = -82.0
    expectedSy = 38.0
    
    deltaX = math.fabs(expectedSx - sx)
    deltaY = math.fabs(expectedSy - sy)        
 
    if ((deltaX > FAIL_DELTA_TOLERANCE) or (deltaY > FAIL_DELTA_TOLERANCE)) :
        raise Exception('Test Failed') 
        

def RunTests() :
    
    TestRotateAndScale()
    TestGeoConversionProblem()
    
try:

    print("Starting Test: TestGeometryConverter")    
        
    # load this library not in the local dir or pythonpath, so we can test it:   
    # assumes it is run from the current dir & exists at this relative location  
    sys.path.append('../../../toolboxes/scripts') 
    import MilitaryUtilities
    
    geometryConverter = MilitaryUtilities.geoConverter
    
    RunTests()
            
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
