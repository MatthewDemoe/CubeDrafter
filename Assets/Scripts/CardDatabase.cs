using Mono.Data.Sqlite;
using System.Data;
using UnityEngine;

public class CardDatabase 
{
    private static string databaseFile = $"{Application.persistentDataPath}/CardCache.db";
    private static string connectionString = $"URI=file:{databaseFile}";

    private static string tableName = "card_data";

    private static CardDatabase _instance;

    string sqlQuery;
    IDbConnection dbconn;
    IDbCommand dbcmd;

    public static CardDatabase Instance
    {
        get 
        { 
            if(null == _instance)
                _instance = new CardDatabase();

            return _instance; 
        } 
    }

    private CardDatabase()
    {
        StartConnection();
    }

    private void StartConnection()
    {
        CreateTable();
    }

    private void CreateTable()
    {
        using (dbconn = new SqliteConnection(connectionString))
        {
            dbconn.Open();
            dbcmd = dbconn.CreateCommand();

            sqlQuery = $"CREATE TABLE IF NOT EXISTS {tableName} (name VARCHAR(255) PRIMARY KEY, image_uri VARCHAR(255), card_layout VARCHAR(255))";

            dbcmd.CommandText = sqlQuery;
            dbcmd.ExecuteScalar();
            
            dbconn.Close();
        }
    }

    public void InsertCardData(CardData cardData)
    {
        using (dbconn = new SqliteConnection(connectionString))
        {
            dbconn.Open();

            IDbCommand cmnd = dbconn.CreateCommand();
            cmnd.CommandText = $"INSERT INTO {tableName} (name, image_uri, card_layout) VALUES (\"{cardData.cardName}\", \"{cardData.imageUri}\", \"{cardData.cardLayout}\")";

            cmnd.ExecuteNonQuery();

            dbconn.Close();
        }
    }

    public bool IsCardCached(string name)
    {
        using (dbconn = new SqliteConnection(connectionString))
        {
            dbconn.Open();

            IDbCommand cmnd_read = dbconn.CreateCommand();
            IDataReader reader;

            string query = $"SELECT * FROM {tableName} WHERE name = \"{name}\"";
            cmnd_read.CommandText = query;
            reader = cmnd_read.ExecuteReader();

            bool cardFound = reader.Read();

            dbconn.Close();

            return cardFound;
        }
    }

    public CardData GetCardData(string cardName)
    {
        CardData cardData = null;

        if (!IsCardCached(cardName))
            return null;

        using (dbconn = new SqliteConnection(connectionString))
        {
            dbconn.Open();            

            IDbCommand cmnd_read = dbconn.CreateCommand();
            IDataReader reader;

            string query = $"SELECT * FROM {tableName} WHERE name = \"{cardName}\"";
            cmnd_read.CommandText = query;
            reader = cmnd_read.ExecuteReader();

            if (!reader.Read())
                return null;

            cardData = new CardData(reader[0].ToString(), reader[1].ToString(), reader[2].ToString());
            dbconn.Close();

            return cardData;
        }
    }
}
