using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("-----Audio Scource-----")]
    [SerializeField] private AudioSource _SFXSource;

    [Header("-----Audio Clips-----")]
    public AudioClip shortClick;
    public AudioClip longClick;
    public AudioClip starClick;
    public AudioClip starHover;



    public void PlayShortClick()
    {
        _SFXSource.PlayOneShot(shortClick);
    }

    public void PlayLongClick()
    {
        _SFXSource.PlayOneShot(longClick);
    }

    public void PlayStarClick()
    {
        _SFXSource.PlayOneShot(starClick);
    }

    public void PlayStarHover()
    {
        _SFXSource.PlayOneShot(starHover);
    }
}
