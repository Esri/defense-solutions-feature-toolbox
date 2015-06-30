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
# TestSymbolUtilities.py
# Description: Unit Tests of SymbolUtilities Module
# Requirements: arcpy 2.7+
# ----------------------------------------------------------------------------------

import unittest
import sys

class Test_SymbolUtilities_SymbolLookup(unittest.TestCase):

	@classmethod
	def setUpClass(self):
		self.symbolLookup = SymbolUtilities.SymbolLookup()

	def test_deltaToCharlie(self) :

		self.assertIsNotNone(self.symbolLookup)

		# Bad ones
		# None
		sic2Check = None
		symbolIdCharlie, name, remarks = self.symbolLookup.getCharlieCodeFromDelta(sic2Check) 
		self.assertEqual(symbolIdCharlie, 'SUGPU----------')

		# Completely bad
		sic2Check = 'XXXXXXXX'
		symbolIdCharlie, name, remarks = self.symbolLookup.getCharlieCodeFromDelta(sic2Check) 
		self.assertEqual(symbolIdCharlie, 'SUGPU----------')

		# Bad length (must be 8 or 20)
		sic2Check = '1110000'
		symbolIdCharlie, name, remarks = self.symbolLookup.getCharlieCodeFromDelta(sic2Check) 
		self.assertEqual(symbolIdCharlie, 'SUGPU----------')

		# Bad Entity, but good symbol set
		sic2Check = '01999999'
		symbolIdCharlie, name, remarks = self.symbolLookup.getCharlieCodeFromDelta(sic2Check) 
		self.assertEqual(symbolIdCharlie, 'SUAP-----------')

		# Good ones
		sic2Check = '10031012111211000000'
		symbolIdCharlie, name, remarks = self.symbolLookup.getCharlieCodeFromDelta(sic2Check) 
		self.assertEqual(symbolIdCharlie, 'SFGAUCI---AA---')

		sic2Check = "10062500002815000000"
		symbolIdCharlie, name, remarks = self.symbolLookup.getCharlieCodeFromDelta(sic2Check) 
		self.assertEqual(symbolIdCharlie, 'GHMPNZ--------X')

		# Only 1 modifier matches
		sic2Check = "10030100001101020501"
		symbolIdCharlie, name, remarks = self.symbolLookup.getCharlieCodeFromDelta(sic2Check) 
		self.assertEqual(symbolIdCharlie, 'SFAPMFFI-------')

		# Only 1 modifier matches
		sic2Check = "10030100001101070103"
		symbolIdCharlie, name, remarks = self.symbolLookup.getCharlieCodeFromDelta(sic2Check) 
		self.assertEqual(symbolIdCharlie, 'SFAPMFCL-------')

		# No modifiers match
		sic2Check = "10030100001101079999"
		symbolIdCharlie, name, remarks = self.symbolLookup.getCharlieCodeFromDelta(sic2Check) 
		self.assertEqual(symbolIdCharlie, 'SFAPMFC--------')

		# Valid but not found(s)
		sic2Check = "10032500002605000000"
		symbolIdCharlie, name, remarks = self.symbolLookup.getCharlieCodeFromDelta(sic2Check) 
		self.assertEqual(symbolIdCharlie, 'SUGPU----------')

		sic2Check = '10060100001101050000'
		symbolIdCharlie, name, remarks = self.symbolLookup.getCharlieCodeFromDelta(sic2Check)
		self.assertEqual(symbolIdCharlie, 'SHAP-----------')

	def test_chalieToDelta(self) :

		self.assertIsNotNone(self.symbolLookup)

		# Bad ones
		# None
		mil2525CharlieSidc = None
		symbolId = self.symbolLookup.getDeltaCodeFromCharlie(mil2525CharlieSidc)
		self.assertEqual(symbolId.is_valid(), False)

		# Bad ones
		# Really Bad
		mil2525CharlieSidc = 'XXXXGLC-------X'
		expectedDeltaSidc  = '10032500001401030000'
		symbolId = self.symbolLookup.getDeltaCodeFromCharlie(mil2525CharlieSidc)
		self.assertEqual(symbolId.is_valid(), False)

		# Bad Length
		mil2525CharlieSidc = 'GFGPGLC--'
		expectedDeltaSidc  = '10032500001401030000'
		symbolId = self.symbolLookup.getDeltaCodeFromCharlie(mil2525CharlieSidc)
		self.assertEqual(symbolId.is_valid(), False)

		# Good ones
		mil2525CharlieSidc = 'GFGPGLC-------X'
		expectedDeltaSidc  = '10032500001401030000'
		symbolId = self.symbolLookup.getDeltaCodeFromCharlie(mil2525CharlieSidc)
		self.assertEqual(symbolId.full_code, expectedDeltaSidc)

		mil2525CharlieSidc = 'SFGAUCI---AAUSG'
		expectedDeltaSidc  = '10031012111211000000'
		symbolId = self.symbolLookup.getDeltaCodeFromCharlie(mil2525CharlieSidc)

		self.assertEqual(symbolId.affiliation, '3')
		self.assertEqual(symbolId.symbol_set, '10')
		self.assertEqual(symbolId.full_code, expectedDeltaSidc)

		print(symbolId)
		print('Human Readable Code: ' + symbolId.human_readable_code)
		print('Regular Code: ' + symbolId.full_code)
		print('Short Code: ' + symbolId.short_code)
		print('Name: ' + symbolId.name)
		print('Remarks: ' + symbolId.remarks)

class Test_SymbolUtilities_SymbolLookupCharlie(unittest.TestCase):

	@classmethod
	def setUpClass(self):
		self.symbolLookup = SymbolUtilities.SymbolLookupCharlie('mil2525c')

	def test_nameToSidc(self) :

		self.assertIsNotNone(self.symbolLookup)

		name2Check = "Aim Point H"
		expectedSic = "GHGPGPWA------X"

		sic = self.symbolLookup.SymbolNametoSymbolID(name2Check) 

		print("Name: " + name2Check + ", returned SIC: " + sic)

		self.assertEqual(sic, expectedSic)

	def test_nameToSidcExt(self) :

		self.assertIsNotNone(self.symbolLookup)

		name2Check = "Limited Access Area"
		expectedSic = "GHGPGAY-------X"
		expectedGeometry = 'Area'
		echelonString = ''
		affiliation = 'HOSTILE'

		sic = self.symbolLookup.SymbolNametoSymbolIDExt(name2Check, echelonString, affiliation, expectedGeometry)

		print("Name: " + name2Check + ", returned SIC: " + sic)

		self.assertEqual(sic, expectedSic)   

	def test_sidcToName(self) :

		self.assertIsNotNone(self.symbolLookup)

		sic2Check = "GHMPOGL-----USG"    
		expectedName = "General Obstacle Line H"
		expectedGeoType = "Line"
		name = self.symbolLookup.symbolIdToName(sic2Check)    
		geoType = self.symbolLookup.symbolIdToGeometryType(sic2Check)
		print("SIC: " + sic2Check + ", returned Name: " + name + ", GeoType: " + geoType)
		self.assertEqual(name, expectedName)
		self.assertEqual(geoType, expectedGeoType)

if __name__ == '__main__':

	print("Starting Test: TestSymbolDictionary")

	# load this library not in the local dir or pythonpath, so we can test it:   
	# assumes it is run from the current dir & exists at this relative location  
	sys.path.append(r'../../../toolboxes/mil2525d/scripts') 

	import SymbolUtilities

	unittest.main()









