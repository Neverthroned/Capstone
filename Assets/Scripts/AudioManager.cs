using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    public AudioClip phase1Music;
    public AudioClip phase2Music;
    public float musicVolume = 0.6f;

    [Header("Plasma")]
    public AudioClip plasmaShoot;
    public AudioClip plasmaWarning;    // Plays when plasma gets close to player
    public float plasmaWarningDistance = 3f;

    [Header("Flamethrower")]
    public AudioClip flamethrowerLoop;

    [Header("Black Hole")]
    public AudioClip blackHoleLoop;
    public float blackHoleDuckVolume = 0f; // How much to reduce all other audio

    private AudioSource _musicSource;
    private AudioSource _sfxSource;
    private AudioSource _flamethrowerSource;
    private AudioSource _blackHoleSource;

    private float _defaultSFXVolume;
    private float _defaultMusicVolume;

    private bool _blackHoleActive;

    private void Awake()
    {
        // Singleton so any script can call AudioManager.Instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create audio sources
        _musicSource = CreateSource("Music", true, 1f);
        _sfxSource = CreateSource("SFX", false, 1f);
        _flamethrowerSource = CreateSource("Flamethrower", true, 1f);
        _blackHoleSource = CreateSource("BlackHole", true, 1f);

        _defaultMusicVolume = _musicSource.volume;
        _defaultSFXVolume = _sfxSource.volume;
    }

    private AudioSource CreateSource(string name, bool loop, float volume)
    {
        GameObject obj = new GameObject($"AudioSource_{name}");
        obj.transform.parent = transform;
        AudioSource source = obj.AddComponent<AudioSource>();
        source.loop = loop;
        source.volume = volume;
        return source;
    }

    // --- MUSIC ---
    public void PlayPhase1Music()
    {
        CrossfadeMusic(phase1Music);
    }

    public void PlayPhase2Music()
    {
        CrossfadeMusic(phase2Music);
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        // Fade out current
        float startVolume = _musicSource.volume;
        float elapsed = 0f;
        float fadeDuration = 1.5f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            _musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        _musicSource.Stop();
        _musicSource.clip = newClip;
        _musicSource.Play();

        // Fade in new
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            _musicSource.volume = Mathf.Lerp(0f, musicVolume, elapsed / fadeDuration);
            yield return null;
        }

        _musicSource.volume = musicVolume;
    }

    // Coroutine wrapper so it can be called as a method
    /* public void CrossfadeMusic(AudioClip newClip)
    {
        StartCoroutine(CrossfadeMusic(newClip));
    } */

    // --- PLASMA ---
    public void PlayPlasmaShoot()
    {
        _sfxSource.PlayOneShot(plasmaShoot);
    }

    public void PlayPlasmaWarning()
    {
        if (!_sfxSource.isPlaying)
            _sfxSource.PlayOneShot(plasmaWarning);
    }

    // --- FLAMETHROWER ---
    public void StartFlamethrower()
    {
        if (flamethrowerLoop == null) return;
        _flamethrowerSource.clip = flamethrowerLoop;
        _flamethrowerSource.Play();
    }

    public void StopFlamethrower()
    {
        _flamethrowerSource.Stop();
    }

    // --- BLACK HOLE ---
    public void StartBlackHole()
    {
        _blackHoleActive = true;
        if (blackHoleLoop != null)
        {
            _blackHoleSource.clip = blackHoleLoop;
            _blackHoleSource.Play();
        }
        StartCoroutine(DuckAudio(blackHoleDuckVolume));
    }

    public void StopBlackHole()
    {
        _blackHoleActive = false;
        _blackHoleSource.Stop();
        StartCoroutine(DuckAudio(_defaultSFXVolume));
    }

    // Smoothly lower/raise all other audio
    private IEnumerator DuckAudio(float targetVolume)
    {
        float startVolume = _musicSource.volume;
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            _musicSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            _flamethrowerSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            _sfxSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        _musicSource.volume = targetVolume;
        _flamethrowerSource.volume = targetVolume;
        _sfxSource.volume = targetVolume;
    }
}