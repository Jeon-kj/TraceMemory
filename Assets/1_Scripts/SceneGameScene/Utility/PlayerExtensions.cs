// PlayerExtensions.cs
using Photon.Realtime;
using ExitGames.Client.Photon;

public static class PlayerExtensions
{
    public static string GetPlayerName(this Player player)
    {
        if (player.CustomProperties.TryGetValue("Name", out object playerName))
        {
            return playerName as string;
        }
        return null;
    }

    public static string GetPlayerGender(this Player player)
    {
        if (player.CustomProperties.TryGetValue("Gender", out object playerGender))
        {
            return playerGender as string;
        }
        return null;
    }

    public static string GetPlayerImageFileName(this Player player)
    {
        if (player.CustomProperties.TryGetValue("ImageFileName", out object playerImageFileName))
        {
            return playerImageFileName as string;
        }
        return null;
    }

    public static string GetPlayerPreLifeName(this Player player)
    {
        if (player.CustomProperties.TryGetValue("PreLifeName", out object playerPreLifeName))
        {
            return playerPreLifeName as string;
        }
        return null;
    }
}
