using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chip : MonoBehaviour
{
    [SerializeField] private Chips chipType;
    
    public Chips ChipType  => chipType;
    
    [HideInInspector]
    public TableNumberPlace currentPlace;
    
}
