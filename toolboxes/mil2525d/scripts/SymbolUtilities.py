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
# SymbolUtilities.py
# Description: Military Feature Symbol Utilities
# Requirements: ArcGIS Desktop
# ----------------------------------------------------------------------------------

import csv
import os
import re
import sqlite3
import sys

class SymbolIdCodeDelta(object) :

	INVALID_FULL_CODE = 'INVALID'
	NOT_SET = 'NOT SET'

	def __init__(self) :

		self.version           = '10'
		self.affiliation       = '1'
		self.real_exercise_sim = '0'
		self.symbol_set  = '00'
		self.status      = '0'
		self.hq_tf_fd    = '0'
		self.echelon_mobility = '00'
		self.entity_code = '000000'
		self.modifier1   = '00'
		self.modifier2   = '00' 

		self.name    = SymbolIdCodeDelta.NOT_SET
		self.remarks = SymbolIdCodeDelta.NOT_SET

		self.full_code = SymbolIdCodeDelta.INVALID_FULL_CODE

	@staticmethod
	def left_zero_pad(string_in, required_length):
		 return string_in.zfill(required_length)

	def is_valid(self):
		return self.symbol_set != '00'

	@property
	def name(self):
		return self.__name

	@name.setter
	def name(self, name):
		self.__name = name

	@property
	def remarks(self):
		return self.__remarks

	@remarks.setter
	def remarks(self, remarks):
		self.__remarks = remarks

	# full_code - to set/get the full code all at once
	@property
	def full_code(self):

		if self.__full_code == SymbolIdCodeDelta.INVALID_FULL_CODE :
			self.populate_code_from_properties()

		return self.__full_code

	@full_code.setter
	def full_code(self, full_code):
		self.__full_code = str(full_code)
		if (full_code != SymbolIdCodeDelta.INVALID_FULL_CODE) :
			self.populate_properties_from_code()

	#####################################################
	# 2525D: A.5.2.1  Set A - First ten digits 

	# version (Digits 1 and 2) 
	@property
	def version(self):
		return self.__version

	@version.setter
	def version(self, version):
		REQUIRED_LENGTH = 2
		if len(version) > REQUIRED_LENGTH :
			return
		self.__version = SymbolIdCodeDelta.left_zero_pad(version, REQUIRED_LENGTH)

	# real_exercise_sim (Digit 3)
	@property
	def real_exercise_sim(self):
		return self.__real_exercise_sim

	@real_exercise_sim.setter
	def real_exercise_sim(self, real_exercise_sim):
		REQUIRED_LENGTH = 1
		if len(real_exercise_sim) > REQUIRED_LENGTH :
			return
		self.__real_exercise_sim = real_exercise_sim

	# affiliation (Digit 4)
	@property
	def affiliation(self):
		return self.__affiliation

	@affiliation.setter
	def affiliation(self, affiliation):
		REQUIRED_LENGTH = 1
		if len(affiliation) > REQUIRED_LENGTH :
			return
		self.__affiliation = affiliation

	# symbol_set (Digits 5 and 6)
	@property
	def symbol_set(self):
		return self.__symbol_set

	@symbol_set.setter
	def symbol_set(self, symbol_set):
		REQUIRED_LENGTH = 2
		if len(symbol_set) > REQUIRED_LENGTH :
			return
		self.__symbol_set = SymbolIdCodeDelta.left_zero_pad(symbol_set, REQUIRED_LENGTH)

	# status (Digit 7)
	@property
	def status(self):
		return self.__status

	@status.setter
	def status(self, status):
		REQUIRED_LENGTH = 1
		if len(status) > REQUIRED_LENGTH :
			return
		self.__status = status

	# hq_tf_fd (Digit 8)
	@property
	def hq_tf_fd(self):
		return self.__hq_tf_fd

	@hq_tf_fd.setter
	def hq_tf_fd(self, hq_tf_fd):
		REQUIRED_LENGTH = 1
		if len(hq_tf_fd) > REQUIRED_LENGTH :
			return
		self.__hq_tf_fd = hq_tf_fd

	# echelon_mobility (Digits 9 and 10)
	@property
	def echelon_mobility(self):
		return self.__echelon_mobility

	@echelon_mobility.setter
	def echelon_mobility(self, echelon_mobility):
		REQUIRED_LENGTH = 2
		if len(echelon_mobility) > REQUIRED_LENGTH :
			return
		self.__echelon_mobility = SymbolIdCodeDelta.left_zero_pad(echelon_mobility, REQUIRED_LENGTH)

	#####################################################
	# 2525D: A.5.2.2  Set B - Second ten digits

	# (full) entity_code (Digits 11-16) 
	@property
	def entity_code(self):
		return self.__entity_code

	@entity_code.setter
	def entity_code(self, entity_code):
		REQUIRED_LENGTH = 6
		if len(entity_code) > REQUIRED_LENGTH :
			return
		self.__entity_code = SymbolIdCodeDelta.left_zero_pad(entity_code, REQUIRED_LENGTH)

	# modifier1 (Digits 17 and 18)
	@property
	def modifier1(self):
		return self.__modifier1

	@modifier1.setter
	def modifier1(self, modifier1):
		REQUIRED_LENGTH = 2
		if len(modifier1) > REQUIRED_LENGTH :
			return
		self.__modifier1 = SymbolIdCodeDelta.left_zero_pad(modifier1, REQUIRED_LENGTH)

	# modifier (Digits 19 and 20)
	@property
	def modifier2(self):
		return self.__modifier2

	@modifier2.setter
	def modifier2(self, modifier2):
		REQUIRED_LENGTH = 2
		if len(modifier2) > REQUIRED_LENGTH :
			return
		self.__modifier2 = SymbolIdCodeDelta.left_zero_pad(modifier2, REQUIRED_LENGTH)

	#####################################################

	def populate_code_from_properties(self):
		string_builder = self.version           # 1-2
		string_builder+= self.real_exercise_sim # 3
		string_builder+= self.affiliation       # 4
		string_builder+= self.symbol_set        # 5-6
		string_builder+= self.status            # 7
		string_builder+= self.hq_tf_fd          # 8
		string_builder+= self.echelon_mobility  # 9-10

		string_builder+= self.entity_code       # 11-16
		string_builder+= self.modifier1         # 17-18
		string_builder+= self.modifier2         # 19-20

		self.__full_code = string_builder

	def populate_properties_from_code(self):

		REQUIRED_CODE_LENGTH = 20

		if len(self.full_code) != REQUIRED_CODE_LENGTH :
			print('Bad Code Length for code: ' + self.full_code)
			self.full_code = SymbolIdCodeDelta.INVALID_FULL_CODE
			return

		self.version           = self.full_code[0:2] # 1-2
		self.real_exercise_sim = self.full_code[2] # 3
		self.affiliation       = self.full_code[3] # 4
		self.symbol_set        = self.full_code[4:6] # 5-6
		self.status            = self.full_code[6] # 7
		self.hq_tf_fd          = self.full_code[7] # 8
		self.echelon_mobility  = self.full_code[8:10] # 9-10
		
		self.entity_code       = self.full_code[10:16] # 11-16
		self.modifier1         = self.full_code[16:18] # 17-18
		self.modifier2         = self.full_code[18:20] # 19-20

	@property
	def human_readable_code(self):
		string_builder = "SS:"   + self.symbol_set
		string_builder+= ":E:"   + self.entity_code
		string_builder+= ":M1:"  + self.modifier1
		string_builder+= ":M2:"  + self.modifier2
		string_builder+= ":AF:"   + self.affiliation

		# optional ones
		if (self.real_exercise_sim is None or self.real_exercise_sim == '0') :
			string_builder+= ":RES:" + self.real_exercise_sim
		if not (self.status is None or self.status == '0') :
			string_builder+= ":ST:" + self.status
		if not (self.echelon_mobility is None or self.echelon_mobility == '00') :
			string_builder+= ":EM:" + self.echelon_mobility
		if not (self.hq_tf_fd is None or self.hq_tf_fd == '0') :
			string_builder+= ":HTD:" + self.hq_tf_fd

		return string_builder

