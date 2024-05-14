using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class LoadingBar : MonoBehaviour
{
    [SerializeField]
    Image loadingBar;

    [SerializeField]
    UnityEvent OnFinishedLoading = new UnityEvent();

    [SerializeField]
    Loadable loadableObject;

    private void Start()
    {
        loadableObject.OnValueChanged.AddListener((i) => 
        {
            loadingBar.fillAmount = i;

            if (1.0f == i)
            {
                OnFinishedLoading.Invoke();
            }
        });        
    }
}
