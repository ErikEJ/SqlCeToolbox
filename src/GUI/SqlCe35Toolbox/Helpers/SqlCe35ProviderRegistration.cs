using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;

namespace ErikEJ.SqlCeToolbox.Helpers
{
    public class SqlCe35ProviderRegistration : RegistrationAttribute
    {
        const string DataSourceGuid = "F0905790-4262-4019-910D-CF4F06F58F6E";
        const string ProviderGuid = "303D8BB1-D62A-4560-9742-79C93E828222";

        public override void Register(RegistrationContext context)
        {
            Key providerKey = null;
            try
            {
                providerKey = context.CreateKey($@"DataProviders\{{{ProviderGuid}}}");
                providerKey.SetValue(null, "SQL Server Compact 3.5 Provider (Simple by ErikEJ)");
                providerKey.SetValue("AssociatedSource", $"{{{DataSourceGuid}}}");
                providerKey.SetValue("Description", "Provider_Description, ErikEJ.SqlCeToolbox.DDEX35.Properties.Resources");
                providerKey.SetValue("DisplayName", "Provider_DisplayName, ErikEJ.SqlCeToolbox.DDEX35.Properties.Resources");
                providerKey.SetValue("InvariantName", "System.Data.SqlServerCe.3.5");
                providerKey.SetValue("PlatformVersion", "2.0");
                providerKey.SetValue("ShortDisplayName", "Provider_ShortDisplayName, ErikEJ.SqlCeToolbox.DDEX35.Properties.Resources");
                providerKey.SetValue("Technology", "{77AB9A9D-78B9-4ba7-91AC-873F5338F1D2}");
                providerKey.SetValue("CodeBase", "$PackageFolder$\\SqlCeToolbox.DDEX35.dll");

                var supportedObjectsKey = providerKey.CreateSubkey("SupportedObjects");
                supportedObjectsKey.CreateSubkey(nameof(IVsDataConnectionProperties))
                    .SetValue(null, "ErikEJ.SqlCeToolbox.DDEX35.SqlCeConnectionProperties");

                var connectionSupportKey = supportedObjectsKey.CreateSubkey(nameof(IVsDataConnectionSupport));
                connectionSupportKey.SetValue(null, "Microsoft.VisualStudio.Data.Framework.AdoDotNet.AdoDotNetConnectionSupport");
                connectionSupportKey.SetValue("Assembly",
                    "Microsoft.VisualStudio.Data.Framework, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");

                supportedObjectsKey.CreateSubkey(nameof(IVsDataObjectSelector))
                    .SetValue(null, "ErikEJ.SqlCeToolbox.DDEX35.SqlCeObjectSelector");

                var dataObjectSupportKey = supportedObjectsKey.CreateSubkey(nameof(IVsDataObjectSupport));
                dataObjectSupportKey.SetValue(null, "Microsoft.VisualStudio.Data.Framework.DataObjectSupport");
                dataObjectSupportKey.SetValue("Assembly",
                    "Microsoft.VisualStudio.Data.Framework, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                dataObjectSupportKey.SetValue("XmlResource", "ErikEJ.SqlCeToolbox.DDEX35.SqlCeObjectSupport");

                var dataViewSupportKey = supportedObjectsKey.CreateSubkey(nameof(IVsDataViewSupport));
                dataViewSupportKey.SetValue(null, "Microsoft.VisualStudio.Data.Framework.DataViewSupport");
                dataViewSupportKey.SetValue("Assembly",
                    "Microsoft.VisualStudio.Data.Framework, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                dataViewSupportKey.SetValue("XmlResource", "ErikEJ.SqlCeToolbox.DDEX35.SqlCeViewSupport");

                var dataSourceKey = context.CreateKey($@"DataSources\{{{DataSourceGuid}}}");
                dataSourceKey.SetValue(null, "SQL Server Compact 3.5 (ErikEJ)");
                dataSourceKey.SetValue("DefaultProvider", $"{{{ProviderGuid}}}");
                var supportingProviderKey = dataSourceKey
                    .CreateSubkey("SupportingProviders")
                    .CreateSubkey($"{{{ProviderGuid}}}");
                supportingProviderKey.SetValue("Description", "Provider_Description, ErikEJ.SqlCeToolbox.DDEX35.Properties.Resources");
                supportingProviderKey.SetValue("DisplayName", "Provider_DisplayName, ErikEJ.SqlCeToolbox.DDEX35.Properties.Resources");
            }
            finally
            {
                providerKey?.Close();
            }
        }

        public override void Unregister(RegistrationContext context)
        {
            context.RemoveKey($@"DataProviders\{{{ProviderGuid}}}");
            context.RemoveKey($@"DataSources\{{{DataSourceGuid}}}");
        }
    }
}
