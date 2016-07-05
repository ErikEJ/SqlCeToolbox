using System;
using System.ComponentModel;
using System.Reflection;
using System.Text;

/// <summary>
/// A class for building a connection string for Local Database on Windows Phone
/// </summary>
public class LocalDatabaseConnectionStringBuilder
{

    public LocalDatabaseConnectionStringBuilder()
    {
    }

    /// <summary>
    /// Build a connection string
    /// </summary>
    /// <param name="dataSourceFile">Name or path to database file</param>
    public LocalDatabaseConnectionStringBuilder(string dataSourceFile)
    {
        this._databaseName = dataSourceFile;
    }

    /// <summary>
    /// Build a connection string
    /// </summary>
    /// <param name="dataSourceFile">Name or path to database file</param>
    /// <param name="readOnly">If true, the connection string will be read-only in appdata:/</param>
    public LocalDatabaseConnectionStringBuilder(string dataSourceFile, bool readOnly)
    {
        this._databaseName = dataSourceFile;
        if (readOnly)
            this._source = DataSourceRootType.AppData;
            this._fileMode = FileMode.ReadOnly;
    }

    /// <summary>
    /// File system location
    /// </summary>
    public enum DataSourceRootType
    {
        /// <summary>
        /// Isolated storage
        /// </summary>
        IsoStore,
        /// <summary>
        /// Installation folder
        /// </summary>
        AppData
    }

    /// <summary>
    /// Mode to use when opening the database file.
    /// </summary>
    public enum FileMode
    {
        /// <summary>
        ///  Allows multiple processes to open and modify the database. 
        ///  This is the default setting if the mode property is not specified.
        /// </summary>
        [Description("Read Write")]
        ReadWrite,
        /// <summary>
        /// Allows you to open a read-only copy of the database.
        /// </summary>
        [Description("Read Only")]
        ReadOnly,
        /// <summary>
        /// Does not allow other processes from opening or modifying the database
        /// </summary>
        [Description("Exclusive")]
        Exclusive,
        /// <summary>
        /// Allows other processes to read, but not modify, the database while you have it open.
        /// </summary>
        [Description("Shared Read")]
        SharedRead
    }

    DataSourceRootType _source = DataSourceRootType.IsoStore;
    string _password = string.Empty;

    int _maxBufferSize = 384;
    int _maxDatabaseSize = 32;

    FileMode _fileMode = FileMode.ReadWrite;

    string _cultureIdentifierString = string.Empty;
    bool _isCaseSensitive = false;
    string _databaseName;

    /// <summary>
    /// Location of data source
    /// </summary>
    [CategoryAttribute("DataSource"),
    DescriptionAttribute("Location of the data source (IsoStore or AppData)"),
    DefaultValueAttribute(DataSourceRootType.IsoStore)]
    public DataSourceRootType DataSourceRoot
    {
        get
        {
            return _source;
        }
        set
        {
            _source = value;
        }
    }


    /// <summary>
    /// The filename of the Database File
    /// This is a required property
    /// </summary>
    [CategoryAttribute("DataSource"),
    DescriptionAttribute("The filename of the Database File")]
    public string DataSourceFile
    {
        get
        {
            return _databaseName;
        }
        set
        {
            _databaseName = value;
        }
    }

    /// <summary>
    /// The database password, which can be up to 40 characters in length. 
    /// If not specified, the default value is no password. 
    /// If you specify a blank password, the database will not be encrypted.
    /// NOTE: You cannot encrypt a database after it has been created.
    /// </summary>
    [CategoryAttribute("Security"),
    DescriptionAttribute("The database password, which can be up to 40 characters in length")]
    public string Password
    {
        get
        {
            return _password;
        }
        set
        {
            _password = value;
        }
    }

