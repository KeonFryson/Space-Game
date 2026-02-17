using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlanetControl : MonoBehaviour
{
    [SerializeField] private Canvas planetsParent;
    [SerializeField] private GameObject[] planets;
    [SerializeField] private MaterialSave _materialSave;

    private float time = 0f;
    private int[] seeds;
    private bool override_time = false;

    private void Start()
    {
        planets = new GameObject[planetsParent.transform.childCount];

        for (int i = 0; i < planetsParent.transform.childCount; i++)
        {
            planets[i] = planetsParent.transform.GetChild(i).gameObject;
        }

        seeds = new int[planets.Length];
        var now = System.DateTime.Now.Millisecond;
        UnityEngine.Random.InitState(now);

        for (int i = 0; i < planets.Length; i++)
        {
            seeds[i] = UnityEngine.Random.Range(0, int.MaxValue);
            var planet = planets[i].GetComponent<IPlanet>();
            if (planet != null)
            {
                planet.SetSeed(seeds[i]);
            }
        }
    }

    private void Update()
    {
        if (isOnGui()) return;

        // Use new Input System for mouse input
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            var pos = Camera.main.ScreenToViewportPoint(Mouse.current.position.ReadValue());
            foreach (var planetObj in planets)
            {
                var planet = planetObj.GetComponent<IPlanet>();
                if (planet != null)
                {
                    planet.SetLight(pos);
                }
            }
        }

        time += Time.deltaTime;
        if (!override_time)
        {
            foreach (var planetObj in planets)
            {
                var planet = planetObj.GetComponent<IPlanet>();
                if (planet != null)
                {
                    planet.UpdateTime(time);
                }
            }
        }
    }

    private bool isOnGui()
    {
        var eventData = new PointerEventData(EventSystem.current);
        eventData.position = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        var result = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, result);

        return result.Exists(x => x.gameObject.GetComponent<Selectable>() != null);
    }
}