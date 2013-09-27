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
# TestUtilities.py
# Description: Common objects/methods used by test scripts
# Requirements: ArcGIS Desktop Standard
# -----------------------------------------------------------------------------

import os

currentPath = os.path.dirname(__file__)
dataPath = os.path.normpath(os.path.join(currentPath, r"../../../data/mil2525c/testdata/"))
geodatabasePath = os.path.normpath(os.path.join(dataPath, r"geodatabases/"))
toolboxesPath = os.path.normpath(os.path.join(currentPath, r"../../../toolboxes/"))

blankMilFeaturesGDB = os.path.join(geodatabasePath, "MilitaryOverlay10.1.1-Blank.gdb")                
inputGDB  = os.path.join(geodatabasePath, "test_inputs.gdb")
inputGDBNonMilitaryFeatures  = os.path.join(geodatabasePath, "test_inputs_non_military_features.gdb")
outputGDB = os.path.join(geodatabasePath, "test_outputs.gdb")
outputGDBTemp = os.path.join(geodatabasePath, "test_outputs_temp.gdb")
outputMessagePath = os.path.join(dataPath, "messagefiles")

toolbox = os.path.join(toolboxesPath, "Military Feature Tools.tbx")
  
