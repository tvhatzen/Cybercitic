using UnityEngine;

public class Leg : Upgrade
{
    
    void Start()
    {
        
    }

    public override void OnPurchase()
    {
        base.OnPurchase();

        // increase stats to increase
        Debug.Log("upgraded leg");
    }
}
