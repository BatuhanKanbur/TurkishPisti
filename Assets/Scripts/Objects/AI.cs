using System;
using System.Linq;
using Managers;
using Structures;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Objects
{
    public class AI : IPlayer
    {
        private float _aiPlayTime;
        private void Start()
        {
            InitPlayer(Constants.AINames[Random.Range(0, Constants.AINames.Length)]);
        }

        private void Update()
        {
            if(!IsReady) return;
            _aiPlayTime += Time.deltaTime;
            if (_aiPlayTime > Constants.AIPlayTime)
            {
                Play();
                _aiPlayTime = 0;
            }
        }

        private void Play()
        {
            int targetCardId = GameManager.Instance.GetStackLastCard();
            foreach (var playerCard in PlayerDeck)
            {
                if (playerCard.Key == targetCardId)
                {
                    PlayCard(playerCard.Key);
                    return;
                }
            }
            PlayCard(PlayerDeck.Keys.Last());
        }
    }
}
