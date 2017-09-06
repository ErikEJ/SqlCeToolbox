using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Reflection;

namespace ReverseEngineer20
{
    public class EFCoreModelAnalyzer
    {
        public string GenerateDebugView(dynamic contextType)
        {
            try
            {
                Type type = contextType;

                var test = typeof(DbContext).GetTypeInfo().IsAssignableFrom(type);

                var info = type.GetTypeInfo();

                DbContext dbContext = Activator.CreateInstance(type) as DbContext;

                //var dbContext = new DbContextOperations(

                //        new OperationReporter(null),

                //        contextType.Assembly,

                //        contextType.Assembly)

                //    .CreateContext(contextType.FullName);

                //var dbContext = DbContextActivator.CreateInstance(type);

                return dbContext.Model.AsModel().DebugView.View;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
