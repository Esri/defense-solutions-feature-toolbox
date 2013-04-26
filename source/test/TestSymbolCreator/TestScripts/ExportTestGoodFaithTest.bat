REM  MilMarkerSymbol Test Driver Script

echo Basic Symbol and Frame
..\TestSymbolCreator.exe SFGPUCI--------

echo All Frames (Position 2)
..\TestSymbolCreator.exe SPGPUCI--------
..\TestSymbolCreator.exe SUGPUCI--------
..\TestSymbolCreator.exe SAGPUCI--------
..\TestSymbolCreator.exe SFGPUCI--------
..\TestSymbolCreator.exe SNGPUCI--------
..\TestSymbolCreator.exe SSGPUCI--------
..\TestSymbolCreator.exe SHGPUCI--------
..\TestSymbolCreator.exe SGGPUCI--------
..\TestSymbolCreator.exe SWGPUCI--------
..\TestSymbolCreator.exe SMGPUCI--------
..\TestSymbolCreator.exe SDGPUCI--------
..\TestSymbolCreator.exe SLGPUCI--------
..\TestSymbolCreator.exe SJGPUCI--------
..\TestSymbolCreator.exe SKGPUCI--------

echo All Statuses (Position 4)
..\TestSymbolCreator.exe SFGPUCI--------
..\TestSymbolCreator.exe SFGAUCI--------
..\TestSymbolCreator.exe SFGCUCI--------
..\TestSymbolCreator.exe SFGDUCI--------
..\TestSymbolCreator.exe SFGXUCI--------
..\TestSymbolCreator.exe SFGFUCI--------

echo Some Test Modifiers (Position 11)
..\TestSymbolCreator.exe SFGPUCI---A----
..\TestSymbolCreator.exe SFGPUCI---B----
..\TestSymbolCreator.exe SFGPUCI---C----
..\TestSymbolCreator.exe SFGPUCI---D----
..\TestSymbolCreator.exe SFGPUCI---E----
..\TestSymbolCreator.exe SFGPUCI---F----
..\TestSymbolCreator.exe SFGPUCI---G----
..\TestSymbolCreator.exe SFGPUCI---H----
..\TestSymbolCreator.exe SFGPUCI---MO---
..\TestSymbolCreator.exe SFGPUCI---MY---
..\TestSymbolCreator.exe SFGPUCI---NS---
..\TestSymbolCreator.exe SFGPUCI---NL---

echo Echeclon (Position 12) 
..\TestSymbolCreator.exe SFGPUCI----A---
..\TestSymbolCreator.exe SFGPUCI----B---
..\TestSymbolCreator.exe SFGPUCI----C---
..\TestSymbolCreator.exe SFGPUCI----D---
..\TestSymbolCreator.exe SFGPUCI----E---
..\TestSymbolCreator.exe SFGPUCI----F---
..\TestSymbolCreator.exe SFGPUCI----G---
..\TestSymbolCreator.exe SFGPUCI----H---
..\TestSymbolCreator.exe SFGPUCI----I---
..\TestSymbolCreator.exe SFGPUCI----J---
..\TestSymbolCreator.exe SFGPUCI----K---
..\TestSymbolCreator.exe SFGPUCI----L---
..\TestSymbolCreator.exe SFGPUCI----M---
..\TestSymbolCreator.exe SFGPUCI----N---

echo Some Test Appendix A (Each Battle Dimension)
echo Space
..\TestSymbolCreator.exe SFPPS----------
echo Air
..\TestSymbolCreator.exe SFAPMFF--------
echo Ground Equipment
..\TestSymbolCreator.exe SFGPEVAL-------
echo Ground Units
..\TestSymbolCreator.exe SFGPUCIL-------
echo Ground Installation
..\TestSymbolCreator.exe SFGPIU----H----
echo Sea Surface
..\TestSymbolCreator.exe SFSPXR---------
echo Sub Surface
..\TestSymbolCreator.exe SFUPSN---------
echo SOF
..\TestSymbolCreator.exe SFFPGP---------

echo Some Test Appendix B (Points Only)
..\TestSymbolCreator.exe GFGPDPT-------X
..\TestSymbolCreator.exe GFMPNF--------X
..\TestSymbolCreator.exe GFMPNDP-------X
..\TestSymbolCreator.exe GFOPED--------X

echo Some Test Appendix C 
..\TestSymbolCreator.exe WOS-ISC---P----
..\TestSymbolCreator.exe WAS-GNM---P----

echo Some Test Appendix D
..\TestSymbolCreator.exe IFPPSRU--------
..\TestSymbolCreator.exe IFGPSRAT-------

echo Some Test Appendix E
..\TestSymbolCreator.exe OFIPR----------
..\TestSymbolCreator.exe OFVPB----------

echo Some Test Appendix G
..\TestSymbolCreator.exe EFOPA----------
..\TestSymbolCreator.exe EFFPJA----H----

REM ----------------------------------------------------------
REM Now mostly the same thing, but with "Assumed/Planned" Status
REM ----------------------------------------------------------

echo Basic Symbol and Frame
..\TestSymbolCreator.exe SFGAUCI--------

echo Some Test Appendix A (Each Battle Dimension)
echo Space
..\TestSymbolCreator.exe SFPAS----------
echo Air
..\TestSymbolCreator.exe SFAAMFF--------
echo Ground Equipment
..\TestSymbolCreator.exe SFGAEVAL-------
echo Ground Units
..\TestSymbolCreator.exe SFGAUCIL-------
echo Ground Installation
..\TestSymbolCreator.exe SFGAIU----H----
echo Sea Surface
..\TestSymbolCreator.exe SFSAXR---------
echo Sub Surface
..\TestSymbolCreator.exe SFUASN---------
echo SOF
..\TestSymbolCreator.exe SFFAGP---------

echo Some Test Appendix B (Points Only)
..\TestSymbolCreator.exe GFGADPT-------X
..\TestSymbolCreator.exe GFMANF--------X
..\TestSymbolCreator.exe GFMANDP-------X
..\TestSymbolCreator.exe GFOAED--------X

echo Some Test Appendix D
..\TestSymbolCreator.exe IFPASRU--------
..\TestSymbolCreator.exe IFGASRAT-------

echo Some Test Appendix E
..\TestSymbolCreator.exe OFIAR----------
..\TestSymbolCreator.exe OFVAB----------
..\TestSymbolCreator.exe OHIAR----------
..\TestSymbolCreator.exe OHVAB----------
..\TestSymbolCreator.exe ONIAR----------
..\TestSymbolCreator.exe ONVAB----------
..\TestSymbolCreator.exe OUIAR----------
..\TestSymbolCreator.exe OUVAB----------


echo Some Test Appendix G
..\TestSymbolCreator.exe EFOAA----------
..\TestSymbolCreator.exe EFFAJA----H----
..\TestSymbolCreator.exe EHOAA----------
..\TestSymbolCreator.exe EHFAJA----H----

echo DONE!

pause