###############################################################################

class SymbolLookup(object) :

	affiliation_charlie_2_delta_char = dict([ \
                ('P', '0'), \
                ('U', '1'), \
                ('A', '2'), \
                ('F', '3'), \
                ('N', '4'), \
                ('S', '5'), \
                ('H', '6'), \
                ])

	hq_tf_fd_charlie_2_delta_char = dict([ \
                ('-', '0'), \
                ('F', '1'), \
                ('A', '2'), \
                ('C', '3'), \
                ('E', '4'), \
                ('G', '5'), \
                ('B', '6'), \
                ('D', '7'), \
                ])

	status_charlie_2_delta_char = dict([ \
                ('P', '0'), \
                ('A', '1'), \
                ('C', '2'), \
                ('D', '3'), \
                ('X', '4'), \
                ('F', '5'), \
                ])

	echelon_mobility_charlie_2_delta_char = dict([ \
                ('-', '00'), \
                ('A', '11'), \
                ('B', '12'), \
                ('C', '13'), \
                ('D', '14'), \
                ('E', '15'), \
                ('F', '16'), \
                ('G', '17'), \
                ('H', '18'), \

                ('I', '21'), \
                ('J', '22'), \
                ('K', '23'), \
                ('L', '24'), \
                ('M', '25'), \
                ('M', '26'), \

                ('O', '31'), \
                ('P', '32'), \
                ('Q', '33'), \
                ('R', '34'), \
                ('S', '35'), \
                ('T', '36'), \
                ('W', '37'), \

                ('U', '41'), \
                ('V', '42'), \

                ('X', '51'), \
                ('Y', '52'), \
                ])

	def __init__(self) :

		self.idDict2525CtoD = {}
		self.initialize()

	def initialized(self) : 
		return len(self.idDict2525CtoD) > 0

	def initialize(self) :

		if self.initialized() :
			return True

		currentPath = os.path.dirname(__file__)
		dataPath = os.path.normpath(os.path.join(currentPath, r"../tooldata"))
		inputFile  = os.path.normpath(os.path.join(dataPath, r"LegacyMappingTableCtoD.csv"))

		print("Initializing using source data: " + inputFile)

		# Open Input File & load into dict (if possible)
		try :
			if sys.version < '3': 
				f_in = open(inputFile, 'rb')
			else: 
				f_in = open(inputFile, "r") 

			reader = csv.reader(f_in, delimiter=',')

			next(reader, None) # skip header row

			# Expected Format:
			#  {2525Charlie1stTen : 2525Charlie1stTen,2525Charlie,2525DeltaSymbolSet,
			#     2525DeltaEntity,2525DeltaMod1,2525DeltaMod2,2525DeltaName,2525DeltaMod1Name,
			#     2525DeltaMod2Name,DeltaToCharlie,Remarks}

			self.idDict2525CtoD = {row[0]:row for row in reader}

		except Exception as openEx :            
			print('Could not open file for reading: ' + str(inputFile))
			return False

		return self.initialized()

	# If debug needed
	# for key in idDict2525CtoD :
	#	print(key)
	

	def getDeltaCodeFromCharlie(self, charlieCodeIn) : 

		symbolId = SymbolIdCodeDelta()

		MINIMUM_CODE_LENGTH = 15

		if (not self.initialized()) or (charlieCodeIn is None) or (len(charlieCodeIn) < MINIMUM_CODE_LENGTH) :
			return symbolId 

		charlieCode = charlieCodeIn.upper()

		# Default to "Retired/Invalid" symbol
		symbolSetString = '98'
		entityString    = '100000'
		mod1String      = '00'
		mod2String      = '00'

		isWeather = (charlieCode[0] == 'W')

		replaceAffilChar  = '*'
		replaceStatusChar = 'P'
		if (isWeather) :
			replaceAffilChar  = charlieCode[1]
			replaceStatusChar = charlieCode[3]

		lookupCharlieCode = charlieCode[0]
		lookupCharlieCode+= replaceAffilChar
		lookupCharlieCode+= charlieCode[2]
		lookupCharlieCode+= replaceStatusChar
		lookupCharlieCode+= charlieCode[4:10]

		print("Using Charlie Lookup: " + lookupCharlieCode)

		try : 
			if not (lookupCharlieCode in self.idDict2525CtoD) :
				# some keys only have an "F" version
				alternateLookupKey = lookupCharlieCode[0] + 'F' + lookupCharlieCode[2:10]
				if not (alternateLookupKey in self.idDict2525CtoD) :
					print("Could not find key: " + lookupCharlieCode)
					return symbolId
				else :
					lookupCharlieCode = alternateLookupKey

			row2525d = self.idDict2525CtoD[lookupCharlieCode]

			symbolSetString = row2525d[2]
			entityString    = row2525d[3]
			mod1String      = row2525d[4]
			mod2String      = row2525d[5]
			name            = row2525d[6]

			if name.endswith(' : Unknown'): #remove this bad entry from name
				name = name[:-10]

			remarks = row2525d[10]
			if remarks == 'Retired' :
				print("WARNING: Retired Symbol=" + lookupCharlieCode)
			elif remarks == 'pass' :
				remarks = 'success' # replace with more meaningful remark

		except : 
			print("Crash with SIDC/key: " + lookupCharlieCode)

		symbolId.symbol_set  = symbolSetString
		symbolId.entity_code = entityString
		symbolId.modifier1   = mod1String
		symbolId.modifier2   = mod2String
		symbolId.name        = name
		symbolId.remarks     = remarks

		# now we have the base symbol, but the remaining attributes are a little messier to map 
		affilChar = charlieCode[1]
		if affilChar in SymbolLookup.affiliation_charlie_2_delta_char :
			symbolId.affiliation = SymbolLookup.affiliation_charlie_2_delta_char[affilChar]

		statusChar = charlieCode[3]
		if statusChar in SymbolLookup.status_charlie_2_delta_char :
			symbolId.status = SymbolLookup.status_charlie_2_delta_char[statusChar]
		
		hqFdTfChar = charlieCode[10]
		if hqFdTfChar in SymbolLookup.hq_tf_fd_charlie_2_delta_char :
			symbolId.hq_tf_fd = SymbolLookup.hq_tf_fd_charlie_2_delta_char[hqFdTfChar]

		echelonChar = charlieCode[11]
		if echelonChar in SymbolLookup.echelon_mobility_charlie_2_delta_char :
			symbolId.echelon_mobility = SymbolLookup.echelon_mobility_charlie_2_delta_char[echelonChar]
		
		return symbolId 

