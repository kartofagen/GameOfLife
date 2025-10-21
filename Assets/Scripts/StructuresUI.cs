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
    private TournamentManager tournamentManager;

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (isInitialized) return;

        gridManager = GetComponent<GridManager>();
        tournamentManager = GetComponent<TournamentManager>();

        cellsModeToggle.onValueChanged.AddListener(OnCellsModeToggled);
        zonesModeToggle.onValueChanged.AddListener(OnZonesModeToggled);
        
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

        if (gridManager.IsPlacingStructure)
            gridManager.CancelStructurePlacement();
            
        if (gridManager.IsPlacingZoneStructure)
            gridManager.CancelZoneStructurePlacement();

        ClearButtons();
    }

    private void CreateStructureButtons()
    {
        if (!isInitialized || structureButtonPrefab == null) return;

        for (int i = 0; i < gridManager.AvailableStructures.Count; ++i)
        {
            StructureData structure = gridManager.AvailableStructures[i];
            if (structure == null) continue;

            GameObject buttonObj = Instantiate(structureButtonPrefab, structuresContainer);
            if (buttonObj == null) continue;
            
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = structure.structureName;
            }
            
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                int index = i;
                button.onClick.AddListener(() => SelectStructure(index));
            }
            
            currentButtons.Add(buttonObj);
        }
    }

    private void CreateZoneButtons()
    {
        if (!isInitialized || zoneButtonPrefab == null) return;

        for (int i = 0; i < gridManager.AvailableZoneStructures.Count; i++)
        {
            ZoneStructureData zoneStructure = gridManager.AvailableZoneStructures[i];
            if (zoneStructure == null) continue;

            GameObject buttonObj = Instantiate(zoneButtonPrefab, structuresContainer);
            if (buttonObj == null) continue;
            
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = zoneStructure.structureName;
            }
            
            Image buttonImage = buttonObj.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = new Color(zoneStructure.zoneColor.r, zoneStructure.zoneColor.g, zoneStructure.zoneColor.b, 0.3f);
            }
            
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

        if (gridManager.IsPlacingStructure || gridManager.IsPlacingZoneStructure)
        {
            UpdateUI();
        }
    }

    private void OnDestroy()
    {
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