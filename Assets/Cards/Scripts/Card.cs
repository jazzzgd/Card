
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cards
{
    public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        [SerializeField]
        private MeshRenderer _avatar;
        [SerializeField]
        private TextMeshPro _name;
        [SerializeField]
        private TextMeshPro _cost;
        [SerializeField]
        private TextMeshPro _attack;
        [SerializeField]
        private TextMeshPro _hp;
        [SerializeField]
        private TextMeshPro _type;
        [SerializeField]
        private TextMeshPro _description;
        [SerializeField]
        private GameObject _attackIndicator;
        [SerializeField]
        private LineRenderer _attackLineRenderer;

        public Player Player { get; private set; }
        private int _currentHp;
        private CardPropertiesData _data;
        public CardState State { get; private set; } = CardState.Deck;
        private bool _interactable = false;

        public Action<Card> OnClick;
        public Action<AttackData> OnAttack;
        public Action<Card> OnDestroyCard;

        public void PseudoConstructor(CardPropertiesData data, Player player)
        {
            _data = data;
            var shader = Shader.Find("TextMeshPro/Sprite");
            var material = new Material(shader);
            material.renderQueue = 2995;
            material.mainTexture = data.Texture;
            _avatar.material = material;
            _name.text = data.Name;
            _cost.text = data.Cost.ToString();
            _attack.text = data.Attack.ToString();
            _hp.text = data.Health.ToString();
            _type.text = data.Type.ToString();
            _description.text = CardUtility.GetDescriptionById(data.Id);
            _currentHp = data.Health;
            Player = player;
        }

        public void SetInteractable(bool value) => _interactable = value;

        public void PassedToHand() => State = CardState.Hand;

        public void PassedToBattle() => State = CardState.Battle;

        public string GetSkill() => CardUtility.GetDescriptionById(_data.Id);

        public int GetAttack() => _data.Attack;

        public void Heal(int hp)
        {
            _currentHp += hp;
            _hp.text = _currentHp.ToString();
        }

        public void AddDamage(int damage)
        {
            if (State == CardState.Battle)
            {
                _currentHp -= damage;
                _hp.text = _currentHp.ToString();
                Debug.Log("Damage to " + _data.Name + " : " + damage);
                StartCoroutine(AnimateDamage());
                if (_currentHp <= 0)
                {
                    State = CardState.Beaten;
                    Debug.Log(_data.Name + " killed");
                }
            }
        }

        private IEnumerator AnimateDamage()
        {
            var durationSec = 0.2f;
            var upScale = new Vector3(1.1f, 1.1f, 1.1f);
            var initialScale = transform.localScale;

            for (float time = 0; time < durationSec * 2; time += Time.deltaTime)
            {
                float progress = Mathf.PingPong(time, durationSec) / durationSec;
                transform.localScale = Vector3.Lerp(initialScale, upScale, progress);
                yield return null;
            }
            transform.localScale = initialScale;
            if (_currentHp <= 0)
            {
                Destroy(gameObject);
                OnDestroyCard?.Invoke(this);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_interactable && State == CardState.Hand)
                transform.localScale *= 1.15f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_interactable && State == CardState.Hand)
                transform.localScale /= 1.15f;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_interactable && State == CardState.Hand)
            {
                transform.localScale /= 1.15f;
                OnClick?.Invoke(this);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_interactable && State == CardState.Battle)
            {
                _attackIndicator.SetActive(true);
                _attackLineRenderer.gameObject.SetActive(true);
                var line = new Vector3[] {
                    _attackIndicator.transform.position,
                    eventData.pointerCurrentRaycast.worldPosition
                };
                _attackLineRenderer.SetPositions(line);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_interactable && State == CardState.Battle)
            {
                _attackLineRenderer.SetPosition(1, eventData.pointerCurrentRaycast.worldPosition);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_interactable && State == CardState.Battle)
            {
                _attackIndicator.SetActive(false);
                _attackLineRenderer.gameObject.SetActive(false);

                var raycastedObject = eventData.pointerCurrentRaycast.gameObject;

                if (raycastedObject == gameObject)
                    return;

                if (raycastedObject.name.Contains("CardPrefab"))
                {
                    var raycastedCard = raycastedObject.GetComponent<Card>();
                    if (raycastedCard.Player != Player &&
                        raycastedCard.State == CardState.Battle)
                    {
                        // Нанесение урона вражеской карте
                        var attackData = new AttackData(this, raycastedCard, null);
                        OnAttack?.Invoke(attackData);
                    }
                }
                else if ((Player == Player.One && raycastedObject.name.Contains("Hero2")) ||
                    (Player == Player.Two && raycastedObject.name.Contains("Hero1")))
                {
                    // Нанесение урона вражескому герою
                    var hero = raycastedObject.GetComponent<Hero>();
                    var attackData = new AttackData(this, null, hero);
                    OnAttack?.Invoke(attackData);
                }
            }
        }
    }
}