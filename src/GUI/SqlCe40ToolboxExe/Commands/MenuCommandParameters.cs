namespace ErikEJ.SqlCeToolbox.Commands
{
    public class MenuCommandParameters
    {
        public enum MenuType
        {
            Table,
            View,
            SP,
            Function,
            Manage
        }
        
        public string Connectionstring { get; set; }
        public string Name { get; set; }
        public MenuType MenuItemType { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }
    }
}