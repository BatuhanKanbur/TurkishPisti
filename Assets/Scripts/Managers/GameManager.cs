using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Objects;
using Structures;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Managers
{
    public class GameManager : Singleton<GameManager>
    {
        private GameState _currentGameState = GameState.Menu;
        private GameState CurrentGameState
        {
            get => _currentGameState;
            set
            {
                if (_currentGameState != value)
                {
                    menuScene.SetActive(value==GameState.Menu);
                    gameScene.SetActive(value==GameState.Game);
                    finishScene.SetActive(value==GameState.Finish);
                }
                _currentGameState = value;
            }
        }

        #region Menu
        [SerializeField] private InputField playerNameInput;
        [SerializeField] private Text playerCountText;
        [SerializeField] private Player mainPlayer;
        [SerializeField] private GameObject menuScene,gameScene,finishScene;
        [SerializeField] private GameObject[] playersInfo;
        private int _playerCount = 2;

        public void StartGameButton()
        {
            if (playerNameInput.text == String.Empty)
                playerNameInput.text = Constants.AINames[Random.Range(0, Constants.AINames.Length)];
            mainPlayer.Init(playerNameInput.text);
            StartGame();
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(Constants.DemoSceneName);
        }

        private void FinishGame()
        {
            List<Tuple<IPlayer,int, int>> playerScores = new List<Tuple<IPlayer,int, int>>();
            foreach (var player in _playerDictionary)
                playerScores.Add(new Tuple<IPlayer, int, int>(player.Key.GetScore().Item1,player.Key.GetScore().Item2,player.Key.GetScore().Item3));
            playerScores.Sort((a, b) => b.Item3.CompareTo(a.Item3));
            playerScores[0] = new Tuple<IPlayer, int, int>(playerScores[0].Item1, playerScores[0].Item2 + 3,playerScores[0].Item3);
            for (int i = 0; i < playerScores.Count; i++)
            {
                playersInfo[i].SetActive(true);
                playersInfo[i].transform.GetChild(1).GetComponent<Text>().text = playerScores[i].Item1.GetPlayerName();
                playersInfo[i].transform.GetChild(2).GetComponent<Text>().text = playerScores[i].Item2.ToString();
            }

            CurrentGameState = GameState.Finish;
        }
        public void PlayerCountChange(int val)
        {
            _playerCount += val;
            if (_playerCount < 2)
                _playerCount = 4;
            if (_playerCount > 4)
                _playerCount = 2;
            for (int i = 0; i < players.Length; i++)
            {
                players[i].gameObject.SetActive(i<_playerCount);
            }
            playerCountText.text = _playerCount.ToString();
        }
        #endregion
        #region GamePlayFields
        private readonly List<int> _deckArray = new List<int>();
        private readonly Dictionary<int,Card> _stackArray = new Dictionary<int, Card>();
        private int _playerTurn;
        public IPlayer[] players;
        [SerializeField] private Text deckCountText;
        [SerializeField] private Transform tableDeck, tableStack;
        private readonly Dictionary<IPlayer, int> _playerDictionary = new Dictionary<IPlayer, int>();
        private int PlayerTurn
        {
            get => _playerTurn;
            set
            {
                if (value > _playerDictionary.Count - 1)
                    PlayerTurn = 0;
                if (_playerTurn != value)
                    PlayerTurnChanged(value);
                _playerTurn = value;
            }
        }
        #endregion

        private GameAssetManager _gameAssetManager;
        private PoolManager _poolManager;


        private void Start()
        {
            _gameAssetManager = GameAssetManager.Instance;
            _poolManager = PoolManager.Instance;
            InitGame();
        }

        private async void InitGame()
        {
            await _gameAssetManager.LoadAssets();
            await _poolManager.CreatePool(_gameAssetManager.GetCardPrefab(), PoolType.Card, 20);
        }

        public void StartGame()
        {
            _deckArray.CreateDeck();
            for (int i = 0; i < _playerCount; i++)
            {
                players[i].OnPlayCard += Play;
                players[i].OnHasNoCardsLeft += NoCardsLeft;
                _playerDictionary.Add(players[i],i);
            }

            int[] newStackArray = _deckArray.DistributeDeck();
            for (int i = 0; i < newStackArray.Length; i++)
            {
                Sprite cardSprite = i < newStackArray.Length - 1
                    ? _gameAssetManager.GetCardBackFace()
                    : _gameAssetManager.GetCard(newStackArray[i]);
                Card newCard = _poolManager.GetObjectFromPool(PoolType.Card).GetComponent<Card>();
                newCard.SetCard(cardSprite,_stackArray.Count,newStackArray[i]);
                newCard.transform.SetPositionAndRotation(tableDeck.position,tableDeck.rotation);
                newCard.transform.SetPositionAndRotation(tableStack,new Vector3(0,0,Random.Range(-30,30)),0.25f);
                _stackArray.Add(newStackArray[i],newCard);
            }

            foreach (var player in _playerDictionary)
            {
                player.Key.SetTurn(player.Value == PlayerTurn);
                DistributeDeckToPlayer(player.Key);
            }
            
            CurrentGameState = GameState.Game;
        }
        private void PlayerTurnChanged(int turnId)
        {
            foreach (var playerDict in _playerDictionary)
            {
                playerDict.Key.SetTurn(playerDict.Value == turnId);
            }
        }
        public int GetStackLastCard() => _stackArray.Count==0 ? -1 : _stackArray.Keys.Last();
        private void DistributeDeckToPlayer(IPlayer targetPlayer)
        {
            if (_deckArray.Count < 1)
            {
                FinishGame();
                return;
            }
            int[] newDeck = _deckArray.DistributeDeck();
            List<Card> newCardList = new List<Card>();
            foreach (var cardId in newDeck)
            {
                Sprite cardSprite = targetPlayer.isMainPlayer == false
                    ? _gameAssetManager.GetCardBackFace()
                    : _gameAssetManager.GetCard(cardId);
                Card newCard = _poolManager.GetObjectFromPool(PoolType.Card).GetComponent<Card>();
                newCard.transform.SetPositionAndRotation(tableDeck.position, tableDeck.rotation);
                newCard.SetCard(cardSprite,0,cardId);
                newCardList.Add(newCard);
            }
            targetPlayer.SetDecks(newDeck,newCardList.ToArray());
            deckCountText.text = _deckArray.Count.ToString();
        }
        private void Play(IPlayer player, int cardId)
        {
            if (PlayerTurn == _playerDictionary[player])
            {
                Card newCard = _poolManager.GetObjectFromPool(PoolType.Card).GetComponent<Card>();
                newCard.SetCard(_gameAssetManager.GetCard(cardId),_stackArray.Count,cardId);
                newCard.transform.SetPositionAndRotation(player.deckParent.transform.position,player.deckParent.transform.rotation);
                newCard.transform.SetPositionAndRotation(tableStack,new Vector3(0,0,Random.Range(-30,30)),0.5f,true,OnCardGoTarget);
                void OnCardGoTarget()
                {
                    if (CardIsMatch(cardId, out var takeType))
                    {
                        switch (takeType)
                        {
                            case TakeType.Normal:
                                player.AddScore(Constants.NormalScore);
                                break;
                            case TakeType.Club2:
                                player.AddScore(Constants.Club2Score);
                                break;
                            case TakeType.Diamond10:
                                player.AddScore(Constants.Diamond10Score);
                                break;
                            case TakeType.AsJoker:
                                player.AddScore(Constants.AsJokerScore);
                                break;
                            case TakeType.Joker:
                                player.AddScore(Constants.JokerScore);
                                break;
                        }
                       _stackArray.Add(cardId,newCard);
                        player.AddCollectScore(_stackArray.Count);
                        foreach (var stack in _stackArray)
                        {
                            GameObject cardObject = stack.Value.gameObject;
                            Vector3 deckTransform = player.deckParent.transform.position;
                            cardObject.transform.Move(deckTransform,0.5f,true, ClearStack);
                        }
                        Debug.Log($"{player.GetPlayerName()} took the cards on the table via {takeType.ToString()}");
                    }
                    else
                    {
                        _stackArray.Add(cardId,newCard);
                    }
                }
                
                if (PlayerTurn + 1 > _playerDictionary.Count - 1)
                    PlayerTurn = 0;
                else
                    PlayerTurn++;
            }
        }
        private void ClearStack()
        {
            foreach (var stack in _stackArray)
            {
                stack.Value.gameObject.SetActive(false);
            }
            _stackArray.Clear();
        }
        private bool CardIsMatch(int cardId,out TakeType takeType)
        {
            if (GetStackLastCard() < 0)
            {
                takeType = TakeType.Normal;
                return false;
            }

            int stackCardId = GetStackLastCard() % 13;
            int playerCardId = cardId % 13;
            if (stackCardId == playerCardId)
            {
                if (stackCardId == Constants.JokerId && playerCardId == Constants.JokerId)
                {
                    takeType = TakeType.Joker;
                }
                else if(cardId==Constants.Diamond10Id)
                {
                    takeType = TakeType.Diamond10;
                }
                else if(cardId==Constants.Club2Id)
                {
                    takeType = TakeType.Club2;
                }
                else
                {
                    takeType = TakeType.Normal;
                }
                return true;
            }

            if(playerCardId == Constants.JokerId | playerCardId == Constants.AsId)
            {
                takeType = TakeType.AsJoker;
                return true;
            }

            takeType = TakeType.Normal;
            return false;
        }
        private void NoCardsLeft(IPlayer player) =>  DistributeDeckToPlayer(player);
        private enum TakeType
        {
            Normal,
            Club2,
            Diamond10,
            AsJoker,
            Joker
        }
        private enum GameState
        {
            Menu,
            Game,
            Finish
        }
    }
}
