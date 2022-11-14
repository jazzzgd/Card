namespace Cards
{
    public enum CardUnitType : byte
    {
        None = 0,
        Murloc = 1,
        Beast = 2,
        Elemental = 3,
        Mech = 4
    }

    public enum HeroClass : byte
    {
        Hunter = 0,
        Mage = 1,
        Warrior = 2,
        Priest = 3
    }

    public enum CardState : byte
    {
        Deck = 0,
        Hand = 1,
        Battle = 2,
        Beaten = 3
    }

    public enum Player : byte { One, Two }
}
