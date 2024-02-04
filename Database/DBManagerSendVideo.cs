namespace CSharpProjectServer.Database;



using System.Data.SqlClient;
using ALib.Database.ALibSqlServer;



public class DBManagerSendVideo
{
    public static int GetManagerIdByUserName(string username)
    {
        username = username.Trim();

        object[,] param = new object[1, 3]
        {
            { "@username", "varchar", username }
        };

        try
        {
            ALibDataReader reader = new ALibDataReader();
            int mId = (int)reader.ExecuteScalarFunction("dbo.GetManagerIdByUsername", param, 15);

            if(mId > 0)
            {
                return mId;
            }
            return 0;
        }
        catch (SqlException es)
        {
            return 0;
        }
        catch (Exception ex)
        {
            return 0;
        }
    }
    public static string[] AppUserNameLinkedToManager(int managerId)
    {
        string[] usernames = null;

        object[,] param = new object[1, 3]
        {
            { "@manId", "int", managerId }
        };

        try
        {
            ALibDataReader reader = new ALibDataReader();
            object[,] o = reader.ExecuteTableValuedFunction("dbo.AllAccNumToAppUsername", "uName", param);

            usernames = new string[o.GetLength(0)];
            for(int i = 0; i < o.GetLength(0); i++)
            {
                usernames[i] = o[i, 0].ToString().Trim();
            }

            return usernames;
        }
        catch(SqlException ex)
        {
            return null;
        }
        catch(Exception ex)
        {
            return null;
        }
    }
    public static bool VideoUploadedToAppToTrue(int vId)
    {
        object[,] param = new object[1, 3]
        {
            { "@videoId", "int", vId }
        };


        try
        {
            ALibDataReader reader = new ALibDataReader();
            reader.ExecuteStoredProcedure("VideoUploadedToTrue", param);

            return true;
        }
        catch (Exception ex) 
        {
            return false;
        }
    }
}