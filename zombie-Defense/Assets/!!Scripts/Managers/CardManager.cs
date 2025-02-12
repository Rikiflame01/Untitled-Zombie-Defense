using System;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    [Header("Card Container")]
    public Transform cardContainer;

    void Start()
    {
        RegisterCardClicks();
    }

    
    void OnEnable()
    {
        ActionManager.OnChooseCard += RegisterCardClicks;
    }

    void OnDisable()
    {
        ActionManager.OnChooseCard -= RegisterCardClicks;
    }

    [ContextMenu("Register Card Clicks")]
    public void RegisterCardClicks()
    {
        Button[] buttons = cardContainer.GetComponentsInChildren<Button>();

        foreach (Button btn in buttons)
        {
            Button capturedButton = btn;
            capturedButton.onClick.RemoveAllListeners();
            capturedButton.onClick.AddListener(() => OnCardClicked(capturedButton));
        }
    }

    public void OnCardClicked(Button btn)
    {
        Card card = btn.GetComponent<Card>();
        if (card != null)
        {
            Debug.Log("Card clicked: " + card.specificCard);
            ActionManager.InvokeCardChosen(card.specificCard.ToString());
        }
        else
        {
            Debug.LogWarning("Clicked button does not have a Card component attached!");
        }
    }
}
