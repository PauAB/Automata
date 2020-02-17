using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class life : DispatchableComponent
{
 
    public float myLife = 100;

    public  override void Dispatch(Message m)
    {
        myLife -= ((DamageMessage)m).damage;
    }
}
