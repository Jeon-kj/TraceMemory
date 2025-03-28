using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using System.Collections;

public static class RoomExtensions
{
    // About Player Order
    public static List<int> GetPlayerOrderList(this Room room)
    {
        if (room.CustomProperties.TryGetValue("PlayerOrder", out object playerOrderObj) && playerOrderObj is int[] playerOrderArray)
        {
            // 리스트로 변환하여 반환
            return playerOrderArray.ToList();
        }
        else
        {
            // PlayerReady 키가 없거나 값이 null이면 빈 리스트 반환
            return new List<int>();
        }
    }

    public static void AddPlayerOrder(this Room room, int actorNumber)
    {
        List<int> playerOrder = PhotonNetwork.CurrentRoom.GetPlayerOrderList();
        playerOrder.Add(actorNumber);
        room.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "PlayerOrder", playerOrder.ToArray() } });
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

    // About Player Ready
    public static void RegisterPlayerReadyStatus(this Room room, int actorNumber)
    {
        ExitGames.Client.Photon.Hashtable readyTable = room.CustomProperties.ContainsKey("PlayerReady")
            ? (ExitGames.Client.Photon.Hashtable)room.CustomProperties["PlayerReady"]
            : new ExitGames.Client.Photon.Hashtable();

        if (!readyTable.ContainsKey(actorNumber.ToString()))
        {
            readyTable[actorNumber.ToString()] = false;

            var newProps = new ExitGames.Client.Photon.Hashtable
            {
                { "PlayerReady", readyTable }
            };

            room.SetCustomProperties(newProps);
            DebugCanvas.Instance.DebugLog($"RegisterPlayerReadyStatus :: {actorNumber}");
        }
    }

    public static Dictionary<int, bool> GetPlayerReadyDictionary(this Room room)
    {
        Dictionary<int, bool> readyDict = new Dictionary<int, bool>();

        if (room.CustomProperties.TryGetValue("PlayerReady", out object readyObj) && readyObj is ExitGames.Client.Photon.Hashtable readyTable)
        {
            foreach (DictionaryEntry entry in readyTable)
            {
                if (int.TryParse(entry.Key.ToString(), out int actorNumber) && entry.Value is bool isReady)
                {
                    readyDict[actorNumber] = isReady;
                }
            }
        }

        return readyDict;
    }


    public static void SetPlayerReadyStatus(this Room room, int actorNumber, bool isReady)
    {
        ExitGames.Client.Photon.Hashtable readyTable = room.CustomProperties.ContainsKey("PlayerReady")
            ? (ExitGames.Client.Photon.Hashtable)room.CustomProperties["PlayerReady"]
            : new ExitGames.Client.Photon.Hashtable();

        readyTable[actorNumber.ToString()] = isReady;

        var newProps = new ExitGames.Client.Photon.Hashtable
        {
            { "PlayerReady", readyTable }
        };

        room.SetCustomProperties(newProps);
        DebugCanvas.Instance.DebugLog($"SetPlayerReadyStatus :: {actorNumber} :: {isReady}");
    }

    public static bool GetPlayerReadyStatus(this Room room, int actorNumber)
    {
        if (room.CustomProperties.TryGetValue("PlayerReady", out object tableObj) && tableObj is ExitGames.Client.Photon.Hashtable readyTable)
        {
            if (readyTable.ContainsKey(actorNumber.ToString()))
            {
                return (bool)readyTable[actorNumber.ToString()];
            }
        }
        return false;
    }

    public static void DelPlayerReady(this Room room, int actorNumber)
    {
        if (!room.CustomProperties.TryGetValue("PlayerReady", out object tableObj) || tableObj is not ExitGames.Client.Photon.Hashtable readyTable)
            return;

        if (readyTable.ContainsKey(actorNumber.ToString()))
        {
            readyTable.Remove(actorNumber.ToString());

            var newProps = new ExitGames.Client.Photon.Hashtable
            {
                { "PlayerReady", readyTable }
            };

            room.SetCustomProperties(newProps);
            DebugCanvas.Instance.DebugLog($"DelPlayerReady :: {actorNumber}");
        }
    }

    public static int GetMaxPlayerNumber(this Room room)
    {
        if (room.CustomProperties.TryGetValue("MaxPlayer", out object maxPlayerObj) && maxPlayerObj is int maxPlayer)
        {
            return maxPlayer;
        }

        // 기본값을 반환 (예: 0 또는 다른 기본값 설정 가능)
        return 0;
    }
}
