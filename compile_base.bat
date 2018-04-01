@rem @setlocal enabledelayedexpansion

@set BASE_DIR_=C:\KISS\%NAMEKEY%\
@set "INSTALL_PATH_REG_KEY=HKCU\Software\KISS\%REGKEY%"
@set "INSTALL_PATH_REG_VALUE=InstallPath"
@for /F "usebackq skip=2 tokens=1-2*" %%A in (`REG QUERY %INSTALL_PATH_REG_KEY% /v %INSTALL_PATH_REG_VALUE% 2^>nul`) do (
  @set "BASE_DIR=%%C"
)

@if not exist "%BASE_DIR%\GameData\csv.arc" (
  @set "BASE_DIR="
)

@if not exist "%BASE_DIR%" (
  @if not exist "%BASE_DIR_%" (
    @echo "正しい%NAMEKEY%のインストールディレクトリを設定してください。指定されたパスは存在しません。：%BASE_DIR%"
    @goto end
  ) else (
    @set BASE_DIR=%BASE_DIR_%
  )
)
@echo "Target: %BASE_DIR%"

@set MANAGED=%BASE_DIR%%NAMEKEY%x64_Data\Managed
@if "%NAMEKEY%" == "COM3D2" (
  @set LOADER_DIR=%BASE_DIR%Sybaris
  @set UI_DIR=%BASE_DIR%Sybaris\UnityInjector
) else (
  @set LOADER_DIR=%BASE_DIR%Sybaris\Loader
  @set UI_DIR=%BASE_DIR%Sybaris\Plugins\UnityInjector
)

@set OUT_NAME=%NAMEKEY%.AlwaysColorChangeEx.Plugin.dll

@set OPTS=%OPTS% /noconfig /optimize+ /nologo /nostdlib+
@set OPTS=%OPTS% /t:library /lib:%MANAGED% /r:UnityEngine.dll /r:UnityEngine.UI.dll /r:JsonFx.Json.dll /r:Assembly-CSharp.dll /r:Assembly-CSharp-firstpass.dll

@if exist "%LOADER_DIR%" (
  @set OPTS=%OPTS% /lib:%LOADER_DIR% /r:UnityInjector.dll /r:ExIni.dll
) else (
  @set OPTS=%OPTS% /r:UnityInjector.dll /r:ExIni.dll
)
@if "%UI_OPTS%" neq "" (
  @set OPTS=%OPTS% /lib:%UI_DIR% %UI_OPTS%
)

@rem @set OPTS=%OPTS% /nowarn:618,168
@rem @set OPTS=%OPTS% /define:DEBUG

@rem @set OPTS=%OPTS% /r:%windir%\Microsoft.NET\Framework\v2.0.50727\mscorlib.dll
@rem @set OPTS=%OPTS% /r:%windir%\Microsoft.NET\Framework\v2.0.50727\System.dll
@set OPTS=%OPTS% /r:%MANAGED%\mscorlib.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.Core.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.Data.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.Xml.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.Xml.Linq.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.Drawing.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.Windows.Forms.dll
@rem @set OPTS=%OPTS% /r:%MANAGED%\UnityEngine.dll
@rem @set OPTS=%OPTS% /r:%MANAGED%\UnityEngine.UI.dll

@set OPTS=%OPTS% /resource:Resource\Folder16.png,folder.png
@set OPTS=%OPTS% /resource:Resource\Picture16.png,picture.png
@set OPTS=%OPTS% /resource:Resource\File16.png,file.png
@set OPTS=%OPTS% /resource:Resource\copy16.png,copy.png
@set OPTS=%OPTS% /resource:Resource\paste16.png,paste.png
@set OPTS=%OPTS% /resource:Resource\plus16.png,plus.png
@set OPTS=%OPTS% /resource:Resource\minus16.png,minus.png
@set OPTS=%OPTS% /resource:Resource\check_on16.png,checkon.png
@set OPTS=%OPTS% /resource:Resource\check_off16.png,checkoff.png

@rem @set CSC=%windir%\Microsoft.NET\Framework\v3.5\csc
@set CSC=%windir%\Microsoft.NET\Framework\v4.0.30319\csc

@%CSC% %OPTS% /out:%OUT_NAME% *.cs /recurse:Data\*.cs /recurse:Util\*.cs /recurse:UI\*.cs /recurse:Render\*.cs %POST_OPTS%
@rem MOVE /Y  %OUT_NAME% %UI_DIR%
@if exist %OUT_NAME% @echo. %OUT_NAME%を出力しました

@rem @if "%1" == "MOVE" (MOVE /Y  %OUT_NAME% %UI_DIR%)

@rem @pause
@rem @endlocal
