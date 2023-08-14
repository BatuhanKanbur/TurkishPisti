using System;
using UnityEngine;

namespace Objects
{
    public class Card : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        private int _cardId;
        public event Action<int> OnObjectClickEvent;
        public void SetCard(Sprite targetSprite,int order,int newCardId)
        {
            spriteRenderer.sprite = targetSprite;
            spriteRenderer.sortingOrder = order;
            _cardId = newCardId;
        }

        private void OnMouseDown()
        {
            OnObjectClickEvent?.Invoke(_cardId);
        }

        private void OnDisable()
        {
            spriteRenderer.sortingOrder = 0;
            StopAllCoroutines();
        }
    }
}
