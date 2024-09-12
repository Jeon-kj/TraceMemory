using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;

public static class RoomExtensions
{
    public static List<int> GetPlayerOrderList(this Room room)
    {
        if (room.CustomProperties.TryGetValue("PlayerOrder", out object playerReadyObj) && playerReadyObj is int[] playerOrderArray)
        {
            // ����Ʈ�� ��ȯ�Ͽ� ��ȯ
            return playerOrderArray.ToList();
        }
        else
        {
            // PlayerReady Ű�� ���ų� ���� null�̸� �� ����Ʈ ��ȯ
            return new List<int>();
        }
    }

    public static List<int> GetPlayerReadyList(this Room room)
    {
        if (room.CustomProperties.TryGetValue("PlayerReady", out object playerReadyObj) && playerReadyObj is int[] playerReadyArray)
        {
            // ����Ʈ�� ��ȯ�Ͽ� ��ȯ
            return playerReadyArray.ToList();
        }
        else
        {
            // PlayerReady Ű�� ���ų� ���� null�̸� �� ����Ʈ ��ȯ
            return new List<int>();
        }
    }

    public static void AddPlayerOrder(this Room room, int actorNumber)
    {
        List<int> playerOrder = PhotonNetwork.CurrentRoom.GetPlayerOrderList();
        playerOrder.Add(actorNumber);
        room.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "PlayerOrder", playerOrder.ToArray() } });
    }

    public static void AddPlayerReady(this Room room, int actorNumber)
    {
        List<int> playerReady = PhotonNetwork.CurrentRoom.GetPlayerReadyList();
        playerReady.Add(actorNumber);
        room.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "PlayerReady", playerReady.ToArray() } });
    }

    public static void DelPlayerOrder(this Room room, int actorNumber)
    {
        List<int> playerOrder = PhotonNetwork.CurrentRoom.GetPlayerOrderList();
        if (playerOrder.Contains(actorNumber))
        {
            playerOrder.Remove(actorNumber);
            room.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "PlayerOrder", playerOrder.ToArray() } });
        }
    }

    public static void DelPlayerReady(this Room room, int actorNumber)
    {
        List<int> playerReady = PhotonNetwork.CurrentRoom.GetPlayerReadyList();
        if (playerReady.Contains(actorNumber))
        {
            playerReady.Remove(actorNumber);
            room.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "PlayerReady", playerReady.ToArray() } });
        }            
    }
}
