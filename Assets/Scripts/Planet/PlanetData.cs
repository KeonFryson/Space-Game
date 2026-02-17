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
}

public enum PlanetType
{
    Continental,
    Desert,
    Ocean,
    GasGiant,
    Arctic,
    Tropical,
    Savannah,
    Alpine,
    Tomb,
    Machine,
    Hive
}

[Serializable]
public class ResourceOutput
{
    public int energy;
    public int minerals;
    public int food;
}

public enum PlanetModifier
{
    AncientRuins,
    TectonicInstability,
    Storms,
    Wildlife,
    UnusualGeology
}