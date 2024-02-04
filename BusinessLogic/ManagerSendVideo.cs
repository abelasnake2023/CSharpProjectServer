namespace CSharpProjectServer.BusinessLogic;



using CSharpProjectServer.Database;
using System.Net.Sockets;
using System.Text;

public class ManagerSendVideo : Server
{
    private string username;
    private int manId;
    private int videoId;
    private string[] allAppUsernameLinkedToManager;



    public ManagerSendVideo(string usernameAndVideoId)
    {
        usernameAndVideoId = usernameAndVideoId.Trim();

        int usernameLength = usernameAndVideoId.IndexOf('\n');
        this.username = usernameAndVideoId.Substring(0, usernameLength).Trim();
        int.TryParse(
            usernameAndVideoId.Substring(usernameLength + 1).Trim(), out this.videoId
            );

        this.manId = DBManagerSendVideo.GetManagerIdByUserName(this.username.Trim());
        this.allAppUsernameLinkedToManager = DBManagerSendVideo.AppUserNameLinkedToManager(this.manId);
    }

    public async void SendToAllOnlineUser()
    {
        if (this.manId > 0)
        {
            if(this.allAppUsernameLinkedToManager.Length > 0)
            {
                NetworkStream stream = null;
                StreamReader reader = null;
                char manOrUser = '\0';
                char whatToDo = '\0';
                string appUsername = null;
                int usernameLength = 0;

                Server.RemoveOfflineTcpClient();
                for (int i = 0; i < Server.allClientOnline.Count; i++)
                {
                    stream = Server.allClientOnline[i].GetStream();
                    reader = new StreamReader(stream);
                    string allThingReaded = "";

                    allThingReaded = await reader.ReadToEndAsync();
                    manOrUser = allThingReaded[0];
                    whatToDo = allThingReaded[1]; // not needed for receiving

                    if (manOrUser == '1') // if it is app
                    {
                        usernameLength = allThingReaded.IndexOf('\n') - 2;
                        appUsername = allThingReaded.Substring(2, usernameLength);

                        for(int j = 0; j < this.allAppUsernameLinkedToManager.Length; j++)
                        {
                            if(appUsername == allAppUsernameLinkedToManager[j])
                            {
                                byte[] writeToStream = Encoding.UTF8.GetBytes("10" + this.videoId.ToString());
                                Console.WriteLine("Trying to send to " + Server.allClientOnline[i].Client.RemoteEndPoint);
                                await stream.WriteAsync(writeToStream, 0, writeToStream.Length);
                                Array.Clear(writeToStream, 0, writeToStream.Length);
                            }
                        }
                    }
                }
            }
        }
    }

    public void IsVideoUploadToAppToTrue()
    {
        if(DBManagerSendVideo.VideoUploadedToAppToTrue(this.videoId))
        {
            Console.WriteLine("Video with id " + this.videoId + " has been sent to all currently online " +
                "users.");
        }
        else
        {
            Console.WriteLine("Video with id " + this.videoId + " has not been sent to all currently online " +
                "users.");
        }
    }
}