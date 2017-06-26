using ErikEJ.SqlCeScripting;

namespace ErikEJ.SqlCeToolbox
{
    public static class RepoHelper
    {

#if V35
        public static string apiVer = "3.5";
#else
        public static string apiVer = "4.0";
#endif

        public static IRepository CreateRepository(string connectionString)
        {
#if V35
            return new DBRepository(connectionString);
#else
            return new DB4Repository(connectionString);
#endif
        }

        public static IRepository CreateServerRepository(string connectionString)
        {
#if V35
            return new ServerDBRepository(connectionString, Properties.Settings.Default.KeepServerSchemaNames);
#else
            return new ServerDBRepository4(connectionString, Properties.Settings.Default.KeepServerSchemaNames);
#endif
        }


        public static IGenerator CreateGenerator(IRepository repo, string file = null)
        {
#if V35
            return new Generator(repo, file, false, false, false, Properties.Settings.Default.KeepServerSchemaNames);
#else
            return new Generator4(repo, file, false, false, false, Properties.Settings.Default.KeepServerSchemaNames);
#endif
        }

        public static ISqlCeHelper CreateHelper()
        {
#if V35
            return new SqlCeHelper();
#else
            return new SqlCeHelper4();
#endif
            
        }

    }
}
