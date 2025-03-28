using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class RoomDisplay : MonoBehaviour
{
    public Text debugText;
    // 현재 방에 들어와 있다고 인식된 플레이어들의 Actor Number. 
    public List<Player> maleInRoom;
    public List<Player> femaleInRoom;

    // 플레이어 프로필 칸
    public GameObject[] males;
    public GameObject[] females;

    Loader Loader;
    PlayerReady playerReady;

    public Text RoomCode;
    public Text NumberOfPlayers;

    private void Awake()
    {
        Loader = FindObjectOfType<Loader>();
        playerReady = FindObjectOfType<PlayerReady>();
    }

    public void UpdatePlayerOrderDisplay()
    {
        List<int> playerOrder = PhotonNetwork.CurrentRoom.GetPlayerOrderList();
        maleInRoom = new List<Player>();
        femaleInRoom = new List<Player>();

        foreach (int actorNumber in playerOrder)
        {
            Player player = PhotonNetwork.PlayerList.FirstOrDefault(player => player.ActorNumber == actorNumber);
            if (player.GetPlayerGender() == "Male")
            {
                if (!maleInRoom.Contains(player)) maleInRoom.Add(player);
            }
            else if (player.GetPlayerGender() == "Female")
            {
                if (!femaleInRoom.Contains(player)) femaleInRoom.Add(player);
            }
        }

        // 플레이어들의 프로필을 순서대로 업데이트
        UpdateProfiles(maleInRoom, males);
        UpdateProfiles(femaleInRoom, females);

        NumberOfPlayersUpdate(maleInRoom.Count + femaleInRoom.Count);
    }

    void UpdateProfiles(List<Player> playerList, GameObject[] profileSlots)
    {
        int i = 0;

        // 리스트의 플레이어들을 순서대로 프로필 UI에 업데이트합니다.
        foreach (Player player in playerList)
        {
            //debugText.text += "inUpdateProfiles - " + player.GetPlayerName() + "\n";
            if (i >= profileSlots.Length) break; // 슬롯이 초과되면 중지
            GameObject profile = profileSlots[i];

            profile.transform.Find("PlayerName").GetComponent<Text>().text = player.GetPlayerName();
            profile.transform.Find("PlayerActorNumber").GetComponent<Text>().text = player.ActorNumber.ToString();
            Loader.LoadPlayerImage(profile.transform.Find("Mask/ImageSource").GetComponent<Image>(), player.GetPlayerImageFileName());
            profile.SetActive(true); // delay가 발생하긴 합니다.
            i++;
        }

        // 남은 슬롯 비활성화
        for (; i < profileSlots.Length; i++)
        {
            profileSlots[i].SetActive(false);
        }
    }

    void NumberOfPlayersUpdate(int numberOfPlayers)
    {
        NumberOfPlayers.text = numberOfPlayers.ToString()+"/"+PhotonNetwork.CurrentRoom.MaxPlayers;
    }

    public void RoomCodeUpdate(string roomCode)
    {
        RoomCode.text = "RoomCode : " + roomCode;
    }

    public void UpdatePlayerReadyStatus()
    {
        DebugCanvas.Instance.DebugLog("UpdatePlayerReadyStatus");
        Dictionary<int, bool> playerReadyDict = playerReady.GetPlayerReadyDictionary();

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            string gender = player.GetPlayerGender();
            GameObject[] targetArray = gender == "Male" ? males : females;
            List<Player> targetList = gender == "Male" ? maleInRoom : femaleInRoom;

            foreach(Player target in targetList) 
            {
                DebugCanvas.Instance.DebugLog($"target :: {target.GetPlayerName()}");
            }
            int index = targetList.IndexOf(player);

            if (index >= 0 && index < targetArray.Length)
            {
                DebugCanvas.Instance.DebugLog("UpdatePlayerReadyStatus2");
                bool isReady = playerReadyDict.TryGetValue(player.ActorNumber, out bool ready) && ready;

                // 플레이어가 targetList에 있을 경우 UI 업데이트
                Color textColor = isReady ? Color.green : Color.black;
                targetArray[index].transform.Find("PlayerName").GetComponent<Text>().color = textColor;
            }
            else
            {
                Debug.LogWarning("Player not found in the target list or index out of bounds");
            }
        }
    }

    public void OnLeftRoom(Player player = null)
    {
        if (player == null) player = PhotonNetwork.LocalPlayer;
        string gender = player.GetPlayerGender();
        GameObject[] targetArray = gender == "Male" ? males : females;
        List<Player> targetList = gender == "Male" ? maleInRoom : femaleInRoom;

        int index = targetList.IndexOf(player);
        targetArray[index].transform.Find("PlayerName").GetComponent<Text>().color = Color.black;
    }
}



