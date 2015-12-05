
echo Running ilmerge...
.\ilmerge /targetplatform:"v4,C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0" /out:C:\Users\Erik\SkyDrive\Dokumenter\Code\SQLCE\ExportSqlCe.exe ..\bin\release\exportsqlce.exe QuickGraph.dll QuickGraph.Data.dll QuickGraph.GraphViz.dll

.\ilmerge /targetplatform:"v4,C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0" /out:C:\Users\Erik\SkyDrive\Dokumenter\Code\SQLCE\ExportSqlCe40.exe ..\bin\release\exportsqlce40.exe QuickGraph.dll QuickGraph.Data.dll QuickGraph.GraphViz.dll

.\ilmerge /targetplatform:"v4,C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0" /out:C:\Users\Erik\SkyDrive\Dokumenter\Code\SQLCE\ExportSqlCe31.exe ..\bin\release\exportsqlce31.exe QuickGraph.dll QuickGraph.Data.dll QuickGraph.GraphViz.dll

.\ilmerge /targetplatform:"v4,C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0" /out:C:\Users\Erik\SkyDrive\Dokumenter\Code\SQLCE\Export2SqlCe.exe ..\bin\release\export2sqlce.exe QuickGraph.dll QuickGraph.Data.dll QuickGraph.GraphViz.dll

del C:\Users\Erik\SkyDrive\Dokumenter\Code\SQLCE\exp*.pdb

echo Copying new Scripting API files...

copy C:\Users\Erik\SkyDrive\Dokumenter\Code\SQLCE\CodePlexTFS\TFS02\exportsqlce\bin\Release\*.dll C:\Users\Erik\SkyDrive\Dokumenter\Code\SQLCE

pause