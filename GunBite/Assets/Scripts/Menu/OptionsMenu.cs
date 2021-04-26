using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using System.Collections.Generic;

public class OptionsMenu : MonoBehaviour
{
	[Header("MIXER")]
	public AudioMixer masterMixer;

	[Header("Panels")]
	public GameObject panelControls;
	public GameObject panelVideo;
	public GameObject panelGame;
	public GameObject panelAudio;

	[Header("GAME SETTINGS")]
	public Toggle showFpsToggle;
	public Toggle extraSoundsToggle;

	[Header("VIDEO SETTINGS")]
	public Toggle fullscreenToggle;
	public Toggle vsyncToggle;
	public TMP_Dropdown resolutionDropdown;
	public TMP_Dropdown qualityDropdown;

	Resolution[] resolutions;
	List<string> Options = new List<string>();

	[Header("AUDIO SETTINGS")]
	public GameObject musicSlider;
	public GameObject sfxSlider;
	public GameObject uiSlider;

	[Header("CONTROLS SETTINGS")]
	public Slider mouseSensitivitySlider;
	public TMP_InputField mouseSensitivityInputField;

    private void Awake()
    {
        if (PlayerPrefs.GetInt("FirstRun") == 0)
        {
			PlayerPrefs.SetInt("FirstRun", 1);
			PlayerPrefs.SetInt("FPS", 0);
			PlayerPrefs.SetInt("Extra", 0);
		}
    }

    public void Start()
	{
		resolutions = Screen.resolutions;

		int currentResolutionIndex = 0;
		int x = 0;

		int lw = -1;
		int lh = -1;

		foreach (var res in resolutions)
		{
			if (lw != res.width || lh != res.height)
			{
				string option = res.width + " x " + res.height;
				Options.Add(option);
				lw = res.width;
				lh = res.height;

				if (lw == Screen.currentResolution.width &&
				lh == Screen.currentResolution.height)
				{
					currentResolutionIndex = x;
				}

				x++;
			}
		}

		resolutionDropdown.ClearOptions();
		resolutionDropdown.AddOptions(Options);
		resolutionDropdown.value = currentResolutionIndex;

        #region PlayerPrefs

        //check resolution
        if (PlayerPrefs.HasKey("Resolution"))
		{
			int resolutionIndex = PlayerPrefs.GetInt("Resolution");
			string resolution = Options[resolutionIndex];
			int ind = resolution.IndexOf('x');
			int width = int.Parse(resolution.Substring(0, ind - 1));
			int height = int.Parse(resolution.Substring(ind + 1));
			Screen.SetResolution(width, height, Screen.fullScreen);
			resolutionDropdown.value = resolutionIndex;
		}

		// check slider values
		mouseSensitivitySlider.value = PlayerPrefs.GetFloat("mouseSensitivity");

		// check full screen
		if (Screen.fullScreen == true)
		{
			fullscreenToggle.isOn = true;
		}
		else if (Screen.fullScreen == false)
		{
			fullscreenToggle.isOn = false;
		}

		//check quality 
		if (PlayerPrefs.HasKey("Quality"))
		{
			int qualityIndex;
			qualityIndex = PlayerPrefs.GetInt("Quality");
			qualityDropdown.value = qualityIndex;
		}

		if (PlayerPrefs.HasKey("Music"))
		{
			float volume;
			volume = PlayerPrefs.GetFloat("Music");
			masterMixer.SetFloat("musicVolume", volume);
			musicSlider.GetComponent<Slider>().value = volume;
		}

		if (PlayerPrefs.HasKey("SFX"))
		{
			float volume;
			volume = PlayerPrefs.GetFloat("SFX");
			masterMixer.SetFloat("sfxVolume", volume);
			sfxSlider.GetComponent<Slider>().value = volume;
		}

		if (PlayerPrefs.HasKey("UI"))
		{
			float volume;
			volume = PlayerPrefs.GetFloat("UI");
			masterMixer.SetFloat("uiVolume", volume);
			uiSlider.GetComponent<Slider>().value = volume;
		}

		//check fps value
		if (PlayerPrefs.GetInt("FPS") == 0)
		{
			showFpsToggle.SetIsOnWithoutNotify(false);
		}
		else
		{
			showFpsToggle.SetIsOnWithoutNotify(true);
		}

		//check extra sounds
		if (PlayerPrefs.GetInt("Extra") == 0)
		{
			extraSoundsToggle.SetIsOnWithoutNotify(false);
		}
		else
		{
			extraSoundsToggle.SetIsOnWithoutNotify(true);
		}

		// check vsync
		if (QualitySettings.vSyncCount == 0)
		{
			fullscreenToggle.SetIsOnWithoutNotify(false);
		}
		else if (QualitySettings.vSyncCount == 1)
		{
			fullscreenToggle.SetIsOnWithoutNotify(true);
		}
        #endregion
    }

