using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using CardSystem;

public class CameraEffectsController : MonoBehaviour
{
    [Header("Camera Shake")]
    [SerializeField] private float _shakeDuration = 0.25f;
    [SerializeField] private float _shakeIntensity = 0.15f;

    [Header("Low Health Vignette")]
    [SerializeField] private int _criticalHealthHP = 3;
    //health percentage below which the vignette appears (0.4 = 40%)
    [SerializeField] private float _lowHealthThreshold = 0.4f;
    [SerializeField] private float _maxVignetteAlpha = 0.6f;
    [SerializeField] private Color _lowHealthColor = new Color(0.6f, 0f, 0f, 1f);
    //pulse speed for the vignette at low health
    [SerializeField] private float _vignettePulseSpeed = 2f;
    [SerializeField] private int _vignetteTextureSize = 256;

    [Header("Ability Flash")]
    [SerializeField] private Color _healFlashColor = new Color(0f, 0.7f, 0.2f, 1f);
    [SerializeField] private Color _shieldFlashColor = new Color(0.2f, 0.5f, 1f, 1f);
    [SerializeField] private float _flashPeakAlpha = 0.35f;
    [SerializeField] private float _flashFadeInDuration = 0.1f;
    [SerializeField] private float _flashHoldDuration = 0.15f;
    [SerializeField] private float _flashFadeOutDuration = 0.5f;

    private Vector3 _originalPos;
    private Coroutine _shakeCoroutine;
    private Image _vignetteImage;
    private float _currentHealthPercent = 1f;
    private int _previousHealth = -1;
    private int _currentHealth;

    private Coroutine _flashCoroutine;
    private bool _isFlashing;

    public static CameraEffectsController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

        _originalPos = transform.localPosition;
        CreateVignetteOverlay();
    }

    private void OnEnable()
    {
        DamageEvents.OnPlayerDamaged += OnPlayerHealthChanged;
        AbilityEvents.OnAbilityUsedDetailed += OnAbilityUsed;
    }

    private void OnDisable()
    {
        DamageEvents.OnPlayerDamaged -= OnPlayerHealthChanged;
        AbilityEvents.OnAbilityUsedDetailed -= OnAbilityUsed;
    }

    private void OnPlayerHealthChanged(int current, int max)
    {
        if (max <= 0)
        {
            return;
        }

        if (_previousHealth >= 0 && current < _previousHealth)
        {
            TriggerShake();
        }

        _previousHealth = current;
        _currentHealth = current;
        _currentHealthPercent = (float)current / max;
    }

    private void OnAbilityUsed(Team team, CardCategory category)
    {
        if (team != Team.Friendly)
        {
            return;
        }

        switch (category)
        {
            case CardCategory.Heal:
                TriggerFlash(_healFlashColor);
                break;
            case CardCategory.Shield:
                TriggerFlash(_shieldFlashColor);
                break;
        }
    }

    private void Update()
    {
        if (!_isFlashing)
        {
            UpdateVignette();
        }
    }

    public void TriggerShake()
    {
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
        }
        _shakeCoroutine = StartCoroutine(ShakeCoro());
    }

    private IEnumerator ShakeCoro()
    {
        float elapsed = 0f;

        while (elapsed < _shakeDuration)
        {
            float strength = _shakeIntensity * (1f - (elapsed / _shakeDuration));
            float offsetX = Random.Range(-strength, strength);
            float offsetY = Random.Range(-strength, strength);

            transform.localPosition = _originalPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localPosition = _originalPos;
        _shakeCoroutine = null;
    }
    
    private void UpdateVignette()
    {
        if (_vignetteImage == null)
        {
            return;
        }

        if (_currentHealthPercent >= _lowHealthThreshold)
        {
            if (_vignetteImage.color.a > 0f)
            {
                _vignetteImage.color = Color.clear;
            }
            return;
        }

        float severity = 1f - (_currentHealthPercent / _lowHealthThreshold);

        if (_currentHealth <= _criticalHealthHP)
        {
            severity = 1f;
        }

        float pulseStrength = Mathf.Lerp(0.05f, 0.4f, severity);
        float pulse = 1f + Mathf.Sin(Time.time * _vignettePulseSpeed * (1f + severity)) * pulseStrength;

        float alpha = Mathf.Clamp01(severity * _maxVignetteAlpha * pulse);

        Color c = _lowHealthColor;
        c.a = alpha;
        _vignetteImage.color = c;
    }
    

    public void TriggerFlash(Color flashColor)
    {
        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
        }
        _flashCoroutine = StartCoroutine(FlashCoro(flashColor));
    }

    private IEnumerator FlashCoro(Color flashColor)
    {
        _isFlashing = true;

        float elapsed = 0f;
        while (elapsed < _flashFadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _flashFadeInDuration);
            Color c = flashColor;
            c.a = Mathf.Lerp(0f, _flashPeakAlpha, t);
            _vignetteImage.color = c;
            yield return null;
        }

        Color peak = flashColor;
        peak.a = _flashPeakAlpha;
        _vignetteImage.color = peak;
        yield return new WaitForSecondsRealtime(_flashHoldDuration);

        elapsed = 0f;
        while (elapsed < _flashFadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _flashFadeOutDuration);
            Color c = flashColor;
            c.a = Mathf.Lerp(_flashPeakAlpha, 0f, t);
            _vignetteImage.color = c;
            yield return null;
        }

        _vignetteImage.color = Color.clear;
        _isFlashing = false;
        _flashCoroutine = null;

    }

    private void CreateVignetteOverlay()
    {
        GameObject canvasGO = new GameObject("VignetteCanvas");
        canvasGO.transform.SetParent(transform);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasGO.AddComponent<CanvasScaler>();

        GameObject imageGO = new GameObject("VignetteImage");
        imageGO.transform.SetParent(canvasGO.transform, false);

        RectTransform rect = imageGO.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        _vignetteImage = imageGO.AddComponent<Image>();
        _vignetteImage.sprite = GenerateVignetteSprite();
        _vignetteImage.color = Color.clear;
        _vignetteImage.raycastTarget = false;
    }

    private Sprite GenerateVignetteSprite()
    {
        int size = _vignetteTextureSize;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - center) / center;
                float dy = (y - center) / center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                float alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((dist - 0.3f) / 0.7f));

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}