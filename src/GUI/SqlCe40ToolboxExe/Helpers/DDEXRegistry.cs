using System;
using Microsoft.Win32;

namespace ErikEJ.SqlCeToolbox.Helpers
{
    internal class DDEXRegistry
    {
        public static void AddDDEX4Registrations()
        {
            //Part 1
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}", "", "SQL Server Compact 4.0 Provider (Simple by ErikEJ)");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}", "AssociatedSource", "{2A7AD6AD-5D61-4817-B45F-681F8D29ECF7}");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}", "Description", "Provider_Description, ErikEJ.SqlCeToolbox.DDEX4.Properties.Resources");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}", "DisplayName", "Provider_DisplayName, ErikEJ.SqlCeToolbox.DDEX4.Properties.Resources");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}", "InvariantName", "System.Data.SqlServerCe.4.0");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}", "PlatformVersion", "2.0");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}", "ShortDisplayName", "Provider_ShortDisplayName, ErikEJ.SqlCeToolbox.DDEX4.Properties.Resources");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}", "Technology", "{77AB9A9D-78B9-4ba7-91AC-873F5338F1D2}");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects", null, "");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataAsyncCommand", null, "");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataCommand", null, "");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataConnectionProperties", "", "Microsoft.VisualStudio.Data.Framework.AdoDotNet.AdoDotNetConnectionProperties");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataConnectionProperties", "Assembly", "Microsoft.VisualStudio.Data.Framework, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataConnectionSupport", "", "Microsoft.VisualStudio.Data.Framework.AdoDotNet.AdoDotNetConnectionSupport");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataConnectionSupport", "Assembly", "Microsoft.VisualStudio.Data.Framework, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataConnectionUIProperties", "", "Microsoft.VisualStudio.Data.Framework.AdoDotNet.AdoDotNetConnectionProperties");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataConnectionUIProperties", "Assembly", "Microsoft.VisualStudio.Data.Framework, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataObjectSelector", "", "ErikEJ.SqlCeToolbox.DDEX4.SqlCeObjectSelector");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataObjectSupport", "", "Microsoft.VisualStudio.Data.Framework.DataObjectSupport");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataObjectSupport", "Assembly", "Microsoft.VisualStudio.Data.Framework, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataObjectSupport", "XmlResource", "ErikEJ.SqlCeToolbox.DDEX4.SqlCeObjectSupport");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "", "ErikEJ.SqlCeToolbox.DDEX4.SqlCeSourceInformation");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "SupportsAnsi92Sql", "True");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "SupportsQuotedIdentifierParts", "True");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "IdentifierOpenQuote", "[");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "IdentifierCloseQuote", "]");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "ServerSeparator", "");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "SchemaSupported", "False");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "SchemaSupportedInDml", "False");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "SchemaSeparator", "");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "ParameterPrefix", "@");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "ParameterPrefixInName", "True");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "DataSourceProduct", "SQL Server Compact 4.0 (Simple by ErikEJ)");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "DataSourceVersion", "4.0");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataViewSupport", "", "Microsoft.VisualStudio.Data.Framework.DataViewSupport");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataViewSupport", "Assembly", "Microsoft.VisualStudio.Data.Framework, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataViewSupport", "XmlResource", "ErikEJ.SqlCeToolbox.DDEX4.SqlCeViewSupport");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataSources\{2A7AD6AD-5D61-4817-B45F-681F8D29ECF7}", "", "SQL Server Compact 4.0 (ErikEJ)");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataSources\{2A7AD6AD-5D61-4817-B45F-681F8D29ECF7}", "DefaultProvider", "{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataSources\{2A7AD6AD-5D61-4817-B45F-681F8D29ECF7}\SupportingProviders", null, "");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataSources\{2A7AD6AD-5D61-4817-B45F-681F8D29ECF7}\SupportingProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}", "Description", "Provider_Description, ErikEJ.SqlCeToolbox.DDEX4.Properties.Resources");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VWDExpress\12.0_Config\DataSources\{2A7AD6AD-5D61-4817-B45F-681F8D29ECF7}\SupportingProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}", "DisplayName", "Provider_DisplayName, ErikEJ.SqlCeToolbox.DDEX4.Properties.Resources");


            //Part 2
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}", "", "SQL Server Compact 4.0 Provider (Simple by ErikEJ)");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}", "AssociatedSource", "{2A7AD6AD-5D61-4817-B45F-681F8D29ECF7}");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}", "Description", "Provider_Description, ErikEJ.SqlCeToolbox.DDEX4.Properties.Resources");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}", "DisplayName", "Provider_DisplayName, ErikEJ.SqlCeToolbox.DDEX4.Properties.Resources");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}", "InvariantName", "System.Data.SqlServerCe.4.0");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}", "PlatformVersion", "2.0");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}", "ShortDisplayName", "Provider_ShortDisplayName, ErikEJ.SqlCeToolbox.DDEX4.Properties.Resources");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}", "Technology", "{77AB9A9D-78B9-4ba7-91AC-873F5338F1D2}");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects", null, "");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataAsyncCommand", null, "");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataCommand", null, "");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataConnectionProperties", "", "Microsoft.VisualStudio.Data.Framework.AdoDotNet.AdoDotNetConnectionProperties");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataConnectionProperties", "Assembly", "Microsoft.VisualStudio.Data.Framework, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataConnectionSupport", "", "Microsoft.VisualStudio.Data.Framework.AdoDotNet.AdoDotNetConnectionSupport");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataConnectionSupport", "Assembly", "Microsoft.VisualStudio.Data.Framework, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataConnectionUIProperties", "", "Microsoft.VisualStudio.Data.Framework.AdoDotNet.AdoDotNetConnectionProperties");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataConnectionUIProperties", "Assembly", "Microsoft.VisualStudio.Data.Framework, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataObjectSelector", "", "ErikEJ.SqlCeToolbox.DDEX4.SqlCeObjectSelector");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataObjectSupport", "", "Microsoft.VisualStudio.Data.Framework.DataObjectSupport");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataObjectSupport", "Assembly", "Microsoft.VisualStudio.Data.Framework, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataObjectSupport", "XmlResource", "ErikEJ.SqlCeToolbox.DDEX4.SqlCeObjectSupport");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "", "ErikEJ.SqlCeToolbox.DDEX4.SqlCeSourceInformation");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "SupportsAnsi92Sql", "True");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "SupportsQuotedIdentifierParts", "True");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "IdentifierOpenQuote", "[");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "IdentifierCloseQuote", "]");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "ServerSeparator", "");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "SchemaSupported", "False");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "SchemaSupportedInDml", "False");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "SchemaSeparator", "");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "ParameterPrefix", "@");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "ParameterPrefixInName", "True");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "DataSourceProduct", "SQL Server Compact 4.0 (Simple by ErikEJ)");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataSourceInformation", "DataSourceVersion", "4.0");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataViewSupport", "", "Microsoft.VisualStudio.Data.Framework.DataViewSupport");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataViewSupport", "Assembly", "Microsoft.VisualStudio.Data.Framework, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}\SupportedObjects\IVsDataViewSupport", "XmlResource", "ErikEJ.SqlCeToolbox.DDEX4.SqlCeViewSupport");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataSources\{2A7AD6AD-5D61-4817-B45F-681F8D29ECF7}", "", "SQL Server Compact 4.0 (ErikEJ)");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataSources\{2A7AD6AD-5D61-4817-B45F-681F8D29ECF7}", "DefaultProvider", "{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataSources\{2A7AD6AD-5D61-4817-B45F-681F8D29ECF7}\SupportingProviders", null, "");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataSources\{2A7AD6AD-5D61-4817-B45F-681F8D29ECF7}\SupportingProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}", "Description", "Provider_Description, ErikEJ.SqlCeToolbox.DDEX4.Properties.Resources");
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\WDExpress\12.0_Config\DataSources\{2A7AD6AD-5D61-4817-B45F-681F8D29ECF7}\SupportingProviders\{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}", "DisplayName", "Provider_DisplayName, ErikEJ.SqlCeToolbox.DDEX4.Properties.Resources");

        }
    }
}
