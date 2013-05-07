#----------------------------------------------------------------------------------
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
#----------------------------------------------------------------------------------
# WriteMessageFile.py
# Description: Converts Military Feature Class to XML file
# Requirements: ArcGIS Desktop Standard
#----------------------------------------------------------------------------------

import arcpy
import DictionaryConstants
import MilitaryUtilities
import re
import os
import tempfile
import traceback
import uuid

### Params:
### 0 - inputFC
### 1 - outputXMLFile
### 2 - orderBy see: http://resources.arcgis.com/en/help/main/10.1/index.html#//018v00000050000000)
###     orderBy now called sort_fields (ex: sort_fields="STATE_NAME A; POP2000 D")

appendFile = False
DEBUG_GEOMETRY_CONVERSION = False # True # switch to bypass geometry conversion to keep feature at original placement

def writeMessageFile() :

    try :

        # Get input feature class
        inputFC = arcpy.GetParameter(0)
        if (inputFC is "" or inputFC is None):
            inputFC = os.path.join(MilitaryUtilities.geoDatabasePath, r"/test_inputs.gdb/FriendlyOperations/FriendlyUnits")
        desc = arcpy.Describe(inputFC)
        if desc == None :
            arcpy.AddError("Bad Input")
            return

        shapeType = desc.shapeType

        # Get output filename
        outputFile = arcpy.GetParameterAsText(1)
        if (outputFile is "") or (outputFile is None) :
            # For a standalone test (debug) if no output filename provided
            if DEBUG_GEOMETRY_CONVERSION : 
                defaultOutputName = "Mil2525CMessages-NoTransform.xml"
            else : 
                defaultOutputName = "Mil2525CMessages.xml"   
            outputFile = os.path.join(os.path.dirname(__file__), defaultOutputName)  
            messageFile=open(outputFile, "w")
            arcpy.AddWarning("No Output set, using default: " + str(outputFile))
        else:
            # appendFile = arcpy.GetParameterAsText(3)
            arcpy.AddMessage("Append File set to " + str(appendFile))
            if (not appendFile) or (not os.path.isfile(outputFile)) :
                messageFile = open(outputFile, "w")
            #if (appendFile is "") or (appendFile is None) or \
            #    (str(appendFile)=="false") or (not os.path.isfile(outputFile)) :
            #    messageFile = open(outputFile, "w")
            #    appendFile = False
            else :
                arcpy.AddMessage("Appending File...")
                # Appending the file is a bit more complicated because we have to remove the 
                # "</messages>" from the end of the original file therefore it can't just be 
                # opened as an append "a+"-we have to create a temp file, read the original file in,
                # except for the line "</messages>", and then write back out
                
                fileToAppend = open(outputFile, "r")
                # Note: didn't work in ArcCatalog unless I opened temp file this way
                temporaryFile = tempfile.NamedTemporaryFile(mode="w", delete=False)

                # Copy the file line by line, but don't include last </messages>
                while True:
                    line = fileToAppend.readline()
                    if line : 
                        if not "</messages>" in line :
                            temporaryFile.write(line)
                    else :
                        break

                # now write those lines back
                fileToAppend.close()
                temporaryFile.close()
                messageFile = open(outputFile, "w")
                temporaryFile = open(temporaryFile.name, "r")
                while True:
                    line = temporaryFile.readline()
                    if line : 
                        messageFile.write(line)
                    else :
                        break

                temporaryFile.close()

        if (messageFile is None) : 
            arcpy.AddError("Output file can't be created, exiting")
            return

        # Sort Order 
        orderBy = arcpy.GetParameterAsText(2)    

        ##################Setup for export############################
        # Densify if this is a polygon FC
        if ("Polygon" == shapeType):
            densifiedFC = "in_memory/DensifiedFC"
            arcpy.CopyFeatures_management(inputFC, densifiedFC)
            arcpy.Densify_edit(densifiedFC, "ANGLE", "", "", 10)
            inputFC = densifiedFC

        # Get fields and coded domains
        CODE_FIELD_NAME = "code"
        DESCRIPTION_FIELD_NAME = "description"
        fieldNameList = []
        fieldNameToDomainName = {}
        for field in desc.Fields:
            if not (field.name in DictionaryConstants.MILFEATURES_FIELD_EXCLUDE_LIST):
                fieldNameList.append(field.name)
                # Get domain if any
                if (field.domain is not None and field.domain != ""):
                    fieldNameToDomainName[field.name] = field.domain
                    dataPath = desc.path
                    gdbPath = dataPath.split(".gdb")[0]
                    gdbPath += ".gdb"
                    arcpy.DomainToTable_management(gdbPath, field.domain, "in_memory/" + field.domain, CODE_FIELD_NAME, DESCRIPTION_FIELD_NAME)
        print fieldNameList
        # Projected or geographic?
        xname = "lon"
        yname = "lat"
        isProjected = desc.spatialReference.type == "Projected"
        if (isProjected):
            xname = "x"
            yname = "y"
        wkid = desc.spatialReference.factoryCode

        ################Begin Export ##########################

        # Open a searchcursor
        rows = arcpy.SearchCursor(inputFC, "", "", "", orderBy)

        # Dictionary to map unique designation to ID
        unitDesignationToId = dict()
        
        featureFields = desc.fields

        ################Write XML file#########################

        if not appendFile :
            messageFile.write("<messages>\n")

        rowCount = 0

        # Iterate through the rows in the cursor
        for row in rows:
            shape = row.shape.getPart(0)
    
            uniqueId = str(rowCount)
            if (row.UniqueDesignation is not None):
                if (row.UniqueDesignation in unitDesignationToId):
                    uniqueId = unitDesignationToId[row.UniqueDesignation]
                else:
                    uniqueId = "{%s}" % str(uuid.uuid4())
                    unitDesignationToId[row.UniqueDesignation] = uniqueId
            else:
                uniqueId = "{%s}" % str(uuid.uuid4())

            # work with "sidc" or "sic"
            try : 
                SymbolIdCodeVal = row.getValue("sic")
            except:
                try : 
                    SymbolIdCodeVal = row.getValue("sidc")
                except:     
                    SymbolIdCodeVal = None                
                                  
            if SymbolIdCodeVal is None:
                msg =  "SIDC is not set - did you run CalcSIDCField first?"
                arcpy.AddError(msg)
                SymbolIdCodeVal = DictionaryConstants.getDefaultSidcForShapeType(shapeType)
            elif DEBUG_GEOMETRY_CONVERSION :
                print "Using Debug SIDC"
                SymbolIdCodeVal = DictionaryConstants.getDefaultSidcForShapeType(shapeType)

            # Note/Important: attributes need to be set in converter so needs declared before geometrytoControlPoints
            attributes = { } 
            conversionNotes = None
            attributes[DictionaryConstants.Tag_Wkid] = wkid  # needed by conversion

            controlPointsString = MilitaryUtilities.parseGeometryToControlPoints(shape)
            requiresConversion = MilitaryUtilities.geoConverter.requiresConversion(SymbolIdCodeVal)
            if requiresConversion and not DEBUG_GEOMETRY_CONVERSION :                
                msg = "SIC: " + SymbolIdCodeVal + " requires conversion/translation"
                print msg
                arcpy.AddMessage(msg)
                transformedPoints, conversionNotes = \
                    MilitaryUtilities.geoConverter.geometrytoControlPoints(SymbolIdCodeVal, controlPointsString, attributes)                
                if (conversionNotes == DictionaryConstants.CONVERSION_IGNORE_SECOND_LINE) : 
                    continue
                elif (transformedPoints is None) :
                    arcpy.AddError("Conversion FAILED" + conversionNotes)
                else :
                    controlPointsString = transformedPoints

            # Write Output Message
            messageFile.write("\t<message v=\"1.0\">\n")
            messageFile.write("\t\t<sic>%s</sic>\n" % SymbolIdCodeVal) 

            ##TODO: see if other types are valid in RuntimeSDK (besides just "position_report"/"update")
            messageFile.write("\t\t<_type>position_report</_type>\n")                        
            messageFile.write("\t\t<_action>update</_action>\n")
            messageFile.write("\t\t<_id>%s</_id>\n" % uniqueId) 
            messageFile.write("\t\t<_control_points>%s</_control_points>\n" % controlPointsString)  
            if not ((conversionNotes is None) or (conversionNotes is "")) : 
                messageFile.write("\t\t<ConversionNotes>%s</ConversionNotes>\n" % conversionNotes)

            # Note: written with attributes below: messageFile.write("\t\t<_wkid>%i</_wkid>\n" % wkid)
                     
            # Check on Military Geometries for Lines/Areas
            if (shapeType is "Point"):
                messageFile.write("\t\t<altitude_depth>%d</altitude_depth>\n" % shape.Z)

            rowCount = rowCount + 1
            messageFile.write("\t\t<MessageCount>%s</MessageCount>\n" % str(rowCount))             

            for key in attributes :
                attrValAsString = str(attributes[key])
                messageFile.write("\t\t<"+key+">" + attrValAsString + "</" + key + ">\n")

            ###################Common Fields/Attributes#####################
            for field in fieldNameList:
                try : 
                    rowVal = row.getValue(field)
                except :
                    print "Could not get row val for field" + field
                    rowVal = None
                if rowVal is not None:
                    fieldValAsString = str(row.getValue(field))
                    messageFile.write("\t\t<"+field+">" + fieldValAsString + "</" + field + ">\n")
            ###################Common Fields/Attributes#####################

            messageFile.write("\t</message>\n")

        messageFile.write("</messages>")

    except: 
        print "Exception: " 
        tb = traceback.format_exc()
        print tb
        arcpy.AddError("Exception")
        arcpy.AddError(tb)
