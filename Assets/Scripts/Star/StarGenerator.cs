using System.Collections.Generic;
using UnityEngine;

public class StarGenerator : MonoBehaviour
{
    public int numStars = 20;
    public Vector2 galaxySize = new Vector2(200, 200);

    public List<StarData> GenerateStars()
    {
        var stars = new List<StarData>();
        for (int i = 0; i < numStars; i++)
        {
            var spectralClass = GetRandomSpectralClass();
            var star = new StarData
            {
                id = i,
                spectralClass = spectralClass,
                color = GetStarColor(spectralClass),
                numPlanets = Random.Range(1, 10),
                position = new Vector2(
                    Random.Range(-galaxySize.x / 2, galaxySize.x / 2),
                    Random.Range(-galaxySize.y / 2, galaxySize.y / 2)
                )
            };
            stars.Add(star);
        }
        return stars;
    }

    private StarSpectralClass GetRandomSpectralClass()
    {
        // Weighted random selection (O and B are rare, M is common)
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
            case StarSpectralClass.O: return new Color(0.6f, 0.8f, 1f); // Blue
            case StarSpectralClass.B: return new Color(0.7f, 0.8f, 1f); // Blue-white
            case StarSpectralClass.A: return new Color(0.8f, 0.85f, 1f); // White
            case StarSpectralClass.F: return new Color(1f, 1f, 0.9f); // Yellow-white
            case StarSpectralClass.G: return new Color(1f, 1f, 0.7f); // Yellow
            case StarSpectralClass.K: return new Color(1f, 0.8f, 0.5f); // Orange
            case StarSpectralClass.M: return new Color(1f, 0.6f, 0.6f); // Red
            default: return Color.white;
        }
    }
}