using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace GorillaMedia
{
    public static class CustomProperty
    {
        public const string ID = "github.com/iiDkRemastered/GorillaMedia";

        public static void SetCustomNetworkProperty()
        {
            if (PhotonNetwork.InRoom)
            {
                Hashtable customProps = new Hashtable();
                customProps.Add(ID, "true");
                PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
            }
        }
    }
}
