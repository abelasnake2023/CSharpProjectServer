namespace CSharpProjectServer;

using ALib.Networking;
using CSharpProjectServer.BusinessLogic.AllResponse;
using System;
using System.Text;

public class Program
{
    private static void Main()
    {
        Server s = new Server();
        s.StartListeningForIncomingConnection();


        //ALibDataNetProtocol.WriteToBinaryFile(SMCD.NewPacketDelimiterFilePath, Encoding.UTF8.GetBytes("loli"));
        byte[] b = ALibDataNetProtocol.ReadBinaryFile(SMCD.NewPacketDelimiterFilePath);
        Console.WriteLine(Encoding.UTF8.GetString(b));
    }
}