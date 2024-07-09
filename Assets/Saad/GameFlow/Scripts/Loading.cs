using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using ProjectCore.Variables;

namespace ProjectCore.Application
{
    public class Loading : MonoBehaviour
    {
        [SerializeField] private float Duration = 2f;
        [SerializeField] private Slider Slider;
        [SerializeField] private Float LoadValue;

        private void Start()
        {
            StartCoroutine(Load());
        }
        private IEnumerator Load()
        {
            float _value = 0;
            DOTween.To(() => _value, x => _value = x, 1, Duration)
                .OnUpdate(() =>
                {
                    Slider.value = _value;
                    LoadValue.SetValue(_value);
                }).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
            yield return null;
        }

    }
}
