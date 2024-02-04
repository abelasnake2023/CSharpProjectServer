namespace CSharpProjectServer;



using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text;
using CSharpProjectServer.BusinessLogic;
using System.Threading;
using CSharpProjectServer.Networking;
using ALib.Networking;
using System;



public class Server
{
    private TcpListener mTcpListener; // the socket
    private IPAddress mIP; // the socket ip address
    private int mPort; // socket port number
    protected static List<TcpClient> allClientOnline; // all clients that are connected currently to TcpListener
    protected static DateTime acceptOldDelimiter;
    protected Timer timer = null;



    static Server()
    {
        allClientOnline = new List<TcpClient>();
        acceptOldDelimiter = DateTime.Now; // just for giving default value when the constructor called
        //timer = new Timer(MyFunc, null, 0, 10000);
    }
    public Server()
    {

    }
    private static void MyFunc(object state)
    {
        // This method will be called by the timer on each interval
        Console.WriteLine($"Timer callback executed at: {acceptOldDelimiter}");
    }



    public void StartListeningForIncomingConnection(IPAddress ipaddr = null, int port = 23000)
    {
        Thread.CurrentThread.Name = "Main Thread"; // accept connection in the main Thread.

        if (ipaddr == null)
        {
            ipaddr = IPAddress.Any;
        }
        if (port <= 0)
        {
            port = 23000;
        }

        mIP = ipaddr;
        mPort = port;
        mTcpListener = new TcpListener(mIP, mPort); // binding the socket to ip address and port number

        try
        {
            mTcpListener.Start();
            TcpClient clientSocket = null;

            while (true)
            {
                try
                {
                    Console.WriteLine("Thread -> " + Thread.CurrentThread.Name);
                    Console.WriteLine("Server trying to connect...");
                    clientSocket = mTcpListener.AcceptTcpClient(); // I make synchronous purposely
                    Server.RemoveOfflineTcpClient(); // whenever removing is must just call this method.
                    allClientOnline.Add(clientSocket);
                    Console.WriteLine("Client with " + clientSocket.Client.RemoteEndPoint + " connected.");

                    // each client in one thread
                    Thread eachClient = new Thread(() => TakeCareOfTCPClient(clientSocket));
                    eachClient.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(".............error.............");      
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
    private void TakeCareOfTCPClient(TcpClient clientSocket) // both manager and users are clients
    {
        NetworkStream stream = null;
        ClientStream clientStream = null;
        string deliUsedForResponse = null;

        try
        {
            stream = clientSocket.GetStream();
            clientStream = new ClientStream(stream);

            Console.WriteLine("starting to execute client stream\n" +
                "until the client is disconnected! Thread name: " + Thread.CurrentThread.Name);
            do
            {
                int noneOldNew;
                clientStream.SetAllByteFromClientStream();
                List<byte[]> packetsWithoutDeli = clientStream.TryToReadOutDelimiter(out noneOldNew);
                if (packetsWithoutDeli == null) // client is disconnected or Unauthorized client
                                                // connected.
                {
                    return;
                }
                if (noneOldNew == -1) // I know this code will not be reached but
                                      // it's good if it exist.
                {
                    Console.WriteLine("Unauthorized Client!");
                    clientStream.CStream.Write(Encoding.UTF8.GetBytes("Unauthorized Client!"));
                    // since the attempt is done
                    // by unauthorized client and unauthorized client doesn't has the delimiter
                    // (not follow ALibPacket)
                    // you don't need to send the message as ALib packet using the class and method
                    // `ALibDataNetProtocol.ToBeSentDataToALibProtocolType`
                    return;
                }
                else
                {
                    if (noneOldNew == 0)
                    {
                        Console.WriteLine("user with the old Delimiter!");
                    }
                    else if (noneOldNew == 1)
                    {
                        Console.WriteLine("user with the new Delimiter!");
                    }
                    Console.WriteLine("authorized client!");
                }

                deliUsedForResponse = clientStream.DeliUsedByUser; // delimiter used for single response

                // Responding to all request from the client
                for (int i = 0; i < packetsWithoutDeli.Count; i++)
                {
                    Console.WriteLine("Amount of Question: " + packetsWithoutDeli.Count);

                    object[] metaDataAndMainData =
                        ALibDataNetProtocol.GetAllDataFromNonDelimitedPacket(packetsWithoutDeli[i]);

                    if (metaDataAndMainData != null)
                    {
                        Console.WriteLine(metaDataAndMainData[0].ToString());
                        Console.WriteLine(metaDataAndMainData[1].ToString());
                        Console.WriteLine(metaDataAndMainData[2].ToString());
                        Console.WriteLine(metaDataAndMainData[3].ToString());

                        RespondToRequest rToReq = new RespondToRequest(metaDataAndMainData, clientStream.CStream,
                             deliUsedForResponse);
                        rToReq.PossibleRespond(); // in the next update `PossibleRespond()` call must be await
                        // call b/c the response may be long and other request may be asked 
                        // from the client.
                    }
                    else
                    {
                        Console.WriteLine("meta data and main data not specified in proper way!");
                    }
                }
            } while (clientStream.CStream.Socket.Connected);
            Console.WriteLine("client disconnected!");
        }
        catch (ObjectDisposedException ob) // may be the stream disconnected
        {
            Console.Out.WriteLine("I think client disconnected!");
        }
        catch (IOException io) // may be the stream disconnected
        {
            Console.Out.WriteLine("I think client disconnected!");
        }
        catch (Exception ex) // other issue
        {
            Console.Out.WriteLine("Error while communicating with the client!");
        }
    }



    protected static void RemoveOfflineTcpClient()
    {
        TcpClient client = null;

        for (int i = 0; i < allClientOnline.Count; i++)
        {
            client = allClientOnline[i];

            // Check if the client is disconnected
            if (!client.Connected)
            {
                // Remove the disconnected client from the list
                allClientOnline.RemoveAt(i);

                // Optionally, you may want to close the TcpClient
                client.Close();
                client.Dispose();
            }
        }
    }
}