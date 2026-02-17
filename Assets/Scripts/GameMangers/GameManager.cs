using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Star Generation")]
    [SerializeField] private int numStars = 20;
    [SerializeField] private Vector2 galaxySize = new Vector2(200, 200);

    [Header("Planet Generation")]
    [SerializeField] private PlanetGenerator planetGenerator;

    // Store generated stars
    private List<StarData> stars = new List<StarData>();
    // Store generated planets per star
    private Dictionary<int, List<PlanetData>> starPlanets = new Dictionary<int, List<PlanetData>>();

    [SerializeField] private GameObject starPrefab;
    [SerializeField] private GameObject planetPrefab;
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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        GenerateStars();
        GeneratePlanetsForStars();
        VisualizeGalaxy();
    }

    private void GenerateStars()
    {
        Debug.Log("Generating stars...");
        stars.Clear();
        float minStarDistance = 200f;

        // Always spawn the first star at (0, 0)
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
        Debug.Log($"Generated Star {firstStar.id} ({firstStar.name}): Class {firstStar.spectralClass}, Planets {firstStar.numPlanets}, Position {firstStar.position}");

        for (int i = 1; i < numStars; i++)
        {
            Vector2 position;
            int attempts = 0;
            const int maxAttempts = 100;

            do
            {
                position = new Vector2(
                    Random.Range(-galaxySize.x / 2, galaxySize.x / 2),
                    Random.Range(-galaxySize.y / 2, galaxySize.y / 2)
                );
                attempts++;
            }
            // Ensure not too close to other stars (including (0,0))
            while (IsTooCloseToOtherStars(position, minStarDistance) && attempts < maxAttempts);

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
            Debug.Log($"Generated Star {star.id} ({star.name}): Class {star.spectralClass}, Planets {star.numPlanets}, Position {star.position}");
        }
    }


    private bool IsTooCloseToOtherStars(Vector2 position, float minDistance)
    {
        foreach (var star in stars)
        {
            if (Vector2.Distance(star.position, position) < minDistance)
                return true;
        }
        return false;
    }

    private void GeneratePlanetsForStars()
    {
        if (planetGenerator == null)
        {
            planetGenerator = FindFirstObjectByType<PlanetGenerator>();
            if (planetGenerator == null)
            {
                Debug.LogError("PlanetGenerator not assigned and not found in scene.");
                return;
            }
        }

        starPlanets.Clear();
        foreach (var star in stars)
        {
            var planets = planetGenerator.GeneratePlanets(star.id, star.spectralClass);
            for (int i = 0; i < planets.Count; i++)
            {
                planets[i].name = GenerateRandomName(); // Use generated name
            }
            starPlanets[star.id] = planets;
        }
    }


    private StarSpectralClass GetRandomSpectralClass()
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

    private Color GetStarColor(StarSpectralClass spectralClass)
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

    private void VisualizeGalaxy()
    {
        foreach (var star in stars)
        {
            // Create a parent GameObject for the star system
            GameObject systemGO = new GameObject($"StarSystem_{star.id}_{star.name}");
            systemGO.transform.position = star.position;

            // Instantiate star as a child of the system
            var starGO = Instantiate(starPrefab, star.position, Quaternion.identity, systemGO.transform);
            starGO.name = star.name; // Set GameObject name
            var sr = starGO.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = star.color;

            // Visualize planets in orbit, as children of the system
            if (starPlanets.TryGetValue(star.id, out var planets))
            {
                float orbitRadius = 8f;
                float orbitStep = 3f;
                float angleStep = 360f / Mathf.Max(planets.Count, 1);

                for (int i = 0; i < planets.Count; i++)
                {
                    float angle = i * angleStep;
                    Vector2 planetOffset = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * (orbitRadius + i * orbitStep);
                    Vector2 planetPos = star.position + planetOffset;

                    // Instantiate planet as a child of the system
                    var planetGO = Instantiate(planetPrefab, planetPos, Quaternion.identity, systemGO.transform);
                    planetGO.name = planets[i].name; // Set GameObject name

                    // Optionally set color/size based on planet data
                    var planetSR = planetGO.GetComponent<SpriteRenderer>();
                    if (planetSR != null)
                        planetSR.color = Color.Lerp(Color.gray, Color.green, planets[i].habitability / 100f);

                    planetGO.transform.localScale = Vector3.one * (0.5f + planets[i].size * 0.05f);

                    // Add PlanetOrbit component
                    var orbit = planetGO.AddComponent<PlanetOrbit>();
                    orbit.starTransform = starGO.transform;
                    orbit.orbitRadius = orbitRadius + i * orbitStep;
                    orbit.orbitSpeed = Random.Range(10f, 30f) / (1f + i); // Vary speed per planet
                    orbit.orbitAngle = angle;
                }
            }
        }
    }
   

    private string GenerateRandomName(int minSyllables = 2, int maxSyllables = 3)
    {
        int syllableCount = Random.Range(minSyllables, maxSyllables + 1);
        System.Text.StringBuilder nameBuilder = new System.Text.StringBuilder();
        for (int i = 0; i < syllableCount; i++)
        {
            string syllable = nameSyllables[Random.Range(0, nameSyllables.Length)];
            nameBuilder.Append(syllable);
        }
        // Capitalize first letter
        if (nameBuilder.Length > 0)
            nameBuilder[0] = char.ToUpper(nameBuilder[0]);
        return nameBuilder.ToString();
    }
    private void OnDrawGizmos()
    {
        // Only draw if stars and starPlanets are initialized and have data
        if (stars == null || starPlanets == null)
            return;

        // Draw stars, planets, and their orbits (existing code)
        foreach (var star in stars)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(star.position, 2f);

            
        }

        // Draw blue line to nearest neighbor and label the distance
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.normal.textColor = Color.cyan;
        labelStyle.fontSize = 16;

        for (int i = 0; i < stars.Count; i++)
        {
            var starA = stars[i];
            float minDist = float.MaxValue;
            int nearestIndex = -1;

            for (int j = 0; j < stars.Count; j++)
            {
                if (i == j) continue;
                float dist = Vector2.Distance(starA.position, stars[j].position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestIndex = j;
                }
            }

            if (nearestIndex != -1)
            {
                var starB = stars[nearestIndex];
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(starA.position, starB.position);

#if UNITY_EDITOR
                // Draw the distance label at the midpoint
                Vector3 midPoint = (starA.position + starB.position) / 2f;
                UnityEditor.Handles.color = Color.cyan;
                UnityEditor.Handles.Label(midPoint, minDist.ToString("F1"), labelStyle);
#endif
            }
        }
    }
}