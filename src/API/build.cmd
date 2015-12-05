call rebuildallexceptsetup.cmd
cd lib
call ilmexpsqlce.cmd
del C:\Users\Erik\SkyDrive\Dokumenter\Code\SQLCE\exp*.zip
del C:\Users\Erik\SkyDrive\Dokumenter\Code\SQLCE\sqlce*.zip
"c:\program files\7-zip\7z" a C:\Users\Erik\SkyDrive\Dokumenter\Code\SQLCE\exportsqlce.zip C:\Users\Erik\SkyDrive\Dokumenter\Code\SQLCE\exportsqlce.exe
"c:\program files\7-zip\7z" a C:\Users\Erik\SkyDrive\Dokumenter\Code\SQLCE\exportsqlce40.zip C:\Users\Erik\SkyDrive\Dokumenter\Code\SQLCE\exportsqlce40.exe
"c:\program files\7-zip\7z" a C:\Users\Erik\SkyDrive\Dokumenter\Code\SQLCE\exportsqlce31.zip C:\Users\Erik\SkyDrive\Dokumenter\Code\SQLCE\exportsqlce31.exe
"c:\program files\7-zip\7z" a C:\Users\Erik\SkyDrive\Dokumenter\Code\SQLCE\export2sqlce.zip C:\Users\Erik\SkyDrive\Dokumenter\Code\SQLCE\export2sqlce.exe
"c:\program files\7-zip\7z" a C:\Users\Erik\SkyDrive\Dokumenter\Code\SQLCE\SqlCeScripting.zip C:\Users\Erik\SkyDrive\Dokumenter\Code\SQLCE\*.dll
cd ..
cd ..
dir *.zip
pause