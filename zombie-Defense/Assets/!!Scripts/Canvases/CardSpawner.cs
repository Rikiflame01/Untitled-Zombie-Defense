using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System;

public class CardSpawner : MonoBehaviour
{
    [Header("Empty Card Data")]
    public CardData[] emptyCardData;

    [Header("Upgrade Card Data")]
    public CardData[] standardUpgradeCardData;
    public CardData[] utilityUpgradeCardData;
    public CardData[] rareUpgradeCardData;

    [Header("Parent Object")]
    public Transform cardParent;

    [Header("Spawn Settings")]
    public int totalCards = 3;
    public float spacing = 150f;

    [Header("Upgrade Rarity Chances")]
    [Tooltip("Chance for a Standard upgrade card (0-1)")]
    public float standardChance = 0.4f;
    [Tooltip("Chance for a Utility upgrade card (0-1)")]
    public float utilityChance = 0.4f;
    [Tooltip("Chance for a Rare upgrade card (0-1)")]
    public float rareChance = 0.2f;

    [Header("Animation Settings")]
    public float slideInDuration = 0.5f;
    public float slideInDelayBetweenCards = 0.1f;
    public float slideInOffset = 300f;
    public float slideOutDuration = 0.5f;

    private GameObject chosenCard = null;

    void OnEnable()
    {
        ActionManager.OnChooseCard += SpawnCards;
        ActionManager.OnChooseCardEnd += RemoveCards;
    }

    void OnDisable()
    {
        ActionManager.OnChooseCard -= SpawnCards;
        ActionManager.OnChooseCardEnd -= RemoveCards;
    }

