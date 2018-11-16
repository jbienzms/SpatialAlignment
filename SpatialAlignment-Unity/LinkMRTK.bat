@ECHO OFF

ECHO.
ECHO ===============================================================================
ECHO =                                 Link MRTK                                   =
ECHO ===============================================================================
ECHO.
ECHO This batch file creates symbolic links to the MRTK so that the full MRTK is not 
ECHO committed to source control as part of this project.
ECHO.
ECHO The MRTK should be downloaded and extracted before running this batch file. 
ECHO If you continue you will be prompted for the full path to the MRTK. 
ECHO.
ECHO Are you ready to continue?
ECHO.
CHOICE /C:YN
IF ERRORLEVEL == 2 GOTO End


:MRTK

SET /p MRTKSource=MRTK for Unity Path? 
IF NOT EXIST "%MRTKSource%\Assets\mcs.rsp" (
ECHO.
ECHO MRTK for Unity not found at %MRTKSource%
ECHO.
GOTO MRTK
)
ECHO MRTK for Unity FOUND
ECHO.


ECHO.
ECHO ===============================================================================
ECHO =                            Copying MRTK RSPs                                =
ECHO ===============================================================================
ECHO.
XCOPY /Y /Q "%MRTKSource%\Assets\*.rsp" "Assets"
ECHO.

ECHO.
ECHO ===============================================================================
ECHO =                               Linking MRTK                                  =
ECHO ===============================================================================
ECHO.
mklink /J "Assets\HoloToolkit" "%MRTKSource%\Assets\HoloToolkit"
REM mklink /J "Assets\HoloToolkit-Examples" "%MRTKSource%\Assets\HoloToolkit-Examples"
REM mklink /J "Assets\HoloToolkit-Preview" "%MRTKSource%\Assets\HoloToolkit-Preview"
ECHO.

PAUSE

:End