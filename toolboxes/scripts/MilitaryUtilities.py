import sqlite3
import uuid
import re
import arcpy
import os
import SymbolDictionary
import GeometryConverter
import DictionaryConstants

# IMPORTANT: assumes Mil2525C.dat is same directory as .py file
symbolDictionaryPath = os.path.join(os.path.dirname(__file__),  "Mil2525C.dat" )
symbolDictionary = SymbolDictionary.SymbolDictionary(symbolDictionaryPath)
geoConverter = GeometryConverter.GeometryConverter(symbolDictionary)

def pointsToArcPyGeometry(pointList, shapeType) : 

    arcPoint = arcpy.Point()
    arcArray = arcpy.Array()
       
    for point in pointList : 
        x = point.split(',')[0]
        y = point.split(',')[1]
        print "(", x, ",", y, ")"
        arcPoint.X = float(x)
        arcPoint.Y = float(y)
        arcArray.add(arcPoint)

    if shapeType == "Point" : 
        return arcPoint
    elif shapeType == "Polyline" : 
        arcPolyline = arcpy.Polyline(arcArray)
        return arcPolyline
    elif shapeType == "Polygon" : 
        arcPolygon = arcpy.Polygon(arcArray)
        return arcPolygon
              
def parsePartToControlPoints(part):
    controlPoints = ""
    sep = ""
    
    for subpart in part:
        try:
            # assume it's a point
            subpartStr = str(subpart.X) + "," + str(subpart.Y)
            controlPoints = controlPoints + sep + subpartStr
            sep = ";"
        except AttributeError:
            # it's an array of parts, i.e. a part
            controlPoints = controlPoints + sep + parsePartToControlPoints(subpart)
            sep = ";"
            
    return controlPoints

def parseGeometryToControlPoints(geom):
    try:
        # assume it's a point
        return str(geom.X) + "," + str(geom.Y)
    except AttributeError:
        # it's not a point
        try:
            controlPoints = ""
            sep = ""
            for i in range(geom.partCount):
                part = geom.getPart(i)
                # part is an array
                for subpart in part:
                    controlPoints = controlPoints + sep + parsePartToControlPoints(part)
                    sep = ";"
            return controlPoints
        except AttributeError:
            # it's a part
            return parsePartToControlPoints(geom)
             
def reverseControlPoints(string):
    revnums = re.split(r'(\;+)', string)
    revnums.reverse()
    revnums = ''.join(revnums)
    return revnums
