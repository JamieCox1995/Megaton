using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Flammable), typeof(Explosive))]
public class FuelContainer : PhysObject
{
    private Flammable _flamable;
    private Explosive _explosive;
}
