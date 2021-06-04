using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;
using Random = UnityEngine.Random;
using HashtablePhoton = ExitGames.Client.Photon.Hashtable;
using Hashtable = System.Collections.Hashtable;

public class NetworkEventDispatcher : IConnectionCallbacks, IMatchmakingCallbacks, IInRoomCallbacks, ILobbyCallbacks, IWebRpcCallback, IErrorInfoCallback, IOnEventCallback
{
    // Network events
    public static event Action<HashtablePhoton> RoomPropertiesUpdateEvent;
    public static event Action<DisconnectCause> DisconnectedEvent;
    public static event Action<Player> PlayerLeftRoomEvent;
    public static event Action<Player> MasterClientSwitchedEvent;

    private static NetworkEventDispatcher _dispatcher;

    #region Initialization
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnBeforeSceneLoadRuntimeMethod()
    {
        _dispatcher = new NetworkEventDispatcher();
        PhotonNetwork.AddCallbackTarget(_dispatcher);
    }
    #endregion

    #region PHOTON CALL BACKS
    public void OnRoomPropertiesUpdate(HashtablePhoton properties)
    {
        RoomPropertiesUpdateEvent?.Invoke(properties);
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        DisconnectedEvent?.Invoke(cause);
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        PlayerLeftRoomEvent?.Invoke(otherPlayer);
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        MasterClientSwitchedEvent?.Invoke(newMasterClient);
    }

    #region Not Used Photon Callbacks
    public void OnConnectedToMaster()
    {

    }

    public void OnEvent(EventData photonEvent)
    {

    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {

    }

    public void OnJoinedRoom()
    {

    }

    public void OnLeftRoom()
    {

    }
    public void OnJoinRandomFailed(short returnCode, string message)
    {

    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {

    }

    public void OnConnected()
    {

    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {

    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {

    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {

    }

    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {

    }

    public void OnCreatedRoom()
    {

    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {

    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, HashtablePhoton changedProps)
    {

    }

    public void OnJoinedLobby()
    {

    }

    public void OnLeftLobby()
    {

    }

    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {

    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {

    }

    public void OnWebRpcResponse(OperationResponse response)
    {

    }

    public void OnErrorInfo(ErrorInfo errorInfo)
    {

    }
    #endregion
    #endregion
}
