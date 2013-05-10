# military-feature-toolbox

The ArcGIS Defense and Intelligence Military Feature Toolbox is a set of tools, scripts, and applications for use in ArcGIS Desktop. These tools provide specialized processing and workflows for military feature symbology.

![Image of Military Feature Toolbox]( ScreenShot.JPG "military-feature-toolbox" )

## Features

* Specialized geoprocessing models and tools for military features including
    * Tools for importing/appending non-military feature classes into a military feature geodatabase
    * Tools for converting military features to ArcGIS Runtime messages (and visa-versa)
	* Tools for setting the SIDC and representation rule fields on a military feature geodatabase

## Requirements

* ArcGIS Desktop 10.1 Advanced
    *  ArcGIS Desktop Advanced license is required to change Representation Rules (e.g. in Append Military Features)
* Visual Studio 2010 or later
* ArcObjects .NET Engine or Desktop Development Kit
* Apache Ant - used to download and extract dependent data and run test drivers

## Instructions

### General Help
[New to Github? Get started here.](http://htmlpreview.github.com/?https://github.com/Esri/esri.github.com/blob/master/help/esri-getting-to-know-github.html)

### Getting Started with the toolbox
* Download required data dependencies 
    * Install and configure Apache Ant
        * Download Ant from the [Apache Ant Project](http://ant.apache.org/bindownload.cgi) and unzip to a location on your machine
        * Set environment variable `ANT_HOME` to Ant Install Location
        * Add Ant\bin to your path: `%ANT_HOME%\bin`
        * NOTE: Ant requires Java [Runtime Environment (JRE) or Developer Kit (JDK)](http://www.oracle.com/technetwork/java/javase/downloads/index.html) to be installed and the environment variable `JAVA_HOME` to be set to this location
        * To verify your Ant Installation: Open Command Prompt> `ant -v` and verify it runs and returns the version correctly 
    * To download the data dependencies 
        * Open Command Prompt>
        * cd military-feature-toolbox
        * > ant
        * Verify “Build Succeeded”  
        * This will create a directory military-feature-toolbox/data with required data files
* Update the local ArcGIS Military Style Files to the latest version
    * Navigate to the folder military-feature-toolbox\data\stylefiles
    * Update/copy all of the .style files from this folder into your ArcGIS Desktop Style folder
    * For example, copy the style files from above location into this Desktop Folder:
    	* {ArcGIS Install Location}\ArcGIS\Desktop10.1\Styles
* Open and build the Visual Studio Solution at military-feature-toolbox\source\AppendMilitaryFeaturesCS
    * To use MSBuild to build the solution
        * Open a Visual Studio Command Prompt: Microsoft Visual Studio 2012 | Visual Studio Tools | Developer Command Prompt for VS2012
        * cd military-feature-toolbox\source\AppendMilitaryFeaturesCS
        * msbuild AppendMilitaryFeatures2010.sln /property:Configuration=Release
* (Optional) Test the command line AppendMilitaryFeatures application
    * cd military-feature-toolbox\application
    * > AppendMilitaryFeatures.exe 
    * This will verify that the necessary products 
    * Note: this will use default values of
        * data\shapefiles\FriendlyForcesSmall.shp 
        * data\geodatabases\test_outputs.gdb
        * Symbol_ID
* Start ArcMap or ArcCatalog and run the Append Military Features Geoprocessing tool at military-feature-toolbox\toolboxes
    *  IMPORTANT: the output military feature geodatabase must not be open in any other application or the tool will fail with "can not obtain Schema Lock" error
* To run all GP Tool unit tests
    * Open Command Prompt>
    * > cd military-feature-toolbox\source\test\TestGPTools
    * > ant
    * Verify "Build Succeeded"

## Resources

* Learn more about Esri's [ArcGIS for Defense maps and apps](http://resources.arcgis.com/en/communities/defense-and-intelligence/).

## Issues

* Find a bug or want to request a new feature?  Please let us know by submitting an issue.

## Contributing

* Anyone and everyone is welcome to contribute.

## Licensing

Copyright 2013 Esri

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

A copy of the license is available in the repository's
[license.txt](license.txt) file.

[](Esri Tags: ArcGIS Defense and Intelligence Military Feature Military Features)
[](Esri Language: Python)