    [ContextMenu("Spawn Cards")]
    public void SpawnCards()
    {
        foreach (Transform child in cardParent)
        {
            Destroy(child.gameObject);
        }
        chosenCard = null;

        int upgradeSlotIndex = UnityEngine.Random.Range(0, totalCards);
        Vector2 startPos = Vector2.zero;
        
        for (int i = 0; i < totalCards; i++)
        {
            GameObject selectedPrefab = null;
            CardData chosenCardData = null;
            
            if (i == upgradeSlotIndex)
            {
                float roll = UnityEngine.Random.value;
                if (roll < standardChance)
                {
                    chosenCardData = GetFilteredCardData(standardUpgradeCardData);
                }
                else if (roll < standardChance + utilityChance)
                {
                    chosenCardData = GetFilteredCardData(utilityUpgradeCardData);
                    if (chosenCardData == null)
                    {
                        chosenCardData = GetFilteredCardData(standardUpgradeCardData);
                    }
                }
                else
                {
                    chosenCardData = GetFilteredCardData(rareUpgradeCardData);
                }
                
                if (chosenCardData == null)
                {
                    chosenCardData = GetRandomCardData(emptyCardData);
                }
            }
            else
            {
                float roll = UnityEngine.Random.value;
                if (roll < standardChance)
                {
                    chosenCardData = GetFilteredCardData(standardUpgradeCardData);
                }
                else if (roll < standardChance + utilityChance)
                {
                    chosenCardData = GetFilteredCardData(utilityUpgradeCardData);
                }
                else
                {
                    chosenCardData = GetFilteredCardData(rareUpgradeCardData);
                }
                
                if (chosenCardData == null)
                {
                    chosenCardData = GetRandomCardData(emptyCardData);
                }
            }

            if (chosenCardData != null)
            {
                selectedPrefab = chosenCardData.prefab;
            }

            if (selectedPrefab != null)
            {
                GameObject newCard = Instantiate(selectedPrefab, cardParent);
                CardInstance instance = newCard.AddComponent<CardInstance>();
                instance.cardData = chosenCardData;
                
                Vector2 targetPosUI = startPos + new Vector2(i * spacing, 0);
                Vector3 targetPosWorld = new Vector3(i * spacing, 0, 0);

                RectTransform rt = newCard.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = targetPosUI + new Vector2(-slideInOffset, 0);
                    rt.DOAnchorPos(targetPosUI, slideInDuration)
                      .SetEase(Ease.OutQuad)
                      .SetDelay(i * slideInDelayBetweenCards);
                }
                else
                {
                    newCard.transform.position = targetPosWorld + new Vector3(-slideInOffset, 0, 0);
                    newCard.transform.DOMove(targetPosWorld, slideInDuration)
                        .SetEase(Ease.OutQuad)
                        .SetDelay(i * slideInDelayBetweenCards);
                }
            }
        }
    }

    public void OnCardSelected(GameObject selectedCard)
    {
        chosenCard = selectedCard;
        CardInstance instance = selectedCard.GetComponent<CardInstance>();
        if (instance != null && instance.cardData != null)
        {
            instance.cardData.currentSpawns++;
        }
        RemoveCards();
    }

    [ContextMenu("Remove Cards")]
    public void RemoveCards()
    {
        List<Transform> cardsToRemove = new List<Transform>();
        foreach (Transform child in cardParent)
        {
            if (child.gameObject != chosenCard)
            {
                cardsToRemove.Add(child);
            }
        }

        foreach (Transform card in cardsToRemove)
        {
            RectTransform rt = card.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.DOAnchorPos(rt.anchoredPosition + new Vector2(-slideInOffset, 0), slideOutDuration)
                  .SetEase(Ease.InQuad)
                  .OnComplete(() => Destroy(card.gameObject));
            }
            else
            {
                card.transform.DOMove(card.transform.position + new Vector3(-slideInOffset, 0, 0), slideOutDuration)
                  .SetEase(Ease.InQuad)
                  .OnComplete(() => Destroy(card.gameObject));
            }
        }
    }

    private CardData GetFilteredCardData(CardData[] cardDataArray)
    {
        if (cardDataArray == null || cardDataArray.Length == 0)
        {
            Debug.LogWarning("Card data array is empty!");
            return null;
        }
        List<CardData> availableCards = new List<CardData>();
        foreach (CardData data in cardDataArray)
        {
            if (data != null && data.prefab != null)
            {
                if (data.maxSpawns == CardData.INFINITE_SPAWNS || data.currentSpawns < data.maxSpawns)
                {
                    Card card = data.prefab.GetComponent<Card>();
                    bool valid = true;
                    if (card != null)
                    {
                        switch (card.specificCard)
                        {
                            case SpecificCard.HealPlayer:
                                valid = IsPlayerHealingCardValid();
                                break;
                            case SpecificCard.RepairWalls:
                                valid = IsRepairWallsCardValid();
                                break;
                            case SpecificCard.FortifyWalls:
                                valid = IsFortifyWallsCardValid();
                                break;
                            default:
                                valid = true;
                                break;
                        }
                    }
                    if (valid)
                        availableCards.Add(data);
                }
            }
        }
        if (availableCards.Count == 0)
            return null;
        int index = UnityEngine.Random.Range(0, availableCards.Count);
        return availableCards[index];
    }

    private CardData GetRandomCardData(CardData[] cardDataArray)
    {
        if (cardDataArray == null || cardDataArray.Length == 0)
        {
            Debug.LogWarning("Card data array is empty!");
            return null;
        }

        List<CardData> availableCards = new List<CardData>();
        foreach (CardData data in cardDataArray)
        {
            if (data != null && data.prefab != null)
            {
                if (data.maxSpawns == CardData.INFINITE_SPAWNS || data.currentSpawns < data.maxSpawns)
                {
                    availableCards.Add(data);
                }
            }
        }

        if (availableCards.Count == 0)
            return null;

        int index = UnityEngine.Random.Range(0, availableCards.Count);
        return availableCards[index];
    }

    private bool IsPlayerHealingCardValid()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                return playerHealth.CurrentHealth < 70;
            }
        }
        return false;
    }

    private bool IsRepairWallsCardValid()
    {
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        if (walls.Length == 0)
            return false;

        int damagedCount = 0;
        foreach (GameObject wall in walls)
        {
            Health wallHealth = wall.GetComponent<Health>();
            if (wallHealth != null)
            {
                if (wallHealth.CurrentHealth < wallHealth.MaxHealth * 0.5f)
                    damagedCount++;
            }
        }
        float ratio = (float)damagedCount / walls.Length;
        return ratio > 0.4f;
    }

    private bool IsFortifyWallsCardValid()
    {
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        return walls.Length > 0;
    }
}
