#----------------------------------------------------------------------------------
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
#----------------------------------------------------------------------------------
# CalculateAttributesFromSidc.py
# Description: Calculates Symbol Display Attributes from an SIDC Field
# Requirements: ArcGIS Desktop
#----------------------------------------------------------------------------------

import arcpy
import os
import traceback

import SymbolUtilities

def symbolIdCodeToAttributesToCode(code) :

	attributes = {}

	if code is None : 
		print("Code is empty, can't continue")
		return

	sidc = SymbolUtilities.SymbolIdCodeDelta()
	sidc.full_code = code

	if not sidc.is_valid() : 
		print("Can't convert to attributes, SIDC not valid: " + code)
		return attributes

	attributes['context']              = sidc.real_exercise_sim
	attributes['identity']             = sidc.affiliation
	attributes['symbolset']            = sidc.symbol_set
	attributes['operationalcondition'] = sidc.status  
	attributes['indicator']            = sidc.hq_tf_fd

	# Tricky : several attribute used for same sidc field so we need to set them all
	attributes['echelon']  = sidc.echelon_mobility

	# TODO: These are set to length=1 in old MilitaryFeatures 
	# so cause a conflict between the 2 schemas...
	# attributes['mobility'] = sidc.echelon_mobility
	# attributes['array']    = sidc.echelon_mobility

	attributes['entity']    = sidc.entity_code
	attributes['modifier1'] = sidc.modifier1
	attributes['modifier2'] = sidc.modifier2

	return attributes

### calculateAttributesFromSidcField - Calculates symbol display attributes from 
### a Symbol ID Code field for a Military Feature Class
###
### Params:
### 0 - input_feature_class (FeatureClass) - input Military Feature Class
### 1 - sidc_field (String) - field to store SIDC value
###
def calculateAttributesFromSidcField() :

	feature, features = None, None

	try :
		arcpy.AddMessage('Starting: CalculateSidcField')

		currentPath = os.path.dirname(__file__)
		defaultDataPath = os.path.normpath(os.path.join(currentPath, \
			r'../../../data/mil2525d/testdata/geodatabases/'))

		# 0 : Get input feature class
		inputFC = arcpy.GetParameter(0)
		if (inputFC == '') or (inputFC is None):
			inputFC = os.path.normpath(os.path.join(defaultDataPath, \
				# r'PairwiseTestData.gdb/MilitaryFeatures/Air'))		
				r'engagementarea.gdb/DirectFireWeapons/DirectFire_FriendlyEquipment'))					

		try : 
			desc = arcpy.Describe(inputFC)
		except :
			desc = None

		if desc == None :
			arcpy.AddError('Could not read Input Feature Class: ' + str(inputFC))
			return

		# 1: sidcField
		sidcField = arcpy.GetParameterAsText(1)

		if (sidcField == '') or (sidcField is None):
			# just pick a known good text field for testing if none supplied
			sidcField = 'SymbolIdDelta' # 'staffcomment' 

		arcpy.AddMessage('Running with Parameters:')
		arcpy.AddMessage('0 - Input Military Feature Class: ' + str(inputFC))
		arcpy.AddMessage('1 - SIDC Field: ' + sidcField)

		# Split this up into fields we *must* have (required) & those that are optional
		# (we will fail this tool if the require ones aren't there)
		REQUIRED_FIELDS = ['identity', 'symbolset', 'entity']
		OPTIONAL_FIELDS = ['context', 'modifier1', 'modifier2', 'echelon', \
			'mobility', 'array', 'indicator', 'operationalcondition' ]

		SYMBOL_ID_FIELD_LIST = REQUIRED_FIELDS + OPTIONAL_FIELDS
		SYMBOL_ID_FIELD_LIST.append(sidcField)

		# Get a list of available feature class fields (we use this in a few places)
		fieldNameList = []

		for field in desc.Fields:
			fieldNameList.append(field.name)
			if field.name == sidcField :
				if field.type != 'String' :
					arcpy.AddError('SIDC Field: ' + sidcField + ' is not of type string/text, type: ' + field.type)
					return

		if not (sidcField in fieldNameList) : 
			arcpy.AddError('Could not find field: ' + sidcField)
			return

		# Yes Python let's me write a statement like this:
		allRequiredFieldsPresent = len([x for x in REQUIRED_FIELDS if x in fieldNameList])\
					 == len(REQUIRED_FIELDS)

		if not allRequiredFieldsPresent :
			arcpy.AddError('Could not find required MIL-2525D(Delta) fields')
			return

		# Open an update cursor (if possible)
		try :
			fieldNameListAsString = ','.join(fieldNameList) # Change into format expected by UpdateCursor
			features = arcpy.gp.UpdateCursor(inputFC, '', None, fieldNameListAsString) 
		except Exception as err: 
			arcpy.AddError('Could not open Input Feature Class ' + str(inputFC))
			arcpy.AddError(traceback.format_exception_only(type(err), err)[0].rstrip())   
			return

		if features is None :
			arcpy.AddError('No available features in Input Feature Class ' + str(inputFC))
			return

		featureCount = 0

		for feature in features : 

			featureCount += 1
			arcpy.AddMessage('Processing feature/message: ' + str(featureCount))

			symbolId = feature.getValue(sidcField)

			if symbolId is None:
				arcpy.AddWarning('Skipping - SIDC in NULL for feature')
				continue

			attributes = symbolIdCodeToAttributesToCode(symbolId)

			if len(attributes) == 0 :
				arcpy.AddWarning('Skipping - could not convert SIDC to attributes: ' + symbolId)
				continue
			
			# Now set these calculated attributes in the feature
			for attribute in attributes:
				if attribute in fieldNameList :
					try : 
						# arcpy.AddMessage("Setting attribute: " + attribute + ' : ' + attributes[attribute])
						feature.setValue(attribute, attributes[attribute])
					except :
						arcpy.AddWarning('Could not set feature value for field: ' + attribute)

			try : 
				# update the feature
				features.updateRow(feature)
			except :
				arcpy.AddError('Could not update feature (probably a schema conflict)')

	except Exception as err: 
		arcpy.AddError(traceback.format_exception_only(type(err), err)[0].rstrip())

	finally :
		if feature : 
			del feature

		if features : 
			del features

if __name__ == '__main__':
	calculateAttributesFromSidcField()
