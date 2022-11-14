namespace Cards {
    public class AttackData {
        public readonly Card Dealer;
        public readonly Card TargetCard;
        public readonly Hero TargetHero;

        public AttackData(Card dealer, Card targetCard, Hero targetHero)
        {
            Dealer = dealer;
            TargetCard = targetCard;
            TargetHero = targetHero;
        }
    }
}