REM @ECHO OFF
ECHO.

pushd .
cd ..
cd ..

REM Set Paths
SET UnityExe="%ProgramFiles%\Unity\Hub\Editor\2018.3.0b11\Editor\Unity.exe"
SET ProjectDir=%cd%\SpatialAlignment-Unity
SET AssetDir=%ProjectDir%\Assets
SET PackageDir=%cd%\External\UnityPackages

REM Set Package Info
SET PackageName=SpatialAlignment
SET PackageVersion=1.0.0
SET PackageFileName=%PackageName%-%PackageVersion%.unitypackage

ECHO Packaging %PackageName% %PackageVersion%

ECHO Defining Asset List
SET Assets=%Assets% Assets\SpatialAlignment
SET Assets=%Assets% Assets\SpatialAlignment-Examples

ECHO Generating %PackageFileName% ...
%UnityExe% -batchmode -projectPath %ProjectDir%\ -exportPackage %Assets% %PackageDir%\%PackageFileName% -quit

IF ERRORLEVEL 1 GOTO PACKAGINGERROR

:SUCCESS
ECHO "Package Success!"
GOTO END

:PACKAGINGERROR
ECHO "Packaging Error"
PAUSE
GOTO END

:END
popd