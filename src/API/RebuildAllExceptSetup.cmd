call "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86

devenv Export2SQLCE.sln /rebuild "Release"

devenv ExportSQLCE.sln /rebuild "Release"

devenv ExportSQLCe31.sln /rebuild "Release"

devenv ExportSQLCE40.sln /rebuild "Release"

devenv SqlCeScripting40.sln /rebuild "Release"

pause

