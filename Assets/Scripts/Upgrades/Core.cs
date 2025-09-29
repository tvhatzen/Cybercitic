using UnityEngine;

public class Core : Upgrade
{
    void Start()
    {
        
    }

    public override void OnPurchase()
    {
        base.OnPurchase();

        // increase stats to increase
        Debug.Log("upgraded core");
    }
}
