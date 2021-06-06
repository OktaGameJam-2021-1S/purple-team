using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using HashtablePhoton = ExitGames.Client.Photon.Hashtable;
using Hashtable = System.Collections.Hashtable;
using System;
using Random = UnityEngine.Random;

namespace GamePlay
{
    public class GameLoader : MonoBehaviour
    {
        [SerializeField] private GameController _gameController;
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private GameObject _safeZonePrefab;
        [SerializeField] private LostKid _lostKid;
        [SerializeField] private ExitCave _exitCave;

        private List<PlayerController> _playerList;
        private PlayerController _localPlayer;
        private Transform _playerSpawnPosition;
        private List<Transform> _refillPositions;
        private List<Transform> _enemySpawnPositions;

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
            yield return BuildMap();
            yield return SpawnPlayers(_playerSpawnPosition);
            yield return SpawnSafeZones(_refillPositions);

            LoadingLog("Finished loading");

            _gameController.Initialize(_localPlayer, _playerList, _lostKid, _exitCave);
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

        private IEnumerator BuildMap()
        {
            LoadingLog("Loading map seed");

            int mapSeed = -1;
            string mapSeedKey = "MapSeed";
            void OnChangedRoomData(HashtablePhoton properties)
            {
                if (properties.ContainsKey(mapSeedKey))
                {
                    mapSeed = (int)properties[mapSeedKey];

                    LoadingLog("Received map seed " + mapSeed);
                }
            }

            void OnMasterClientSwitched(Player newMasterClient)
            {
                if (mapSeed == -1 && PhotonNetwork.IsMasterClient)
                {
                    GenerateMapSeed();
                }
            }

            void GenerateMapSeed()
            {
                LoadingLog("Generating viewIDs");

                HashtablePhoton mapSeedHash = new HashtablePhoton()
                {
                    { mapSeedKey, (int) Random.Range(0, 100000) }
                };

                PhotonNetwork.CurrentRoom.SetCustomProperties(mapSeedHash);
            }

            OnChangedRoomData(PhotonNetwork.CurrentRoom.CustomProperties);

            NetworkEventDispatcher.RoomPropertiesUpdateEvent += OnChangedRoomData;
            NetworkEventDispatcher.MasterClientSwitchedEvent += OnMasterClientSwitched;

            if (mapSeed == -1 && PhotonNetwork.IsMasterClient)
            {
                GenerateMapSeed();
            }

            while (mapSeed == -1) yield return null;

            NetworkEventDispatcher.RoomPropertiesUpdateEvent -= OnChangedRoomData;
            NetworkEventDispatcher.MasterClientSwitchedEvent -= OnMasterClientSwitched;

            LoadingLog("Bulding map");

            bool finishedBuilding = false;

            DungeonHelper.Instance.BuildDungeon(Convert.ToUInt32(mapSeed),
            delegate ()
            {
                finishedBuilding = true;
            });

            while (!finishedBuilding) yield return null;

            yield return null;

            _lostKid.transform.position = DungeonHelper.Instance.GetObjectivePoint().position;
            _exitCave.transform.position = DungeonHelper.Instance.GetPlayerSpawnPoint().position;

            _playerSpawnPosition = DungeonHelper.Instance.GetPlayerSpawnPoint();
            _refillPositions = DungeonHelper.Instance.GetRefillPoints();
            _enemySpawnPositions = DungeonHelper.Instance.GetEnemySpawnPoints();

            _refillPositions.Add(_lostKid.transform);
        }

        private IEnumerator SpawnPlayers(Transform spawnPosition)
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
                        _playerList[i].photonView.ViewID = (int)viewID;
                        _playerList[i].photonView.TransferOwnership(PhotonNetwork.PlayerList[i]);

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

                foreach (PlayerController player in _playerList)
                {
                    int id = PhotonNetwork.AllocateViewID(true);

                    viewIDs.Add("playerViewID" + player.photonView.OwnerActorNr, id);
                }

                PhotonNetwork.CurrentRoom.SetCustomProperties(viewIDs);
            }

            foreach (Player player in PhotonNetwork.PlayerList)
            {
                Vector3 randomOffset = new Vector3(1, 0, 1) * Random.Range(-1, 1) * 2;
                PlayerController photonPlayer = Instantiate(_playerPrefab, spawnPosition.position + randomOffset, _playerPrefab.transform.rotation).GetComponent<PlayerController>();
                photonPlayer.gameObject.name = "Player " + player.ActorNumber;
                photonPlayer.photonView.OwnerActorNr = player.ActorNumber;
                photonPlayer.photonView.ControllerActorNr = player.ActorNumber;
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

            yield return null;
        }

        private IEnumerator SpawnSafeZones(List<Transform> safeZonePositions)
        {
            LoadingLog("Spawning " + safeZonePositions.Count + " safe zones");
            foreach (Transform safePosition in safeZonePositions)
            {
                Instantiate(_safeZonePrefab, safePosition.position, safePosition.rotation);
            }

            yield return null;
        }

    }
}
