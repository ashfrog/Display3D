using UnityEngine;
using UnityEngine.Events;

public class EnableInit : MonoBehaviour
{
    public UnityEvent unityEvent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        unityEvent?.Invoke();
    }
}
