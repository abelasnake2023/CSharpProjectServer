namespace CSharpProjectServer.BusinessLogic;



using ALib.Networking;
using CSharpProjectServer.BusinessLogic.AllResponse;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;



public class RespondToRequest
{
    private NetworkStream stream;
    private string dataType;
    private object mainData;
    private string category;
    private string username;
    private string deliUsedForResponse;



    public RespondToRequest(object[] metaDataAndMainData, NetworkStream stream, string deliUsedForResponse)
    {
        this.stream = stream; // the stream we will respond on
        this.dataType = metaDataAndMainData[0].ToString(); // only string
        this.mainData = metaDataAndMainData[1]; // can be any data type
        this.category = metaDataAndMainData[2].ToString(); // only string
        this.username = metaDataAndMainData[3].ToString(); // only string
        this.deliUsedForResponse = deliUsedForResponse;
    }



    // this is the main place where the majority of reply occur on the client Stream.
    public void PossibleRespond()
    {
        if(this.category == ALibDataNetProtocol.GetCategoriesByKey("S.M.C.D"))
            // Super manager change delimiter
        {
            string newDelimiter = (string)this.mainData;
            SMCD smcd = new SMCD(this.username, newDelimiter);
            string replyForChangeDelimiter = "";
            bool done = smcd.ChangeDelimiter(out replyForChangeDelimiter);
            if (done)
            {
                byte[] toBeSent = ALibDataNetProtocol.ToBeSentDataToALibProtocolType
                    (replyForChangeDelimiter, "string",
                    "00", SMCD.SuperManagerUsername, Encoding.UTF8.GetBytes(this.deliUsedForResponse));
                stream.Write(toBeSent);
            }
            else
            {
                byte[] toBeSent = ALibDataNetProtocol.ToBeSentDataToALibProtocolType
                    (replyForChangeDelimiter, "string",
                    "00", SMCD.SuperManagerUsername, Encoding.UTF8.GetBytes(this.deliUsedForResponse));
                stream.Write(toBeSent);
            }
        }
        else if(this.category == ALibDataNetProtocol.GetCategoriesByKey("M.S.V.A"))
            // Manager Send Video For All it's user related
        {

        }
        else if(this.category == ALibDataNetProtocol.GetCategoriesByKey("M.S.V.D"))
        {

        }
        else if(this.category == ALibDataNetProtocol.GetCategoriesByKey("M.S.V.I"))
        {

        }
        else if(this.category == ALibDataNetProtocol.GetCategoriesByKey("M.S.V.S"))
        {

        }
        else if (this.category == ALibDataNetProtocol.GetCategoriesByKey("M.S.T.A"))
        {

        }
        else if (this.category == ALibDataNetProtocol.GetCategoriesByKey("M.S.T.D"))
        {

        }
        else if (this.category == ALibDataNetProtocol.GetCategoriesByKey("M.S.T.I"))
        {

        }
        else if (this.category == ALibDataNetProtocol.GetCategoriesByKey("M.S.T.S"))
        {

        }
        else if (this.category == ALibDataNetProtocol.GetCategoriesByKey("M.R.D"))
        {

        }
        else if (this.category == ALibDataNetProtocol.GetCategoriesByKey("U.R.D"))
        {

        }
        else if (this.category == ALibDataNetProtocol.GetCategoriesByKey("U.S.T.M"))
        {

        }
        else
        {
            // category not found exception
            Console.WriteLine("Category not found Exception!");
        }
    }
}