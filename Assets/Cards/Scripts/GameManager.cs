using System.Collections.Generic;
using System.Linq;
using Cards.ScriptableObjects;
using UnityEngine;

namespace Cards
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField, Range(10, 30), Space]
        private int _maxDeckSize = 20;

        [SerializeField, Header("Наборы карт, из которых формируются колоды игроков"), Space]
        private CardPackConfiguration[] _player1Packs;
        [SerializeField]
        private CardPackConfiguration[] _player2Packs;

        [SerializeField, Space]
        private Transform _player1DeckParent;
        [SerializeField]
        private Transform _player2DeckParent;
        [SerializeField]
        private PlayerHand _player1Hand;
        [SerializeField]
        private PlayerHand _player2Hand;
        [SerializeField]
        private GameObject _cardPrefab;
        [SerializeField]
        private Hero _player1Hero;
        [SerializeField]
        private Hero _player2Hero;
        [SerializeField, Space]
        private CameraController _cameraController;

        private Queue<Card> _player1DeckCards;
        private Queue<Card> _player2DeckCards;
        private Player _activePlayer;

        private void Awake()
        {
            CreatePlayerDecks();
            PopulateHands();
            _player1Hand.SetActivePlayer(true);
            _activePlayer = Player.One;
        }

        public void EndTurn()
        {
            if (_activePlayer == Player.One)
            {
                _player1Hand.SetActivePlayer(false);
                _player2Hand.SetActivePlayer(true);
                _player1Hero.IncrementMana();
                _activePlayer = Player.Two;
                if (_player2DeckCards.Count > 0)
                    _player2Hand.AddCardFromDeck(_player2DeckCards.Dequeue());
                else
                    _player2Hero.AddDamage(8 - _player2Hand.Cards.Length);
            }
            else if (_activePlayer == Player.Two)
            {
                _player1Hand.SetActivePlayer(true);
                _player2Hand.SetActivePlayer(false);
                _player2Hero.IncrementMana();
                _activePlayer = Player.One;
                if (_player1DeckCards.Count > 0)
                    _player1Hand.AddCardFromDeck(_player1DeckCards.Dequeue());
                else
                    _player1Hero.AddDamage(8 - _player2Hand.Cards.Length);
            }
            _cameraController.RotateAroundY180();
        }

        private void CreatePlayerDecks()
        {
            var random = new System.Random();

            Queue<Card> CreatePlayerDeck(CardPackConfiguration[] packs, Transform deckParent, Player player)
            {
                IEnumerable<CardPropertiesData> cardData = new List<CardPropertiesData>();
                foreach (var pack in packs) cardData = pack.UnionProperties(cardData);
                var cardDataArray = cardData.ToArray();
                random.Shuffle(cardDataArray);

                var deckSize = Mathf.Min(_maxDeckSize, cardDataArray.Length);
                var cards = new Card[deckSize];
                var position = Vector3.zero;
                var index = 0;

                foreach (var data in cardData)
                {
                    position += new Vector3(0, 0.5f, 0);
                    var cardGameObject = Instantiate(_cardPrefab, deckParent);
                    cardGameObject.transform.localPosition = position;
                    cardGameObject.transform.localScale = Vector3.one;
                    cardGameObject.transform.localRotation = Quaternion.Euler(0, 180, 0);
                    var cardController = cardGameObject.GetComponent<Card>();
                    cardController.PseudoConstructor(data, player);
                    cards[index] = cardController;
                    index++;
                    if (index >= deckSize)
                        break;
                }
                return new Queue<Card>(cards.Reverse());
            }

            _player1DeckCards = CreatePlayerDeck(_player1Packs, _player1DeckParent, Player.One);
            _player2DeckCards = CreatePlayerDeck(_player2Packs, _player2DeckParent, Player.Two);
        }

        private void PopulateHands()
        {
            _player1Hand.AddCardsFromDeck(_player1DeckCards.Take(8));
            _player2Hand.AddCardsFromDeck(_player2DeckCards.Take(8));
            for (int i = 0; i < 8; i++)
            {
                _player1DeckCards.Dequeue();
                _player2DeckCards.Dequeue();
            }
        }
    }
}