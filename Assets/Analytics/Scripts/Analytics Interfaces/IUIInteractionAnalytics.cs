
using System.Collections.Generic;

public interface IUIInteractionAnalytics 
{
    public void UIInteraction(string screen, string element, string type, string action, Dictionary<string, object> additionalData);
}
