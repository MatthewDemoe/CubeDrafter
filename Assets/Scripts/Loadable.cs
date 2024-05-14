using UnityEngine;
using UnityEngine.Events;

public class Loadable : MonoBehaviour
{
    public UnityEvent<float> OnValueChanged = new UnityEvent<float>();
}
