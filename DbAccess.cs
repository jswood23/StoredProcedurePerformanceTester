using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Unions;

namespace StoredProcedurePerformanceTester;

public record DbConnection(string DataSource, string Database, string Username, string Password);

[GenerateUnion]
public partial record SprocParam
{
    private SprocParam() { }
    
    public record IntParam : SprocParam
    {
        public int Value { get; init; }
        
        public IntParam(int value)
        {
            this.Value = value;
        }
    }
    
    public record StringParam : SprocParam
    {
        public string Value { get; init; }
        
        public StringParam(string value)
        {
            this.Value = value;
        }
    }
}

public class DbAccess : IDisposable
{
    private readonly string _dataSource;
    private readonly string _database;
    private readonly string _username;
    private readonly string _password;
    private SqlConnectionStringBuilder _builder;

    public DbAccess(IConfigurationRoot config)
    {
        DbConnection? dbConnection = config.GetRequiredSection("dbconnection").Get<DbConnection>();

        if (dbConnection == null)
        {
            throw new ValidationException("DBConnection is not configured in appsettings.json");
        }
        
        _dataSource = dbConnection.DataSource;
        _database = dbConnection.Database;
        _username = dbConnection.Username;
        _password = dbConnection.Password;
        
        _builder = new SqlConnectionStringBuilder();
    }

    public async Task RunSproc(string sprocName, Dictionary<string, SprocParam> parameters)
    {
        _builder = new SqlConnectionStringBuilder();
        _builder.DataSource = _dataSource;
        _builder.InitialCatalog = _database;
        _builder.UserID = _username;
        _builder.Password = _password;
        _builder.TrustServerCertificate = true;
        
        using (SqlConnection connection = new SqlConnection(_builder.ConnectionString))
        {
            try
            {
                connection.Open();
                String sql = "EXEC " + sprocName;
                foreach (var (key, value) in parameters)
                {
                    sql += " " + key + " = ";
                    sql += value.Match(
                        intParam => intParam.Value.ToString(),
                        stringParam => "'" + stringParam.Value + "'"
                    );
                    sql += parameters.Last().Key == key ? ";" : ",";
                }
                // Console.WriteLine("Executing command:\n" + sql + "\n\n");

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ReadSingleRow(reader);
                        }
                    }
                }
                connection.Close();
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
            // Console.WriteLine("\n\nDone.");
        }
    }

    private static void ReadSingleRow(IDataRecord dataRecord)
    {
        for (int i = 0; i < 11; i++)
        {
            string str = dataRecord[i].ToString();
            str = str.Length >= 7 ? str.Substring(0, 7) : str;
            // Console.Write(str + "\t");
        }
        // Console.WriteLine();
    }
    
    public void Dispose()
    {
        _builder = null;
    }
}