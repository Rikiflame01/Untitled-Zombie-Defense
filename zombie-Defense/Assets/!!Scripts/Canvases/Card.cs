using UnityEngine;
using UnityEngine.UI;

public enum CardType { Empty, Upgrade }
public enum CardRarity { None, Standard, Utility, Rare }
public enum SpecificCard
{
    Empty,
    FortifyWalls,
    HealPlayer,
    IncreaseClipSize,
    IncreaseDamage,
    Piercing,
    RepairWalls,
    Rifle
}

public class Card : MonoBehaviour
{
    public CardType cardType;
    public CardRarity rarity;
    public SpecificCard specificCard;
}
