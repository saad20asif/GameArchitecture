using UnityEngine;

public class GameHud : MonoBehaviour
{
    public virtual void Show()
    {
        // Basic implementation of showing the HUD
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        // Basic implementation of hiding the HUD
        gameObject.SetActive(false);
    }

    // Other common HUD methods can go here
}
