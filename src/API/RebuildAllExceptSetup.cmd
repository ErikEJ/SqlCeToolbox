set ORIGINAL_DIR=%CD% 

call "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\Tools\vsdevcmd.bat"

chdir /d %ORIGINAL_DIR% 

@REM  msbuild Export2SQLCE.sln /t:Rebuild /p:Configuration=Release
@REM  copy bin\Release\*.dll ..\..\..\
@REM  copy bin\x86\Release\*.dll ..\..\..\
@REM  cd lib
@REM  .\ilmerge /targetplatform:"v4,C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2" /out:..\..\..\..\Export2SqlCe.exe ..\bin\release\export2sqlce.exe QuickGraph.dll QuickGraph.Data.dll QuickGraph.GraphViz.dll
@REM  cd ..
@REM  pause

@REM  msbuild ExportSQLCE.sln /t:Rebuild /p:Configuration=Release
@REM  copy bin\Release\*.dll ..\..\..\
@REM  copy bin\x86\Release\*.dll ..\..\..\
@REM  cd lib
@REM  .\ilmerge /targetplatform:"v4,C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2" /out:..\..\..\..\ExportSqlCe.exe ..\bin\release\exportsqlce.exe QuickGraph.dll QuickGraph.Data.dll QuickGraph.GraphViz.dll
@REM  cd ..
@REM  pause


@REM  msbuild ExportSQLCE40.sln /t:Rebuild /p:Configuration=Release
@REM  copy bin\Release\*.dll ..\..\..\
@REM  copy bin\x86\Release\*.dll ..\..\..\
@REM  cd lib
@REM  .\ilmerge /targetplatform:"v4,C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2" /out:..\..\..\..\ExportSqlCe40.exe ..\bin\release\exportsqlce40.exe QuickGraph.dll QuickGraph.Data.dll QuickGraph.GraphViz.dll
@REM  cd ..
@REM  pause

msbuild SqlCeScripting40.sln /p:Configuration=Release
copy bin\Release\*.dll ..\..\..\
copy bin\x86\Release\*.dll ..\..\..\
copy bin\x64\Release\*.dll ..\..\..\
pause