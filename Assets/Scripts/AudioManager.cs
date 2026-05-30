using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Clips")]
    public AudioClip holdClip;
    public AudioClip gameOverClip;
    public AudioClip clearClip;   // som base da limpeza

    private AudioSource sfxSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
    }

    public void PlayHold()
    {
        if (holdClip != null)
            sfxSource.PlayOneShot(holdClip);
    }

    public void PlayGameOver()
    {
        if (gameOverClip != null)
            sfxSource.PlayOneShot(gameOverClip);
    }

    public void PlayClear()
    {
        if (clearClip == null) return;

        // Varia o pitch aleatoriamente entre 0.9 e 1.1 para cada reprodução
        float originalPitch = sfxSource.pitch;
        sfxSource.pitch = Random.Range(0.9f, 1.1f);
        sfxSource.PlayOneShot(clearClip);
        sfxSource.pitch = originalPitch;
    }
}