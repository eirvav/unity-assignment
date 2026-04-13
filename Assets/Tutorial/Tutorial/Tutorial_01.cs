using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

public class Tutorial_01 : MonoBehaviour
{

    public GameObject LightGameObject;

    public Camera targetCamera;
    public Color solidColour = new Color(0.12f, 0.18f, 0.24f);


    public Text PlayPauseButtonText;
    [Tooltip("Here we assign the SetVolume callback by script. It allows us to apply the slider value as initial volume value")]
    public Slider VolumeSlider;
    public AudioSource audioSource;

    public Material SkyboxMaterial;    // skybox material that uses _MainTex

    private bool _is360Image = false;

    public Transform SpawnPosition;
    public XROrigin VRPlayerOrigin;
    public InputAction RespawnAction;

    void Awake()
    {
        targetCamera = Camera.main;
        VolumeSlider.onValueChanged.AddListener(SetVolume);
    }

    void Start()
    {
        SetVolume(VolumeSlider.value);
    }

    public void ToggleLight()
    {
        LightGameObject.SetActive(!LightGameObject.activeSelf);
    }

    public void TogleBackgroundMode()
    {

        if (_is360Image)
        {
            targetCamera.clearFlags = CameraClearFlags.SolidColor;
            targetCamera.backgroundColor = solidColour;
            _is360Image = false;
        }
        else
        {
            RenderSettings.skybox = SkyboxMaterial;
            targetCamera.clearFlags = CameraClearFlags.Skybox;
            _is360Image = true;
        }

    }

    public void TogglePlayPause()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            PlayPauseButtonText.text = ">";
        }
        else
        {
            audioSource.Play();
            PlayPauseButtonText.text = "II";
        }

    }

    public List<AudioClip> AudioClips;
    public void SelectAudioClip(int index)
    {
        bool audioWasPlaying = audioSource.isPlaying;
        audioSource.Pause();
        audioSource.clip = AudioClips[index];
        if (audioWasPlaying) audioSource.Play();
    }

    public void SetVolume(float value)
    {
        audioSource.volume = value;
    }
    public void SetLooping(bool value)
    {
        audioSource.loop = value;
    }



    void OnEnable()
    {
        if (RespawnAction != null)
        {
            RespawnAction.Enable();
            RespawnAction.performed += OnRespawnPerformed;
        }
    }

    void OnDisable()
    {
        if (RespawnAction != null)
        {
            RespawnAction.Disable();
            RespawnAction.performed -= OnRespawnPerformed;
        }
    }

    private void OnRespawnPerformed(InputAction.CallbackContext context)
    {
        Respawn();
    }


    [ContextMenu("Trigger Respawn")]
    void Respawn()
    {
        if (VRPlayerOrigin != null)
        {
            VRPlayerOrigin.transform.position = SpawnPosition != null ? SpawnPosition.position : Vector3.zero;
            VRPlayerOrigin.transform.rotation = SpawnPosition != null ? SpawnPosition.rotation : Quaternion.identity;
        }
        else
        {
            Debug.LogError("VRPlayerOrigin is not assigned.");
        }
    }


}