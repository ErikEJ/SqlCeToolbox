echo Running ilmerge...
.\ilmerge /targetplatform:"v4,C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0" /out:..\..\..\..\ExportSqlCe.exe ..\bin\release\exportsqlce.exe QuickGraph.dll QuickGraph.Data.dll QuickGraph.GraphViz.dll

.\ilmerge /targetplatform:"v4,C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0" /out:..\..\..\..\ExportSqlCe40.exe ..\bin\release\exportsqlce40.exe QuickGraph.dll QuickGraph.Data.dll QuickGraph.GraphViz.dll

.\ilmerge /targetplatform:"v4,C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0" /out:..\..\..\..\ExportSqlCe31.exe ..\bin\release\exportsqlce31.exe QuickGraph.dll QuickGraph.Data.dll QuickGraph.GraphViz.dll

.\ilmerge /targetplatform:"v4,C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0" /out:..\..\..\..\Export2SqlCe.exe ..\bin\release\export2sqlce.exe QuickGraph.dll QuickGraph.Data.dll QuickGraph.GraphViz.dll
