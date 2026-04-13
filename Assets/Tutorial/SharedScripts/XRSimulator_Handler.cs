

using UnityEngine;

public class XRSimulator_Handler : MonoBehaviour
{
    public GameObject SimulatorGameObject;
    void OnEnable()
    {
#if UNITY_EDITOR
        SimulatorGameObject.SetActive(true);
#endif
    }
}
