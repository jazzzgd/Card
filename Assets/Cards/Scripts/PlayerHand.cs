namespace Cards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class PlayerHand : MonoBehaviour
    {
        [SerializeField]
        private Transform[] _cardParents;
        [SerializeField]
        private PlayerBattleField _battleField;
        [SerializeField]
        private GameManager _gameManager;

        public Card[] Cards = new Card[8];

        private bool _activePlayer = false;
        private bool _cardPlayed = false;

        public void SetActivePlayer(bool value)
        {
            _activePlayer = value;
            foreach (var card in Cards)
                card?.SetInteractable(value);
            if (value)
                _cardPlayed = false;
        }

        public void AddCardsFromDeck(IEnumerable<Card> cards)
            => StartCoroutine(AddCardsFromDeckRoutine(cards));

        public void AddCardFromDeck(Card card)
            => StartCoroutine(AddCardsFromDeckRoutine(new List<Card> { card }));

        private IEnumerator AddCardsFromDeckRoutine(IEnumerable<Card> cards)
        {
            var newCards = new Queue<Card>(cards);
            for (int i = 0; i < Cards.Length; i++)
            {
                if (Cards[i] == null)
                {
                    var card = newCards.Dequeue();
                    Cards[i] = card;
                    card.OnClick += OnCardClick;
                    card.PassedToHand();
                    yield return new WaitForSeconds(0.1f);
                    StartCoroutine(MoveCardToHand(card, _cardParents[i]));
                }
            }
        }

        private IEnumerator MoveCardToHand(Card card, Transform target)
        {
            var time = 0f;
            var startPos = card.transform.position;
            var endPos = target.position;

            while (time < 1f)
            {
                card.transform.position = Vector3.Lerp(startPos, endPos, time);
                time += Time.deltaTime;
                yield return null;
            }
            card.transform.Rotate(0, 0, 180);
            card.SetInteractable(_activePlayer);
        }

        private void OnCardClick(Card card)
        {
            if (!_cardPlayed)
            {
                _battleField.AddCardFromHand(card);
                var index = Array.IndexOf(Cards, card);
                Cards[index] = null;
                foreach (var c in Cards)
                    c?.SetInteractable(false);
                card.OnClick -= OnCardClick;
                _cardPlayed = true;
            }
        }
    }
}