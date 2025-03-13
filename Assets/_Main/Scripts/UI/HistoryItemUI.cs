using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HistoryItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winningNumberText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI betAmountText;
    [SerializeField] private TextMeshProUGUI winAmountText;
    [SerializeField] private TextMeshProUGUI BetTypeText;
    
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color winColor = new Color(0.7f, 1.0f, 0.7f);
    [SerializeField] private Color loseColor = new Color(1.0f, 0.7f, 0.7f);
    
    public void SetupItem(GameRecord record)
    {
        
        if (winningNumberText)
        {
            winningNumberText.text = record.winningNumber.ToString();
        }
        
        if (resultText)
        {
            resultText.text = record.isWin ? "Win" : "Lose";
            resultText.color = record.isWin ? Color.green : Color.red;
        }
        
        if (betAmountText)
        {
            betAmountText.text = $"${record.betAmount}";
        }
        
        if (winAmountText)
        {
            if (record.isWin)
            {
                winAmountText.text = $"+${record.winAmount}";
                winAmountText.color = Color.green;
            }
            else
            {
                winAmountText.text = $"-${record.betAmount}";
                winAmountText.color = Color.red;
            }
        }
        // Set background color based on win/lose
        if (backgroundImage)
        {
            backgroundImage.color = record.isWin ? winColor : loseColor;
        }

        if (record.bets.Count > 1)
            BetTypeText.text = "Mixed Bet";
        else 
            BetTypeText.text = record.bets[0].betType.ToString();
    }
}
