using Microsoft.SqlServer.Dac;
using System.Data.SqlClient;

namespace ErikEJ.SqlCeToolbox.Helpers
{
    public class DacFxHelper
    {
        private readonly SqlCeToolboxPackage _package;
        public DacFxHelper(SqlCeToolboxPackage package)
        {
            _package = package;
        }

        public void RunDacPackage(SqlConnectionStringBuilder builder, string dacPacFileName)
        {
            var dacOptions = new DacDeployOptions {BlockOnPossibleDataLoss = true};

            var dacServiceInstance = new DacServices(builder.ConnectionString);
            dacServiceInstance.ProgressChanged += (s, e) => _package.SetStatus(e.Message);
            dacServiceInstance.Message += (s, e) => _package.SetStatus(e.Message.Message);
            using (DacPackage dacpac = DacPackage.Load(dacPacFileName))
            {
                dacServiceInstance.Deploy(dacpac, builder.InitialCatalog,
                                        upgradeExisting: true,
                                        options: dacOptions);
            }
        }
    }
}
