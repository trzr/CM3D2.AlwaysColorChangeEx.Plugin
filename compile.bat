@setlocal

@SET CM3D2_DIR=C:\KISS\CM3D2
@SET CM3D2_MANAGED=%CM3D2_DIR%\CM3D2x64_Data\Managed
@SET LOADER_DIR=%CM3D2_DIR%\Sybaris\Loader
@SET UI_DIR=%CM3D2_DIR%\Sybaris\Plugins\UnityInjector

@SET OUT_NAME=CM3D2.AlwaysColorChangeEx.Plugin.dll
@SET OPTS=%OPTS% /noconfig /optimize+ /nologo /nostdlib+
@SET OPTS=%OPTS% /t:library /lib:%CM3D2_MANAGED% /r:UnityEngine.dll /r:UnityEngine.UI.dll /r:JsonFx.Json.dll /r:Assembly-CSharp.dll /r:Assembly-CSharp-firstpass.dll
@SET OPTS=%OPTS% /lib:%LOADER_DIR% /r:UnityInjector.dll /r:ExIni.dll
@rem @SET OPTS=%OPTS% /lib:%UI_DIR% /r:CM3D2.DynamicLoader.Plugin.dll

@rem @SET OPTS=%OPTS% /nowarn:618,168
@rem @SET OPTS=%OPTS% /define:DEBUG

@rem @SET OPTS=%OPTS% /r:%windir%\Microsoft.NET\Framework\v2.0.50727\mscorlib.dll
@rem @SET OPTS=%OPTS% /r:%windir%\Microsoft.NET\Framework\v2.0.50727\System.dll
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

@rem @SET OPTS=%OPTS% /resource:Resource\Folder16.png,folder.png
@rem @SET OPTS=%OPTS% /resource:Resource\Picture16.png,picture.png
@rem @SET OPTS=%OPTS% /resource:Resource\File16.png,file.png
@rem @SET OPTS=%OPTS% /resource:Resource\copy16.png,copy.png
@rem @SET OPTS=%OPTS% /resource:Resource\paste16.png,paste.png
@rem @SET OPTS=%OPTS% /resource:Resource\plus16.png,plus.png
@rem @SET OPTS=%OPTS% /resource:Resource\minus16.png,minus.png

@rem @SET CSC=%windir%\Microsoft.NET\Framework\v3.5\csc
@SET CSC=%windir%\Microsoft.NET\Framework\v4.0.30319\csc

@%CSC% %OPTS% /out:%OUT_NAME% /recurse:Data\*.cs /recurse:Util\*.cs /recurse:UI\*.cs *.cs
@rem MOVE /Y  %OUT_NAME% %UI_DIR%
@if exist %OUT_NAME% @echo. %OUT_NAME%ÇèoóÕÇµÇ‹ÇµÇΩ

@if "%1" == "MOVE" (MOVE /Y  %OUT_NAME% %UI_DIR%)
@echo "%OPTS%" "%1"


@rem @pause
@endlocal
