namespace CSharpProjectServer.BusinessLogic.AllResponse;



using ALib.Networking;
using CSharpProjectServer.Database;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;



public class SMCD : Server
{
    private static string newPacketDelimiterFilePath;
    private static string oldPacketDelimiterFilePath;
    private static string appVersionFilePath;
    private static string superManagerUsername;
    private string newDelimiter;
    private string username;



    static SMCD()
    {
        superManagerUsername = "abelasnake"; // can be changed
        SMCD.newPacketDelimiterFilePath = @"C:\Users\user\Documents\C#\C# Code\CSharpProject\CSharpProjectServer\app file\newPacketDelimiter.bin";
        SMCD.oldPacketDelimiterFilePath = @"C:\Users\user\Documents\C#\C# Code\CSharpProject\CSharpProjectServer\app file\oldPacketDelimiter.bin";
        SMCD.appVersionFilePath = @"C:\Users\user\Documents\C#\C# Code\CSharpProject\CSharpProjectServer\app file\appVersion.bin";
    }
    public SMCD(string username, string delimiter)
    {
        this.newDelimiter = delimiter;
        this.username = username;
    }



    public static string NewPacketDelimiterFilePath
    {
        get
        {
            return newPacketDelimiterFilePath;
        }
    }
    public static string OldPacketDelimiterFilePath
    {
        get
        {
            return oldPacketDelimiterFilePath;
        }
    }
    public static string SuperManagerUsername
    {
        get { return superManagerUsername; }
    }
    public static string AppVersionFilePath
    {
        get
        {
            return appVersionFilePath;
        }
    }



    public bool ChangeDelimiter(out string reply)
    {
        if (SMCD.superManagerUsername != this.username)
        {
            Debug.WriteLine("You are not the manager!");
            reply = "You are not the manager!";
            return false;
        }
        else if(Server.acceptOldDelimiter > DateTime.Now)
        {
            Console.WriteLine("Super Manager: Please note that you are required to wait a minimum of 1 day\n " +
                "before making any changes to the delimiter following the most recent delimiter modification.");
            Debug.WriteLine("Super Manager: Please note that you are required to wait a minimum of 1 day\n " +
                "before making any changes to the delimiter following the most recent delimiter modification.");
            reply = "Super Manager: Please note that you are required to wait a minimum of 1 day\n " +
                "before making any changes to the delimiter following the most recent delimiter modification.";
            return false;
        }
        else if(Server.acceptOldDelimiter < DateTime.Now)
        {
            Console.WriteLine("Valid to change the delimiter");
            Debug.WriteLine("Valid to change the delimiter");
        }


        DateTime setLastExpireDate = Server.acceptOldDelimiter; // I'm doing this if any error is encountered
        // to roll back!
        Console.WriteLine("Expiry date modified:- add 1 Day!");
        Server.acceptOldDelimiter = DateTime.Now.AddDays(1); // ranging the validity of the old Delimiter

        byte[] oldDelimiter = null;
        byte[] newDelimiter = Encoding.UTF8.GetBytes(this.newDelimiter);
        // First pass the current delimiter to the old delimiter file
        try
        {
            // reading the delimiter that will be old
            using (FileStream fileStream = new FileStream(newPacketDelimiterFilePath,
                FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader binaryReader = new BinaryReader(fileStream))
                {
                    FileInfo fileInfo = new FileInfo(newPacketDelimiterFilePath);

                    long bufferLength = fileInfo.Length;
                    oldDelimiter = new byte[bufferLength];
                    binaryReader.Read(oldDelimiter, 0, oldDelimiter.Length);

                    Console.WriteLine("to be old Delimiter: " + Encoding.UTF8.GetString(oldDelimiter));
                }
            }

            // writing to the old delimiter file
            using (FileStream fileStream = new FileStream(oldPacketDelimiterFilePath,
                FileMode.Truncate, FileAccess.Write))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                {
                    binaryWriter.Write(oldDelimiter, 0, oldDelimiter.Length);
                }
            }
        }
        catch (Exception ex)
        {
            Server.acceptOldDelimiter = setLastExpireDate;
            Debug.WriteLine("Unable to read/write the files that contain the delimiters!");
            Console.WriteLine("Unable to read/write the files that contain the delimiters!");
            reply = "Unable to read/write the files that contain the delimiters!";
            return false;
        }

        // Now you can write the new delimiter to the new delimiter file
        try
        {
            using (FileStream fileStream = new FileStream(newPacketDelimiterFilePath,
            FileMode.Truncate, FileAccess.Write))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                {
                    binaryWriter.Write(newDelimiter, 0, newDelimiter.Length);
                }
            }
        }
        catch (Exception ex)
        {
            Server.acceptOldDelimiter = setLastExpireDate;
            Console.WriteLine("Unable to write the new delimiter to the file!");
            Debug.WriteLine("Unable to write the new delimiter to the file!");
            reply = "Unable to write the new delimiter to the file!";
            return false;
        }

        //some essential thing to be done after setting the new delimiter.
        AfterNewDeliSet();

        reply = "Delimiter changed successfully!";
        return true;
    }
    private async Task AfterNewDeliSet()
    {
        //setting the version
        byte[] appVersion = ALibDataNetProtocol.ReadBinaryFile(appVersionFilePath);
        double appVersionDouble = BitConverter.ToDouble(appVersion);
        appVersionDouble += 0.01;
        byte[] newAppVersion = BitConverter.GetBytes(appVersionDouble);
        ALibDataNetProtocol.WriteToBinaryFile(appVersionFilePath, newAppVersion);

        //making new Delimiter accept to false for all client
        DBSuperManager super = new DBSuperManager(this.username);
        super.NewDeliAcceptToFalse();

        //Get all online users and send them the new Delimiter
        Server.RemoveOfflineTcpClient();
        NetworkStream stream = null;
        byte[] oldDelimiterByte = ALibDataNetProtocol.ReadBinaryFile(SMCD.OldPacketDelimiterFilePath);
        byte[] newDelimiterByte = ALibDataNetProtocol.ReadBinaryFile(SMCD.NewPacketDelimiterFilePath);
        string newDelimiterStr = Encoding.UTF8.GetString(newDelimiterByte);
        foreach (var c in Server.allClientOnline)
        {
            stream = c.GetStream();
            byte[] toBeSent = ALibDataNetProtocol.ToBeSentDataToALibProtocolType(newDelimiterStr, "string", "00",
                SMCD.SuperManagerUsername, oldDelimiterByte);
            await stream.WriteAsync(toBeSent);
        }
    }
}