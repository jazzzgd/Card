namespace Cards
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class PlayerBattleField : MonoBehaviour
    {
        [SerializeField]
        private Transform _parent;
        [SerializeField]
        private Hero _opponentHero;
        [SerializeField]
        private PlayerBattleField _opponentBattleField;
        [SerializeField]
        private GameManager _gameManager;

        public List<Card> Cards = new List<Card>();

        public void AddCardFromHand(Card card)
        {
            Cards.Add(card);
            card.PassedToBattle();
            card.OnAttack += OnAttackByCard;
            card.OnDestroyCard += OnDestroyCard;
            UpdatePositions();
            ApplySkill(card);
        }

        private void UpdatePositions()
        {
            var startX = _parent.position.x;
            var index = 0;
            foreach (var card in Cards)
            {
                var xPosition = startX - 4f * index + 2 * (Cards.Count - 1);
                card.transform.position = new Vector3(xPosition, card.transform.position.y, _parent.position.z);
                index++;
            }
        }

        private void ApplySkill(Card card)
        {
            var skill = card.GetSkill()
                .Replace("<b>", "")
                .Replace("</b>", "")
                .Replace("\n", "")
                .Replace("  ", "");
            var opponentCards = _opponentBattleField.Cards;

            switch (skill)
            {
                case "Battlecry: Deal 1 damage":
                    if (opponentCards.Count > 0)
                        opponentCards[0].AddDamage(1);
                    break;
                case "Battlecry: Restore 2 Health":
                    if (Cards.Count > 1)
                        Cards.FirstOrDefault(c => !c.Equals(card))?.Heal(2);
                    break;
                case "Battlecry: Restore 2 Health to all friendly characters":
                    if (Cards.Count > 1)
                    {
                        var team = Cards.Where(c => !c.Equals(card));
                        foreach (var teammate in team)
                            teammate.Heal(2);
                    }
                    break;
                case "Battlecry: Deal 3 damage to the enemy hero":
                    _opponentHero.AddDamage(2);
                    break;
                case "Battlecry: Deal 2 damage":
                    if (opponentCards.Count > 0)
                        opponentCards[0].AddDamage(2);
                    break;
                case "Charge":
                    if (opponentCards.Count > 0)
                        opponentCards[0].AddDamage(card.GetAttack());
                    break;
                default:
                    Debug.Log("Unknown skill: " + skill);
                    return;
            }
            Debug.Log("Skill applied: " + skill);
        }

        private void OnAttackByCard(AttackData data)
        {
            var opponentSkills = _opponentBattleField.Cards.Select(c => c.GetSkill());
            var opponentHasTaunt = opponentSkills.Any(skill => skill.Contains("Taunt"));
            var targetCardHasTaunt = data.TargetCard?.GetSkill()?.Contains("Taunt") ?? false;

            if (opponentHasTaunt && !targetCardHasTaunt)
                Debug.Log("Attack failed: opponent has card with Taunt skill");
            else
            {
                var damage = data.Dealer.GetAttack();
                if (data.TargetCard != null)
                    data.TargetCard.AddDamage(damage);
                else if (data.TargetHero != null)
                    data.TargetHero.AddDamage(damage);

                StartCoroutine(EndTurnAfterDelay());
            }
        }

        private IEnumerator EndTurnAfterDelay()
        {
            yield return new WaitForSeconds(0.5f);
            _gameManager.EndTurn();
        }

        private void OnDestroyCard(Card card)
        {
            Cards.Remove(card);
            UpdatePositions();
        }
    }
}