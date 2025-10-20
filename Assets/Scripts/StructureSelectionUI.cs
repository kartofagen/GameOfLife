using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class StructureSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject structureButtonPrefab;
    [SerializeField] private Transform structureButtonContainer;
    [SerializeField] private TextMeshProUGUI currentStructureText;
    [SerializeField] private GameObject structurePanel;

    private GridManager gridManager;
    private List<Button> structureButtons = new List<Button>();

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        CreateStructureButtons();
        UpdateCurrentStructureText();
    }

    private void CreateStructureButtons()
    {
        // Clear existing buttons
        foreach (Transform child in structureButtonContainer)
        {
            Destroy(child.gameObject);
        }
        structureButtons.Clear();

        // Create buttons for each structure
        for (int i = 0; i < gridManager.AvailableStructures.Count; i++)
        {
            StructureData structure = gridManager.AvailableStructures[i];
            GameObject buttonObj = Instantiate(structureButtonPrefab, structureButtonContainer);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            
            buttonText.text = $"{i + 1}. {structure.structureName}";
            
            int index = i; // Capture index for closure
            button.onClick.AddListener(() => SelectStructure(index));
            
            structureButtons.Add(button);
        }
    }

    private void SelectStructure(int index)
    {
        gridManager.StartStructurePlacement(gridManager.AvailableStructures[index]);
        UpdateCurrentStructureText();
    }

    private void UpdateCurrentStructureText()
    {
        if (gridManager.IsPlacingStructure)
        {
            StructureData currentStructure = gridManager.AvailableStructures[GetCurrentStructureIndex()];
            currentStructureText.text = $"Placing: {currentStructure.structureName}\nPress R to rotate, Right Click to cancel";
        }
        else
        {
            currentStructureText.text = "No structure selected";
        }
    }

    private int GetCurrentStructureIndex()
    {
        // This would need to be tracked in GridManager
        // For simplicity, we'll just find it
        return 0; // You'd need to implement proper tracking
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            structurePanel.SetActive(!structurePanel.activeSelf);
        }

        if (gridManager.IsPlacingStructure)
        {
            UpdateCurrentStructureText();
        }
    }
}