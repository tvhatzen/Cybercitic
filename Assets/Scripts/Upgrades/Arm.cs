using UnityEngine;

public class Arm : Upgrade
{
    void Start()
    {
        
    }

    public override void OnPurchase()
    {
        base.OnPurchase();

        // increase stats to increase
        Debug.Log("upgraded arm");
    }
}
