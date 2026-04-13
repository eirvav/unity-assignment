
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CustomToggleButton : MonoBehaviour
{
    private bool _value = false;

    public Text TextObject;
    public string Text_whenInactive;
    public string Text_whenActive;

    public UnityEvent<bool> OnToggleButtonValue;

    void OnEnable()
    {
        SetTextObjectFromValue();
    }
    void SetTextObjectFromValue()
    {
        if (_value) TextObject.text = Text_whenActive;
        else TextObject.text = Text_whenInactive;
    }
    public void ToggleButtonValue()
    {
        _value = !_value;
        SetTextObjectFromValue();
        OnToggleButtonValue?.Invoke(_value);
    }
}