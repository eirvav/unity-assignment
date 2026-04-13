using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Tutorial_03_Video : MonoBehaviour
{
    private const string VideoDropdownPlaceholderText = "Click to Select Video";

    public Text PlayPauseButtonText;
    public Slider VolumeSlider;
    public Slider SeekSlider;
    public Text SeekSliderValueText;
    public Dropdown VideoClipDropdown;

    public VideoPlayer videoPlayer;
    public AudioSource videoAudioSource;
    public RawImage VideoScreen;
    public RenderTexture VideoRenderTexture;
    public List<VideoClip> VideoClips;

    private bool _isUpdatingSeekSlider;

    void Awake()
    {
        VideoScreen = ResolveVideoScreen(VideoScreen);

        if (VolumeSlider != null)
            VolumeSlider.onValueChanged.AddListener(SetVolume);

        if (SeekSlider != null)
        {
            SeekSlider.minValue = 0f;
            SeekSlider.maxValue = 1f;
            SeekSlider.wholeNumbers = false;
            SeekSlider.onValueChanged.AddListener(SetSeekPosition);
        }

        if (VideoClipDropdown != null)
            VideoClipDropdown.onValueChanged.AddListener(SelectVideoClip);

        if (videoPlayer != null)
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            if (videoAudioSource != null)
                videoPlayer.SetTargetAudioSource(0, videoAudioSource);

            if (VideoRenderTexture != null)
                videoPlayer.targetTexture = VideoRenderTexture;
        }

        if (VideoScreen != null && VideoRenderTexture != null)
        {
            if (VideoScreen.color == Color.black)
                VideoScreen.color = Color.white;

            VideoScreen.texture = VideoRenderTexture;
        }
    }

    void Start()
    {
        if (VolumeSlider != null)
            SetVolume(VolumeSlider.value);

        PopulateVideoClipDropdown();

        if (VideoClips != null && VideoClips.Count > 0)
        {
            SelectVideoClip(0);
            UpdateVideoDropdownPlaceholder();
        }
        else
            ResetSeekSlider();
    }

    void Update()
    {
        RefreshSeekSlider();
    }

    public void TogglePlayPause()
    {
        if (videoPlayer == null)
            return;

        if (videoPlayer.clip == null && VideoClips != null && VideoClips.Count > 0)
            SetVideoClip(0, false);

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

    public void SetSeekPosition(float value)
    {
        if (_isUpdatingSeekSlider || videoPlayer == null || !videoPlayer.canSetTime)
            return;

        double clipLength = videoPlayer.length;
        if (clipLength <= 0d)
            return;

        double targetTime = Mathf.Clamp01(value) * (float)clipLength;
        videoPlayer.time = targetTime;
        UpdateSeekSliderValueText(targetTime, clipLength);
    }

    public void SelectVideoClip(int index)
    {
        SetVideoClip(index, true);
    }

    private void SetVideoClip(int index, bool resumePlayback)
    {
        if (videoPlayer == null || VideoClips == null || index < 0 || index >= VideoClips.Count)
            return;

        bool wasPlaying = videoPlayer.isPlaying;
        videoPlayer.Pause();

        videoPlayer.clip = VideoClips[index];
        ResizeRenderTextureToClip(VideoRenderTexture, videoPlayer.clip);
        videoPlayer.targetTexture = VideoRenderTexture;

        if (VideoScreen != null)
            VideoScreen.texture = VideoRenderTexture;

        if (VideoClipDropdown != null)
            VideoClipDropdown.SetValueWithoutNotify(index);

        videoPlayer.Prepare();
        ResetSeekSlider();
        UpdatePlayPauseButtonText("Play");

        if (resumePlayback && wasPlaying)
        {
            videoPlayer.Play();
            UpdatePlayPauseButtonText("Pause");
        }
    }

    private void PopulateVideoClipDropdown()
    {
        if (VideoClipDropdown == null)
            return;

        VideoClipDropdown.ClearOptions();

        var options = new List<string>();
        if (VideoClips != null)
        {
            foreach (var clip in VideoClips)
            {
                if (clip != null)
                    options.Add(clip.name.Replace("_", " "));
            }
        }

        VideoClipDropdown.AddOptions(options);
        VideoClipDropdown.SetValueWithoutNotify(0);
        VideoClipDropdown.RefreshShownValue();
        UpdateVideoDropdownPlaceholder();
    }

    private void UpdateVideoDropdownPlaceholder()
    {
        if (VideoClipDropdown != null && VideoClipDropdown.captionText != null)
            VideoClipDropdown.captionText.text = VideoDropdownPlaceholderText;
    }

    private void RefreshSeekSlider()
    {
        if (SeekSlider == null || videoPlayer == null || videoPlayer.clip == null)
            return;

        double clipLength = videoPlayer.length;
        if (clipLength <= 0d)
        {
            UpdateSeekSliderValueText(videoPlayer.time, 0d);
            return;
        }

        _isUpdatingSeekSlider = true;
        SeekSlider.SetValueWithoutNotify((float)(videoPlayer.time / clipLength));
        _isUpdatingSeekSlider = false;

        UpdateSeekSliderValueText(videoPlayer.time, clipLength);
    }

    private void ResetSeekSlider()
    {
        if (SeekSlider != null)
        {
            _isUpdatingSeekSlider = true;
            SeekSlider.SetValueWithoutNotify(0f);
            _isUpdatingSeekSlider = false;
        }

        double clipLength = videoPlayer != null ? videoPlayer.length : 0d;
        UpdateSeekSliderValueText(0d, clipLength);
    }

    private void UpdateSeekSliderValueText(double currentTime, double clipLength)
    {
        if (SeekSliderValueText == null)
            return;

        SeekSliderValueText.text = $"{FormatTime(currentTime)}\n{FormatTime(clipLength)}";
    }

    private void UpdatePlayPauseButtonText(string text)
    {
        if (PlayPauseButtonText != null)
            PlayPauseButtonText.text = text;
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

    private RawImage ResolveVideoScreen(RawImage assignedScreen)
    {
        if (assignedScreen == null)
            return null;

        var candidateScreens = assignedScreen.GetComponentsInChildren<RawImage>(true);
        foreach (var candidate in candidateScreens)
        {
            if (candidate != assignedScreen)
                return candidate;
        }

        return assignedScreen;
    }

    private string FormatTime(double timeInSeconds)
    {
        if (timeInSeconds <= 0d)
            return "00:00";

        int totalSeconds = Mathf.FloorToInt((float)timeInSeconds);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes:00}:{seconds:00}";
    }
}
