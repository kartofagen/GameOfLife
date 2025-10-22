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

    private GridManager _gridManager;
    private List<GameObject> _currentButtons = new List<GameObject>();
    private bool _isInitialized = false;

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (_isInitialized) return;

        _gridManager = GetComponent<GridManager>();

        cellsModeToggle.onValueChanged.AddListener(OnCellsModeToggled);
        zonesModeToggle.onValueChanged.AddListener(OnZonesModeToggled);
        
        UpdateUI();
        
        _isInitialized = true;
    }

    private void OnCellsModeToggled(bool isOn)
    {
        if (isOn && _isInitialized)
        {
            SetMode();
            CreateStructureButtons();
            zonesModeToggle.isOn = false;
            UpdateUI();
        }
    }

    private void OnZonesModeToggled(bool isOn)
    {
        if (isOn && _isInitialized)
        {
            SetMode();
            CreateZoneButtons();
            cellsModeToggle.isOn = false;
            UpdateUI();
        }
    }

    private void SetMode()
    {
        if (!_isInitialized) return;

        if (_gridManager.IsPlacingStructure)
            _gridManager.CancelStructurePlacement();
            
        if (_gridManager.IsPlacingZoneStructure)
            _gridManager.CancelZoneStructurePlacement();

        ClearButtons();
    }

    private void CreateStructureButtons()
    {
        if (!_isInitialized || structureButtonPrefab == null) return;

        for (int i = 0; i < _gridManager.AvailableStructures.Count; ++i)
        {
            StructureData structure = _gridManager.AvailableStructures[i];
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
            
            _currentButtons.Add(buttonObj);
        }
    }

    private void CreateZoneButtons()
    {
        if (!_isInitialized || zoneButtonPrefab == null) return;

        for (int i = 0; i < _gridManager.AvailableZoneStructures.Count; ++i)
        {
            ZoneStructureData zoneStructure = _gridManager.AvailableZoneStructures[i];
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
                buttonImage.color = new Color(zoneStructure.zoneColor.r,
                                              zoneStructure.zoneColor.g,
                                              zoneStructure.zoneColor.b, 0.3f);
            }
            
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                int index = i;
                button.onClick.AddListener(() => SelectZoneStructure(index));
            }
            
            _currentButtons.Add(buttonObj);
        }
    }

    private void SelectStructure(int index)
    {
        if (!_isInitialized || index < 0 || index >= _gridManager.AvailableStructures.Count) return;

        _gridManager.StartStructurePlacement(_gridManager.AvailableStructures[index]);
        UpdateUI();
    }

    private void SelectZoneStructure(int index)
    {
        if (!_isInitialized || index < 0 || index >= _gridManager.AvailableZoneStructures.Count) return;

        _gridManager.StartZoneStructurePlacement(_gridManager.AvailableZoneStructures[index]);
        UpdateUI();
    }

    private void ClearButtons()
    {
        foreach (GameObject button in _currentButtons)
        {
            if (button != null)
                Destroy(button);
        }
        _currentButtons.Clear();
    }

    private void UpdateUI()
    {
        if (!_isInitialized) return;

        if (cellsModeToggle.isOn)
        {
            currentModeText.text = "CELLS MODE";
            
            if (_gridManager.IsPlacingStructure)
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
            
            if (_gridManager.IsPlacingZoneStructure)
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
        if (!_isInitialized) return;

        if (_gridManager.IsPlacingStructure || _gridManager.IsPlacingZoneStructure)
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
}