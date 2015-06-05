# ----------------------------------------------------------------------------------
# Copyright 2015 Esri
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
# Calculate2525DeltaSidcFromCharlieSidc.py
# Description: GP Tool to Calculate 2525D SIDC from 2525C SIDC
# Requirements: ArcGIS Desktop
# ----------------------------------------------------------------------------------

import arcpy
import os
import traceback

import SymbolUtilities

### calculate2525DeltaSidcFromCharlieSidc - Calculates a Symbol ID Code field from the previous 
###     version (Charlie) to the current version (Delta)
###
### Params:
### 0 - input_feature_class (FeatureClass) - input Military Feature Class
### 1 - sidc_field_2525_C(Charlie) (String) - field to read 2525 C(Charlie) SIDC value
### 2 - sidc_field_2525_D(Delta) (String) - field to store 2525 D(Delta) SIDC value
### 3 - conversion_remarks_field (optional) - reason code could not be converted if applicable
###
def calculate2525DeltaSidcFromCharlieSidc() :

	feature, features = None, None

	try :
		arcpy.AddMessage('Starting: Calculate2525DeltaSidcFromCharlieSidc')

		currentPath = os.path.dirname(__file__)
		defaultDataPath = os.path.normpath(os.path.join(currentPath, \
			r'../../../data/mil2525d/testdata/geodatabases/'))

		# 0 : Get input feature class
		inputFC = arcpy.GetParameter(0)
		if (inputFC == '') or (inputFC is None):
			inputFC = os.path.normpath(os.path.join(defaultDataPath, \
				r'test_inputs.gdb/FriendlyOperations/FriendlyUnits'))

		try : 
			desc = arcpy.Describe(inputFC)
		except :
			desc = None

		if desc == None :
			arcpy.AddError('Could not read Input Feature Class: ' + str(inputFC))
			return

		# 1: sidc_field_2525_C
		sidcFieldCharlie = arcpy.GetParameterAsText(1)

		if (sidcFieldCharlie == '') or (sidcFieldCharlie is None):
			sidcFieldCharlie = 'sic'

		# 2: sidc_field_2525_D
		sidcFieldDelta = arcpy.GetParameterAsText(2)

		if (sidcFieldDelta == '') or (sidcFieldDelta is None):
			sidcFieldDelta = 'sidc'

		# 3 : conversion remarks (optional)
		conversionRemarksField = arcpy.GetParameterAsText(3)

		if (conversionRemarksField == '') or (conversionRemarksField is None):
			# TODO: for debug, remove for production
			conversionRemarksField = 'staffcomment'
			# conversionRemarksField = None		
		
		arcpy.AddMessage('Running with Parameters:')
		arcpy.AddMessage('0 - Input Military Feature Class: ' + str(inputFC))
		arcpy.AddMessage('1 - SIDC Field (Charlie): ' + sidcFieldCharlie)
		arcpy.AddMessage('2 - SIDC Field (Delta): ' + sidcFieldDelta)
		if not conversionRemarksField is None : 
			arcpy.AddMessage('3 - Conversion Remarks Field: ' + conversionRemarksField)

		# the list of fields we want to check exist & are of the correct type
		fieldNameToCheckList = [sidcFieldCharlie, sidcFieldDelta]
		if not conversionRemarksField is None:
			fieldNameToCheckList.append(conversionRemarksField)

		# the list of actual feature class fields found
		fieldNameList = []

		# Check for Text/String field type
		for field in desc.Fields:
			if field.name in fieldNameToCheckList:
				fieldNameList.append(field.name) # we only need these fields
				if field.type != 'String' : 
					arcpy.AddError('SIDC Field: ' + field.name + \
						' is not of type string/text, type: ' + \
						field.type)
					return

		# Check selected field names exist
		if not (sidcFieldCharlie in fieldNameList) : 
			arcpy.AddError('Could not find field: ' + sidcFieldCharlie)
			return

		if not (sidcFieldDelta in fieldNameList) : 
			arcpy.AddError('Could not find field: ' + sidcFieldDelta)
			return

		if not (conversionRemarksField is None) and not (conversionRemarksField in fieldNameList) : 
			arcpy.AddWarning('Could not find field: ' + conversionRemarksField)
			conversionRemarksField = None

		symbolLookup = SymbolUtilities.SymbolLookup()

		if not symbolLookup.initialized() : 
			arcpy.AddError('Could not load dependent data files from tooldata')

		# Open an update cursor (if possible)
		try :            
			fieldNameListAsString = ','.join(fieldNameList) # Change into format expected by UpdateCursor
			features = arcpy.gp.UpdateCursor(inputFC, '', None, fieldNameListAsString) 
		except Exception as err: 
			arcpy.AddError('Could not open Input Feature Class ' + str(inputFC))
			arcpy.AddError(traceback.format_exception_only(type(err), err)[0].rstrip())           
			return

		featureCount = 0

		for feature in features : 

			featureCount += 1
			arcpy.AddMessage('Processing feature/message: ' + str(featureCount))

			mil2525CharlieSidc = None
			try : 
				mil2525CharlieSidc = feature.getValue(sidcFieldCharlie)
			except :
				arcpy.AddWarning('Could not get feature value for field: ' + sidcFieldCharlie)
					
			if mil2525CharlieSidc is not None:

				symbolId = symbolLookup.getDeltaCodeFromCharlie(mil2525CharlieSidc)

				if not symbolId.is_valid() : 
					print("Could not convert 2525Charlie SIDC: " + mil2525CharlieSidc)
					continue
				
				symbolIdCodeDelta = symbolId.full_code # .human_readable_code
				conversionRemarks = symbolId.remarks

				try : 
					feature.setValue(sidcFieldDelta, symbolIdCodeDelta)

					if not (conversionRemarksField is None or conversionRemarks is None) :
						feature.setValue(conversionRemarksField, conversionRemarks)

					features.updateRow(feature)
				except :
					arcpy.AddError('Could not update feature value for field: ' + sidcFieldDelta)

	except Exception as err: 
		arcpy.AddError(traceback.format_exception_only(type(err), err)[0].rstrip())

	finally :
		if feature : 
			del feature

		if features : 
			del features

if __name__ == '__main__':
	calculate2525DeltaSidcFromCharlieSidc()
