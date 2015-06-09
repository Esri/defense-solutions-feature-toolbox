# ----------------------------------------------------------------------------------
# Copyright 2013-2015 Esri
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
# ----------------------------------------------------------------------------------
# CalculateSIDCFieldCharlie.py
# Description: Sets the SIDC field from the Representation Rule name 
# Requirements: ArcGIS Desktop Standard
# ----------------------------------------------------------------------------------

import arcpy
import os
import sys
import traceback

import SymbolUtilities

### Params:
### 0 - inputFC
### 1 - sidc field
### 2 - echeclon field
### 3 - affiliation ("NOT_SET", "FRIENDLY", "HOSTILE", "NEUTRAL", "UNKNOWN")
def calculateSidcFieldCharlie() : 

	try :

		arcpy.AddMessage('Starting: CalculateSIDCFieldCharlie')

		currentPath = os.path.dirname(__file__)
		defaultDataPath = os.path.normpath(os.path.join(currentPath, \
			r'../../../data/mil2525d/testdata/geodatabases/'))

		# Get input feature class
		inputFC = arcpy.GetParameter(0)
		if (inputFC is "" or inputFC is None):
			inputFC = os.path.normpath(os.path.join(defaultDataPath, \
				r'test_inputs.gdb\FriendlyOperations\FriendlyEquipment'))

		if not arcpy.Exists(inputFC) :
			msg = "Input Dataset does not exist: " + str(inputFC) + " - exiting"
			arcpy.AddError(msg)
			return

		desc = arcpy.Describe(inputFC)

		# Get SIDC Field
		SIDCField = arcpy.GetParameterAsText(1)
		if (SIDCField == "" or SIDCField is None):
			SIDCField = "sidc"

		standard = arcpy.GetParameterAsText(2)
		symbolDictionary = SymbolUtilities.SymbolLookupCharlie(standard)

		# Get Echelon field (to be used to determine echelon attribute)
		EchelonField = arcpy.GetParameterAsText(3)
		if (EchelonField == "" or EchelonField is None):
			EchelonField = "echelon"

		# Used to infer the affiliation from the dataset name
		dataPath = desc.path
		datasetName = desc.name

		# Get affiliation, needed because SIDC affiliation cannot always be derived from feature attributes
		affiliation = arcpy.GetParameterAsText(4)
		if (not (affiliation == "")) and (not affiliation is None) and \
			(not affiliation in SymbolUtilities.SymbolLookupCharlie.validAffiliations) :
			if (affiliation != "NOT_SET") :
				msg = "ValidAffiliations are " + str(SymbolUtilities.SymbolLookupCharlie.validAffiliations)
				arcpy.AddWarning(msg)
				affiliation = ""

		if (affiliation == "") or (affiliation is None) or (affiliation == "NOT_SET") :
			affiliation = ""
			# TRICKY: Military Features did not have an "affiliation" attribute 
			# so we need to try to derive from the feature class name
			# If not set, then try to derive from the feature class name 
			# This will work with the default Military Features lpk/FGDB layers (except METOC), 
			# but perhaps not others that use a different convention
			dataPathUpper = dataPath.upper()
			datasetNameUpper = datasetName.upper()
			if SymbolUtilities.SymbolLookupCharlie.FRIENDLY_AFFILIATION in dataPathUpper \
				or SymbolUtilities.SymbolLookupCharlie.FRIENDLY_AFFILIATION in datasetNameUpper :
				affiliation = SymbolUtilities.SymbolLookupCharlie.FRIENDLY_AFFILIATION
			elif SymbolUtilities.SymbolLookupCharlie.HOSTILE_AFFILIATION in dataPathUpper \
				or SymbolUtilities.SymbolLookupCharlie.HOSTILE_AFFILIATION in datasetNameUpper :
				affiliation = SymbolUtilities.SymbolLookupCharlie.HOSTILE_AFFILIATION
			elif SymbolUtilities.SymbolLookupCharlie.NEUTRAL_AFFILIATION in dataPathUpper \
				or SymbolUtilities.SymbolLookupCharlie.NEUTRAL_AFFILIATION in datasetNameUpper :
				affiliation = SymbolUtilities.SymbolLookupCharlie.NEUTRAL_AFFILIATION
			elif SymbolUtilities.SymbolLookupCharlie.UNKNOWN_AFFILIATION in dataPathUpper \
				or SymbolUtilities.SymbolLookupCharlie.UNKNOWN_AFFILIATION in datasetNameUpper :
				affiliation = SymbolUtilities.SymbolLookupCharlie.UNKNOWN_AFFILIATION

			if (affiliation is "") or (affiliation is None) :
				# default to Friendly, if still not set            
				arcpy.AddWarning("WARNING: could not determine affiliation, defaulting to " + \
							   SymbolUtilities.SymbolLookupCharlie.FRIENDLY_AFFILIATION)
				affiliation = SymbolUtilities.SymbolLookupCharlie.FRIENDLY_AFFILIATION

		##Print Settings
		arcpy.AddMessage('Running with Parameters:')
		arcpy.AddMessage('0 - Input Military Feature Class: ' + str(inputFC))
		arcpy.AddMessage('1 - SIDC Field: ' + str(SIDCField))
		arcpy.AddMessage('2 - Standard: ' + standard)
		arcpy.AddMessage('3 - EchelonField: ' + str(EchelonField))
		arcpy.AddMessage('4 - Affiliation: ' + str(affiliation))

		##Calculation Code

		#Get Symbol Field Name
		fieldNameList = []
		for field in desc.Fields:
			fieldNameList.append(field.name)

		updatefields = []

		if (SIDCField in fieldNameList) :
			updatefields.append(SIDCField)
		else :
			arcpy.AddError("SIDC Field not found: " + SIDCField)
			return

		CODE_FIELD_NAME = "code"
		DESCRIPTION_FIELD_NAME = "description"
		fieldNameToDomainName = {}

		# Many different fields used for this rep rule field
		symbolNameFieldName = "symbolname"
		if ("ruleid" in fieldNameList):
			symbolNameFieldName = "ruleid"
		elif ("symbol_id" in fieldNameList):
			symbolNameFieldName = "Symbol_ID"
		elif ("symbolrule" in fieldNameList):
			symbolNameFieldName = "symbolrule"
		updatefields.append(symbolNameFieldName)

		if (EchelonField in fieldNameList):
			updatefields.append(EchelonField)

		# Strip off any FeatureDatasets from the GDB name (TODO: this only works with GDBs for now)
		gdbPath = dataPath.split(".gdb")[0]
		gdbPath += ".gdb"

		for field in desc.Fields:
			if field.name in updatefields:
				# Get domain if any
				if (field.domain is not None and field.domain != ""):
					fieldNameToDomainName[field.name] = field.domain
					if arcpy.Exists("in_memory/" + field.domain):
						arcpy.Delete_management("in_memory/" + field.domain)
					try:
						arcpy.DomainToTable_management(gdbPath, field.domain, \
													   "in_memory/" + field.domain, \
													   CODE_FIELD_NAME, DESCRIPTION_FIELD_NAME)
					except:
						arcpy.AddError('Could not export domain: ' + field.domain + \
							' from ' + gdbPath + ' - tool may not behave as expected.')


		with arcpy.da.UpdateCursor(inputFC, updatefields) as cursor:

			featureCount = 0

			for row in cursor:

				featureCount += 1
				arcpy.AddMessage('Processing feature: ' + str(featureCount))

				echelonString = ""
				symbolname = "NOT_FOUND_IN_REP_RULE"

				if (symbolNameFieldName in fieldNameToDomainName):
					domain = fieldNameToDomainName[symbolNameFieldName]
					symbolRuleCode = row[1]
					if symbolRuleCode is None :
						arcpy.AddError('Symbol Rule is Null, setting to default symbol')
					else : 
						whereClause = "%s = %s" % (CODE_FIELD_NAME, symbolRuleCode)
						domainRows = arcpy.gp.SearchCursor("in_memory/" + domain, whereClause)
						for domainRow in domainRows:
							symbolname = domainRow.getValue(DESCRIPTION_FIELD_NAME)
							break

				echelonString = '0'				
				if (EchelonField in updatefields) and (EchelonField in fieldNameToDomainName) :
					echelonValue = row[2]
					if echelonValue is not None:
						domain = fieldNameToDomainName[EchelonField]
						if not echelonString is None :
							whereClause = "%s = %s" % (CODE_FIELD_NAME, echelonValue)
							domainRows = arcpy.SearchCursor("in_memory/" + domain, whereClause)
							for domainRow in domainRows:
								echelonString = domainRow.getValue(DESCRIPTION_FIELD_NAME)
								echelonString = echelonString.upper()
								break

				expectedGeometry = SymbolUtilities.SymbolLookupCharlie.getGeometryStringFromShapeType(desc.shapeType)

				sidc = symbolDictionary.SymbolNametoSymbolIDExt(symbolname, echelonString, affiliation, expectedGeometry)
				validSic = symbolDictionary.isValidSidc(sidc)

				if not validSic :
					# this should not happen, but final check
					defaultSidc = SymbolUtilities.SymbolLookupCharlie.getDefaultSidcForShapeType(desc.shapeType)
					print("Invalid Sic Code: " + sidc + ", using default: " + defaultSidc)
					sidc = defaultSidc

				row[0] = sidc

				# update the feature
				cursor.updateRow(row)

		# Set output
		arcpy.SetParameter(5, inputFC)            

	except arcpy.ExecuteError: 
		# Get the tool error messages 
		msgs = arcpy.GetMessages() 
		arcpy.AddError(msgs) 

	except:
		# Get the traceback object
		tb = sys.exc_info()[2]
		tbinfo = traceback.format_tb(tb)[0]

		# Concatenate information together concerning the error into a message string
		pymsg = "PYTHON ERRORS:\nTraceback info:\n" + tbinfo + "\nError Info:\n" + str(sys.exc_info()[1])
		msgs = "ArcPy ERRORS:\n" + arcpy.GetMessages() + "\n"

		# Return python error messages for use in script tool or Python Window
		arcpy.AddError(pymsg)
		arcpy.AddError(msgs)

if __name__ == '__main__':
	calculateSidcFieldCharlie()