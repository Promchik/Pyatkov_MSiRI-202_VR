using TMPro;
using UnityEngine;

public class MachinePanel : MonoBehaviour
{
    public TMP_Text stateText;
    public TMP_Text axesText;
    public TMP_Text messageText;

  
    public void UpdateUI(bool isRunning, float x, float y, float z)
    {
        if (stateText != null)
            stateText.text = isRunning ? "Работает" : "Остановлен";

        if (axesText != null)
            axesText.text = $"X: {x:F1} мм\nY: {y:F1} мм\nZ: {z:F1} мм";
    }

    
    public void ShowMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }
}