    /// <summary>
    /// The largest amount of memory, in kilobytes, that a local database can use before it starts flushing changes to disk. 
    /// If not specified, the default value is 384. 
    /// The maximum value is 5120.
    /// </summary>
    [CategoryAttribute("Advanced"),
    DescriptionAttribute("Largest amount of memory, in kilobytes, that a local database can use before it starts flushing changes to disk")]
    public int MaxBufferSize
    {
        get
        {
            return _maxBufferSize;
        }
        set
        {
            if (value < 384)
            {
                _maxBufferSize = 384;
            }
            else if (value > 5120)
            {
                _maxBufferSize = 5120;
            }
            else
            {
                _maxBufferSize = value;
            }
        }
    }
    /// <summary>
    /// The maximum size of a local database, in megabytes. 
    /// If not specified, the default value is 32. 
    /// The maximum value is 512.
    /// </summary>
    [CategoryAttribute("Advanced"),
    DescriptionAttribute("The maximum size of a local database, in megabytes")]
    public int MaxDatabaseSize
    {
        get
        {
            return _maxDatabaseSize;
        }
        set
        {
            if (value < 32)
            {
                _maxDatabaseSize = 32;
            }
            if (value > 512)
            {
                _maxDatabaseSize = 512;
            }
            else
            {
                _maxDatabaseSize = value;
            }
        }
    }

    /// <summary>
    /// The mode to use when opening the database file. 
    /// </summary>
    [CategoryAttribute("Advanced"),
    DescriptionAttribute("The mode to use when opening the database file"),
    DefaultValue(FileMode.ReadWrite)]
    public FileMode Mode
    {
        get
        {
            return _fileMode;
        }
        set
        {
            _fileMode = value;
        }
    }

    /// <summary>
    /// The culture code to use with the database. 
    /// For example, en-US for United States English.
    /// NOTE: This property is ignored if used when connecting to an existing database.
    /// </summary>
    [CategoryAttribute("Advanced"),
    DescriptionAttribute("The culture code (ie: en-US) to use with the database")]
    public string CultureIdentifier
    {
        get
        {
            return _cultureIdentifierString;
        }
        set
        {
            _cultureIdentifierString = value;
        }
    }
    /// <summary>
    /// A Boolean value that determines whether or not the database collation is case-sensitive.
    /// NOTE: This property is ignored if used when connecting to an existing database.
    /// </summary>
    [CategoryAttribute("Advanced"),
    DescriptionAttribute("Whether or not the database collation is case-sensitive")]
    public bool CaseSensitive
    {
        get
        {
            return _isCaseSensitive;
        }
        set
        {
            _isCaseSensitive = value;
        }
    }

    /// <summary>
    /// Builds the connection string based on paramters provided
    /// </summary>
    /// <returns>connection string to local database</returns>
    [Browsable(false)]
    public string ConnectionString
    {
        get
        {
            StringBuilder connectionBuilder = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_databaseName))
            {
                throw new ArgumentException("DataSourceFile is a required property");
            }

            connectionBuilder.Append(string.Format("Data Source='{0}:/{1}';", _source.ToString().ToLower(), _databaseName));

            if (_source == DataSourceRootType.AppData)
            {
                if (_fileMode != FileMode.ReadOnly)
                {
                    throw new ArgumentException("Mode must be 'read only' when using appdata storage");
                }
                connectionBuilder.Append(string.Format("File Mode={0};", _fileMode.GetStringValue()));
            }
            else
            {
                if (_fileMode != FileMode.ReadWrite)
                {
                    connectionBuilder.Append(string.Format("File Mode={0};", _fileMode.GetStringValue()));
                }
            }

            if (!string.IsNullOrEmpty(_password))
            {
                connectionBuilder.Append(string.Format("Password='{0}';", _password));
            }

            if (_maxBufferSize != 384)
                connectionBuilder.Append(string.Format("Max Buffer Size={0};", _maxBufferSize));

            if (_maxDatabaseSize != 32)
                connectionBuilder.Append(string.Format("Max Database Size={0};", _maxDatabaseSize));

            if (!string.IsNullOrEmpty(_cultureIdentifierString))
            {
                connectionBuilder.Append(string.Format("Culture Identifier={0};", _cultureIdentifierString));
            }

            if (_isCaseSensitive)
            {
                connectionBuilder.Append(string.Format("Case Sensitive={0};", _isCaseSensitive));
            }

            return connectionBuilder.ToString();
        }
    }
}

public static class EnumUtil
{
    public static string GetStringValue(this Enum value)
    {
        FieldInfo fi = value.GetType().GetField(value.ToString());
        DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
        if (attributes.Length > 0)
        {
            return attributes[0].Description;
        }
        else
        {
            return value.ToString();
        }
    }
    
}