using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlanetData
{
    public int id;
    public string name;
    public PlanetType planetType;
    public int size;
    public int habitability;
    public ResourceOutput resources;
    public List<PlanetModifier> modifiers;
    public int? ownerEmpireID;

    public PlanetData()
    {
        modifiers = new List<PlanetModifier>();
    }
    public Color[] GetColorPalette()
    {
        return PlanetColorPalette.GetColorsForType(planetType);
    }
}

public enum PlanetType
{
    Continental,
    Desert,
    Ocean,
    GasGiant,
    Arctic,
    Lava,
    DryTerran
}
 
public class ResourceOutput
{
    public int energy;
    public int minerals;
    public int food;

    public override string ToString()
    {
        return $"Energy: {energy}, Minerals: {minerals}, Food: {food}";
    }
}

public enum PlanetModifier
{
    AncientRuins,
    TectonicInstability,
    Storms,
    Wildlife,
    UnusualGeology
}