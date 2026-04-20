using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TransformAnimator : MonoBehaviour
{
    [System.Serializable]
    public class AnimationDatabase
    {
        public List<Animation> animations;
    }

    [System.Serializable]
    public class Animation
    {
        public string name;

        [System.Serializable]
        public class FrameData<T>
        {
            public T value;
            public float timeStamp;
        }

        [System.Serializable]
        public class AnimEvent
        {
            public string functionName;
            public float timeStamp;
        }

        public List<FrameData<Vector3>> position = new List<FrameData<Vector3>>();
        public List<FrameData<Vector3>> rotation = new List<FrameData<Vector3>>();
        public List<AnimEvent> events = new List<AnimEvent>();
    }

    [System.Serializable]
    public class CustomAnimator
    {
        [Tooltip("Animasyonun oynatılacağı görsel obje (Çocuk Obje)")]
        [SerializeField] private GameObject body;

        [Tooltip("Event fonksiyonlarının aranacağı scriptlerin bulunduğu obje (Genelde Root/Ana Objedir). Boş bırakılırsa body objesinde arar.")]
        [SerializeField] private GameObject eventTarget;

        [Tooltip("Aktifse, animasyon objenin başlangıçtaki local pozisyon ve rotasyonu üzerine eklenerek oynatılır.")]
        [SerializeField] private bool applyRelative = true;

        public List<Animation> animations = new List<Animation>();

        private Sequence currentSequence;
        
        // Objenin ilk baştaki konumunu tutacak değişkenler
        private Vector3 startLocalPosition;
        private Vector3 startLocalRotation;
        private bool isStartTransformCached = false;

        public void LoadFromJson(string jsonText)
        {
            try
            {
                AnimationDatabase database = JsonUtility.FromJson<AnimationDatabase>(jsonText);

                if (database != null && database.animations != null)
                {
                    animations.Clear();
                    animations.AddRange(database.animations);
                    Debug.Log($"[TransformAnimator] {animations.Count} adet animasyon JSON'dan başarıyla yüklendi.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"JSON okuma hatası: {e.Message}");
            }
        }


        public void PlayAnimation(Animation animation, bool loop = false)
        {
            if (body == null) return;

            if (applyRelative && !isStartTransformCached)
            {
                startLocalPosition = body.transform.localPosition;
                startLocalRotation = body.transform.localEulerAngles;
                isStartTransformCached = true;
            }

            // Relative ayarı açıksa base koordinatları kullan, kapalıysa sıfırdan başla
            Vector3 basePos = applyRelative ? startLocalPosition : Vector3.zero;
            Vector3 baseRot = applyRelative ? startLocalRotation : Vector3.zero;

            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();

            // 1. POZİSYONLARI EKLE
            for (int i = 0; i < animation.position.Count; i++)
            {
                var frame = animation.position[i];
                float startTime = (i == 0) ? 0f : animation.position[i - 1].timeStamp;
                float duration = frame.timeStamp - startTime;

                // Frame değerine objenin başlangıç offsetini ekle
                Vector3 targetPos = frame.value + basePos;

                Tween posTween = body.transform.DOLocalMove(targetPos, duration).SetEase(Ease.Linear);
                currentSequence.Insert(startTime, posTween);
            }

            // 2. ROTASYONLARI EKLE
            for (int i = 0; i < animation.rotation.Count; i++)
            {
                var frame = animation.rotation[i];
                float startTime = (i == 0) ? 0f : animation.rotation[i - 1].timeStamp;
                float duration = frame.timeStamp - startTime;

                // Frame değerine objenin başlangıç offsetini ekle
                Vector3 targetRot = frame.value + baseRot;

                Tween rotTween = body.transform.DOLocalRotate(targetRot, duration).SetEase(Ease.Linear);
                currentSequence.Insert(startTime, rotTween);
            }

            // --- 3. EVENTLERİ EKLE ---
            GameObject targetObject = eventTarget != null ? eventTarget : body;

            for (int i = 0; i < animation.events.Count; i++)
            {
                var animEvent = animation.events[i];
                string funcName = animEvent.functionName;

                currentSequence.InsertCallback(animEvent.timeStamp, () =>
                {
                    targetObject.SendMessage(funcName, SendMessageOptions.DontRequireReceiver);
                });
            }

            if (loop) currentSequence.SetLoops(-1, LoopType.Restart);

            currentSequence.Play();
        }

        public void PlayAnimation(string animName, bool loop = false)
        {
            Animation animToPlay = animations.Find(a => a.name == animName);
            if (animToPlay != null) PlayAnimation(animToPlay, loop);
            else Debug.LogWarning($"Animasyon bulunamadı: {animName}");
        }
    }

    [Header("Animator Settings")]
    [SerializeField] private CustomAnimator animator;

    [Header("Data Source")]
    [Tooltip("Projedeki .json uzantılı dosyayı buraya sürükleyin")]
    [SerializeField] private TextAsset jsonFile;

    [Header("Debug Controls")]
    [SerializeField] bool loadJsonNow = false;
    [SerializeField] bool playAnim = false;
    [SerializeField] string animName = "cusdeve";

    void Start()
    {
        if (jsonFile != null)
        {
            animator.LoadFromJson(jsonFile.text);
        }
    }

    void Update()
    {
        if (loadJsonNow)
        {
            if (jsonFile != null) animator.LoadFromJson(jsonFile.text);
            loadJsonNow = false;
        }

        if (playAnim)
        {
            animator.PlayAnimation(animName);
            playAnim = false;
        }
    }

    public void Play(string animationName, bool loop = false)
    {
        animator.PlayAnimation(animationName, loop);
    }

    [ContextMenu("JSON'dan Animasyonları Yükle")]
    public void LoadJsonInEditor()
    {
        if (jsonFile != null)
        {
            animator.LoadFromJson(jsonFile.text);

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif
            Debug.Log("<color=green><b>[BAŞARILI]</b></color> Animasyonlar JSON'dan çekildi ve Inspector'a kaydedildi!");
        }
        else
        {
            Debug.LogError("JSON yüklenemedi! Lütfen 'Json File' boşluğuna bir dosya sürükleyin.");
        }
    }
}