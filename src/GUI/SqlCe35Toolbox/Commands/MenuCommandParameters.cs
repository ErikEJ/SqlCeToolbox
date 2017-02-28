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
    }
}