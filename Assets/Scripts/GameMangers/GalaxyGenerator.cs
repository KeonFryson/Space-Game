using System.Collections.Generic;
using UnityEngine;

public class GalaxyGenerator
{
    private static readonly string[] nameSyllables = new[]
    {
         
        "zor", "vel", "tria", "luma", "ser", "quar", "meth", "orion", "astra", "pho",
        "kel", "vara", "lith", "syl", "mond", "pyra", "zen", "thal", "kyro", "nova",
        "dra", "ulon", "fera", "sola", "quent", "vex", "talon", "rya", "gala", "nura",
        "kri", "zeth", "vora", "shun", "pyr", "xal", "omra", "theru", "zora", "vrin",
        "shara", "koru", "ryx", "fyra", "dral", "venar", "mylo", "tryn", "syra", "zori",
        "kyla", "veth", "solin", "grax", "ulith", "qira", "naru", "zylo", "kelth", "phara",
        "xor", "vrum", "liron", "kyreth", "zorin", "thora", "yvra", "shyl", "vond", "qorin",
        "sylax", "narith", "vyko", "telu", "drith", "azrel", "qyra", "valon", "oryx", "fyrin",
        "selu", "trax", "zulon", "kyrin", "ostra", "zhala", "neth", "volar", "shen", "krya",
        "yonar", "zyla", "thrix", "ulmar", "vorin", "quira", "shira", "drax", "pylor", "sena",
        "ralu", "zhor", "myra", "vask", "kylor", "trel", "shova", "ulrax", "pyron", "xeru",
        "valis", "drox", "fira", "synth", "koral", "zavor", "nyl", "toraq", "vrex", "sorin",
        "lumae", "qeth", "zunar", "xira", "prylon", "deth", "shyra", "xelun", "tholor", "marix",
        "zenal", "kyth", "vraen", "selo", "dorin", "fynox", "treya", "xorin", "vraelo", "qen",
        "zulen", "myrix", "phel", "dranor", "savax", "urim", "vyral", "xelar", "qynth", "shorin",
        "lurex", "zanith", "korin", "vylar", "solyx", "dareth", "fyron", "shylo", "vorux", "talin",
        "quenix", "rylax", "zhev", "thalon", "molir", "sarix", "unel", "gyrin", "shorix", "praal",
        "narvox", "xyral", "telith", "zomra", "krinax", "selyr", "valok", "drolyn", "porax", "felyn",
        "oryn", "zethor", "tavik", "kyral", "phorin", "sorak", "drelix", "mythra", "uliv", "vrath"
    };

