using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class LookUpToPlay : MonoBehaviour
{
    public Camera vrCamera;
    public float triggerThreshold = 0.7f; // Quanto pra cima precisa olhar
    public float holdDuration = 2f; // Tempo que precisa manter o olhar pra cima
    public Image loadingImage; // Imagem com Fill Method = Radial 360
    public VideoPlayer videoPlayer;

    private float timer = 0f;
    private bool hasPlayed = false;

    void Start()
    {
        // Escuta o evento de término do vídeo
        videoPlayer.loopPointReached += OnVideoFinished;

        // Começa com a imagem visível
        loadingImage.gameObject.SetActive(false);
    }

    void Update()
    {
        if (hasPlayed) return;

        float lookY = vrCamera.transform.forward.y;

        if (lookY > triggerThreshold)
        {
            loadingImage.gameObject.SetActive(true);
            timer += Time.deltaTime;
            loadingImage.fillAmount = timer / holdDuration;

            if (timer >= holdDuration)
            {
                videoPlayer.Play();
                hasPlayed = true;
                loadingImage.gameObject.SetActive(false); // some com o loading
            }
        }
        else
        {
            timer = 0f;
            loadingImage.fillAmount = 0f;
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        hasPlayed = false;
        timer = 0f;
        loadingImage.fillAmount = 0f;
    }
}
