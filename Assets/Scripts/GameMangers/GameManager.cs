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

    private List<StarData> stars = new List<StarData>();
    private Dictionary<int, List<PlanetData>> starPlanets = new Dictionary<int, List<PlanetData>>();

    [SerializeField] private GameObject starPrefab;
    [SerializeField] private GameObject planetPrefab;

    private const int NumEmpires = 3;
    private static readonly string[] empireNames = { "Player", "Enemy1", "Enemy2" };
    private Dictionary<int, int> starEmpireOwnership = new Dictionary<int, int>();

    private static readonly Color[] empireColors = new[]
    {
        new Color(0.2f, 0.6f, 1f, 0.2f), // Blue
        new Color(1f, 0.4f, 0.2f, 0.2f), // Orange
        new Color(0.4f, 1f, 0.4f, 0.2f), // Green
    };

    private Material voronoiMaterial;

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
        stars = GalaxyGenerator.GenerateStars(numStars, galaxySize, NumEmpires, starEmpireOwnership, empireNames);
        starPlanets = GalaxyGenerator.GeneratePlanetsForStars(stars, starEmpireOwnership, planetGenerator);
        VisualizeGalaxy();
    }

    private void DebugStarOwnership()
    {
        foreach (var star in stars)
        {
            int empireId = starEmpireOwnership.TryGetValue(star.id, out var eid) ? eid : -1;
            if (empireId >= 0 && empireId < empireNames.Length)
            {
                string ownerName = empireNames[empireId];
                Debug.Log($"Star '{star.name}' (ID: {star.id}) is owned by: {ownerName} (Empire ID: {empireId})");
            }
        }
    }

    private void VisualizeGalaxy()
    {
        foreach (var star in stars)
        {
            GameObject systemGO = new GameObject($"StarSystem_{star.id}_{star.name}");
            systemGO.transform.position = star.position;

            var starGO = Instantiate(starPrefab, star.position, Quaternion.identity, systemGO.transform);
            starGO.name = star.name;
            var sr = starGO.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = star.color;

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

                    var planetGO = Instantiate(planetPrefab, planetPos, Quaternion.identity, systemGO.transform);
                    planetGO.name = planets[i].name;

                    var planetSR = planetGO.GetComponent<SpriteRenderer>();
                    if (planetSR != null)
                    {
                        var palette = planets[i].GetColorPalette();
                        if (palette != null && palette.Length > 0)
                        {
                            planetSR.color = palette[0];
                        }
                        else
                        {
                            planetSR.color = Color.Lerp(Color.gray, Color.green, planets[i].habitability / 100f);
                        }
                    }

                    planetGO.transform.localScale = Vector3.one * (0.5f + planets[i].size * 0.05f);

                    var orbit = planetGO.AddComponent<PlanetOrbit>();
                    orbit.starTransform = starGO.transform;
                    orbit.orbitRadius = orbitRadius + i * orbitStep;
                    orbit.orbitSpeed = Random.Range(10f, 30f) / (1f + i);
                    orbit.orbitAngle = angle;
                }
            }
        }
    }

    private void CreateVoronoiMaterial()
    {
        if (voronoiMaterial == null)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            voronoiMaterial = new Material(shader);
            voronoiMaterial.hideFlags = HideFlags.HideAndDontSave;
            voronoiMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            voronoiMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            voronoiMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            voronoiMaterial.SetInt("_ZWrite", 0);
        }
    }

    private void OnRenderObject()
    {
        // Only show Voronoi if zoom is greater than 200
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic || cam.orthographicSize <= 200)
            return;

        if (stars == null || starPlanets == null)
            return;

        CreateVoronoiMaterial();
        voronoiMaterial.SetPass(0);

        int gridResolution = 60;
        float minX = -galaxySize.x / 2;
        float maxX = galaxySize.x / 2;
        float minY = -galaxySize.y / 2;
        float maxY = galaxySize.y / 2;

        float maxTerritoryRadius = 350f;

        var starCenters = new List<(Vector2 pos, int empireId)>();
        foreach (var star in stars)
        {
            int empireId = starEmpireOwnership.TryGetValue(star.id, out var eid) ? eid : -1;
            starCenters.Add((star.position, empireId));
        }

        float cellWidth = (maxX - minX) / gridResolution;
        float cellHeight = (maxY - minY) / gridResolution;

        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);
        GL.Begin(GL.QUADS);

        for (int gx = 0; gx < gridResolution; gx++)
        {
            for (int gy = 0; gy < gridResolution; gy++)
            {
                float x = Mathf.Lerp(minX, maxX, gx / (float)(gridResolution - 1));
                float y = Mathf.Lerp(minY, maxY, gy / (float)(gridResolution - 1));
                Vector2 cellPos = new Vector2(x, y);

                float minDist = float.MaxValue;
                int nearestEmpire = -1;
                foreach (var (pos, empireId) in starCenters)
                {
                    float dist = Vector2.Distance(cellPos, pos);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearestEmpire = empireId;
                    }
                }

                if (nearestEmpire >= 0 && nearestEmpire < empireColors.Length && minDist <= maxTerritoryRadius)
                {
                    Color color = empireColors[nearestEmpire];
                    color.a = 0.5f; // Make sure alpha is visible
                    GL.Color(color);

                    GL.Vertex3(x, y, -1); // Z = -1 for visibility
                    GL.Vertex3(x + cellWidth, y, -1);
                    GL.Vertex3(x + cellWidth, y + cellHeight, -1);
                    GL.Vertex3(x, y + cellHeight, -1);
                }
            }
        }

        GL.End();
        GL.PopMatrix();
    }

    private void OnDrawGizmos()
    {
        if (stars == null || starPlanets == null)
            return;

        foreach (var star in stars)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(star.position, 2f);
        }

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
            }
        }
    }
}