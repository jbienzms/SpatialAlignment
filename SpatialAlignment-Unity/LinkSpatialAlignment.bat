@ECHO OFF

ECHO.
ECHO ===============================================================================
ECHO =                                 Link SpatialAlignment                                   =
ECHO ===============================================================================
ECHO.
ECHO This batch file creates symbolic links to  Spatial Alignment.
ECHO.
ECHO Spatial Alignment should be downloaded and extracted before running this batch file. 
ECHO If you continue you will be prompted for the full path to SA. 
ECHO.
ECHO Are you ready to continue?
ECHO.
CHOICE /C:YN
IF ERRORLEVEL == 2 GOTO End


:SpatialAlignment

SET /p SpatialAlignmentSource=SpatialAlignment for Unity Path? 
IF NOT EXIST "%SpatialAlignmentSource%\Assets\SpatialAlignment\SpatialFrame.cs" (
ECHO.
ECHO SpatialAlignment for Unity not found at %SpatialAlignmentSource%
ECHO.
GOTO SpatialAlignment
)
ECHO SpatialAlignment for Unity FOUND
ECHO.


ECHO.
ECHO ===============================================================================
ECHO =                               Linking SpatialAlignment                                  =
ECHO ===============================================================================
ECHO.
mklink /J "Assets\SpatialAlignment" "%SpatialAlignmentSource%\Assets\SpatialAlignment"
mklink /J "Assets\SpatialAlignment-Examples" "%SpatialAlignmentSource%\Assets\SpatialAlignment-Examples"
ECHO.

PAUSE

:End