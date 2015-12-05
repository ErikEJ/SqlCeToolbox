using System.ComponentModel;
using System;

[DefaultProperty("Caption")]
public class DatabaseInfo
{
    [Category("MetaData")]
    [ReadOnly(true)]
    public string ConnectionString { get; set; }
    
    [Browsable(false)]
    [ReadOnly(true)]
    public DatabaseType DatabaseType { get; set; }

    [Category("MetaData")]
    [ReadOnly(true)]
    public string Caption { get; set; }

    [Category("MetaData")]
    [ReadOnly(true)]
    public bool FromServerExplorer { get; set; }

    [Category("MetaData")]
    [ReadOnly(true)]
    public bool FileIsMissing { get; set; }

    [Category("MetaData")]
    [ReadOnly(true)]
    public string ServerVersion { get; set; }

    [Category("Connected")]
    [ReadOnly(true)]
    [Description("Locale ID")]
    public int LCID { get; set; }

    [Category("Connected")]
    [ReadOnly(true)]
    public string EncryptionMode { get; set; }

    [Category("Connected")]
    [ReadOnly(true)]
    public bool? CaseSensitive { get; set; }

    [Category("Connected")]
    [ReadOnly(true)]
    public string Size { get; set; }

    [Category("Connected")]
    [ReadOnly(true)]
    public string SpaceAvailable { get; set; }

    [Category("Connected")]
    [ReadOnly(true)]
    public string Created { get; set; }

}

public class TableInfo
{
    [ReadOnly(true)]
    public string Name { get; set; }

    [ReadOnly(true)]
    public Int64 RowCount { get; set; }
}


public enum DatabaseType
{
    SQLCE35,
    SQLCE40,
    SQLServer,
    SQLite
}
/// <summary>
/// A model used to communicate with DescriptionDialog for table+column descriptions
/// </summary>
public class TableColumnInfo
{
    public string Name { get; set; }
    public string Metadata { get; set; }
    public string Description { get; set; }
}