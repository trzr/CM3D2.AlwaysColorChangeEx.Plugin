SET CM3D2_Managed=C:\KISS\CM3D2_KAIZOU\CM3D2x64_Data\Managed
C:\Windows\Microsoft.NET\Framework\v3.5\csc /t:library /lib:%CM3D2_Managed% /r:UnityEngine.dll /r:UnityInjector.dll /r:Assembly-CSharp.dll /r:Assembly-CSharp-firstpass.dll *.cs
pause
