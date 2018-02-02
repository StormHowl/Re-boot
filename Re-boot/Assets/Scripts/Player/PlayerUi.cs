using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Class which entirely manages the Ui. Because we won't have more items concerning Ui, this one is sufficiant.
/// Gathers every Ui element which needs to change : Health, Cartridge and Rewind. (Cooldown = Rewind)
/// </summary>
public class PlayerUi : MonoBehaviour {

    [Header("Health elements")]
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Text _currentHealthText;
    [SerializeField] private Text _maxHealthText;
    [SerializeField] private Text _teamRemainingLives;

    [Header("Rewind elements")]
    [SerializeField] private Image _cooldownImage;
    [SerializeField] private Text _cooldownText;
    private float _initialCooldownValue;
    private float _currentCooldownValue;

    [Header("Cartridge elements")]
    [SerializeField] private Text _currentCartridgeText;
    [SerializeField] private Text _maxCartridgeText;

    [Header("Bricks Inventory")]
    [SerializeField] private Text _currentAmountText;
    [SerializeField] private Text _maxAmountText;

    [Header("Sets")] 
    [SerializeField] private GameObject _inGameUiSet;
    [SerializeField] private GameObject _menuUiSet;
    [SerializeField] private GameObject _cooldownOverlay;


    void Start()
    {
        _cooldownText.enabled = false;
    }

    public void SetMaxHealthValue(float value)
    {
        _maxHealthText.text = "/ " + (int)value;
        _healthSlider.maxValue = value;
        SetCurrentHealthValue(value);
    }

    public void SetCurrentHealthValue(float value)
    {
        _currentHealthText.text = ((int)value).ToString();
        _healthSlider.value = value;
    }

    public void UpdateCooldownValue(float value)
    {
        _currentCooldownValue -= value * _initialCooldownValue;
        _cooldownImage.fillAmount -= value;
        _cooldownText.text = Mathf.CeilToInt(_currentCooldownValue).ToString();

        if (_currentCooldownValue <= 0f)
        {
            _cooldownImage.fillAmount = 0;
            _cooldownText.enabled = false;
            
        }
    }

    public void SetCooldownUsed(float cooldown)
    {
        _cooldownImage.fillAmount = 1;
        _cooldownText.enabled = true;
        _cooldownText.text = Mathf.CeilToInt(cooldown).ToString();
        _initialCooldownValue = cooldown;
        _currentCooldownValue = cooldown;
    }

    public void SetMaxCartridgeClipContent(int value)
    {
        _maxCartridgeText.text = "/ " + value;
    }

    public void SetCurrentCartridgeClipContent(int value)
    {
        _currentCartridgeText.text = value.ToString();
    }

    public void SetMaxAmountClipContent(int value)
    {
        _maxAmountText.text = "/ " + value;
    }

    public void SetCurrentAmountClipContent(int value)
    {
        _currentAmountText.text = value.ToString();
    }

    public void SetMenuPlayerReference(PlayerSetup player)
    {
        _menuUiSet.GetComponent<SelectionMenu>().SetPlayer(player);
    }

    public void SetTeamRemainingLives(int value)
    {
        _teamRemainingLives.text = "x " + value;
    }

    /// <summary>
    /// To activate class selection Ui and so make in game Ui not visible anymore.
    /// </summary>
    public void ActivateClassSelectionUi()
    {
        _inGameUiSet.SetActive(false);
        _menuUiSet.SetActive(true);
    }

    /// <summary>
    /// To make the InGame Ui visible and the Class Selection menu invisible.
    /// </summary>
    public void ActivateInGameUi()
    {
        _menuUiSet.SetActive(false);
        _inGameUiSet.SetActive(true);
    }

    public void SetRewindOverlay(bool state)
    {
        _cooldownOverlay.SetActive(state);
    }

    public void WaypointsVisibilityToggled(bool visible)
    {
        NGameManager.Instance.ToggleWaypointsVisibility(visible);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
