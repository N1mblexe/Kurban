using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; 
using DG.Tweening;

public class TimeController : MonoBehaviour
{
    public static TimeController Instance { get; private set; }

    [Header("Post Process Referansları")]
    [SerializeField] private Volume globalVolume;
    private Vignette vignette;
    private LensDistortion lensDistortion;

    [Header("Efekt Ayarları")]
    [SerializeField] private float maxVignetteIntensity = 0.45f;
    [SerializeField] private float maxLensDistortionIntensity = -0.5f; 

    private float defaultFixedDeltaTime;
    private Sequence timeSequence;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            defaultFixedDeltaTime = Time.fixedDeltaTime;
            DontDestroyOnLoad(gameObject);
            
            if (globalVolume.profile.TryGet(out vignette) && globalVolume.profile.TryGet(out lensDistortion))
            {
                vignette.intensity.value = 0f;
                lensDistortion.intensity.value = 0f;
            }
        }
        else { Destroy(gameObject); }
    }

    public void ModifyTimeWithEffects(float targetScale, float duration, float transition = 0.2f)
    {
        timeSequence?.Kill();
        timeSequence = DOTween.Sequence().SetUpdate(true);

        timeSequence.Append(DOTween.To(() => Time.timeScale, x => UpdateEverything(x), targetScale, transition).SetEase(Ease.OutExpo));
        
        timeSequence.AppendInterval(duration);

        timeSequence.Append(DOTween.To(() => Time.timeScale, x => UpdateEverything(x), 1f, transition).SetEase(Ease.InQuad));
    }

    private void UpdateEverything(float currentScale)
    {
        Time.timeScale = currentScale;
        Time.fixedDeltaTime = defaultFixedDeltaTime * currentScale;

        float lerpPct = 1f - currentScale; 

        if (vignette != null)
            vignette.intensity.value = Mathf.Lerp(0f, maxVignetteIntensity, lerpPct);
        
        if (lensDistortion != null)
            lensDistortion.intensity.value = Mathf.Lerp(0f, maxLensDistortionIntensity, lerpPct);
    }

    public void ResetInstant()
    {
        timeSequence?.Kill();
        UpdateEverything(1f);
    }
}