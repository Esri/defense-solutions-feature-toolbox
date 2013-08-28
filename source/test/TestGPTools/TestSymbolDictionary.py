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
# Description: Stand-alone unit test of SymbolDictionary class
# -----------------------------------------------------------------------------

import sys
import traceback
 
symbolDictionary = None

def TestSimpleDictionaryGeometryException() : 
    
    if (symbolDictionary is None) :
        raise Exception('Null SymbolDictionary') 
            
    sic = "GFGPOLAGS-****X"
    expectedGeometryConversionType = "GCT_ArrowWithOffset"
    actualGeometryConversionType = "Not done"
    
    actualGeometryConversionType = symbolDictionary.symbolIdToGeometryConversionType(sic)
    
    print "SIC: " + sic + ", returned Conversion: " + actualGeometryConversionType
       
    if (actualGeometryConversionType <> expectedGeometryConversionType) : 
        raise Exception('Test Failed')  
    
def TestNameSicMapping() :
    
    if (symbolDictionary is None) :
        raise Exception('Null SymbolDictionary')
    
    name2Check = "Aim Point H"
    expectedSic = "GHGPGPWA------X"
    
    sic = symbolDictionary.SymbolNametoSymbolID(name2Check) 
   
    print "Name: " + name2Check + ", returned SIC: " + sic

    if (sic <> expectedSic) :
        raise Exception('Test Failed')     

def TestNameSicMappingExt() :
    name2Check = "Limited Access Area"
    expectedSic = "GHGPGAY-------X"
    
    expectedGeometry = 'Area'
    echelonString = ''
    affiliation = 'HOSTILE'
    
    sic = symbolDictionary.SymbolNametoSymbolIDExt(name2Check, echelonString, affiliation, expectedGeometry)

    print "Name: " + name2Check + ", returned SIC: " + sic

    if (sic <> expectedSic) :
        raise Exception('Test Failed')   

def TestSicNameMapping() :
        
    if (symbolDictionary is None) :
        raise Exception('Null SymbolDictionary') 
        
    # SIC: GHMPOGL-----USG, returned Name: General Obstacle Line, GeoType: Line
    sic2Check = "GHMPOGL-----USG"    
    expectedName = "General Obstacle Line"
    expectedGeoType = "Line"
    
    name = symbolDictionary.symbolIdToName(sic2Check)    
    geoType = symbolDictionary.symbolIdToGeometryType(sic2Check)
    
    print "SIC: " + sic2Check + ", returned Name: " + name + ", GeoType: " + geoType
    
    if not ((name == expectedName) and (geoType == expectedGeoType)) :
        raise Exception('Test Failed') 
    
def TestSicNameMappingApp6() :     

    if (symbolDictionary is None) :
        raise Exception('Null SymbolDictionary')
    
    name2Check = "DIP Position F"
    expectedSic = "GFCPOXSD-------"
    
    sic = symbolDictionary.SymbolNametoSymbolID(name2Check) 
   
    print "Name: " + name2Check + ", returned SIC: " + sic

    if (sic <> expectedSic) :
        raise Exception('Test Failed')     
    
def TestNameSicMappingApp6() :
    
    if (symbolDictionary is None) :
        raise Exception('Null SymbolDictionary')
    
    name2Check = "Recruitment (coerced impressed)"
    expectedSic = "GFOPPU---------"
    
    sic = symbolDictionary.SymbolNametoSymbolID(name2Check) 
   
    print "Name: " + name2Check + ", returned SIC: " + sic

    if (sic <> expectedSic) :
        raise Exception('Test Failed')                
       
def CustomTest():
    if (symbolDictionary is None) :
        raise Exception('Null SymbolDictionary')
    
    print "Put your own custom test / reproducer here"          
    
def RunTests2525() :
    
    global symbolDictionary
    
    MilitaryUtilities.setSymbologyStandard("2525")
    symbolDictionary = MilitaryUtilities.getSymbolDictionary()
    
    if symbolDictionary is None : 
        print "Could not create SymbolDictionary"
        raise Exception('Test Failed') 
        
    # For testing a particular bug:
    # CustomTest()
    
    TestSicNameMapping()
    TestNameSicMapping()
    TestNameSicMappingExt()
    TestSimpleDictionaryGeometryException()

def RunTestsAPP6() :
    
    global symbolDictionary
    
    MilitaryUtilities.symbolDictionary = None # reset this for test
    
    MilitaryUtilities.setSymbologyStandard("APP6")
    symbolDictionary = MilitaryUtilities.getSymbolDictionary()
    
    if symbolDictionary is None : 
        print "Could not create SymbolDictionary"
        raise Exception('Test Failed') 
    
    if symbolDictionary.getSymbologyStandard() <> "APP6" : 
        print "Could not create APP6 SymbolDictionary"
        raise Exception('Test Failed')       
        
    # For testing a particular bug:
    # CustomTest()
    
    TestSicNameMappingApp6()
    TestNameSicMappingApp6()
    
try:

    print("Starting Test: TestSymbolDictionary")    
        
    # load this library not in the local dir or pythonpath, so we can test it:   
    # assumes it is run from the current dir & exists at this relative location  
    sys.path.append('../../../toolboxes/scripts') 
    import MilitaryUtilities
        
    RunTests2525()
    RunTestsAPP6()
            
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
