using UnityEngine;
using UnityEngine.UI;

public enum CardType { Empty, Upgrade }
public enum CardRarity { None, Standard, Utility, Rare }

public class Card : MonoBehaviour
{
    public CardType cardType;
    public CardRarity rarity;
}
