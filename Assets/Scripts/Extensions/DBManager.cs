using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
/// <summary>
/// Database helper utility.
/// </summary>
public static class DBManager
{
    /// <summary>
    /// Database reader object
    /// </summary>
    public static IDataReader reader;
    private static IDbConnection dbcon; 

    /// <summary>
    /// Initiate a SQLite connection with the specified database filepath.
    /// </summary>
    /// <param name="dbName">Filename of database .db file</param>
    public static void InitiateConnection(string dbName)
    {
        //prev connection
        if (dbcon != null)
            CloseConnection();
        string path = "URI=file:" + System.IO.Path.Combine(Application.persistentDataPath, dbName);
        Debug.Log(path);
        dbcon = new SqliteConnection(path);
        dbcon.Open();

        Debug.Log($"DB file at {path} was successfully opened");
    }
    /// <summary>
    /// A query handler that sets the reader object.
    /// </summary>
    /// <param name="query">SQL Query</param>
    public static void QueryDB(string query)
    {
        if (dbcon.State != ConnectionState.Open)
        {
            Debug.LogError("No connection was found to query! Successfully run InitiateConnection() before calling a query", (Object)dbcon);
            return;
        }
        if (reader != null)
        {
            reader.Close();
            reader = null;
        }

        IDbCommand dbcmd;
        dbcmd = dbcon.CreateCommand();

        dbcmd.CommandText = query;
        reader = dbcmd.ExecuteReader();
        dbcmd.Dispose();

    }
    /// <summary>
    /// Close any open database connections
    /// </summary>
    public static void CloseConnection()
    {
        //clean everything up
        if (reader != null)
        {
            reader.Close();
            reader = null;
        }
        if (dbcon != null)
        {
            dbcon.Close();
            dbcon = null;
        }
    }
}