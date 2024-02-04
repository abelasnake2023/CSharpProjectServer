namespace CSharpProjectServer.Networking;



using ALib.Networking;
using CSharpProjectServer.BusinessLogic.AllResponse;
using System.Net.Sockets;
using System.Text;



public class ClientStream : Server
{
    private NetworkStream cStream;
    private byte[] allByteFromClientStream;
    private string deliUsedByUser;



    public ClientStream(NetworkStream clientStream)
    {
        this.cStream = clientStream;
    }



    public NetworkStream CStream
    {
        get { return this.cStream; }
    }
    public void SetAllByteFromClientStream()
    {
        ALibDataNetProtocol read = new ALibDataNetProtocol();
        this.allByteFromClientStream = read.ReadAllFromNetworkStream(this.cStream);
    }
    public byte[] GetAllByteFromClientStream()
    {
        return this.allByteFromClientStream;
    }
    public string DeliUsedByUser // if both used then the new one is sent
    {
        get
        {
            return this.deliUsedByUser;
        }
    }



    public List<byte[]> TryToReadOutDelimiter(out int newOldNone)
    {
        // none -> -1
        // old -> 0
        // new -> 1

        List<byte[]> packetWithoutNewDeli = null;
        List<byte[]> packetWithoutOldDeli = null;
        byte[] nB = null;
        byte[] oB = null;

        for (int i = 0; i < 2; i++)
        {
            nB = ALibDataNetProtocol.ReadBinaryFile(SMCD.NewPacketDelimiterFilePath);
            packetWithoutNewDeli = ALibDataNetProtocol.RidOutDelimiter(allByteFromClientStream,
             nB);
            oB = ALibDataNetProtocol.ReadBinaryFile(SMCD.OldPacketDelimiterFilePath);
            packetWithoutOldDeli = ALibDataNetProtocol.RidOutDelimiter(allByteFromClientStream,
                oB);
        }


        if (packetWithoutNewDeli == null && packetWithoutOldDeli == null)
        {
            newOldNone = -1;
            deliUsedByUser = null;
            return null;
        }
        else if(packetWithoutNewDeli == null && packetWithoutOldDeli != null)
        {
            if(DateTime.Now > Server.acceptOldDelimiter) // time expired for old Delimiter.
            {
                Console.WriteLine("Time Expired");
                deliUsedByUser = null;
                newOldNone = -1;
                return null;
            }
            else
            {
                Console.WriteLine("Time not Expired");
                this.deliUsedByUser = Encoding.UTF8.GetString(oB);
                newOldNone = 0;
                return packetWithoutOldDeli;
            }
        }
        else if(packetWithoutNewDeli != null && packetWithoutOldDeli != null)
        {
            this.deliUsedByUser = Encoding.UTF8.GetString(nB); // if both used then the delimiter
                                                               // will be the new one
            newOldNone = 1;

            //since both used the packet of both must be returned
            List<byte[]> bothUsed = packetWithoutOldDeli; // first add the old b/c that's the first
            // one sent
            foreach (var b in packetWithoutNewDeli)
            {
                bothUsed.Add(b);
            }

            return bothUsed;
        }
        else if(packetWithoutNewDeli != null && packetWithoutOldDeli == null)
        {
            this.deliUsedByUser = Encoding.UTF8.GetString(nB);
            newOldNone = 1;

            return packetWithoutNewDeli;
        }
        else //event though else can't exist since I need to initialize the out parameter and I should return
             //I will write it
        {
            newOldNone = -1;
            deliUsedByUser = null;
            return null;
        }
    }
}