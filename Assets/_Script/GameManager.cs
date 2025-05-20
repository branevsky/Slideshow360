using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

public class GameManager : MonoBehaviour
{
    [Header("Referência da esfera (material com shader 360)")]
    public Renderer sphereRenderer; 
    
    [Header("Referência do vídeo")]
    public VideoPlayer videoPlayer;
    public RenderTexture videoRenderTexture; 
    public GameObject videoPlane;

    [Header("URLs remotas das imagens (opcional)")]
    private string remoteImageURL = "https://i.postimg.cc/qBt50DCM/";
    private string imageName = "teste-360-";

    private List<Texture2D> loadedTextures = new List<Texture2D>();

    private int currentIndex = 0;
    private XRControls controls;
    private bool canNavigate = true;
    private int startImage = 2;//mudar para o numero da primeira imagem depois
    private int numImages = 2;//mudar para o 6 depois
    private bool loadWeb = false;

    void Start()
    {
        StartCoroutine(TryLoadRemoteThenFallbackImages());

        videoPlayer.isLooping = true;
        videoPlayer.targetTexture = videoRenderTexture;
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();

        controls = new XRControls();
        controls.Enable();
        controls.Gameplay.Enable();

        foreach (var device in UnityEngine.InputSystem.InputSystem.devices)
        {
            Debug.Log("GameManager - Dispositivo: " + device.name + " | Layout: " + device.layout);
        }
    }
    private void OnVideoPrepared(VideoPlayer vp)
    {
        AdjustPlaneToVideo();
    }

    private void AdjustPlaneToVideo()
    {
        float videoWidth = videoPlayer.texture.width;
        float videoHeight = videoPlayer.texture.height;

        if (videoHeight == 0) return; // evita divisão por zero

        float aspectRatio = videoWidth / videoHeight;

        // Escala original em Y é 1, então ajustamos X proporcionalmente
        Vector3 scale = transform.localScale;
        scale.x = scale.y * aspectRatio;
        transform.localScale = scale;
    }

    IEnumerator TryLoadRemoteThenFallbackImages()
    {
        bool loadedAny = false;

        if (loadWeb)
        {
            // Tenta carregar imagens das URLs
            for (int i = startImage; i <= numImages; i++)
            {
                UnityWebRequest request = UnityWebRequestTexture.GetTexture($"{remoteImageURL}{imageName}{i}.jpg");
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Texture2D tex = DownloadHandlerTexture.GetContent(request);
                    loadedTextures.Add(tex);
                    Debug.Log($"Imagem remota carregada: {remoteImageURL}{i}.jpg");
                    loadedAny = true;
                }
                else
                {
                    Debug.LogWarning($"Falha ao carregar imagem remota: {remoteImageURL}{imageName}{i}.jpg | Erro: {request.error}");
                }
            }
        }

        // Se não conseguiu nenhuma, tenta local
        if (!loadedAny)
        {
            Debug.Log("Nenhuma imagem remota carregada. Tentando carregar localmente...");
            yield return StartCoroutine(LoadAllImagesFromStreamingAssets());
        }

        // Aplica a primeira imagem, se houver
        if (loadedTextures.Count > 0)
        {
            ApplyTexture(currentIndex);
        }
        else
        {
            Debug.LogWarning("Nenhuma imagem foi carregada (remota ou local).");
        }
    }

    IEnumerator LoadAllImagesFromStreamingAssets()
    {
        string path = Application.streamingAssetsPath;

#if UNITY_ANDROID && !UNITY_EDITOR
// No Android, você precisa saber os nomes dos arquivos com antecedência.

    for (int i = startImage; i <= numImages; i++)
    {
        string fullPath = Path.Combine(path, $"{imageName}{i}.jpg");

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(fullPath))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(request);
                loadedTextures.Add(tex);
                Debug.Log("Imagem carregada (Android): " + $"{imageName}{i}.jpg");
            }
            else
            {
                Debug.LogWarning("Erro ao carregar imagem no Android: " + fullPath + "\n" + request.error);
            }
        }
    }
#else
        // No Editor e plataformas desktop, você pode listar normalmente
        string[] imageFiles = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);

        foreach (string filePath in imageFiles)
        {
            if (filePath.EndsWith(".jpg") || filePath.EndsWith(".png"))
            {
                string url = "file://" + filePath;

                using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
                {
                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        Texture2D tex = DownloadHandlerTexture.GetContent(request);
                        loadedTextures.Add(tex);
                        Debug.Log("Imagem carregada (Editor/PC): " + filePath);
                    }
                    else
                    {
                        Debug.LogWarning("Erro ao carregar imagem local: " + filePath + "\n" + request.error);
                    }
                }
            }
        }
#endif
    }

    public void NextImage()
    {
        int total = loadedTextures.Count + 1; // +1 por causa do vídeo
        if (total == 0) return;

        currentIndex = (currentIndex + 1) % total;
        ApplyTexture(currentIndex);
    }

    public void PreviousImage()
    {
        int total = loadedTextures.Count + 1;
        if (total == 0) return;

        currentIndex = (currentIndex - 1 + total) % total;
        ApplyTexture(currentIndex);
    }

    private void ApplyTexture(int index)
    {
        if (index == 0)
        {
            // Mostrar o vídeo
            if (videoPlane != null)
            {
                videoPlane.SetActive(true);
            }

            if (sphereRenderer != null)
            {
                sphereRenderer.enabled = false; // Esconde a esfera
            }

            videoPlayer.frame = 0;
            videoPlayer.Play();
            Debug.Log("GameManager - Exibindo vídeo no plano");
        }
        else
        {
            // Mostrar imagem
            int imageIndex = index - 1;

            if (imageIndex >= 0 && imageIndex < loadedTextures.Count)
            {
                videoPlayer.Stop();

                if (sphereRenderer != null)
                {
                    sphereRenderer.enabled = true;
                    sphereRenderer.material.mainTexture = loadedTextures[imageIndex];
                }

                if (videoPlane != null)
                {
                    videoPlane.SetActive(false);
                }

                Debug.Log("GameManager - Exibindo imagem " + imageIndex);
            }
            else
            {
                Debug.LogWarning("GameManager - Índice de imagem inválido: " + imageIndex);
            }
        }
    }

    private void Update()
    {
        if (controls.Gameplay.ButtonA.WasPressedThisFrame())
        {
            Debug.Log("GameManager - Botão A pressionado");
            NextImage();
        }

        if (controls.Gameplay.ButtonB.WasPressedThisFrame())
        {
            Debug.Log("GameManager - Botão B pressionado");
            PreviousImage();
        }

        Vector2 dir = controls.Gameplay.Thumbstick.ReadValue<Vector2>();

        if (Mathf.Abs(dir.x) < 0.5f)
        {
            canNavigate = true;
        }

        if (canNavigate)
        {
            if (dir.x > 0.5f)
            {
                Debug.Log("GameManager - Direita");
                NextImage();
                canNavigate = false;
            }
            else if (dir.x < -0.5f)
            {
                Debug.Log("GameManager - Esquerda");
                PreviousImage();
                canNavigate = false;
            }
        }
    }
}