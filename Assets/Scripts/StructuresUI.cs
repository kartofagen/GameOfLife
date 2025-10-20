// CombinedStructureUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class StructuresUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject structureButtonPrefab;
    [SerializeField] private GameObject zoneButtonPrefab;
    [SerializeField] private Transform structuresContainer;
    [SerializeField] private TextMeshProUGUI currentModeText;
    [SerializeField] private TextMeshProUGUI instructionText;

    [Header("Mode Toggles")]
    [SerializeField] private Toggle cellsModeToggle;
    [SerializeField] private Toggle zonesModeToggle;

    private GridManager gridManager;
    private List<GameObject> currentButtons = new List<GameObject>();
    private bool isInitialized = false;

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (isInitialized) return;

        gridManager = GetComponent<GridManager>();
        
        if (gridManager == null)
        {
            Debug.LogError("GridManager not found in scene!");
            return;
        }

        // Set up mode toggles
        cellsModeToggle.onValueChanged.AddListener(OnCellsModeToggled);
        zonesModeToggle.onValueChanged.AddListener(OnZonesModeToggled);
        
        // Start with cells mode
        cellsModeToggle.isOn = true;
        UpdateUI();
        
        isInitialized = true;
    }

    private void OnCellsModeToggled(bool isOn)
    {
        if (isOn && isInitialized)
        {
            SetMode(PlacementMode.Cells);
            CreateStructureButtons();
            zonesModeToggle.isOn = false;
            UpdateUI();
        }
    }

    private void OnZonesModeToggled(bool isOn)
    {
        if (isOn && isInitialized)
        {
            SetMode(PlacementMode.Zones);
            CreateZoneButtons();
            cellsModeToggle.isOn = false;
            UpdateUI();
        }
    }

    private void SetMode(PlacementMode mode)
    {
        if (!isInitialized) return;

        // Cancel any active placements
        if (gridManager.IsPlacingStructure)
            gridManager.CancelStructurePlacement();
            
        if (gridManager.IsPlacingZoneStructure)
            gridManager.CancelZoneStructurePlacement();

        // Clear existing buttons
        ClearButtons();
    }

    private void CreateStructureButtons()
    {
        if (!isInitialized || structureButtonPrefab == null) return;

        // Create buttons for each structure
        for (int i = 0; i < gridManager.AvailableStructures.Count; i++)
        {
            StructureData structure = gridManager.AvailableStructures[i];
            if (structure == null) continue;

            GameObject buttonObj = Instantiate(structureButtonPrefab, structuresContainer);
            if (buttonObj == null) continue;
            
            // Set up button text
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = structure.structureName;
            }
            
            // Set up button click
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                int index = i;
                button.onClick.AddListener(() => SelectStructure(index));
            }
            
            currentButtons.Add(buttonObj);
        }

        // Add "Free Drawing" button at the top
        GameObject freeDrawButton = Instantiate(structureButtonPrefab, structuresContainer);
        if (freeDrawButton != null)
        {
            freeDrawButton.transform.SetAsFirstSibling(); // Move to top
            
            TextMeshProUGUI buttonText = freeDrawButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Free Drawing";
            }
            
            Button button = freeDrawButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(SelectFreeDrawing);
            }
            
            // Highlight free drawing as default
            var colors = button.colors;
            colors.normalColor = new Color(0.8f, 0.9f, 1.0f);
            button.colors = colors;
            
            currentButtons.Add(freeDrawButton);
        }
    }

    private void CreateZoneButtons()
    {
        if (!isInitialized || zoneButtonPrefab == null) return;

        // Create buttons for each zone structure
        for (int i = 0; i < gridManager.AvailableZoneStructures.Count; i++)
        {
            ZoneStructureData zoneStructure = gridManager.AvailableZoneStructures[i];
            if (zoneStructure == null) continue;

            GameObject buttonObj = Instantiate(zoneButtonPrefab, structuresContainer);
            if (buttonObj == null) continue;
            
            // Set up button text
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = zoneStructure.structureName;
            }
            
            // Set up button color to match zone color
            Image buttonImage = buttonObj.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = new Color(zoneStructure.zoneColor.r, zoneStructure.zoneColor.g, zoneStructure.zoneColor.b, 0.3f);
            }
            
            // Set up button click
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                int index = i;
                button.onClick.AddListener(() => SelectZoneStructure(index));
            }
            
            currentButtons.Add(buttonObj);
        }
    }

    private void SelectStructure(int index)
    {
        if (!isInitialized || index < 0 || index >= gridManager.AvailableStructures.Count) return;

        gridManager.StartStructurePlacement(gridManager.AvailableStructures[index]);
        UpdateUI();
    }

    private void SelectZoneStructure(int index)
    {
        if (!isInitialized || index < 0 || index >= gridManager.AvailableZoneStructures.Count) return;

        gridManager.StartZoneStructurePlacement(gridManager.AvailableZoneStructures[index]);
        UpdateUI();
    }

    private void SelectFreeDrawing()
    {
        if (!isInitialized) return;

        // Cancel any structure placement to return to free drawing mode
        if (gridManager.IsPlacingStructure)
        {
            gridManager.CancelStructurePlacement();
        }
        UpdateUI();
    }

    private void ClearButtons()
    {
        foreach (GameObject button in currentButtons)
        {
            if (button != null)
                Destroy(button);
        }
        currentButtons.Clear();
    }

    private void UpdateUI()
    {
        if (!isInitialized) return;

        if (cellsModeToggle.isOn)
        {
            currentModeText.text = "CELLS MODE";
            
            if (gridManager.IsPlacingStructure)
            {
                instructionText.text = "Placing Structure\n• LMB: Place\n• RMB: Cancel\n• R: Rotate";
            }
            else
            {
                instructionText.text = "Free Drawing\n• LMB: Create cells\n• RMB: Remove cells\n• Select pattern below";
            }
        }
        else if (zonesModeToggle.isOn)
        {
            currentModeText.text = "ZONES MODE";
            
            if (gridManager.IsPlacingZoneStructure)
            {
                instructionText.text = "Placing Zone\n• LMB: Place\n• RMB: Cancel";
            }
            else
            {
                instructionText.text = "Zone Placement\n• Select zone type below\n• Each zone has custom rules";
            }
        }
    }

    private void Update()
    {
        if (!isInitialized) return;

        // Update UI in real-time based on placement state
        if (gridManager.IsPlacingStructure || gridManager.IsPlacingZoneStructure)
        {
            UpdateUI();
        }

        // Quick mode switching with number keys
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            cellsModeToggle.isOn = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            zonesModeToggle.isOn = true;
        }

        // Escape key to cancel placement
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gridManager.IsPlacingStructure)
            {
                gridManager.CancelStructurePlacement();
                UpdateUI();
            }
            else if (gridManager.IsPlacingZoneStructure)
            {
                gridManager.CancelZoneStructurePlacement();
                UpdateUI();
            }
        }
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (cellsModeToggle != null)
            cellsModeToggle.onValueChanged.RemoveListener(OnCellsModeToggled);
        if (zonesModeToggle != null)
            zonesModeToggle.onValueChanged.RemoveListener(OnZonesModeToggled);
    }

    public enum PlacementMode
    {
        Cells,
        Zones
    }
}