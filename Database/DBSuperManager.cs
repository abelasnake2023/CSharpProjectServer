namespace CSharpProjectServer.Database;



using ALib.Database.ALibSqlServer;
using CSharpProjectServer.BusinessLogic.AllResponse;
using System.Diagnostics;



public class DBSuperManager
{
    private string username;


    public DBSuperManager(string username)
    {
        this.username = username;
    }


    public bool NewDeliAcceptToFalse()
    {
        if(this.username != SMCD.SuperManagerUsername)
        {
            Debug.WriteLine("Not super manager exception!");
            return false;
        }

        try
        {
            ALibDataReader reader = new ALibDataReader();
            reader.ExecuteStoredProcedure("NewDeliAcceptToFalse");

            return true;
        }
        catch(Exception ex)
        {
            Debug.WriteLine("Exception while trying to work with the Database!");
            return false;
        }
    }
}