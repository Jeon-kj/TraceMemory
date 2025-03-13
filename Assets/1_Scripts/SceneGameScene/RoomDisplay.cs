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
    // ���� �濡 ���� �ִٰ� �νĵ� �÷��̾���� Actor Number. 
    public List<Player> maleInRoom;
    public List<Player> femaleInRoom;

    // �÷��̾� ������ ĭ
    public GameObject[] males;
    public GameObject[] females;

    public Loader Loader;

    public Text RoomCode;
    public Text NumberOfPlayers;

    private void Awake()
    {
        Loader = FindObjectOfType<Loader>();
    }

    public void UpdatePlayerOrderDisplay()
    {
        List<int> playerOrder = PhotonNetwork.CurrentRoom.GetPlayerOrderList();
        maleInRoom = new List<Player>();
        femaleInRoom = new List<Player>();

        //debugText.text += "maleInRoom.Count : " + maleInRoom.Count() + "\n";

        foreach (int actorNumber in playerOrder)
        {
            Player player = PhotonNetwork.PlayerList.FirstOrDefault(player => player.ActorNumber == actorNumber);
            //debugText.text += player.GetPlayerName() + "\n";
            if (player.GetPlayerGender() == "Male")
            {
                if (!maleInRoom.Contains(player)) maleInRoom.Add(player);
            }
            else if (player.GetPlayerGender() == "Female")
            {
                if (!femaleInRoom.Contains(player)) femaleInRoom.Add(player);
            }
        }

        // �÷��̾���� �������� ������� ������Ʈ
        UpdateProfiles(maleInRoom, males);
        UpdateProfiles(femaleInRoom, females);

        NumberOfPlayersUpdate(maleInRoom.Count + femaleInRoom.Count);
    }

    void UpdateProfiles(List<Player> playerList, GameObject[] profileSlots)
    {
        int i = 0;

        // ����Ʈ�� �÷��̾���� ������� ������ UI�� ������Ʈ�մϴ�.
        foreach (Player player in playerList)
        {
            //debugText.text += "inUpdateProfiles - " + player.GetPlayerName() + "\n";
            if (i >= profileSlots.Length) break; // ������ �ʰ��Ǹ� ����
            GameObject profile = profileSlots[i];

            profile.transform.Find("PlayerName").GetComponent<Text>().text = player.GetPlayerName();
            profile.transform.Find("PlayerActorNumber").GetComponent<Text>().text = player.ActorNumber.ToString();
            Loader.LoadPlayerImage(profile.transform.Find("Mask/ImageSource").GetComponent<Image>(), player.GetPlayerImageFileName());
            profile.SetActive(true); // delay�� �߻��ϱ� �մϴ�.
            i++;
        }

        // ���� ���� ��Ȱ��ȭ
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
        List<int> playerReady = PhotonNetwork.CurrentRoom.GetPlayerReadyList();


        foreach (Player player in PhotonNetwork.PlayerList)
        {
            string gender = player.GetPlayerGender();
            GameObject[] targetArray = gender == "Male" ? males : females;
            List<Player> targetList = gender == "Male" ? maleInRoom : femaleInRoom;

            int index = targetList.IndexOf(player);

            if (index >= 0 && index < targetArray.Length)
            {
                // �÷��̾ targetList�� ���� ��� UI ������Ʈ
                Color textColor = playerReady.Contains(player.ActorNumber) ? Color.green : Color.black;
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



