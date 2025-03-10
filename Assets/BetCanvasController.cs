using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BetCanvasController : MonoBehaviour
{
    [SerializeField] private GameObject canvasParent;
    [SerializeField] private Button tenButton;
    [SerializeField] private Button fiftyButton;
    [SerializeField] private Button hunderedButton;
    [SerializeField] private Button twoHunderedButton;
    [SerializeField] private Button cancelBetButton;
    [SerializeField] private Button spinButton;
    [SerializeField] private Color buttonOutlineOpenColor;
    [SerializeField] private Color buttonOutlineCloseColor;
    // Start is called before the first frame update
    
    private void OnEnable()
    {
        tenButton.onClick.AddListener(OnTenButtonClicked);
        fiftyButton.onClick.AddListener(OnFiftyButtonClicked);
        hunderedButton.onClick.AddListener(OnHunderedButtonClicked);
        twoHunderedButton.onClick.AddListener(OnTwoHunderedButtonClicked);
        cancelBetButton.onClick.AddListener(OnCancelButtonClicked);
        spinButton.onClick.AddListener(OnSpinButtonClicked);
    }


    private void OnDisable()
    {
        tenButton.onClick.RemoveAllListeners();
        fiftyButton.onClick.RemoveAllListeners();
        hunderedButton.onClick.RemoveAllListeners();
        twoHunderedButton.onClick.RemoveAllListeners();
        cancelBetButton.onClick.RemoveAllListeners();
        spinButton.onClick.RemoveAllListeners();
    }

    private void OnBetChangeButtonClicked(Chips chips)
    {
        switch (chips)
        {
            case Chips.Ten:
                tenButton.GetComponent<Image>().color = buttonOutlineOpenColor;
                fiftyButton.GetComponent<Image>().color = buttonOutlineCloseColor;
                hunderedButton.GetComponent<Image>().color = buttonOutlineCloseColor;
                twoHunderedButton.GetComponent<Image>().color = buttonOutlineCloseColor;
                break;
            case Chips.Fifty:
                tenButton.GetComponent<Image>().color = buttonOutlineCloseColor;
                fiftyButton.GetComponent<Image>().color = buttonOutlineOpenColor;
                hunderedButton.GetComponent<Image>().color = buttonOutlineCloseColor;
                twoHunderedButton.GetComponent<Image>().color = buttonOutlineCloseColor;
                break;
            case Chips.Hundered:
                tenButton.GetComponent<Image>().color = buttonOutlineCloseColor;
                fiftyButton.GetComponent<Image>().color = buttonOutlineCloseColor;
                hunderedButton.GetComponent<Image>().color = buttonOutlineOpenColor;
                twoHunderedButton.GetComponent<Image>().color = buttonOutlineCloseColor;
                break;
            case Chips.TwoHundered:
                tenButton.GetComponent<Image>().color = buttonOutlineCloseColor;
                fiftyButton.GetComponent<Image>().color = buttonOutlineCloseColor;
                hunderedButton.GetComponent<Image>().color = buttonOutlineCloseColor;
                twoHunderedButton.GetComponent<Image>().color = buttonOutlineOpenColor;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(chips), chips, null);
        }
    }

    private void OnTwoHunderedButtonClicked()
    {
        OnBetChangeButtonClicked(Chips.TwoHundered);
        EventManager.TriggerEvent(GameEvents.OnGameBetChanged,Chips.TwoHundered);
    }

    private void OnHunderedButtonClicked()
    {
        OnBetChangeButtonClicked(Chips.Hundered);
        EventManager.TriggerEvent(GameEvents.OnGameBetChanged,Chips.Hundered);
    }

    private void OnFiftyButtonClicked()
    {
        OnBetChangeButtonClicked(Chips.Fifty);
        EventManager.TriggerEvent(GameEvents.OnGameBetChanged,Chips.Fifty);
    }

    private void OnTenButtonClicked()
    {
        OnBetChangeButtonClicked(Chips.Ten);
        EventManager.TriggerEvent(GameEvents.OnGameBetChanged,Chips.Ten);
    }
    
    private void OnCancelButtonClicked()
    {
        EventManager.TriggerEvent(GameEvents.OnCancelBetButtonClicked);
    }
    private void OnSpinButtonClicked()
    {
        canvasParent.SetActive(false);
        EventManager.TriggerEvent(GameEvents.OnSpinButtonClicked);
    }
}
