using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Com.Kawaiisun.SimpleHostile
{
    [System.Serializable]
    public class ProfileData
    {
        public string username;
        public int level;
        public int xp;

        public ProfileData()
        {
            this.username = "Default Username";
            this.level = 00;
            this.xp = 0;
        }
        public ProfileData(string u, int l, int x)
        {
            username = u;
            level = l;
            xp = x;
        }
    }

    public class Launcher : MonoBehaviourPunCallbacks
    {
        public TMP_InputField usernameField;
        public static ProfileData myProfileData = new ProfileData();


        public void Awake()
        {


            PhotonNetwork.AutomaticallySyncScene = true;

            myProfileData = Data.LoadProfile();
            usernameField.text = myProfileData.username;
            Connect();
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Successfully CONNECTED!");
           
            base.OnConnectedToMaster();
        }

        public override void OnJoinedRoom()
        {
            StartGame();
            base.OnJoinedRoom();
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Create();
            base.OnJoinRandomFailed(returnCode, message);
        }

        public void Connect()
        {
            Debug.Log("Connecting to server");
            PhotonNetwork.GameVersion = "0.0.0";
            PhotonNetwork.ConnectUsingSettings();
        }

        public void Join()
        {
            PhotonNetwork.JoinRandomRoom();
        }

        public void Create()
        {
            PhotonNetwork.CreateRoom("");
        }

        public void StartGame()
        {
            if (string.IsNullOrEmpty(usernameField.text))
            {
                myProfileData.username = "RAND_USER_" + Random.Range(100, 1000);
            }
            else
            {
                myProfileData.username = usernameField.text;

            }

            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                Data.SaveProfile(myProfileData);
                PhotonNetwork.LoadLevel(1);
            }
        }
    }
}