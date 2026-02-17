using System.Collections.Generic;
using UnityEngine;

public class PlanetGenerator : MonoBehaviour
{
    public int planetsPerStarmin = 1;
    public int planetsPerStarmax = 5;



    // Update GeneratePlanets to accept empireId
    public List<PlanetData> GeneratePlanets(int starId, StarSpectralClass starClass, int ownerEmpireID)
    {
        int planetsPerStar = Random.Range(planetsPerStarmin, planetsPerStarmax);
        var planets = new List<PlanetData>();
        bool habitabilityOver90Spawned = false;

        for (int i = 0; i < planetsPerStar; i++)
        {
            var planetType = GetRandomPlanetType(starClass);
            int baseHabitability = PlanetColorPalette.GetDefaultHabitability(planetType);
            int habitability = Mathf.Clamp(baseHabitability + Random.Range(-10, 11), 0, 100);

            if (starId == 0 && !habitabilityOver90Spawned && i == planetsPerStar - 1)
            {
                habitability = Random.Range(99, 101);
                habitabilityOver90Spawned = true;
            }
            else if (starId == 0 && habitability > 90)
            {
                habitabilityOver90Spawned = true;
            }

            var planet = new PlanetData
            {
                id = starId * 100 + i,
                planetType = planetType,
                size = Random.Range(30, 45),
                habitability = habitability,
                resources = GenerateResources(),
                modifiers = GenerateModifiers(),
                ownerEmpireID = ownerEmpireID
            };

            Color[] palette = planet.GetColorPalette();

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