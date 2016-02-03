@setlocal
@rem @set CM3D2_DIR=C:\KISS\CM3D2

@set "INSTALL_PATH_REG_KEY=HKCU\Software\KISS\カスタムメイド3D2"
@set "INSTALL_PATH_REG_VALUE=InstallPath"
@for /F "usebackq skip=2 tokens=1-2*" %%A in (`REG QUERY %INSTALL_PATH_REG_KEY% /v %INSTALL_PATH_REG_VALUE% 2^>nul`) do (
  @set "CM3D2_DIR=%%C"
)
@if not exist "%CM3D2_DIR%\GameData\csv.arc" (
  @set "CM3D2_DIR="
)

@if not exist "%CM3D2_DIR%" (
  @echo "正しいCM3D2のインストールディレクトリを設定してください。指定されたパスは存在しません。：%CM3D2_DIR%"
  @goto end
)

@set CM3D2_MANAED=%CM3D2_DIR%\CM3D2x64_Data\Managed
@set LOADER_DIR=%CM3D2_DIR%\Sybaris\Loader
@set UI_DIR=%CM3D2_DIR%\Sybaris\Plugins\UnityInjector
@set OUT_NAME=CM3D2.AlwaysColorChangeEx.Plugin.dll
@set OPTS=/optimize+ /t:library /lib:%CM3D2_MANAED% /r:UnityEngine.dll /r:UnityEngine.UI.dll /lib:%LOADER_DIR% /r:UnityInjector.dll /r:ExIni.dll /r:Assembly-CSharp.dll /r:Assembly-CSharp-firstpass.dll /lib:%UI_DIR%
@set OPTS=%OPTS% /nowarn:618,168
@rem %windir%\Microsoft.NET\Framework\v3.5\csc %OPTS% /define:DEBUG /out:%OUT_NAME% /recurse:*.cs
@rem @%windir%\Microsoft.NET\Framework\v3.5\csc %OPTS% /out:%OUT_NAME% /recurse:Util\*.cs /recurse:Data\*.cs *.cs
@%windir%\Microsoft.NET\Framework\v3.5\csc %OPTS% /out:%OUT_NAME% /recurse:*.cs
@rem MOVE /Y  %OUT_NAME% %UI_DIR%

:end
@pause
@endlocal