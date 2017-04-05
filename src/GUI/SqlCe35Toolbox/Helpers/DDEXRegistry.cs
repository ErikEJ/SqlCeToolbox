using Microsoft.Win32;

namespace ErikEJ.SqlCeToolbox.Helpers
{
    internal static class DdexRegistry
    {
        public static void AddDdex4Registrations(string ver)
        {
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}", ver),"", "SQL Server Compact 4.0 Provider (Simple by ErikEJ)");
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}", ver), "AssociatedSource", "{2A7AD6AD-5D61-4817-B45F-681F8D29ECF7}");
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}", ver), "Description", "Provider_Description, ErikEJ.SqlCeToolbox.DDEX4.Properties.Resources");
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}", ver), "DisplayName", "Provider_DisplayName, ErikEJ.SqlCeToolbox.DDEX4.Properties.Resources");
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}", ver), "InvariantName", "System.Data.SqlServerCe.4.0");
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}", ver), "PlatformVersion", "2.0");
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}", ver), "ShortDisplayName", "Provider_ShortDisplayName, ErikEJ.SqlCeToolbox.DDEX4.Properties.Resources");
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}", ver), "Technology", "{77AB9A9D-78B9-4ba7-91AC-873F5338F1D2}");

            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}\SupportedObjects", ver), null, "");
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}\SupportedObjects\IVsDataAsyncCommand", ver), null, "");
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}\SupportedObjects\IVsDataCommand", ver), null, "");
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}\SupportedObjects\IVsDataConnectionProperties", ver), "", "Microsoft.VisualStudio.Data.Framework.AdoDotNet.AdoDotNetConnectionProperties");
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}\SupportedObjects\IVsDataConnectionProperties", ver), "Assembly", string.Format("Microsoft.VisualStudio.Data.Framework, Version={0}.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", ver));
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}\SupportedObjects\IVsDataConnectionSupport", ver), "", "Microsoft.VisualStudio.Data.Framework.AdoDotNet.AdoDotNetConnectionSupport");
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}\SupportedObjects\IVsDataConnectionSupport", ver), "Assembly", string.Format("Microsoft.VisualStudio.Data.Framework, Version={0}.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", ver));
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}\SupportedObjects\IVsDataConnectionUIProperties", ver), "", "Microsoft.VisualStudio.Data.Framework.AdoDotNet.AdoDotNetConnectionProperties");
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}\SupportedObjects\IVsDataConnectionUIProperties", ver), "Assembly", string.Format("Microsoft.VisualStudio.Data.Framework, Version={0}.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", ver));
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}\SupportedObjects\IVsDataObjectSelector", ver), "", "ErikEJ.SqlCeToolbox.DDEX4.SqlCeObjectSelector");
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}\SupportedObjects\IVsDataObjectSupport", ver), "", "Microsoft.VisualStudio.Data.Framework.DataObjectSupport");
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}\SupportedObjects\IVsDataObjectSupport", ver), "Assembly", string.Format("Microsoft.VisualStudio.Data.Framework, Version={0}.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", ver));
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}\SupportedObjects\IVsDataObjectSupport", ver), "XmlResource", "ErikEJ.SqlCeToolbox.DDEX4.SqlCeObjectSupport");
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}\SupportedObjects\IVsDataSourceInformation", ver), "", "ErikEJ.SqlCeToolbox.DDEX4.SqlCeSourceInformation");
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}\SupportedObjects\IVsDataViewSupport", ver), "", "Microsoft.VisualStudio.Data.Framework.DataViewSupport");
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}\SupportedObjects\IVsDataViewSupport", ver), "Assembly", string.Format("Microsoft.VisualStudio.Data.Framework, Version={0}.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", ver));
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}\SupportedObjects\IVsDataViewSupport", ver), "XmlResource", "ErikEJ.SqlCeToolbox.DDEX4.SqlCeViewSupport");
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataSources\{{2A7AD6AD-5D61-4817-B45F-681F8D29ECF7}}", ver), "", "SQL Server Compact 4.0 (ErikEJ)");
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataSources\{{2A7AD6AD-5D61-4817-B45F-681F8D29ECF7}}", ver), "DefaultProvider", "{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}");
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataSources\{{2A7AD6AD-5D61-4817-B45F-681F8D29ECF7}}\SupportingProviders", ver), null, "");
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataSources\{{2A7AD6AD-5D61-4817-B45F-681F8D29ECF7}}\SupportingProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}", ver), "Description", "Provider_Description, ErikEJ.SqlCeToolbox.DDEX4.Properties.Resources");
            Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataSources\{{2A7AD6AD-5D61-4817-B45F-681F8D29ECF7}}\SupportingProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}", ver), "DisplayName", "Provider_DisplayName, ErikEJ.SqlCeToolbox.DDEX4.Properties.Resources");
        }
    }
}
