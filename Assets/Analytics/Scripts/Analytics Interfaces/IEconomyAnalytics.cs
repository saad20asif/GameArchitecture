using System;
using System.Collections.Generic;

public interface IEconomyAnalytics 
{
    public void EconomySink(string placement, string type, string name, int amount, Dictionary<string, object> additionalData);

    public void EconomySource(string placement, string type, string name, int amount, Dictionary<string, object> additionalData);
}