    #region Panels
    public void GamePanel()
	{
		panelControls.SetActive(false);
		panelVideo.SetActive(false);
		panelGame.SetActive(true);
		panelAudio.SetActive(false);
	}

	public void VideoPanel()
	{
		panelControls.SetActive(false);
		panelVideo.SetActive(true);
		panelGame.SetActive(false);
		panelAudio.SetActive(false);
	}

	public void AudioPanel()
	{
		panelControls.SetActive(false);
		panelVideo.SetActive(false);
		panelGame.SetActive(false);
		panelAudio.SetActive(true);
	}

	public void ControlsPanel()
	{
		panelControls.SetActive(true);
		panelVideo.SetActive(false);
		panelGame.SetActive(false);
		panelAudio.SetActive(false);
	}

	public void KeyBindingsPanel()
	{
		panelControls.SetActive(false);
		panelVideo.SetActive(false);
		panelGame.SetActive(false);
		panelAudio.SetActive(false);
	}
	#endregion

	#region PanelGame

	public void ShowFPS()
	{
		if (PlayerPrefs.GetInt("FPS") == 0)
		{
			PlayerPrefs.SetInt("FPS", 1);
			showFpsToggle.SetIsOnWithoutNotify(true);
		}
		else if (PlayerPrefs.GetInt("FPS") == 1)
		{
			PlayerPrefs.SetInt("FPS", 0);
			showFpsToggle.SetIsOnWithoutNotify(false);
		}
	}

	public void ExtraSounds()
    {
		if (PlayerPrefs.GetInt("Extra") == 0)
		{
			PlayerPrefs.SetInt("Extra", 1);
			extraSoundsToggle.SetIsOnWithoutNotify(true);
		}
		else if (PlayerPrefs.GetInt("Extra") == 1)
		{
			PlayerPrefs.SetInt("Extra", 0);
			extraSoundsToggle.SetIsOnWithoutNotify(false);
		}
	}


	#endregion

	#region PanelVideo

	public void FullScreen()
	{
		Screen.fullScreen = !Screen.fullScreen;

		fullscreenToggle.SetIsOnWithoutNotify(!Screen.fullScreen);
	}

	public void SetResolution(int resolutionIndex)
	{
		string resolution = Options[resolutionIndex];
		int ind = resolution.IndexOf('x');
		int width = int.Parse(resolution.Substring(0, ind - 1));
		int height = int.Parse(resolution.Substring(ind + 1));
		Screen.SetResolution(width, height, Screen.fullScreen);
		PlayerPrefs.SetInt("Resolution", resolutionIndex);
	}

	public void SetQuality(int qualityIndex)
	{
		QualitySettings.SetQualityLevel(qualityIndex);
		PlayerPrefs.SetInt("Quality", qualityIndex);
	}

	public void Vsync()
	{
		if (QualitySettings.vSyncCount == 0)
		{
			QualitySettings.vSyncCount = 1;
			vsyncToggle.SetIsOnWithoutNotify(true);
		}
		else if (QualitySettings.vSyncCount == 1)
		{
			QualitySettings.vSyncCount = 0;
			vsyncToggle.SetIsOnWithoutNotify(false);
		}
	}

	#endregion

	#region PanelAudio

	public void SetMusicVolume()
    {
		float volume = musicSlider.GetComponent<Slider>().value;
        masterMixer.SetFloat("musicVolume", Mathf.Log10(volume) * 20);
		PlayerPrefs.SetFloat("Music", volume);
    }

    public void SetSFXVolume()
    {
		float volume = sfxSlider.GetComponent<Slider>().value;
		masterMixer.SetFloat("sfxVolume", Mathf.Log10(volume) * 20);
		PlayerPrefs.SetFloat("SFX", volume);
    }

    public void SetUIVolume()
    {
		float volume = uiSlider.GetComponent<Slider>().value;
		masterMixer.SetFloat("uiVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("UI", volume);
    }

    #endregion

    #region PanelControls

    public void SetMouseSensitivity()
	{
		float mouseSensitivity = mouseSensitivitySlider.value;
		mouseSensitivityInputField.text = mouseSensitivity.ToString("0.##");
		PlayerPrefs.SetFloat("mouseSensitivity", mouseSensitivity);
	}

	public void SetMouseSensitivityInput()
	{
		float mouseSensitivity = float.Parse(mouseSensitivityInputField.text);
		mouseSensitivitySlider.value = mouseSensitivity;
		PlayerPrefs.SetFloat("mouseSensitivity", mouseSensitivity);
	}

	#endregion

	public void PlayClick()
	{
		SoundManager.Instance.PlayOneShot("Click");
	}

	public void PlayHover()
	{
		SoundManager.Instance.Play("Hover");
	}
}
