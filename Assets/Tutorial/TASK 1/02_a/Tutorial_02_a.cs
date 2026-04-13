using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

public class Tutorial_02_a : MonoBehaviour
{
        public Camera targetCamera;
    public Color solidColour = new Color(0.12f, 0.18f, 0.24f);

    // Video UI (only visible when Video selected)
    public GameObject VideoPanel;

    public Text PlayPauseButtonText;
    [Tooltip("Here we assign the SetVolume callback by script. It allows us to apply the slider value as initial volume value")]
    public Slider VolumeSlider;

    // Video
    public VideoPlayer videoPlayer;
    public AudioSource videoAudioSource;

    // Standard vs 360 presentation
    public RenderTexture StandardVideoRenderTexture;
    public RenderTexture _360VideoRenderTexture;
    public GameObject StandardVideoScreen;     // a Quad/Plane in the scene
    public Material video360SkyboxMaterial;    // skybox material that uses _MainTex

    public VideoClip StandardVideoClip;
    public VideoClip _360VideoClip;



    public Transform SpawnPosition;
    public XROrigin VRPlayerOrigin;
    public InputAction RespawnAction;






    void Awake()
    {
        targetCamera = Camera.main;

        VolumeSlider.onValueChanged.AddListener(SetVolume);

        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, videoAudioSource);

    }

    void Start()
    {
        SetVolume(VolumeSlider.value);
    }

    private int _mode;
    public void SetVideoMode(int mode)
    {
        _mode = mode;
        // hide/show the extra video UI

        if (_mode == 0)
        {
            VideoPanel.SetActive(false);

            videoPlayer.Pause();
            StandardVideoScreen.SetActive(false);

            targetCamera.clearFlags = CameraClearFlags.SolidColor;
            targetCamera.backgroundColor = solidColour;
        }

        if (_mode == 1)
        {
            VideoPanel.SetActive(true);

            videoPlayer.Pause();
            PlayPauseButtonText.text = "Play";

            videoPlayer.clip = StandardVideoClip;
            ResizeRenderTextureToClip(StandardVideoRenderTexture, videoPlayer.clip);
            StandardVideoScreen.SetActive(true);

            videoPlayer.targetTexture = StandardVideoRenderTexture;
            // videoPlayer.Play();

            targetCamera.clearFlags = CameraClearFlags.SolidColor;
            targetCamera.backgroundColor = solidColour;
        }

        if (_mode == 2)
        {
            VideoPanel.SetActive(true);

            videoPlayer.Pause();
            PlayPauseButtonText.text = "Play";

            videoPlayer.clip = _360VideoClip;
            videoPlayer.targetTexture = _360VideoRenderTexture;
            StandardVideoScreen.SetActive(false);
            RenderSettings.skybox = video360SkyboxMaterial;
            targetCamera.clearFlags = CameraClearFlags.Skybox;
            // videoPlayer.Play();
        }
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
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            PlayPauseButtonText.text = "Play";
        }
        else
        {
            videoPlayer.Play();
            PlayPauseButtonText.text = "Pause";
        }
    }

    public void SetVolume(float value)
    {
        videoAudioSource.volume = value;
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
