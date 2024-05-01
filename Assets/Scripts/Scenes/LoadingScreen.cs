using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField]
    Image loadingBar;

    [SerializeField]
    UnityEvent OnFinishedLoading = new UnityEvent();

    void Start()
    {
        StartCoroutine(StartDraft());
    }

    private IEnumerator StartDraft()
    {
        float timer = 0.0f;

        while (timer < 5.0f)
        {
            yield return new WaitForEndOfFrame();

            timer += Time.deltaTime;
            loadingBar.fillAmount = UtilMath.Lmap(timer, 0.0f, 5.0f, 0.0f, 1.0f);
        }

        OnFinishedLoading.Invoke();
    }
}
