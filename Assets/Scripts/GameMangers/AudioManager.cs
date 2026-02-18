using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip unclickSound;


    private void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    public void PlayClickSound()
    {
        if (clickSound != null)
            GetComponent<AudioSource>().PlayOneShot(clickSound);
    }

    public void PlayUnclickSound()
    {
        if (unclickSound != null)
            GetComponent<AudioSource>().PlayOneShot(unclickSound);
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
            GetComponent<AudioSource>().PlayOneShot(clip);
    }


    public void StopSound()
    {
        GetComponent<AudioSource>().Stop();
    }

}
