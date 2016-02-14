@setlocal enabledelayedexpansion
@rem @set CM3D2_DIR=C:\KISS\CM3D2\
@set PLATFORM=x64

@set "INSTALL_PATH_REG_KEY=HKCU\Software\KISS\カスタムメイド3D2"
@set "INSTALL_PATH_REG_VALUE=InstallPath"

@for /F "usebackq skip=2 tokens=1,2,3" %%A in (`REG QUERY %INSTALL_PATH_REG_KEY% /v %INSTALL_PATH_REG_VALUE%`) do @(
  @set "CM3D2_DIR0=%%C"
)

@if not exist "%CM3D2_DIR%" (
  @set "CM3D2_DIR=%CM3D2_DIR0%"
)

@if not exist "%CM3D2_DIR%\GameData\csv.arc" (
  @set "CM3D2_DIR="
)

@if not exist "%CM3D2_DIR%" (
  @echo "正しいCM3D2のインストールディレクトリを設定してください。指定されたパスは存在しません。：%CM3D2_DIR%"
  @goto end
)


@set CM3D2_MANAGED=%CM3D2_DIR%CM3D2%PLATFORM%_Data\Managed
@set LOADER_DIR=%CM3D2_DIR%Sybaris\Loader

@set OUT_NAME=CM3D2.AlwaysColorChangeEx.Plugin.dll

@SET OPTS=/noconfig /optimize+ /nologo /nostdlib+
@SET OPTS=%OPTS% /t:library /lib:%CM3D2_MANAGED% /r:UnityEngine.dll /r:UnityEngine.UI.dll /r:JsonFx.Json.dll /r:Assembly-CSharp.dll /r:Assembly-CSharp-firstpass.dll
@if exist "%LOADER_DIR%" (
  @SET OPTS=%OPTS% /lib:%LOADER_DIR% /r:UnityInjector.dll /r:ExIni.dll
) else (
  @SET OPTS=%OPTS% /r:UnityInjector.dll /r:ExIni.dll
)

@SET OPTS=%OPTS% /r:%CM3D2_MANAGED%\mscorlib.dll
@SET OPTS=%OPTS% /r:%CM3D2_MANAGED%\System.dll
@SET OPTS=%OPTS% /r:%CM3D2_MANAGED%\System.Core.dll
@SET OPTS=%OPTS% /r:%CM3D2_MANAGED%\System.Data.dll
@SET OPTS=%OPTS% /r:%CM3D2_MANAGED%\System.Xml.dll
@SET OPTS=%OPTS% /r:%CM3D2_MANAGED%\System.Xml.Linq.dll
@SET OPTS=%OPTS% /r:%CM3D2_MANAGED%\System.Drawing.dll
@SET OPTS=%OPTS% /r:%CM3D2_MANAGED%\System.Windows.Forms.dll
@rem @SET OPTS=%OPTS% /r:%CM3D2_MANAGED%\UnityEngine.dll
@rem @SET OPTS=%OPTS% /r:%CM3D2_MANAGED%\UnityEngine.UI.dll

@SET OPTS=%OPTS% /resource:Resource\Folder16.png,folder.png
@SET OPTS=%OPTS% /resource:Resource\Picture16.png,picture.png
@SET OPTS=%OPTS% /resource:Resource\File16.png,file.png

@rem @SET CSC=%windir%\Microsoft.NET\Framework\v3.5\csc
@SET CSC=%windir%\Microsoft.NET\Framework\v4.0.30319\csc


@%CSC% %OPTS% /out:%OUT_NAME% /recurse:*.cs
@if exist %OUT_NAME% @echo. %OUT_NAME%を出力しました
@rem MOVE /Y  %OUT_NAME% %UI_DIR%

:end
@pause
@endlocal