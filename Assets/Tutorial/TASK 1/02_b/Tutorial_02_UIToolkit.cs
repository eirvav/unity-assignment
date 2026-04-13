using System;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class Tutorial_02_UIToolkit : MonoBehaviour
{
    [Header("Scene")]
    public Camera targetCamera;
    public Color solidColour = new Color(0.12f, 0.18f, 0.24f);

    [Header("UI Toolkit")]
    public UIDocument uiDocument;

    [Header("Video")]
    public VideoPlayer videoPlayer;
    public AudioSource videoAudioSource;

    [Header("Standard vs 360 Presentation")]
    public RenderTexture StandardVideoRenderTexture;
    public RenderTexture _360VideoRenderTexture;
    public Material video360SkyboxMaterial;

    public VideoClip StandardVideoClip;
    public VideoClip _360VideoClip;

    [Header("Respawn")]
    public Transform SpawnPosition;
    public XROrigin VRPlayerOrigin;
    public InputAction RespawnAction;

    private VisualElement _videoPanel;
    private Image _standardVideoImage;
    private Button _playPauseButton;
    private Slider _volumeSlider;

    private Button _mode0Button;
    private Button _mode1Button;
    private Button _mode2Button;
    private Button _nextSceneButton;

    private int _mode;
    private bool _eventsRegistered;

    public SceneLoader NextSceneLoader;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (videoPlayer != null && videoAudioSource != null)
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.SetTargetAudioSource(0, videoAudioSource);
        }
    }

    private void OnEnable()
    {
        BindUI();
        RegisterUIEvents();

        if (RespawnAction != null)
        {
            RespawnAction.Enable();
            RespawnAction.performed += OnRespawnPerformed;
        }
    }

    private void Start()
    {
        if (_volumeSlider != null)
        {
            float initialVolume = videoAudioSource != null ? videoAudioSource.volume : _volumeSlider.value;
            _volumeSlider.SetValueWithoutNotify(initialVolume);
            SetVolume(initialVolume);
        }

        if (_standardVideoImage != null && StandardVideoRenderTexture != null)
            _standardVideoImage.image = StandardVideoRenderTexture;

        SetVideoMode(0);
    }

    private void OnDisable()
    {
        UnregisterUIEvents();

        if (RespawnAction != null)
        {
            RespawnAction.performed -= OnRespawnPerformed;
            RespawnAction.Disable();
        }
    }

    private void BindUI()
    {
        if (uiDocument == null)
        {
            Debug.LogError($"{nameof(Tutorial_02_UIToolkit)}: UIDocument is not assigned.");
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError($"{nameof(Tutorial_02_UIToolkit)}: rootVisualElement is null.");
            return;
        }

        _videoPanel = root.Q<VisualElement>("VideoPanel");
        _standardVideoImage = root.Q<Image>("StandardVideoImage");
        _playPauseButton = root.Q<Button>("PlayPauseButton");
        _volumeSlider = root.Q<Slider>("VolumeSlider");

        _mode0Button = root.Q<Button>("Mode0Button");
        _mode1Button = root.Q<Button>("Mode1Button");
        _mode2Button = root.Q<Button>("Mode2Button");

        _nextSceneButton = root.Q<Button>("NextSceneButton");

        if (_videoPanel == null) Debug.LogWarning("UI element not found: VideoPanel");
        if (_standardVideoImage == null) Debug.LogWarning("UI element not found: StandardVideoImage");
        if (_playPauseButton == null) Debug.LogWarning("UI element not found: PlayPauseButton");
        if (_volumeSlider == null) Debug.LogWarning("UI element not found: VolumeSlider");
        if (_mode0Button == null) Debug.LogWarning("UI element not found: Mode0Button");
        if (_mode1Button == null) Debug.LogWarning("UI element not found: Mode1Button");
        if (_mode2Button == null) Debug.LogWarning("UI element not found: Mode2Button");

        if (_nextSceneButton == null) Debug.LogWarning("UI element not found: NextSceneButton");

    }

    private void RegisterUIEvents()
    {
        if (_eventsRegistered)
            return;

        if (_playPauseButton != null)
            _playPauseButton.clicked += TogglePlayPause;

        if (_volumeSlider != null)
            _volumeSlider.RegisterValueChangedCallback(OnVolumeSliderChanged);

        if (_mode0Button != null)
            _mode0Button.clicked += OnMode0Clicked;

        if (_mode1Button != null)
            _mode1Button.clicked += OnMode1Clicked;

        if (_mode2Button != null)
            _mode2Button.clicked += OnMode2Clicked;

        if (_nextSceneButton != null)
            _nextSceneButton.clicked += OnNextSceneClicked;

        _eventsRegistered = true;
    }



    private void UnregisterUIEvents()
    {
        if (!_eventsRegistered)
            return;

        if (_playPauseButton != null)
            _playPauseButton.clicked -= TogglePlayPause;

        if (_volumeSlider != null)
            _volumeSlider.UnregisterValueChangedCallback(OnVolumeSliderChanged);

        if (_mode0Button != null)
            _mode0Button.clicked -= OnMode0Clicked;

        if (_mode1Button != null)
            _mode1Button.clicked -= OnMode1Clicked;

        if (_mode2Button != null)
            _mode2Button.clicked -= OnMode2Clicked;

        _eventsRegistered = false;
    }

    private void OnMode0Clicked()
    {
        SetVideoMode(0);
    }
    private void OnMode1Clicked()
    {
        SetVideoMode(1);
    }
    private void OnMode2Clicked()
    {
        SetVideoMode(2);
    }

    private void OnNextSceneClicked()
    {
        if (NextSceneLoader != null) NextSceneLoader.LoadNextScene();
    }

    private void OnVolumeSliderChanged(ChangeEvent<float> evt)
    {
        SetVolume(evt.newValue);
    }

    public void SetVideoMode(int mode)
    {
        _mode = mode;

        if (_mode == 0)
        {
            SetElementDisplay(_videoPanel, false);
            SetElementDisplay(_standardVideoImage, false);

            if (videoPlayer != null)
                videoPlayer.Pause();

            UpdatePlayPauseButtonText("Play");

            if (targetCamera != null)
            {
                targetCamera.clearFlags = CameraClearFlags.SolidColor;
                targetCamera.backgroundColor = solidColour;
            }
        }

        if (_mode == 1)
        {
            SetElementDisplay(_videoPanel, true);
            SetElementDisplay(_standardVideoImage, true);

            if (videoPlayer != null)
            {
                videoPlayer.Pause();
                videoPlayer.clip = StandardVideoClip;

                ResizeRenderTextureToClip(StandardVideoRenderTexture, videoPlayer.clip);

                videoPlayer.targetTexture = StandardVideoRenderTexture;

                if (_standardVideoImage != null)
                    _standardVideoImage.image = StandardVideoRenderTexture;
            }

            UpdatePlayPauseButtonText("Play");

            if (targetCamera != null)
            {
                targetCamera.clearFlags = CameraClearFlags.SolidColor;
                targetCamera.backgroundColor = solidColour;
            }
        }

        if (_mode == 2)
        {
            SetElementDisplay(_videoPanel, true);
            SetElementDisplay(_standardVideoImage, false);

            if (videoPlayer != null)
            {
                videoPlayer.Pause();
                videoPlayer.clip = _360VideoClip;
                videoPlayer.targetTexture = _360VideoRenderTexture;
            }

            if (video360SkyboxMaterial != null)
                RenderSettings.skybox = video360SkyboxMaterial;

            UpdatePlayPauseButtonText("Play");

            if (targetCamera != null)
                targetCamera.clearFlags = CameraClearFlags.Skybox;
        }
    }

    private void SetElementDisplay(VisualElement element, bool visible)
    {
        if (element != null)
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void UpdatePlayPauseButtonText(string text)
    {
        if (_playPauseButton != null)
            _playPauseButton.text = text;
    }

    private void ResizeRenderTextureToClip(RenderTexture rt, VideoClip clip)
    {
        if (rt == null || clip == null)
            return;

        int clipWidth = (int)clip.width;
        int clipHeight = (int)clip.height;

        if (clipWidth <= 0 || clipHeight <= 0)
            return;

        if (rt.width == clipWidth && rt.height == clipHeight)
            return;

        rt.Release();
        rt.width = clipWidth;
        rt.height = clipHeight;
        rt.Create();
    }

    public void TogglePlayPause()
    {
        if (videoPlayer == null)
            return;

        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            UpdatePlayPauseButtonText("Play");
        }
        else
        {
            videoPlayer.Play();
            UpdatePlayPauseButtonText("Pause");
        }
    }

    public void SetVolume(float value)
    {
        if (videoAudioSource != null)
            videoAudioSource.volume = value;
    }

    private void OnRespawnPerformed(InputAction.CallbackContext context)
    {
        Respawn();
    }

    [ContextMenu("Trigger Respawn")]
    public void Respawn()
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