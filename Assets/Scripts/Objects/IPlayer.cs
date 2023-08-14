using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Structures;
using TMPro;
using UnityEngine;

namespace Objects
{
    public abstract class IPlayer : MonoBehaviour
    {
        public event Action<IPlayer,int> OnPlayCard;
        public event Action<IPlayer> OnHasNoCardsLeft;
        protected readonly Dictionary<int,Card> PlayerDeck = new Dictionary<int, Card>();
        private int _playerScore;
        private int _totalCollectedCards;
        public bool isMainPlayer;
        protected bool IsReady;
        public AutoLayout deckParent;
        [SerializeField] private TextMeshPro playerNameText,playerScoreText;
        protected void InitPlayer(string newPlayerName)
        {
            playerNameText.text = newPlayerName;
        }

        public string GetPlayerName() => playerNameText.text;

        public void SetTurn(bool isReady)
        {
            IsReady = isReady;
        }

        public void SetDecks(int[] newDeck,Card[] newCards)
        {
            for (int i = 0; i < newDeck.Length; i++)
            {
                PlayerDeck.Add(newDeck[i],newCards[i]);
                newCards[i].transform.SetParent(deckParent.transform);
                if(!isMainPlayer) continue;
                newCards[i].OnObjectClickEvent += PlayCard;
            }
            newDeck.DebugPlayerDeck(GetPlayerName());
            deckParent.AlignChildren();
        }

        public void AddScore(int score)
        {
            _playerScore += score;
            playerScoreText.text = _playerScore.ToString();
        }

        public void AddCollectScore(int cardCount)
        {
            _totalCollectedCards += cardCount;
        }

        public Tuple<IPlayer,int, int> GetScore() => new Tuple<IPlayer,int, int>(this,_playerScore, _totalCollectedCards);

        protected void PlayCard(int cardId)
        {
            if (IsReady)
            {
                OnPlayCard?.Invoke(this, cardId);
                PlayerDeck[cardId].OnObjectClickEvent -= PlayCard;
                PlayerDeck[cardId].gameObject.SetActive(false);
                PlayerDeck[cardId].transform.parent = null;
                PlayerDeck.Remove(cardId);
                deckParent.AlignChildren();
                Debug.Log(GetPlayerName() + $" has been played the {cardId}");
                if(PlayerDeck.Count==0)
                    NoCardsLeft();
            }
            else
            {
                Debug.Log(GetPlayerName() + $" tried to play when it was not their turn");
            }
        }

        private void NoCardsLeft()
        {
            Debug.Log(GetPlayerName() + " the cards are out!");
            OnHasNoCardsLeft?.Invoke(this);
        }
    }
}