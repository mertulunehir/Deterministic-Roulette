using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouletteBetController : MonoBehaviour
{
    private Chips _currentSelectedChip = Chips.Ten;
    private int currentBetAmount = 0;
    
    private void OnEnable()
    {
        EventManager.Subscribe(GameEvents.OnGameBetChanged,OnBetChanged);
        EventManager.Subscribe(GameEvents.OnCancelBetButtonClicked,OnCancelBet);
        EventManager.Subscribe(GameEvents.OnSpinButtonClicked,OnSpinWheel);
    }



    private void OnDisable()
    {
        EventManager.Unsubscribe(GameEvents.OnGameBetChanged,OnBetChanged);
        EventManager.Unsubscribe(GameEvents.OnCancelBetButtonClicked,OnCancelBet);
        EventManager.Unsubscribe(GameEvents.OnSpinButtonClicked,OnSpinWheel);
    }
    
    
    private void OnCancelBet(object[] obj)
    {
        currentBetAmount = 0;
    }
    private void OnSpinWheel(object[] obj)
    {
        if (currentBetAmount > 0)
        {
            //TODO : start wheel rotate
        }
    }
    
    private void OnBetChanged(object[] obj)
    {
        _currentSelectedChip = (Chips)obj[0];
    }


}