    public static List<StarData> GenerateStars(int numStars, Vector2 galaxySize, int numEmpires, Dictionary<int, int> starEmpireOwnership, string[] empireNames)
    {
        var stars = new List<StarData>();
        starEmpireOwnership.Clear();
        float minStarDistance = 200f;
        float minEmpireStartDistance = 5000f;
        float regionWidth = galaxySize.x / numEmpires;
        float leftBound = -galaxySize.x / 2;
        float rightBound = galaxySize.x / 2;

        var spectralClass = GetRandomSpectralClass();
        var firstStar = new StarData
        {
            id = 0,
            name = GenerateRandomName(),
            spectralClass = spectralClass,
            color = GetStarColor(spectralClass),
            numPlanets = Random.Range(1, 10),
            position = Vector2.zero
        };
        stars.Add(firstStar);
        starEmpireOwnership[firstStar.id] = 0;

        for (int i = 1; i < numStars; i++)
        {
            Vector2 position;
            int attempts = 0;
            const int maxAttempts = 100;
            int region = (i * numEmpires) / numStars;
            float regionStart = leftBound + region * regionWidth;
            float regionEnd = regionStart + regionWidth;
            do
            {
                position = new Vector2(
                    Random.Range(regionStart + 20f, regionEnd - 20f),
                    Random.Range(-galaxySize.y / 2 + 20f, galaxySize.y / 2 - 20f)
                );
                attempts++;
            }
            while (IsTooCloseToOtherStars(position, minStarDistance, stars) && attempts < maxAttempts);
            spectralClass = GetRandomSpectralClass();
            var star = new StarData
            {
                id = i,
                name = GenerateRandomName(),
                spectralClass = spectralClass,
                color = GetStarColor(spectralClass),
                numPlanets = Random.Range(1, 10),
                position = position
            };
            stars.Add(star);
        }

        List<int> availableStarIds = new List<int>();
        for (int i = 1; i < stars.Count; i++)
            availableStarIds.Add(stars[i].id);
        List<Vector2> empireStartPositions = new List<Vector2> { Vector2.zero };
        for (int empire = 0; empire < numEmpires; empire++)
        {
            int starsAssigned = (empire == 0) ? 1 : 0;
            int firstStarId = (empire == 0) ? 0 : -1;
            if (empire != 0 && availableStarIds.Count > 0)
            {
                float regionCenterX = leftBound + regionWidth * empire + regionWidth / 2;
                Vector2 regionCenter = new Vector2(regionCenterX, 0);
                float minDist = float.MaxValue;
                int candidateStarId = -1;
                foreach (int starId in availableStarIds)
                {
                    Vector2 starPos = stars[starId].position;
                    bool farEnough = true;
                    foreach (var otherEmpirePos in empireStartPositions)
                    {
                        if (Vector2.Distance(starPos, otherEmpirePos) < minEmpireStartDistance)
                        {
                            farEnough = false;
                            break;
                        }
                    }
                    if (!farEnough)
                        continue;
                    float distToRegion = Vector2.Distance(starPos, regionCenter);
                    if (distToRegion < minDist)
                    {
                        minDist = distToRegion;
                        candidateStarId = starId;
                    }
                }
                if (candidateStarId != -1)
                {
                    firstStarId = candidateStarId;
                    starEmpireOwnership[firstStarId] = empire;
                    empireStartPositions.Add(stars[firstStarId].position);
                    availableStarIds.Remove(firstStarId);
                    starsAssigned++;
                }
            }
            if (starsAssigned < 2 && availableStarIds.Count > 0)
            {
                Vector2 refStarPos = stars[firstStarId].position;
                float minDist = float.MaxValue;
                int closestStarId = -1;
                foreach (int starId in availableStarIds)
                {
                    Vector2 starPos = stars[starId].position;
                    float dist = Vector2.Distance(starPos, refStarPos);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestStarId = starId;
                    }
                }
                if (closestStarId != -1)
                {
                    starEmpireOwnership[closestStarId] = empire;
                    empireStartPositions.Add(stars[closestStarId].position);
                    availableStarIds.Remove(closestStarId);
                    starsAssigned++;
                }
            }
        }
        foreach (var star in stars)
        {
            if (!starEmpireOwnership.ContainsKey(star.id))
                starEmpireOwnership[star.id] = -1;
        }
        return stars;
    }

    public static Dictionary<int, List<PlanetData>> GeneratePlanetsForStars(List<StarData> stars, Dictionary<int, int> starEmpireOwnership, PlanetGenerator planetGenerator)
    {
        var starPlanets = new Dictionary<int, List<PlanetData>>();
        if (planetGenerator == null)
        {
            planetGenerator = Object.FindFirstObjectByType<PlanetGenerator>();
            if (planetGenerator == null)
            {
                Debug.LogError("PlanetGenerator not assigned and not found in scene.");
                return starPlanets;
            }
        }
        foreach (var star in stars)
        {
            int empireId = starEmpireOwnership.TryGetValue(star.id, out var eid) ? eid : -1;
            var planets = planetGenerator.GeneratePlanets(star.id, star.spectralClass, empireId);
            for (int i = 0; i < planets.Count; i++)
            {
                planets[i].name = GenerateRandomName();
            }
            starPlanets[star.id] = planets;
        }
        return starPlanets;
    }

    // Add this method to the GalaxyGenerator class

    public static List<AsteroidData> GenerateAsteroids(
        int numAsteroids,
        Vector2 galaxySize,
        List<StarData> stars,
        float minDistanceFromStars = 300f)
    {
        var asteroids = new List<AsteroidData>();
        float leftBound = -galaxySize.x / 2;
        float rightBound = galaxySize.x / 2;
        float bottomBound = -galaxySize.y / 2;
        float topBound = galaxySize.y / 2;

        for (int i = 0; i < numAsteroids; i++)
        {
            Vector2 position;
            int attempts = 0;
            const int maxAttempts = 100;
            do
            {
                position = new Vector2(
                    Random.Range(leftBound + 20f, rightBound - 20f),
                    Random.Range(bottomBound + 20f, topBound - 20f)
                );
                attempts++;
            }
            while (IsTooCloseToOtherStars(position, minDistanceFromStars, stars) && attempts < maxAttempts);

            var asteroid = new AsteroidData
            {
                id = i,
                name = GenerateRandomName(2, 2) + "-Ast",
                position = position,
                size = Random.Range(20f, 100f)
            };
            asteroids.Add(asteroid);
        }
        return asteroids;
    }



    private static bool IsTooCloseToOtherStars(Vector2 position, float minDistance, List<StarData> stars)
    {
        foreach (var star in stars)
        {
            if (Vector2.Distance(star.position, position) < minDistance)
                return true;
        }
        return false;
    }

    private static StarSpectralClass GetRandomSpectralClass()
    {
        float r = Random.value;
        if (r < 0.01f) return StarSpectralClass.O;
        if (r < 0.03f) return StarSpectralClass.B;
        if (r < 0.08f) return StarSpectralClass.A;
        if (r < 0.15f) return StarSpectralClass.F;
        if (r < 0.30f) return StarSpectralClass.G;
        if (r < 0.60f) return StarSpectralClass.K;
        return StarSpectralClass.M;
    }

    private static Color GetStarColor(StarSpectralClass spectralClass)
    {
        switch (spectralClass)
        {
            case StarSpectralClass.O: return new Color(0.6f, 0.8f, 1f);
            case StarSpectralClass.B: return new Color(0.7f, 0.8f, 1f);
            case StarSpectralClass.A: return new Color(0.8f, 0.85f, 1f);
            case StarSpectralClass.F: return new Color(1f, 1f, 0.9f);
            case StarSpectralClass.G: return new Color(1f, 1f, 0.7f);
            case StarSpectralClass.K: return new Color(1f, 0.8f, 0.5f);
            case StarSpectralClass.M: return new Color(1f, 0.6f, 0.6f);
            default: return Color.white;
        }
    }

    private static string GenerateRandomName(int minSyllables = 2, int maxSyllables = 3)
    {
        int syllableCount = Random.Range(minSyllables, maxSyllables + 1);
        System.Text.StringBuilder nameBuilder = new System.Text.StringBuilder();
        for (int i = 0; i < syllableCount; i++)
        {
            string syllable = nameSyllables[Random.Range(0, nameSyllables.Length)];
            nameBuilder.Append(syllable);
        }
        if (nameBuilder.Length > 0)
            nameBuilder[0] = char.ToUpper(nameBuilder[0]);
        return nameBuilder.ToString();
    }
}
