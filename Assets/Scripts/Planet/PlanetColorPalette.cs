using UnityEngine;

public static class PlanetColorPalette
{
    public static Color[] GetColorsForType(PlanetType type)
    {
        switch (type)
        {
            case PlanetType.GasGiant:
                return new[] { new Color(0.24f, 0.13f, 0.15f), new Color(0.94f, 0.71f, 0.25f) }; // Example: brown/yellow
            case PlanetType.Continental:
                return new[] { new Color(0.36f, 0.67f, 0.36f), new Color(0.22f, 0.44f, 0.22f) }; // Example: green
            case PlanetType.Desert:
                return new[] { new Color(0.93f, 0.80f, 0.45f), new Color(0.85f, 0.65f, 0.13f) }; // Example: yellow
            case PlanetType.Ocean:
                return new[] { new Color(0.22f, 0.44f, 0.67f), new Color(0.13f, 0.27f, 0.53f) }; // Example: blue
            case PlanetType.Arctic:
                return new[] { new Color(0.90f, 0.98f, 1.0f), new Color(0.56f, 0.83f, 0.94f) }; // Example: light blue
            case PlanetType.Lava:
                return new[] { new Color(0.98f, 0.54f, 0.20f), new Color(0.67f, 0.18f, 0.18f) }; // Example: orange/red
            case PlanetType.DryTerran:
                return new[] { new Color(0.87f, 0.54f, 0.20f), new Color(0.53f, 0.33f, 0.18f) }; // Example: brown
            default:
                return new[] { Color.gray, Color.white };
        }
    }

    // Returns the default habitability for each planet type (0-100)
    public static int GetDefaultHabitability(PlanetType type)
    {
        switch (type)
        {
            case PlanetType.Continental:
                return 90;
            case PlanetType.Ocean:
                return 80;
            case PlanetType.Desert:
                return 60;
            case PlanetType.DryTerran:
                return 55;
            case PlanetType.Arctic:
                return 40;
            case PlanetType.Lava:
                return 10;
            case PlanetType.GasGiant:
                return 0;
            default:
                return 30;
        }
    }
}