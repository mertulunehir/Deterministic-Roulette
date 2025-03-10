using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouletteWheelNumberController : MonoBehaviour
{

    [SerializeField] private Transform[] numberPositions;

    public Transform GetNumberTransform(int number)
    {
        return numberPositions[number];
    }

}
