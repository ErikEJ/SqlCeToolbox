# SQLite & SQL Server Compact Toolbox

[![Twitter Follow](https://img.shields.io/twitter/follow/ErikEJ.svg?style=social&label=Follow)](http://twitter.com/ErikEJ) [![Join the chat at https://gitter.im/SqlCeToolbox](https://badges.gitter.im/SqlCEToolbox/Lobby.svg)](https://gitter.im/SqlCeToolbox/Lobby)  [![AppVeyor](https://ci.appveyor.com/api/projects/status/r3pv323quuaoqw4f?svg=true)](https://ci.appveyor.com/project/ErikEJ/sqlcetoolbox/branch/master) 

[My tools and utilities for embedded database development](http://erikej.github.io/SqlCeToolbox/)

Visual Studio & SSMS 17.x extension, standalone app and command line tools, for managing all aspects of your SQL Server Compact/SQLite database files' data and schema, including generation of code, database diagrams and database documentation.

If you use my free tools, I would be very grateful for a [rating or review here](https://marketplace.visualstudio.com/items?itemName=ErikEJ.SQLServerCompactSQLiteToolbox#review-details)

# Documentation

[Getting started guide](https://github.com/ErikEJ/SqlCeToolbox/wiki)

[Known issues with workarounds](https://github.com/ErikEJ/SqlCeToolbox/wiki/Known-issues)

[Release notes for released versions and daily builds](https://github.com/ErikEJ/SqlCeToolbox/wiki/Release-notes)

[Command line tools](https://github.com/ErikEJ/SqlCeToolbox/wiki/Command-line-tools)

[Scripting API samples](https://github.com/ErikEJ/SqlCeToolbox/wiki/Scripting-API-samples)

# Downloads/builds

## Visual Studio Extension

**Release**

Download the latest version of the Visual Studio extension (for both 3.5, 4.0 and SQLite) from [Visual Studio MarketPlace](https://marketplace.visualstudio.com/items?itemName=ErikEJ.SQLServerCompactSQLiteToolbox)

Or just install from Tools, Extensions and Updates in Visual Studio! ![](https://github.com/ErikEJ/SqlCeToolbox/blob/master/img/ext.png)

**Daily build**

You can download the daily build from [VSIX Gallery](http://vsixgallery.com/extensions/41521019-e4c7-480c-8ea8-fc4a2c6f50aa/extension.vsix). 

You can also automatically get the [latest build of the Master branch directly in Visual Studio](https://github.com/ErikEJ/SqlCeToolbox/wiki/Subscribing-to-latest-%22daily%22-build)

If you need the Visual Studio 2010 extension, please contact me, and I can provide a link! 

## SQL Server Management Studio (SSMS) 17/18 Extension

You find the extension under the View menu in SSMS.

**Release**

Download the latest version of the SSMS 17 extension (for both 3.5, 4.0, SQLite and SQL Server) from [Visual Studio MarketPlace](https://marketplace.visualstudio.com/items?itemName=ErikEJ.SQLServerCompactSQLiteToolboxforSSMS)

**Daily build**

You can download the daily build of the SSMS 18 extension from [VSIX Gallery](http://vsixgallery.com/extensions/d6c77c32-fe4b-4f6d-ad5d-f7b755212760/extension.vsix)

**Installing the SSMS 17 extension**

Use the following command line:

`"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\vsixinstaller.exe" "full path to extension.vsix"`

**Installing the SSMS 18 extension**

As the VSIX 2017 installer does not support into a VS Shell edition, and generally because SSMS extensions are unsupported, you will have to manually install the extension (at least until I decide to create an installer)

From an administrator command prompt:

Create a folder called "SqlCeToolbox" under `C:\Program Files (x86)\Microsoft SQL Server Management Studio 18\Common7\IDE\Extensions\`

Copy the extension.vsix that you downloaded from VSIX Gallery to the new folder.

Unzip the extension.vsix to the new folder. For example: `"C:\program files\7-zip\7z.exe" e extension.vsix -y`

You should now have 48 files in the SqlCeToolbox folder, and the extension is "installed".

## EF Core Power Tools for Visual Studio 2017 and later

The EF Core Power Tools have moved to [this repository](https://github.com/ErikEJ/EFCorePowerTools)

## Standalone for SQL Server Compact 4.0 and 3.5 SP2 

**Daily build**

You can download the latest 4.0 daily build [from AppVeyor here](https://ci.appveyor.com/api/projects/ErikEJ/sqlcetoolbox/artifacts/SqlCe40ToolBox.zip?branch=master)

You can download the latest 3.5 daily build [from AppVeyor here](https://ci.appveyor.com/api/projects/ErikEJ/sqlcetoolbox/artifacts/SqlCe35ToolBox.zip?branch=master)

## Command line tools

**Release**

You can download the latest release of the command line tools from the [Github releases here](https://github.com/ErikEJ/SqlCeToolbox/releases)

# How do I contribute

If you encounter a bug or have a feature request, please use the [Issue Tracker](https://github.com/ErikEJ/SqlCeToolbox/issues/new)

The Toolbox is open source and welcomes any contribution. There are a number of issues in the [Backlog](https://github.com/ErikEJ/SqlCeToolbox/issues?q=is%3Aissue+milestone%3ABacklog+is%3Aclosed) that the project needs help with!

![](https://github.com/ErikEJ/SqlCeToolbox/blob/master/img/toolbox1.png)
