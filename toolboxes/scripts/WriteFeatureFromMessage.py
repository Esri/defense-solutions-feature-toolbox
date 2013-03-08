import arcpy
import MessageIterator
import MilitaryUtilities
import GeometryConverter
import DictionaryConstants
import os.path
import traceback

### Params:
### 0 - inputXMLFileName
### 1 - outputFeatureClass

def writeFeaturesFromMessageFile() :

    # Get the input message file
    inputFileName = arcpy.GetParameterAsText(0)
    if (inputFileName is "" or inputFileName is None):
        inputFileName = "C:/DefenseTemplates/MilitaryFeatures/Utilities/MilitaryFeatureConverters/TestData/Mil2525CMessages.xml"

    if not os.path.isfile(inputFileName) :
        arcpy.AddError("Bad Input")
        return

    inputFile=open(inputFileName, "r")

    # Get the output feature class
    outputFC = arcpy.GetParameter(1)
    if (outputFC is "" or outputFC is None):
        outputFC = "C:/DefenseTemplates/MilitaryFeatures/Utilities/MilitaryFeatureConverters/TestData/Default.gdb/FriendlyOperationsL_Test_3857" 
    desc = arcpy.Describe(outputFC)
    if desc == None :
        print "Bad Output Dataset" + outputFC 
        return

    shapeType = desc.shapeType;

    print "Exporting message objects from: " + str(inputFileName)
    print "To Feature Class: " + str(outputFC)
    print "That match shape type: " + shapeType

    ruleFieldName = MilitaryUtilities.symbolDictionary.initializeRulesByMilitaryFeatures(outputFC) 

    if (ruleFieldName == "") or (ruleFieldName is None) :
        arcpy.AddError("RuleFieldName not found, exiting")
        return

    # Projected or geographic?
    xname = "lon"
    yname = "lat"
    isProjected = desc.spatialReference.type == "Projected"
    if (isProjected):
        xname = "x"
        yname = "y"
    outputWkid = desc.spatialReference.factoryCode

    ################Begin Export ##########################
    
    featureFields = desc.fields

    # Iterate through the messages and check the shape
    WRITE_OUTPUT = True # debug switch when output not needed
    newRow = None
    newRows = None

    try : 

        if WRITE_OUTPUT : 
            newRows = arcpy.InsertCursor(outputFC)
        messageCount = 0

        # for each message in the message file, get its attributes and copy to the output FeatureClass
        for sic, controlPoints, attributes in MessageIterator.MessageIterator(inputFileName) :
            print sic, controlPoints, attributes

            geoType = MilitaryUtilities.geoConverter.expectedGeometryType(sic)
            if not DictionaryConstants.isCorrectShapeTypeForFeature(geoType, shapeType) : 
                skipMsg = "Skipping SIC: " + sic + " - does not match feature type" + shapeType
                arcpy.AddMessage(skipMsg)
                continue

            # Used those SIC that map to 2 lines (ex. Task Screen/Guard/Cover)
            repeatForPairFeatures = True
            repeatCount = 0

            while repeatForPairFeatures :

                outputPointList, conversionNotes = MilitaryUtilities.geoConverter.controlPointsToGeometry(sic, controlPoints, attributes)
                if outputPointList == None :
                    msg = "Failed to Convert Points from Military to MilFeature format for SIDC: " + sic
                    arcpy.AddError(msg)
                    arcpy.AddError("Conversion Notes: " + conversionNotes)
                    repeatForPairFeatures = False
                    continue

                inputWkid = 0
                if attributes.has_key(DictionaryConstants.Tag_Wkid) :
                    inputWkid = int(attributes[DictionaryConstants.Tag_Wkid])

                if outputWkid != inputWkid :
                    msg = "ERROR: Input Message and Output Feature WKIDs do not match (InsertFeature will fail)"
                    arcpy.AddError(msg)
                    msg = "Output WKID = " + str(outputWkid) + " , Input WKID = " + str(inputWkid)
                    arcpy.AddError(msg)

                ruleId, symbolName = MilitaryUtilities.symbolDictionary.symbolIdToRuleId(sic)

                if ruleId < 0 :
                    arcpy.AddWarning("WARNING: Could not map ruleId to SIDC: " + sic)

                # For those SIC that map to 2 lines (ex. Task Screen/Guard/Cover)
                # will need to clone/repeat the message here for Left/Right Upper/Lower pair
                repeatForPairFeatures = False 
                geoConversion = MilitaryUtilities.symbolDictionary.symbolIdToGeometryConversionType(sic)
                if (geoConversion == DictionaryConstants.GCT_TWOLINE) or \
                    (geoConversion == DictionaryConstants.GCT_TWOLINE3OR4PT) :
                    if repeatCount > 0 : 
                        repeatForPairFeatures = False # Only do once
                        ## TODO: find better way to set rule Id for 2nd line (Left/Right) version
                        # This is quite kludgy, and relies on the 2nd ruleid code being the 1st + 1
                        # and this may not always be the case
                        ruleId = ruleId + 1
                    else : 
                        repeatForPairFeatures = True 
                        attributes[DictionaryConstants.Tag_TwoLinesNeeded] = "True"
                        # don't let id get repeated, so append "_2"
                        if attributes.has_key(DictionaryConstants.Tag_Id) : 
                            attributes[DictionaryConstants.Tag_Id] = attributes[DictionaryConstants.Tag_Id] + "_2"
                repeatCount = repeatCount + 1

                arcpy.AddMessage("Adding feature #" + str(messageCount) + " with SIDC: " + sic)
                if WRITE_OUTPUT : 
                    try : 
                        shape = MilitaryUtilities.pointsToArcPyGeometry(outputPointList, shapeType)
                        newRow = newRows.newRow()
                        newRow.setValue(desc.shapeFieldName, shape)
                        newRow.setValue(ruleFieldName, ruleId)
                        newRow.setValue("sidc", sic)
                        # add any extra fields
                        for field in featureFields :  
                            if not (field.name in DictionaryConstants.MILFEATURES_FIELD_EXCLUDE_LIST) :
                                lowerFieldName = field.name.lower()
                                # we don't the case of the attribute so have to search
                                for key in attributes.keys() :                                     
                                    lowerKey = key.lower() 
                                    if (lowerKey == lowerFieldName) :
                                        try : 
                                            newRow.setValue(field.name, attributes[key])
                                        except : 
                                            print "Could not add: Field: " + field.name + ", Value: " + str(attributes[key])

                        newRows.insertRow(newRow) 
                        arcpy.AddMessage("Message successfully added: " + str(messageCount))
                    except : 
                        arcpy.AddError("ERROR: Exception while adding new feature (does Spatial Ref match?)")
                        tb = traceback.format_exc()
                        print tb
                else :
                    print "WRITING OUTPUT:"
                    print "SIC: " + sic + ", Name: " + symbolName                
                    print "Adding geometry to feature, with points: "
                    for point in outputPointList : 
                        x = point.split(',')[0]
                        y = point.split(',')[1]
                        print "(", x, ",", y, ")"                                     
                
            messageCount += 1

    except :
        print "Exception: " 
        tb = traceback.format_exc()
        print tb

    finally :
        # Delete cursor and row objects to remove locks on the data 
        if not newRow is None : 
            del newRow 
        if not newRows is None : 
            del newRows


