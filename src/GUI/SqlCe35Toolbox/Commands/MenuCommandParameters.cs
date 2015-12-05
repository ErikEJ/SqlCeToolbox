namespace ErikEJ.SqlCeToolbox.Commands
{

    public enum MenuType
    {
        Table,
        View,
        SP,
        Function,
        Manage,
        FK,
        PK
    }

    public class MenuCommandParameters
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public MenuType MenuItemType { get; set; }
        public DatabaseInfo DatabaseInfo { get; set; }

        //public string Caption { get; set; }
        //public Helpers.DatabaseType DatabaseType { get; set; }
        //public bool FromServerExplorer { get; set; }
        //public string ServerVersion { get; set; }
    }
}