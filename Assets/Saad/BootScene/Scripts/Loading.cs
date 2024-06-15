using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System.Threading;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
    [SerializeField] private float Duration = 2f;
    [SerializeField] private Slider Slider;
    [SerializeField] private Float LoadValue;

    AsyncOperation _asyncLoad;

    private void Start()
    {
        StartCoroutine(Load());
        StartCoroutine(PreloadScene());
    }
    private IEnumerator Load()
    {
        float _value = 0;
        DOTween.To(() => _value, x => _value = x, 1, Duration)
            .OnUpdate(() => {
                Slider.value = _value;
                LoadValue.SetValue(_value);
            }).OnComplete(() => {
                ActivateGameScene();
            });
        yield return null;
    }
    private void ActivateGameScene()
    {
        print("Activate called! ");
        if (_asyncLoad != null)
        {
            _asyncLoad.allowSceneActivation = true;
        }
            
    }

    IEnumerator PreloadScene()
    {
        _asyncLoad = SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Additive);
        _asyncLoad.allowSceneActivation = false;
        while (!_asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
