@SET CM3D2_DIR=C:\KISS\CM3D2
@SET CM3D2_MANAED=%CM3D2_DIR%\CM3D2x64_Data\Managed
@SET LOADER_DIR=%CM3D2_DIR%\Sybaris\Loader
@SET UI_DIR=%CM3D2_DIR%\Sybaris\Plugins\UnityInjector
@SET OUT_NAME=CM3D2.AlwaysColorChange.Plugin.dll
@SET OPTS=/optimize /t:library /lib:%CM3D2_MANAED% /r:UnityEngine.dll /r:UnityEngine.UI.dll /lib:%LOADER_DIR% /r:UnityInjector.dll /r:ExIni.dll /r:Assembly-CSharp.dll /r:Assembly-CSharp-firstpass.dll /lib:%UI_DIR%

@rem %windir%\Microsoft.NET\Framework\v3.5\csc %OPTS% /define:DEBUG /out:%OUT_NAME% /recurse:*.cs
@%windir%\Microsoft.NET\Framework\v3.5\csc %OPTS% /out:%OUT_NAME% /recurse:*.cs
@rem MOVE /Y  %OUT_NAME% %UI_DIR%

@pause
