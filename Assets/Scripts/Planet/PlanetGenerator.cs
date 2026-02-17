using System.Collections.Generic;
using UnityEngine;

public class PlanetGenerator : MonoBehaviour
{
    public int planetsPerStar = 5;

    public List<PlanetData> GeneratePlanets(int starId, StarSpectralClass starClass)
    {
        var planets = new List<PlanetData>();
        for (int i = 0; i < planetsPerStar; i++)
        {
            var planet = new PlanetData
            {
                id = starId * 100 + i,
                planetType = GetRandomPlanetType(starClass),
                size = Random.Range(10, 25),
                habitability = Random.Range(30, 100),
                resources = GenerateResources(),
                modifiers = GenerateModifiers(),
                ownerEmpireID = null
            };
            planets.Add(planet);
        }
        return planets;
    }

    private PlanetType GetRandomPlanetType(StarSpectralClass starClass)
    {
        // Example: weight planet types by star class
        switch (starClass)
        {
            case StarSpectralClass.O:
            case StarSpectralClass.B:
                return PlanetType.GasGiant;
            case StarSpectralClass.G:
            case StarSpectralClass.K:
                return (PlanetType)Random.Range(0, 3); // Continental, Desert, Ocean
            default:
                return (PlanetType)Random.Range(0, System.Enum.GetValues(typeof(PlanetType)).Length);
        }
    }

    private ResourceOutput GenerateResources()
    {
        return new ResourceOutput
        {
            energy = Random.Range(0, 6),
            minerals = Random.Range(0, 6),
            food = Random.Range(0, 6)
        };
    }

    private List<PlanetModifier> GenerateModifiers()
    {
        var mods = new List<PlanetModifier>();
        if (Random.value < 0.2f) mods.Add(PlanetModifier.AncientRuins);
        if (Random.value < 0.1f) mods.Add(PlanetModifier.TectonicInstability);
        return mods;
    }
}

public enum StarSpectralClass
{
    O, B, A, F, G, K, M
}