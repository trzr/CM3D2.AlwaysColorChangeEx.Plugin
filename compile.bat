@setlocal enabledelayedexpansion

@set BASE_DIR_=C:\KISS\COM3D2\

@set "INSTALL_PATH_REG_KEY=HKCU\Software\KISS\カスタムオーダーメイド3D2"
@set "INSTALL_PATH_REG_VALUE=InstallPath"
@for /F "usebackq skip=2 tokens=1-2*" %%A in (`REG QUERY %INSTALL_PATH_REG_KEY% /v %INSTALL_PATH_REG_VALUE% 2^>nul`) do (
  @set "BASE_DIR=%%C"
)

@if not exist "%BASE_DIR%\GameData\csv.arc" (
  @set "BASE_DIR="
)

@if not exist "%BASE_DIR%" (
  @if not exist "%BASE_DIR_%" (
    @echo "正しいCOM3D2のインストールディレクトリを設定してください。指定されたパスは存在しません。：%BASE_DIR%"
    @goto end
  ) else (
    @set BASE_DIR=%BASE_DIR_%
  )
)
@echo "target: %BASE_DIR%"
@set MANAGED=%BASE_DIR%COM3D2x64_Data\Managed
@set LOADER_DIR=%BASE_DIR%Sybaris
@set UI_DIR=%BASE_DIR%Sybaris\UnityInjector

@set OUT_NAME=COM3D2.AlwaysColorChangeEx.Plugin.dll
@set OPTS=%OPTS% /define:COM3D2;UNITY_5_6_OR_NEWER;UNITY_5_5_OR_NEWER;
@set OPTS=%OPTS% /noconfig /optimize+ /nologo /nostdlib+
@set OPTS=%OPTS% /t:library /lib:%MANAGED% /r:UnityEngine.dll /r:UnityEngine.UI.dll /r:JsonFx.Json.dll /r:Assembly-CSharp.dll /r:Assembly-CSharp-firstpass.dll
@if exist "%LOADER_DIR%" (
  @set OPTS=%OPTS% /lib:%LOADER_DIR% /r:UnityInjector.dll /r:ExIni.dll
) else (
  @set OPTS=%OPTS% /r:UnityInjector.dll /r:ExIni.dll
)
@rem @set OPTS=%OPTS% /lib:%UI_DIR% /r:CM3D2.DynamicLoader.Plugin.dll

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

@rem @set OPTS=%OPTS% /resource:Resource\Folder16.png,folder.png
@rem @set OPTS=%OPTS% /resource:Resource\Picture16.png,picture.png
@rem @set OPTS=%OPTS% /resource:Resource\File16.png,file.png
@rem @set OPTS=%OPTS% /resource:Resource\copy16.png,copy.png
@rem @set OPTS=%OPTS% /resource:Resource\paste16.png,paste.png
@rem @set OPTS=%OPTS% /resource:Resource\plus16.png,plus.png
@rem @set OPTS=%OPTS% /resource:Resource\minus16.png,minus.png
@rem @set OPTS=%OPTS% /resource:Resource\check_on16.png,checkon.png
@rem @set OPTS=%OPTS% /resource:Resource\check_off16.png,checkoff.png


@rem @set CSC=%windir%\Microsoft.NET\Framework\v3.5\csc
@set CSC=%windir%\Microsoft.NET\Framework\v4.0.30319\csc

@%CSC% %OPTS% /out:%OUT_NAME% /recurse:Data\*.cs /recurse:Util\*.cs /recurse:UI\*.cs /recurse:Render\*.cs *.cs
@rem MOVE /Y  %OUT_NAME% %UI_DIR%
@if exist %OUT_NAME% @echo. %OUT_NAME%を出力しました

@if "%1" == "MOVE" (MOVE /Y  %OUT_NAME% %UI_DIR%)
@echo %OPTS% %1

@rem @pause
@endlocal
