using UnityEngine;
using UnityEngine.UIElements;

public class VRMenuController : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;
    [SerializeField] Renderer demoRenderer;
    [SerializeField] Transform demoObject;
    [SerializeField] float spinSpeed = 45f;

    Label titleLabel;
    Label statusLabel;
    Label counterLabel;
    VisualElement colorPreview;

    readonly Color[] colors =
    {
        new Color(0.23f, 0.64f, 0.95f),
        new Color(0.95f, 0.74f, 0.20f),
        new Color(0.35f, 0.83f, 0.49f),
        new Color(0.93f, 0.36f, 0.36f)
    };

    int counter;
    int colorIndex;
    bool spinning;

    void Awake()
    {
        if (uiDocument == null)
        {
            Debug.LogWarning("UIDocument is missing.", this);
            return;
        }

        var root = uiDocument.rootVisualElement;
        titleLabel = root.Q<Label>("title-label");
        statusLabel = root.Q<Label>("status-label");
        counterLabel = root.Q<Label>("counter-label");
        colorPreview = root.Q<VisualElement>("color-preview");

        RefreshUI();
    }

    void Update()
    {
        if (spinning && demoObject != null)
            demoObject.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }

    public void AddOne()
    {
        counter++;
        RefreshUI();
    }

    public void NextColor()
    {
        colorIndex = (colorIndex + 1) % colors.Length;
        RefreshUI();
    }

    public void ToggleSpin()
    {
        spinning = !spinning;
        RefreshUI();
    }

    public void ResetDemo()
    {
        counter = 0;
        colorIndex = 0;
        spinning = false;

        if (demoObject != null)
            demoObject.rotation = Quaternion.identity;

        RefreshUI();
    }

    void RefreshUI()
    {
        var currentColor = colors[colorIndex];

        if (titleLabel != null)
            titleLabel.text = "Hello VR UI Toolkit";

        if (statusLabel != null)
            statusLabel.text = spinning ? "Status: spinning" : "Status: waiting";

        if (counterLabel != null)
            counterLabel.text = $"Counter: {counter}";

        if (colorPreview != null)
            colorPreview.style.backgroundColor = currentColor;

        if (demoRenderer != null)
            demoRenderer.material.color = currentColor;
    }
}
