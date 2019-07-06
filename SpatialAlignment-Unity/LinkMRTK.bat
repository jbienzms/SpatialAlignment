@ECHO OFF

ECHO.
ECHO ===============================================================================
ECHO =                                 Make Links                                  =
ECHO ===============================================================================
ECHO.
ECHO This batch file creates symbolic links for the MRTK and other Unity 
ECHO source-based libraries that are used by this project. The process for sharing 
ECHO MRTK across projects is documented here:
ECHO.
ECHO http://www.wikiholo.net/index.php?title=Sharing_HoloToolkit
ECHO.
ECHO The libraries used by this project are:
ECHO.
ECHO * Mixed Reality Toolkit (MRTK) for Unity
ECHO.
ECHO All libraries should be downloaded and extracted before running this batch file. 
ECHO If you continue you will be prompted for the full path of each of the above 
ECHO libraries. 
ECHO.
ECHO Are you ready to continue?
ECHO.
CHOICE /C:YN
IF ERRORLEVEL == 2 GOTO End


:MRTK

SET /p MRTKSource=MRTK for Unity Path? 
IF NOT EXIST "%MRTKSource%\Assets\MixedRealityToolkit.meta" (
ECHO.
ECHO MRTK for Unity not found at %MRTKSource%
ECHO.
GOTO MRTK
)
ECHO MRTK for Unity FOUND
ECHO.

ECHO.
ECHO ===============================================================================
ECHO =                               Linking MRTK                                  =
ECHO ===============================================================================
ECHO.
mklink /J "Assets\MixedRealityToolkit" "%MRTKSource%\Assets\MixedRealityToolkit"
mklink /J "Assets\MixedRealityToolkit.Examples" "%MRTKSource%\Assets\MixedRealityToolkit.Examples"
mklink /J "Assets\MixedRealityToolkit.Providers" "%MRTKSource%\Assets\MixedRealityToolkit.Providers"
mklink /J "Assets\MixedRealityToolkit.SDK" "%MRTKSource%\Assets\MixedRealityToolkit.SDK"
mklink /J "Assets\MixedRealityToolkit.Services" "%MRTKSource%\Assets\MixedRealityToolkit.Services"
mklink /J "Assets\MixedRealityToolkit.Tests" "%MRTKSource%\Assets\MixedRealityToolkit.Tests"
ECHO.

PAUSE

:End