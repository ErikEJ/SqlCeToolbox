using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ErikEJ.SqlCeScripting;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace ErikEJ.SqlCeScripting
{
    public class DataContextHelper
    {
        private StringBuilder _sbResult = new StringBuilder();

        public void GenerateWPDataContext(IRepository repository, string connectionString, string dcPath)
        {
            if (dcPath.ToUpperInvariant().EndsWith(".CS") || dcPath.ToUpperInvariant().EndsWith(".VB"))
            { }
            else
            {
                throw new Exception("DataContext file name must end with either .cs or .vb");
            }

            string sqlMetalPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v7.0A\WinSDK-NetFx40Tools", "InstallationFolder", null);
            if (string.IsNullOrEmpty(sqlMetalPath))
            {
                sqlMetalPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v8.0A", "InstallationFolder", string.Empty) + "bin\\NETFX 4.0 Tools\\";
                if (string.IsNullOrEmpty(sqlMetalPath))
                {
                    throw new Exception("Could not find SQLMetal location in registry");
                }
            }
            sqlMetalPath = Path.Combine(sqlMetalPath, "sqlmetal.exe");

            if (!File.Exists(sqlMetalPath))
            {
                throw new Exception("Could not find SqlMetal in the expected location: " + sqlMetalPath);
            }

            string model = Path.GetFileNameWithoutExtension(dcPath).Replace(" ", string.Empty).Replace("#", string.Empty).Replace(".", string.Empty).Replace("-", string.Empty);
            model = model + "Context";

            string parameters = " /provider:SQLCompact /code:\"" + dcPath + "\"";
            parameters += " /conn:\"" + connectionString + "\"";
            parameters += " /context:" + model;
            parameters += " /pluralize";
            //parameters += " /serialization:Unidirectional";
            string sqlmetalResult = RunSqlMetal(sqlMetalPath, parameters);
            if (!File.Exists(dcPath))
            {
                throw new Exception("Error during SQL Metal run: " + sqlmetalResult);
            }

            string sdfFileName = string.Empty;
            List<KeyValuePair<string, string>> dbInfo = repository.GetDatabaseInfo();
            foreach (var kvp in dbInfo)
            {
                if (kvp.Key == "Database")
                {
                    sdfFileName = kvp.Value;
                    break;
                }
            }
            sdfFileName = Path.GetFileName(sdfFileName);

            if (dcPath.ToUpperInvariant().EndsWith(".VB"))
            {
                FixDataContextVB(dcPath, model, null, sdfFileName, repository);
            }
            else
            {
                FixDataContextCS(dcPath, model, null, sdfFileName, repository);
            }
        }

        public string RunSqlMetal(string sqlMetalPath, string parameters)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardError = true;

            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = sqlMetalPath;
            startInfo.Arguments = parameters;

            using (var _command = new Process())
            {
                _command.StartInfo = startInfo;
                _command.ErrorDataReceived += new DataReceivedEventHandler(_command_ErrorDataReceived);
                _command.OutputDataReceived += new DataReceivedEventHandler(_command_OutputDataReceived);
                _command.EnableRaisingEvents = true;
                _command.Start();
                _command.BeginOutputReadLine();
                _command.BeginErrorReadLine();
                _command.WaitForExit();
                return _sbResult.ToString();
            }
        }

        void _command_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            _sbResult.AppendLine("SqlMetal output: " + e.Data);
        }

        void _command_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            _sbResult.AppendLine("SqlMetal error: " + e.Data);
        }
            
        private static string T(int n) 
        { 
            return new String('\t', n); 
        }

        private static string ToUpperFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        public static void FixDataContextCS(string path, string model, string nameSpace, string sdfFileName, IRepository repository)
        {
            string debugWriter = @"
public class DebugWriter : TextWriter
{
    private const int DefaultBufferSize = 256;
    private System.Text.StringBuilder _buffer;

    public DebugWriter()
    {
        BufferSize = 256;
        _buffer = new System.Text.StringBuilder(BufferSize);
    }

    public int BufferSize
    {
        get;
        private set;
    }

    public override System.Text.Encoding Encoding
    {
        get { return System.Text.Encoding.UTF8; }
    }

    #region StreamWriter Overrides
    public override void Write(char value)
    {
        _buffer.Append(value);
        if (_buffer.Length >= BufferSize)
            Flush();
    }

    public override void WriteLine(string value)
    {
        Flush();

        using(var reader = new StringReader(value))
        {
            string line; 
            while( null != (line = reader.ReadLine()))
                System.Diagnostics.Debug.WriteLine(line);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            Flush();
    }

    public override void Flush()
    {
        if (_buffer.Length > 0)
        {
            System.Diagnostics.Debug.WriteLine(_buffer);
            _buffer.Clear();
        }
    }
    #endregion
}

";

            List<string> dcLines = System.IO.File.ReadAllLines(path).ToList();
            var n = string.Empty;
            if (!string.IsNullOrEmpty(nameSpace))
            {
                n += "\t";
            }
            var t = "\t";

            int i = dcLines.IndexOf(n + "public partial class " + model + " : System.Data.Linq.DataContext");
            if (i > -1)
            {
                dcLines.Insert(i - 2, debugWriter);
                dcLines.Insert(i - 2, "");
                dcLines.Insert(i - 2, "using Microsoft.Phone.Data.Linq;");
                dcLines.Insert(i - 2, "using Microsoft.Phone.Data.Linq.Mapping;");
                dcLines.Insert(i - 2, "using System.IO.IsolatedStorage;");
                dcLines.Insert(i - 2, "using System.IO;");
            }

            i = dcLines.IndexOf(n + "public partial class " + model + " : System.Data.Linq.DataContext");
            if (i > -1)
            {
                dcLines.RemoveAt(i - 1);
                dcLines.RemoveAt(i - 2);
                i++;
                i++;
                dcLines.Insert(i++, n + t + "public static string ConnectionString = \"Data Source=isostore:/" + sdfFileName + "\";");
                dcLines.Insert(i++, "");
                dcLines.Insert(i++, n + t + "public static string ConnectionStringReadOnly = \"Data Source=appdata:/" + sdfFileName + ";File Mode=Read Only;\";");
                dcLines.Insert(i++, "");
                dcLines.Insert(i++, n + t + "public static string FileName = \"" + sdfFileName + "\";");
                dcLines.Insert(i++, "");
                dcLines.Insert(i++, n + t + "public " + model + "(string connectionString) : base(connectionString)");
                dcLines.Insert(i++, n + t + "{");
                dcLines.Insert(i++, n + t + t + "OnCreated();");
                dcLines.Insert(i++, n + t + "}");
            }

            i = dcLines.IndexOf(n + t + "public " + model + "(string connection) : ");
            if (i > -1)
            {
                dcLines.RemoveRange(i, 6);
            }

            i = dcLines.IndexOf(n + t + "public " + model + "(System.Data.IDbConnection connection) : ");
            if (i > -1)
            {
                dcLines.RemoveRange(i, 6);
            }

            i = dcLines.IndexOf(n + t + "public " + model + "(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : ");
            if (i > -1)
            {
                dcLines.RemoveRange(i, 6);
            }

            i = dcLines.IndexOf(n + t + "public " + model + "(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : ");
            if (i > -1)
            {
                dcLines.RemoveRange(i, 6);
            }

            i = dcLines.IndexOf(n + t + "private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();");
            if (i > -1)
            {
                dcLines.RemoveAt(i);

                dcLines.Insert(i++, n + t + "public bool CreateIfNotExists()");
                dcLines.Insert(i++, n + t + "{");
                dcLines.Insert(i++, n + T(2) + "bool created = false;");
                dcLines.Insert(i++, n + T(2) + "if (!this.DatabaseExists())");
                dcLines.Insert(i++, n + T(2) + "{");
                dcLines.Insert(i++, n + T(3) + "string[] names = this.GetType().Assembly.GetManifestResourceNames();");
                dcLines.Insert(i++, n + T(3) + "string name = names.Where(n => n.EndsWith(FileName)).FirstOrDefault();");
                dcLines.Insert(i++, n + T(3) + "if (name != null)");
                dcLines.Insert(i++, n + T(3) + "{");
                dcLines.Insert(i++, n + T(4) + "using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))");
                dcLines.Insert(i++, n + T(4) + "{");
                dcLines.Insert(i++, n + T(5) + "if (resourceStream != null)");
                dcLines.Insert(i++, n + T(5) + "{");
                dcLines.Insert(i++, n + T(6) + "using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())");
                dcLines.Insert(i++, n + T(6) + "{");
                dcLines.Insert(i++, n + T(7) + "using (IsolatedStorageFileStream fileStream = new IsolatedStorageFileStream(FileName, FileMode.Create, myIsolatedStorage))");
                dcLines.Insert(i++, n + T(7) + "{");
                dcLines.Insert(i++, n + T(8) + "using (BinaryWriter writer = new BinaryWriter(fileStream))");
                dcLines.Insert(i++, n + T(8) + "{");
                dcLines.Insert(i++, n + T(9) + "long length = resourceStream.Length;");
                dcLines.Insert(i++, n + T(9) + "byte[] buffer = new byte[32];");
                dcLines.Insert(i++, n + T(9) + "int readCount = 0;");
                dcLines.Insert(i++, n + T(9) + "using (BinaryReader reader = new BinaryReader(resourceStream))");
                dcLines.Insert(i++, n + T(9) + "{");
                dcLines.Insert(i++, n + T(10) + "// read file in chunks in order to reduce memory consumption and increase performance");
                dcLines.Insert(i++, n + T(10) + "while (readCount < length)");
                dcLines.Insert(i++, n + T(10) + "{");
                dcLines.Insert(i++, n + T(11) + "int actual = reader.Read(buffer, 0, buffer.Length);");
                dcLines.Insert(i++, n + T(11) + "readCount += actual;");
                dcLines.Insert(i++, n + T(11) + "writer.Write(buffer, 0, actual);");
                dcLines.Insert(i++, n + T(10) + "}");
                dcLines.Insert(i++, n + T(9) + "}");
                dcLines.Insert(i++, n + T(8) + "}");
                dcLines.Insert(i++, n + T(7) + "}");
                dcLines.Insert(i++, n + T(6) + "}");
                dcLines.Insert(i++, n + T(6) + "created = true;");
                dcLines.Insert(i++, n + T(5) + "}");
                dcLines.Insert(i++, n + T(5) + "else");
                dcLines.Insert(i++, n + T(5) + "{");
                dcLines.Insert(i++, n + T(6) + "this.CreateDatabase();");
                dcLines.Insert(i++, n + T(6) + "created = true;");
                dcLines.Insert(i++, n + T(5) + "}");
                dcLines.Insert(i++, n + T(4) + "}");
                dcLines.Insert(i++, n + T(3) + "}");
                dcLines.Insert(i++, n + T(3) + "else");
                dcLines.Insert(i++, n + T(3) + "{");
                dcLines.Insert(i++, n + T(4) + "this.CreateDatabase();");
                dcLines.Insert(i++, n + T(4) + "created = true;");
                dcLines.Insert(i++, n + T(3) + "}");
                dcLines.Insert(i++, n + T(2) + "}");
                dcLines.Insert(i++, n + T(2) + "return created;");
                dcLines.Insert(i++, n + t + "}");

                dcLines.Insert(i++, n + t + "");
                dcLines.Insert(i++, n + t + "public bool LogDebug");
                dcLines.Insert(i++, n + t + "{");
                dcLines.Insert(i++, n + T(2) + "set");
                dcLines.Insert(i++, n + T(2) + "{");
                dcLines.Insert(i++, n + T(3) + "if (value)");
                dcLines.Insert(i++, n + T(3) + "{");
                dcLines.Insert(i++, n + T(4) + "this.Log = new DebugWriter();");
                dcLines.Insert(i++, n + T(3) + "}");
                dcLines.Insert(i++, n + T(2) + "}");
                dcLines.Insert(i++, n + t + "}");
                dcLines.Insert(i++, n + t + "");
                
            }

            AddIndexes(repository, dcLines, n, true);
            FixSerialisation(dcLines, n + t, true);

            System.IO.File.WriteAllLines(path, dcLines.ToArray());
        }

        public static void FixDataContextVB(string path, string model, string nameSpace, string sdfFileName, IRepository repository)
        {
            string debugWriter = @"
#Region ""DebugWriter""
Public Class DebugWriter
    Inherits TextWriter
    Private Const DefaultBufferSize As Integer = 256
    Private _buffer As System.Text.StringBuilder

    Public Sub New()
        BufferSize = 256
        _buffer = New System.Text.StringBuilder(BufferSize)
    End Sub

    Public Property BufferSize() As Integer
        Get
            Return m_BufferSize
        End Get
        Private Set(value As Integer)
            m_BufferSize = value
        End Set
    End Property
    Private m_BufferSize As Integer

    Public Overrides ReadOnly Property Encoding() As System.Text.Encoding
        Get
            Return System.Text.Encoding.UTF8
        End Get
    End Property

#Region ""StreamWriter Overrides""
    Public Overrides Sub Write(value As Char)
        _buffer.Append(value)
        If _buffer.Length >= BufferSize Then
            Flush()
        End If
    End Sub

    Public Overrides Sub WriteLine(value As String)
        Flush()

        Using reader = New StringReader(value)
            Dim line As String
            ' Read and display lines from the file until the end of
            ' the file is reached.
            Do
                line = reader.ReadLine()
                If Not (line Is Nothing) Then
                    System.Diagnostics.Debug.WriteLine(line)
                End If
            Loop Until line Is Nothing
        End Using
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            Flush()
        End If
    End Sub

    Public Overrides Sub Flush()
        If _buffer.Length > 0 Then
            System.Diagnostics.Debug.WriteLine(_buffer)
            _buffer.Clear()
        End If
    End Sub
    Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
        target = value
        Return value
    End Function
#End Region
End Class
#End Region
";

            string createIfNotExists = @"
    Public Function CreateIfNotExists() As Boolean
        Dim created As Boolean = False
        Using db = New {0}({0}.ConnectionString)
            If Not db.DatabaseExists() Then
                Dim names As String() = Me.[GetType]().Assembly.GetManifestResourceNames()
                Dim name As String = names.Where(Function(n) n.EndsWith(FileName)).FirstOrDefault()
                If name IsNot Nothing Then
                    Using resourceStream As Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name)
                        If resourceStream IsNot Nothing Then
                            Using myIsolatedStorage As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication()
                                Using fileStream As New IsolatedStorageFileStream(FileName, FileMode.Create, myIsolatedStorage)
                                    Using writer As New BinaryWriter(fileStream)
                                        Dim length As Long = resourceStream.Length
                                        Dim buffer As Byte() = New Byte(32) {}
                                        Dim readCount As Integer = 0
                                        Using reader As New BinaryReader(resourceStream)
                                            ' read file in chunks in order to reduce memory consumption and increase performance
                                            While readCount < length
                                                Dim actual As Integer = reader.Read(buffer, 0, buffer.Length)
                                                readCount += actual
                                                writer.Write(buffer, 0, actual)
                                            End While
                                        End Using
                                    End Using
                                End Using
                            End Using
                            created = True
                        Else
                            db.CreateDatabase()
                            created = True
                        End If
                    End Using
                Else
                    db.CreateDatabase()
                    created = True
                End If
            End If
        End Using
        Return created
    End Function

	Public WriteOnly Property LogDebug() As Boolean
        Set(value As Boolean)
            If value Then
                Me.Log = New DebugWriter()
            End If
        End Set
    End Property
";

            List<string> dcLines = System.IO.File.ReadAllLines(path).ToList();
            var n = string.Empty;
            if (!string.IsNullOrEmpty(nameSpace))
            {
                n += "\t";
            }
            var t = "\t";
            
            int i = dcLines.IndexOf("Imports System.Reflection");
            if (i > -1)
            {
                dcLines.Insert(i++, "Imports Microsoft.Phone.Data.Linq");
                dcLines.Insert(i++, "Imports Microsoft.Phone.Data.Linq.Mapping");
                dcLines.Insert(i++, "Imports System.IO.IsolatedStorage");
                dcLines.Insert(i++, "Imports System.IO");
            }

            i = dcLines.IndexOf(n + "Partial Public Class " + model);
            if (i > -1)
            {
                dcLines.Insert(i - 2, debugWriter);
                dcLines.Insert(i - 2, "");
            }

            i = dcLines.IndexOf(n + t + "Public Sub New(ByVal connection As String)");
            if (i > -1)
            {
                dcLines.RemoveRange(i, 4);
            }

            i = dcLines.IndexOf(n + t + "Public Sub New(ByVal connection As System.Data.IDbConnection)");
            if (i > -1)
            {
                dcLines.RemoveRange(i, 4);
            }

            i = dcLines.IndexOf(n + t + "Public Sub New(ByVal connection As String, ByVal mappingSource As System.Data.Linq.Mapping.MappingSource)");
            if (i > -1)
            {
                dcLines.RemoveRange(i, 4);
            }

            i = dcLines.IndexOf(n + t + "Public Sub New(ByVal connection As System.Data.IDbConnection, ByVal mappingSource As System.Data.Linq.Mapping.MappingSource)");
            if (i > -1)
            {
                dcLines.RemoveRange(i, 4);
            }

            i = dcLines.IndexOf(n + "Partial Public Class " + model);
            if (i > -1)
            {
                dcLines.RemoveAt(i - 1);
                dcLines.RemoveAt(i - 2);
                i++;
                i++;
                i++;

                dcLines.Insert(i++, n + t + "Public Shared ConnectionString As String = \"Data Source=isostore:/" + sdfFileName + ";\"");
                dcLines.Insert(i++, "");
                dcLines.Insert(i++, n + t + "Public Shared ConnectionStringReadOnly As String = \"Data Source=appdata:/" + sdfFileName + ";File Mode=Read Only;\"");
                dcLines.Insert(i++, "");                
                dcLines.Insert(i++, n + t + "Public Shared FileName As String = \"" + sdfFileName + "\"");
                dcLines.Insert(i++, "");
                dcLines.Insert(i++, n + t + "Public Sub New(ByVal connection As String)");
                dcLines.Insert(i++, n + t + t + "MyBase.New(connection)");
                dcLines.Insert(i++, n + t + t + "OnCreated()");
                dcLines.Insert(i++, n + t + "End Sub");
            }

            i = dcLines.IndexOf(n + t + "Private Shared mappingSource As System.Data.Linq.Mapping.MappingSource = New AttributeMappingSource()");
            if (i > -1)
            {
                dcLines.RemoveAt(i);
                createIfNotExists = createIfNotExists.Replace("{0}", model);
                dcLines.Insert(i++, n + t + createIfNotExists);
            }

            AddIndexes(repository, dcLines, n, false);
            FixSerialisation(dcLines, n + t, false);

            System.IO.File.WriteAllLines(path, dcLines.ToArray());

        }

        private static void AddIndexes(IRepository repository, List<string> dcLines, string n, bool cs)
        {

            for (int y = 0; y < dcLines.Count; y++)
            {
                string attr = "<Global.System.Data.Linq.Mapping.TableAttribute(";
                if (cs)
                    attr = "[global::System.Data.Linq.Mapping.TableAttribute(";
                if (dcLines[y].StartsWith(n + attr))
                {
                    string tableName = string.Empty;
                    // if the Name attribute is used, that is the table name, otherwise use class name
                    string[] names = dcLines[y].Split('"');
                    if (names.Count() > 1)
                        tableName = names[1];
                    string[] words = dcLines[y + 1].Split(' ');
                    if (words.Count() > 3)
                    {
                        if (string.IsNullOrEmpty(tableName))
                            tableName = words[3];
                        List<Index> indexList = repository.GetIndexesFromTable(tableName);
                        List<PrimaryKey> pkList = repository.GetAllPrimaryKeys().Where(pk => pk.TableName == tableName).ToList();
                        //If there are indexes, add them
                        if (indexList.Count > 0)
                        {
                            IEnumerable<string> uniqueIndexNameList = indexList.Select(ind => ind.IndexName).Distinct();
                            foreach (string uniqueIndexName in uniqueIndexNameList)
                            {
                                string colList = string.Empty;
                                IOrderedEnumerable<Index> indexesByName = from ind in indexList
                                                                          where ind.IndexName == uniqueIndexName
                                                                          orderby ind.OrdinalPosition
                                                                          select ind;

                                // Check if a Unique index overlaps an existing primary key
                                // If that is the case, do not add the duplicate index
                                // as this will cause LINQ to SQL to crash 
                                // when doing updates with rowversion columns
                                var ixList = indexesByName.ToList();
                                if (ixList.Count > 0 && ixList[0].Unique)
                                {
                                    int i = 0;
                                    foreach (var pk in pkList)
                                    {
                                        if (ixList.Count > i)
                                        {
                                            if (pk.ColumnName != ixList[i].ColumnName)
                                            {
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            break;
                                        }
                                        i++;
                                    }
                                    if (i > 0)
                                        continue;
                                }


                                bool unique = false;
                                var idx = indexesByName.First();
                                if (idx.Unique)
                                {
                                    unique = true;
                                }

                                foreach (Index col in indexesByName)
                                {
                                    colList += string.Format("{0} {1}, ", ToUpperFirst(col.ColumnName.Replace(" ", string.Empty)), col.SortOrder.ToString());
                                }
                                colList = colList.Remove(colList.Length - 2, 2);
                                string indexAttr = "<Index(Name:=\"{0}\", Columns:=\"{1}\", IsUnique:={2})> _";
                                if (cs)
                                    indexAttr = "[Index(Name=\"{0}\", Columns=\"{1}\", IsUnique={2})]";
                                string falseString = "False";
                                if (cs)
                                    falseString = "false";
                                string trueString = "True";
                                if (cs)
                                    trueString = "true";

                                dcLines[y - 1] += Environment.NewLine + n + string.Format(indexAttr, idx.IndexName, colList, unique ? trueString : falseString);
                            }
                        }
                    }
                }
            }
        }

        private static void FixSerialisation(List<string> dcLines, string n, bool cs)
        {
            for (int y = 0; y < dcLines.Count; y++)
            {
                string attr = "<Global.System.Data.Linq.Mapping.AssociationAttribute(";
                if (cs)
                    attr = "[global::System.Data.Linq.Mapping.AssociationAttribute(";
                if (dcLines[y].StartsWith(n + attr))
                {
                    if (cs)
                    {
                        dcLines[y - 1] += Environment.NewLine + n + "[global::System.Runtime.Serialization.IgnoreDataMember]";
                    }
                    else //VB
                    {
                        dcLines[y - 1] += Environment.NewLine + n + "<Global.System.Runtime.Serialization.IgnoreDataMember> _";
                    }
                }
            }
        }

        public static Dictionary<string, string> SplitIntoMultipleFiles(string dcPath, string nameSpace, string model)
        {
            //Fist part is the class name, second in the file contents
            Dictionary<string, string> split = new Dictionary<string, string>();

            List<string> dcLines = System.IO.File.ReadAllLines(dcPath).ToList();
            var n = string.Empty;
            if (!string.IsNullOrEmpty(nameSpace))
            {
                n += "\t";
            }

            string usings = @"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.235
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.IO;
using System.IO.IsolatedStorage;

using Microsoft.Phone.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq;

";

            string remove1 =
@"	public System.Data.Linq.Table<{0}> {0}
	{
		get
		{
			return this.GetTable<{0}>();
		}
	}
";

            string remove2 =
@"		public System.Data.Linq.Table<{0}> {0}
		{
			get
			{
				return this.GetTable<{0}>();
			}
		}";

            string remove3 =
@"	public System.Data.Linq.Table<{0}> {0}s
	{
		get
		{
			return this.GetTable<{0}>();
		}
	}
";

            string remove4 =
@"		public System.Data.Linq.Table<{0}> {0}s
		{
			get
			{
				return this.GetTable<{0}>();
			}
		}";
            string contextName = string.Empty;
            List<string> systemClasses = new List<string>();
            for (int y = 0; y < dcLines.Count; y++)
            {
                if (dcLines[y].StartsWith(n + "public partial class ") || dcLines[y].StartsWith(n + "public class DebugWriter"))
                {
                    var parts = dcLines[y].Split(' ');
                    if (parts.Count() > 3)
                    {
                        string className = parts[3];
                        if (string.IsNullOrEmpty(contextName) && !dcLines[y].StartsWith(n + "public class DebugWriter"))
                            contextName = className;
                        if (className == ":")
                            className = parts[2];

                        if (className.StartsWith("@__"))
                        {
                            systemClasses.Add(className);
                        }
                        else
                        {
                            int x = y - 1;
                            while (!string.IsNullOrEmpty(dcLines[x]))
                            {
                                x--;
                            }
                            string finalClass = usings;

                            while ((x < dcLines.Count - 1) && dcLines[x] != n + "}")
                            {
                                finalClass += Environment.NewLine + dcLines[x];
                                x++;
                            }
                            finalClass += Environment.NewLine + "}";
                            split.Add(className, finalClass);
                        }
                        
                    }
                }
            }

            if (split.Count > 0)
            {
                foreach (var item in systemClasses)
                {
                    split[contextName] = split[contextName].Replace(remove1.Replace("{0}", item), string.Empty);
                    split[contextName] = split[contextName].Replace(remove2.Replace("{0}", item), string.Empty);
                    split[contextName] = split[contextName].Replace(remove3.Replace("{0}", item), string.Empty);
                    split[contextName] = split[contextName].Replace(remove4.Replace("{0}", item), string.Empty);
                }
            }

            return split;
        }


    }
}
