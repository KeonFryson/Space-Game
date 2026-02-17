using UnityEngine;

public class PlanetOrbit : MonoBehaviour
{
    public Transform starTransform;
    public float orbitRadius;
    public float orbitSpeed;
    public float orbitAngle; // in degrees

    void Update()
    {
        if (starTransform == null) return;

        orbitAngle += orbitSpeed * Time.deltaTime;
        float rad = orbitAngle * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * orbitRadius;
        transform.position = (Vector2)starTransform.position + offset;
    }
}