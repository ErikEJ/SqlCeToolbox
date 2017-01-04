using Microsoft.SqlServer.Dac;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

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
            dacServiceInstance.Message += (s, e) => _package.SetStatus(e.Message.Message);
            using (var dacpac = DacPackage.Load(dacPacFileName))
            {
                dacServiceInstance.Deploy(dacpac, builder.InitialCatalog,
                                        upgradeExisting: true,
                                        options: dacOptions);
            }
            _package.SetStatus("Database deployed successfully to LocalDB");
        }

        public Task RunDacPackageAsync(SqlConnectionStringBuilder builder, string dacPacFileName, CancellationToken ct = default(CancellationToken))
        {
            return Task.Factory.StartNew(() => RunDacPackage(builder, dacPacFileName), ct); 
        }
    }
}
