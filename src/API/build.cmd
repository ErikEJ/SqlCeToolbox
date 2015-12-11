call rebuildallexceptsetup.cmd
cd lib
call ilmexpsqlce.cmd
cd ..
echo Copying new Scripting API files...
copy bin\Release\*.dll ..\..\..\

del ..\..\..\*.pdb
del ..\..\..\exp*.zip
del ..\..\..\sqlce*.zip
pause
"c:\program files\7-zip\7z" a ..\..\..\exportsqlce.zip ..\..\..\exportsqlce.exe
"c:\program files\7-zip\7z" a ..\..\..\exportsqlce40.zip ..\..\..\exportsqlce40.exe
"c:\program files\7-zip\7z" a ..\..\..\exportsqlce31.zip ..\..\..\exportsqlce31.exe
"c:\program files\7-zip\7z" a ..\..\..\export2sqlce.zip ..\..\..\export2sqlce.exe
"c:\program files\7-zip\7z" a ..\..\..\SqlCeScripting.zip ..\..\..\*.dll
cd ..\..\..\
dir *.zip
pause