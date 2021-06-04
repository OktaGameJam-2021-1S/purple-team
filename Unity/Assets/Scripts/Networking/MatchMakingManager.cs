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

namespace Networking
{
    public class MatchMakingManager : IConnectionCallbacks, IMatchmakingCallbacks, IInRoomCallbacks, ILobbyCallbacks, IWebRpcCallback, IErrorInfoCallback, IOnEventCallback
    {
        public event Action FailedToFindAMatchEvent;
        public event Action CancelMatchmakinEvent;
        public event Action MatchFoundedEvent;

        public const byte TargetNumberOfPlayers = 2;

        public void StartMatchMaking()
        {
            MatchMakingLog("Starting photon matchmaking");

            PhotonNetwork.ConnectUsingSettings();
        }

        public void CancelMatchMaking()
        {
            MatchMakingLog("Request cancel photon matchmaking");
            PhotonNetwork.Disconnect();
        }

        #region LOG
        private void MatchMakingLog(string log)
        {
            Debug.Log("[MatchMaking] - " + log);
        }
        #endregion

        #region PHOTON CALL BACKS
        public void OnConnectedToMaster()
        {
            PhotonNetwork.JoinRandomRoom();
        }

        public void OnDisconnected(DisconnectCause cause)
        {
            MatchMakingLog("Disconnected from matchmaking, cause - photon: " + cause.ToString());

            PhotonNetwork.RemoveCallbackTarget(this);

            switch (cause)
            {
                case DisconnectCause.DisconnectByClientLogic: CancelMatchmakinEvent?.Invoke(); break;
                default: FailedToFindAMatchEvent?.Invoke(); break;
            }
        }

        public void OnJoinRandomFailed(short returnCode, string message)
        {
            string roomName = "Public " + Random.Range(1000, 10000);

            byte maxNumberOfPlayers = TargetNumberOfPlayers;

            RoomOptions options = new RoomOptions
            {
                MaxPlayers = maxNumberOfPlayers,
                EmptyRoomTtl = 100,
                PublishUserId = true, // Important to keep the user reference to the networking
            };

            PhotonNetwork.CreateRoom(roomName, options, null);
        }

        public void OnJoinedRoom()
        {
            MatchMakingLog("Joined a room!");

            if (PhotonNetwork.PlayerList.Length >= TargetNumberOfPlayers)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
                PhotonNetwork.RemoveCallbackTarget(this);
                MatchFoundedEvent?.Invoke();
            }
        }

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            MatchMakingLog("A player has joined the room!");

            if (PhotonNetwork.PlayerList.Length >= TargetNumberOfPlayers)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
                PhotonNetwork.RemoveCallbackTarget(this);
                MatchFoundedEvent?.Invoke();
            }
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            MatchMakingLog("A player has left the room " + otherPlayer.NickName + " - " + otherPlayer.ActorNumber);
        }

        #region Not Used Photon Callbacks
        public void OnRoomPropertiesUpdate(HashtablePhoton properties)
        {

        }
        public void OnEvent(EventData photonEvent)
        {

        }
        public void OnLeftRoom()
        {

        }
        public void OnJoinRoomFailed(short returnCode, string message)
        {

        }

        public void OnMasterClientSwitched(Player newMasterClient)
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
}