class SymbolLookupCharlie(object) :
	
	DEFAULT_POINT_SIDC = "SUGPU----------"
	DEFAULT_LINE_SIDC  = "GUGPGLB-------X"
	DEFAULT_AREA_SIDC  = "GUGPGAG-------X"

	NOT_FOUND = 'NOT_FOUND'

	UNKNOWN_GEOMETRY_STRING     = "Unknown"
	POINT_STRING                = "Point"
	LINE_STRING                 = "Line"
	AREA_STRING                 = "Area"
	GEOMETRY_STRING             = "Geometry"

	affiliation_to_frame_char = dict([ \
                ('U', 'U'), \
                ('P', 'U'), \
                ('G', 'U'), \
                ('W', 'U'), \
                ('-', 'U'), \
                ('H', 'H'), \
                ('S', 'H'), \
                ('N', 'N'), \
                ('L', 'N'), \
                ('F', 'F'), \
                ('M', 'F'), \
                ('A', 'F'), \
                ('D', 'F'), \
                ('J', 'F'), \
                ('K', 'F'), \
                ])

	echelonToSIC1112 = dict([ \
                ('TEAM/CREW', '-A'), \
                ('SQUAD', '-B'), \
                ('SECTION', '-C'), \
                ('PLATOON/DETACHMENT', '-D'), \
                ('COMPANY/BATTERY/TROOP', '-E'), \
                ('BATTALION/SQUADRON', '-F'), \
                ('REGIMENT/GROUP', '-G'), \
                ('BRIGADE', '-H'), \
                ('DIVISION', '-I'), \
                ('CORPS/MEF', '-J'), \
                ('ARMY', '-K'), \
                ('ARMY GROUP/FRONT', '-L'), \
                ('REGION', '-M'), \
                ('COMMAND', '-N') \
                ])

	# Symbol/Rule ID Name to SIDC mapping 
	nameToSIC = dict([ \
	###########################################
	## TODO - If you have rule names that do not match the standard Military Features convention
	## you must add them here(in UpperCase), or otherwise set in this dictionary. Example shown:
	###########################################
			("STRYKER BATTALION",            "SFGPUCII---F---"), \
			("STRYKER CAVALRY TROOP",        "SFGPUCRRL--E---"), \
			("FIELD ARTILLERY BATTALION",    "SFGPUCF----F---"), \
			("STRYKER HEADQUARTERS COMPANY", "SFGPUH-----E---"), \
			("BRIGADE SUPPORT BATTALION",    "SFGPU------F---"), \
			("INFANTRY PLATOON F", "SFGPUCI----D---")
			])

	FRIENDLY_AFFILIATION = "FRIENDLY"
	HOSTILE_AFFILIATION  = "HOSTILE"
	NEUTRAL_AFFILIATION  = "NEUTRAL"
	UNKNOWN_AFFILIATION  = "UNKNOWN"

	validAffiliations = { FRIENDLY_AFFILIATION, HOSTILE_AFFILIATION, NEUTRAL_AFFILIATION, UNKNOWN_AFFILIATION } 

	affiliationToAffiliationChar = dict([ \
				(FRIENDLY_AFFILIATION, 'F'), \
				(HOSTILE_AFFILIATION, 'H'), \
				(NEUTRAL_AFFILIATION, 'N'), \
				(UNKNOWN_AFFILIATION, 'U')])

	@staticmethod
	def getDefaultSidcForGeometryString(geoString) : 
		if geoString == SymbolLookupCharlie.POINT_STRING  : 
			return SymbolLookupCharlie.DEFAULT_POINT_SIDC
		elif geoString == LINE_STRING :
			return SymbolLookupCharlie.DEFAULT_LINE_SIDC         
		elif geoString == AREA_STRING : 
			return SymbolLookupCharlie.DEFAULT_AREA_SIDC
		else :
			return SymbolLookupCharlie.DEFAULT_POINT_SIDC

	@staticmethod
	def getGeometryStringFromShapeType(shapeType) :
		if shapeType == "Point" : 
			return SymbolLookupCharlie.POINT_STRING
		elif shapeType == "Polyline" :
			return SymbolLookupCharlie.LINE_STRING         
		elif shapeType == "Polygon" : 
			return SymbolLookupCharlie.AREA_STRING
		else :
			return SymbolLookupCharlie.POINT_STRING

	@staticmethod
	def getDefaultSidcForShapeType(shapeType) : 
		if shapeType == "Point" : 
			return SymbolLookupCharlie.DEFAULT_POINT_SIDC
		elif shapeType == "Polyline" :
			return SymbolLookupCharlie.DEFAULT_LINE_SIDC         
		elif shapeType == "Polygon" : 
			return SymbolLookupCharlie.DEFAULT_AREA_SIDC
		else :
			return SymbolLookupCharlie.DEFAULT_POINT_SIDC

	def __init__(self, standard) :

		self.sqlitedb = None
		self.initialize(standard)

	def initialized(self) : 
		return self.sqlitedb != None

	def initialize(self, standard) :

		if self.initialized() :
			return True

		try :
			self.sqlitedb = sqlite3.connect(':memory:')

			cur = self.sqlitedb.cursor()
			cur.execute('''CREATE TABLE SymbolInfo (
						ID TEXT,
						Name TEXT,
						SymbolId TEXT,
						StyleFile TEXT,
						Category TEXT,
						GeometryType TEXT,
						GeometryConversionType TEXT,
						Tags TEXT 
						)''')

			currentPath = os.path.dirname(__file__)
			dataPath = os.path.normpath(os.path.join(currentPath, r"../tooldata"))

			# TODO: use standard to choose between mil2525c & app6b
			inputFile  = os.path.normpath(os.path.join(dataPath, r"mil2525c.csv"))

			if sys.version < '3': 
				csv_fp=open(inputFile, 'rb')
			else: 
				csv_fp=open(inputFile, 'r')

			reader = csv.reader(csv_fp)
			next(reader, None) # skip header row

			cur.executemany('''
				INSERT INTO SymbolInfo (ID,Name,SymbolId,StyleFile,Category,GeometryType,GeometryConversionType,Tags)
				VALUES (?,?,?,?,?,?,?,?)''', reader)

			self.sqlitedb.commit()

		except Exception as openEx :            
			print('Could not open file for reading: ' + str(inputFile))
			self.sqlitedb = None
			return False

		return self.initialized()

	# Helper to RegEx test/validate a SIDC for basic correctness 
	# IMPORTANT: does not guarantee correctness
	def isValidSidc(self, sidc) :
		validSicRegex = "^[SGWIOE][PUAFNSHGWMDLJKO\-][PAGSUFXTMOEVLIRNZC\-][APCDXF\-][A-Z0-9\-]{6}[A-Z\-]{2}[A-Z0-9\-]{2}[AECGNSX\-]$"
		matching = bool(re.match(validSicRegex, sidc))
		return matching

	def getAffiliationChar(self, sic) :
		ch = sic.upper()[1]
		if ch in SymbolLookupCharlie.affiliation_to_frame_char :
			return SymbolLookupCharlie.affiliation_to_frame_char[ch]
		else :
			print("Unrecognized affiliation")
			return 'U'

	def getMaskedSymbolIdFirst10(self, sic) : 
		if len(sic) < 10 :
			upperSic = SymbolLookupCharlie.DEFAULT_POINT_SIDC 
		else :
			upperSic = sic.upper()
		maskedSic = upperSic[0] + self.getAffiliationChar(upperSic) \
			+ upperSic[2] + 'P' + upperSic[4:10]
		return maskedSic[0:10]

	def getSymbolAttribute(self, symbolId, attribute) : 

		if not self.initialized() :
			return SymbolLookupCharlie.NOT_FOUND

		sqliteCursor = self.sqlitedb.cursor()

		lookupSic = self.getMaskedSymbolIdFirst10(symbolId)  
		lookupSic = lookupSic.upper()                  

		query = "select " + attribute + " from SymbolInfo where (ID = ?)"
		sqliteCursor.execute(query, (lookupSic,))
		sqliteRow = sqliteCursor.fetchone()

		# some only have 'F' version
		if (sqliteRow == None) :
			lookupSic = lookupSic[0] + 'F' + lookupSic[2] + 'P' + lookupSic[4:10]
			sqliteCursor.execute(query, (lookupSic,))
			sqliteRow = sqliteCursor.fetchone()            

		if (sqliteRow == None) :
			print ("WARNING: " + symbolId + ":" + attribute + " NOT FOUND")
			val = "None"
		else :
			val = sqliteRow[0]

		# print symbolId, attribute, val
		return val

	def symbolIdToName(self, symbolId) :
		symbolName = self.getSymbolAttribute(symbolId, "Name")
		return symbolName

	def SymbolNametoSymbolID(self, symbolName) :
		# Lookup when looking up the Dictionary Name exactly as it appears in the Dictionary 
		return self.SymbolNametoSymbolIDExt(symbolName, "", "", "")

	def SymbolNametoSymbolIDExt(self, symbolName, echelonString, affiliation, expectedGeometry) :

		if not self.initialized() :
			return SymbolLookupCharlie.NOT_FOUND

		# Attempts to handle the many name cases that show up in Military
		# Features
		# A straight Dictionary Name to SIDC case should always work, but the
		# names
		# don't always show up in that form, use SymbolNametoSymbolID for
		# simple case

		foundSIC = False
		add2Map = False
		symbolNameUpper = symbolName.upper()
		
		# Tricky: the Append Features Tools adds to the base name with "~" so
		# remove all after "~"
		# see SymbolCreator.cs/GetRuleNameFromSidc for separator
		# character/format
		symbolNameUpper = symbolNameUpper.split("~")[0].strip()

		# print ("Using Symbol " + sidc)
		if (symbolNameUpper in self.nameToSIC):
			# Skip the SQL query, because we have already found this one (or it
			# is hardcoded)
			sidc = self.nameToSIC[symbolNameUpper]
			foundSIC = True
		else:
			sqliteConn = None
			sqliteCursor = None

			sqliteCursor = self.sqlitedb.cursor()

			# SQL query (or two) to find SIC
			sqliteCursor.execute("SELECT SymbolId FROM SymbolInfo WHERE UPPER(Name) = ?", (symbolNameUpper,))
			sqliteRow = sqliteCursor.fetchone()

			if (sqliteRow == None):
				# if it is not found with the supplied name, we need to try a
				# few more cases:
				# remove 1) affilition 2) "Left" / "Right"
				if self.endsInAffilationString(symbolNameUpper) :
					symbolNameUpper = symbolNameUpper[0:-2] 
				elif self.endsInLeft(symbolNameUpper) : 
					symbolNameUpper = symbolNameUpper[0:-5]
				elif self.endsInRight(symbolNameUpper) : 
					symbolNameUpper = symbolNameUpper[0:-6]

				if (symbolNameUpper in self.nameToSIC):
					# Check again with modfied name
					sidc = self.nameToSIC[symbolNameUpper]
					foundSIC = True
				else :
					queryval = symbolNameUpper + "%"
					sqliteCursor.execute("SELECT SymbolId FROM SymbolInfo WHERE UPPER(Name) like ?", (queryval,))
					sqliteRow = sqliteCursor.fetchone()

					# Yet another failing case "some have '-' some don't, ex.
					# "Task - Screen" <-> "Task Screen"
					if (sqliteRow == None):
						queryval = '%' + symbolNameUpper + '%'
						sqliteCursor.execute("SELECT SymbolId FROM SymbolInfo WHERE (UPPER(Name) like ?)", (queryval,))
						sqliteRow = sqliteCursor.fetchone()

			if (sqliteRow != None):
				foundSIC = True
				sidc = sqliteRow[0].replace("*", "-")
				add2Map = True

		if (foundSIC) and self.isValidSidc(sidc):
			# If it is now a valid SIDC, replace chars 11 and 12 (in Python
			# that's 10 and 11) with the echelon code
			if (echelonString in self.echelonToSIC1112):
				sidc = sidc[0:10] + self.echelonToSIC1112[echelonString] + sidc[12:]

			# Then check affiliation char (the correct one is not always returned)
			if not ((affiliation is None) or (affiliation is "")) :  
				affiliationChar = sidc[1]
				expectedAffiliationChar = SymbolLookupCharlie.affiliationToAffiliationChar[affiliation]
	
				if affiliationChar != expectedAffiliationChar :
					print("Unexpected Affiliation Char: " + affiliationChar + " != " + expectedAffiliationChar)
					sidc = sidc[0] + expectedAffiliationChar + sidc[2:]

			if add2Map : 
				# add the query results to the map (if valid)
				self.nameToSIC[symbolNameUpper] = sidc            
				print("Adding to Map: [" + symbolNameUpper + ", " + sidc + "]")
		else:
			defaultSidc = SymbolLookupCharlie.getDefaultSidcForGeometryString(expectedGeometry)
			sidc = defaultSidc
			warningMsg = "Warning: Could not map " + symbolNameUpper + " to valid SIDC - returning default: " + sidc
			print(warningMsg)

		return sidc

	def symbolIdToGeometryType(self, symbolId) :

		if not self.initialized() :
			return SymbolLookupCharlie.UNKNOWN_GEOMETRY_STRING

		sqliteCursor = self.sqlitedb.cursor()

		lookupSic = self.getMaskedSymbolIdFirst10(symbolId)
		lookupSic = lookupSic.upper()

		query = "select GeometryType from SymbolInfo where (ID = ?)"
		sqliteCursor.execute(query, (lookupSic,))
		sqliteRow = sqliteCursor.fetchone()

		# some only have 'F' version
		if (sqliteRow == None) :
			lookupSic = lookupSic[0] + 'F' + lookupSic[2] + 'P' + lookupSic[4:10]
			sqliteCursor.execute(query, (lookupSic,))
			sqliteRow = sqliteCursor.fetchone()                   

		if (sqliteRow == None) :
			geoType = SymbolLookupCharlie.UNKNOWN_GEOMETRY_STRING
		else :
			geoChar = sqliteRow[0]

			if (geoChar == 'P') :
				geoType = SymbolLookupCharlie.POINT_STRING
			elif (geoChar == 'L') :
				geoType = SymbolLookupCharlie.LINE_STRING
			elif (geoChar == 'A') :
				geoType = SymbolLookupCharlie.AREA_STRING

		# print symbolId, geoType
		return geoType

	def endsInAffilationString(self, str) : 
		endsInRegex = ".* [FHNU]$"
		matching = bool(re.match(endsInRegex, str.upper()))
		return matching

	def endsInLeft(self, str) : 
		endsInRegex = ".*LEFT$"
		matching = bool(re.match(endsInRegex, str.upper()))
		return matching

	def endsInRight(self, str) : 
		endsInRegex = ".*RIGHT$"
		matching = bool(re.match(endsInRegex, str.upper()))
		return matching