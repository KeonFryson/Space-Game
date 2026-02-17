using System;
using UnityEngine;

[Serializable]
public class StarData
{
    public int id;
    public string name;
    public StarSpectralClass spectralClass;
    public Color color;
    public int numPlanets;
    public Vector2 position;
}