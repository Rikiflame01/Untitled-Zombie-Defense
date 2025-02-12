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

    void OnEnable()
    {
        ActionManager.OnChooseCard += SpawnCards;
        ActionManager.OnChooseCardEnd +=RemoveCards;
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

        int upgradeSlotIndex = UnityEngine.Random.Range(0, totalCards);
        Vector2 startPos = Vector2.zero;
        
        for (int i = 0; i < totalCards; i++)
        {
            GameObject selectedPrefab = null;

            if (i == upgradeSlotIndex)
            {
                float roll = UnityEngine.Random.value;
                CardData chosenCardData = null;

                if (roll < standardChance)
                {
                    chosenCardData = GetRandomCardData(standardUpgradeCardData);
                }
                else if (roll < standardChance + utilityChance)
                {
                    chosenCardData = GetRandomCardData(utilityUpgradeCardData);
                }
                else
                {
                    chosenCardData = GetRandomCardData(rareUpgradeCardData);
                }

                if (chosenCardData == null)
                {
                    chosenCardData = GetRandomCardData(utilityUpgradeCardData);
                }

                if (chosenCardData != null)
                {
                    selectedPrefab = chosenCardData.prefab;
                    chosenCardData.currentSpawns++;
                }
            }
            else
            {
                CardData chosenCardData = GetRandomCardData(emptyCardData);
                if (chosenCardData != null)
                {
                    selectedPrefab = chosenCardData.prefab;
                    chosenCardData.currentSpawns++;
                }
            }

            if (selectedPrefab != null)
            {
                GameObject newCard = Instantiate(selectedPrefab, cardParent);

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

    [ContextMenu("Remove Cards")]
    public void RemoveCards()
    {
        List<Transform> cardsToRemove = new List<Transform>();
        foreach (Transform child in cardParent)
        {
            cardsToRemove.Add(child);
        }

        foreach (Transform card in cardsToRemove)
        {
            RectTransform rt = card.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.DOAnchorPos(rt.anchoredPosition + new Vector2(-slideInOffset, 0), slideInDuration)
                  .SetEase(Ease.InQuad)
                  .OnComplete(() => Destroy(card.gameObject));
            }
            else
            {
                card.transform.DOMove(card.transform.position + new Vector3(-slideInOffset, 0, 0), slideInDuration)
                  .SetEase(Ease.InQuad)
                  .OnComplete(() => Destroy(card.gameObject));
            }
        }
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
}
