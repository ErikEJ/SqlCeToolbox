del bin\*.* /S /Q

call rebuildallexceptsetup.cmd

del ..\..\..\*.pdb
del ..\..\..\exp*.zip
del ..\..\..\sqlce*.zip
"c:\program files\7-zip\7z" a ..\..\..\exportsqlce.zip ..\..\..\exportsqlce.exe
"c:\program files\7-zip\7z" a ..\..\..\exportsqlce40.zip ..\..\..\exportsqlce40.exe
"c:\program files\7-zip\7z" a ..\..\..\exportsqlce31.zip ..\..\..\exportsqlce31.exe
"c:\program files\7-zip\7z" a ..\..\..\export2sqlce.zip ..\..\..\export2sqlce.exe
"c:\program files\7-zip\7z" a ..\..\..\SqlCeScripting.zip ..\..\..\*.dll
dir ..\..\..\*.zip
pause