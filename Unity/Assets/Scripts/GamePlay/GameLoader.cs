using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using HashtablePhoton = ExitGames.Client.Photon.Hashtable;
using Hashtable = System.Collections.Hashtable;

namespace GamePlay
{
    public class GameLoader : MonoBehaviour
    {
        [SerializeField] private GameController _gameController;
        [SerializeField] private GameObject _playerPrefab;

        private List<PlayerController> _playerList;
        private PlayerController _localPlayer;

        private void Awake()
        {
            _playerList = new List<PlayerController>();
        }

        private void Start()
        {
            StartCoroutine(LoadGame());
        }

        #region LOG
        private void LoadingLog(string log)
        {
            Debug.Log("[Loading] - " + log);
        }
        #endregion

        private IEnumerator LoadGame()
        {
            yield return WaitPlayersToJoin();
            yield return SpawnPlayers();

            _gameController.Initialize(_localPlayer, _playerList);
            _gameController.StartGame();
        }

        private IEnumerator WaitPlayersToJoin()
        {
            LoadingLog("Waiting playes to join the gameplay scene");

            int playersJoinned = 0;
            void OnChangedRoomData(HashtablePhoton properties)
            {
                playersJoinned = 0;
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    string key = "JoinnedLevel" + PhotonNetwork.PlayerList[i].ActorNumber;
                    if (properties.ContainsKey(key) || PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(key))
                    {
                        playersJoinned++;
                    }
                }

                LoadingLog("Total players joinned the game: " + playersJoinned);
            }

            NetworkEventDispatcher.RoomPropertiesUpdateEvent += OnChangedRoomData;

            LoadingLog("Sending that I joinned the game " + PhotonNetwork.LocalPlayer.ActorNumber);

            HashtablePhoton joinnedData = new HashtablePhoton()
            {
                {"JoinnedLevel" + PhotonNetwork.LocalPlayer.ActorNumber, true }
            };

            PhotonNetwork.CurrentRoom.SetCustomProperties(joinnedData);

            while (playersJoinned < PhotonNetwork.PlayerList.Length) yield return null;

            NetworkEventDispatcher.RoomPropertiesUpdateEvent -= OnChangedRoomData;
        }

        private IEnumerator SpawnPlayers()
        {
            LoadingLog("Spawning players");

            bool receivedViewIds = false;
            void OnChangedRoomData(HashtablePhoton properties)
            {
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    string key = "playerViewID" + PhotonNetwork.PlayerList[i].ActorNumber;
                    if (properties.TryGetValue(key, out object viewID))
                    {
                        _playerList[i].ViewID = (int)viewID;
                        _playerList[i].TransferOwnership(PhotonNetwork.PlayerList[i]);

                        receivedViewIds = true;
                    }
                }
            }

            void OnMasterClientSwitched(Player newMasterClient)
            {
                if (!receivedViewIds && PhotonNetwork.IsMasterClient)
                {
                    GenerateViewIds();
                }
            }

            void GenerateViewIds()
            {
                LoadingLog("Generating viewIDs");

                HashtablePhoton viewIDs = new HashtablePhoton();

                foreach (PhotonView photonView in _playerList)
                {
                    int id = PhotonNetwork.AllocateViewID(true);

                    viewIDs.Add("playerViewID" + photonView.OwnerActorNr, id);
                }

                PhotonNetwork.CurrentRoom.SetCustomProperties(viewIDs);
            }

            foreach (Player player in PhotonNetwork.PlayerList)
            {
                PlayerController photonPlayer = Instantiate(_playerPrefab).GetComponent<PlayerController>();
                photonPlayer.OwnerActorNr = player.ActorNumber;
                photonPlayer.ControllerActorNr = player.ActorNumber;
                _playerList.Add(photonPlayer);

                if (player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    _localPlayer = photonPlayer;
                }
            }

            OnChangedRoomData(PhotonNetwork.CurrentRoom.CustomProperties);

            NetworkEventDispatcher.RoomPropertiesUpdateEvent += OnChangedRoomData;
            NetworkEventDispatcher.MasterClientSwitchedEvent += OnMasterClientSwitched;

            if (!receivedViewIds && PhotonNetwork.IsMasterClient)
            {
                GenerateViewIds();
            }

            while (!receivedViewIds) yield return null;

            NetworkEventDispatcher.RoomPropertiesUpdateEvent -= OnChangedRoomData;
            NetworkEventDispatcher.MasterClientSwitchedEvent -= OnMasterClientSwitched;
        }

    }
}
