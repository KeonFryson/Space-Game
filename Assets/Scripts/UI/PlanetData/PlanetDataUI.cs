using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using Unity.VisualScripting;

public class PlanetDataUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI ownerEmpire;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI massText;
    [SerializeField] private TextMeshProUGUI habitabilityText;
 
    [SerializeField] private TextMeshProUGUI resourcesText;
    [SerializeField] private TextMeshProUGUI modifiersText;

    public static PlanetDataUI Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        if (panel != null)
            panel.SetActive(false);
    }

    public void ShowPlanetData(PlanetData data, string ownerName)
    {
        ClearText();
        Debug.Log("Showing planet data: " + (data != null ? data.name : "null"));
        if (panel == null || data == null) return;

        nameText.text = $"Name: {data.name}";
        typeText.text = $"Type: {data.planetType}";
        massText.text = $"Size: {data.size}";
        habitabilityText.text = $"Habitability: {data.habitability}";
        ownerEmpire.text = $"Empire: {ownerName}";
        resourcesText.text = $"Resources: {data.resources}";
        modifiersText.text = $"Modifiers: {FormatModifiers(data.modifiers)}";

        EmptyTextDisactive();

        panel.SetActive(true);
    }

    public void ShowStarData(StarData data)
    {
        ClearText();
        Debug.Log("Showing star data: " + (data != null ? data.name : "null"));
        if (panel == null || data == null) return;

        nameText.text = $"Name: {data.name}";
        typeText.text = $"Spectral Class: {data.spectralClass}";
        habitabilityText.text = $"Position: {data.position}";

        ownerEmpire.text = "";
        resourcesText.text = "";
        modifiersText.text = "";

        EmptyTextDisactive();

        panel.SetActive(true);
    }

    public void HidePanel()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    

    public void ClearText()
    {
        nameText.text = "";
        typeText.text = "";
        massText.text = "";
        habitabilityText.text = "";
        ownerEmpire.text = "";
        resourcesText.text = "";
        modifiersText.text = "";
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private string FormatModifiers(System.Collections.Generic.List<PlanetModifier> modifiers)
    {
        if (modifiers == null || modifiers.Count == 0)
            return "None";
        StringBuilder sb = new StringBuilder();
        foreach (var mod in modifiers)
        {
            sb.Append(mod.ToString()).Append(", ");
        }
        if (sb.Length > 2)
            sb.Length -= 2; // Remove last comma and space
        return sb.ToString();
    }


    private void EmptyTextDisactive()
    {
        ToggleText(nameText);
        ToggleText(typeText);
        ToggleText(massText);
        ToggleText(habitabilityText);
        ToggleText(ownerEmpire);
        ToggleText(resourcesText);
        ToggleText(modifiersText);
    }

    private void ToggleText(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) return;

        textComponent.gameObject.SetActive(!string.IsNullOrEmpty(textComponent.text));
    }

}