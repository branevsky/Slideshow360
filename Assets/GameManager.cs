using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Referência da esfera (material com shader 360)")]
    public Renderer sphereRenderer;

    public List<Texture2D> loadedTextures = new List<Texture2D>();

    private int currentIndex = 0;
    private XRControls controls;
    private bool canNavigate = true;

    void Start()
    {
        //StartCoroutine(LoadAllImagesFromStreamingAssets());
        ApplyTexture(loadedTextures[0]);

        controls = new XRControls();
        controls.Enable();
        controls.Gameplay.Enable();

        foreach (var device in UnityEngine.InputSystem.InputSystem.devices)
        {
            Debug.Log("GameManager - Dispositivo: " + device.name + " | Layout: " + device.layout);
        }
    }

    IEnumerator LoadAllImagesFromStreamingAssets()
    {
        string path = Application.streamingAssetsPath;

        string[] imageFiles = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
        foreach (string filePath in imageFiles)
        {
            // Só tenta carregar arquivos de imagem
            if (filePath.EndsWith(".jpg") || filePath.EndsWith(".png"))
            {
                string url = "file://" + filePath;

                using (WWW www = new WWW(url))
                {
                    yield return www;

                    if (string.IsNullOrEmpty(www.error))
                    {
                        Texture2D tex = www.texture;
                        loadedTextures.Add(tex);
                    }
                    else
                    {
                        Debug.LogWarning("Erro ao carregar imagem: " + filePath + "\n" + www.error);
                    }
                }
            }
        }

        // Aplica a primeira imagem, se houver
        if (loadedTextures.Count > 0)
        {
            ApplyTexture(loadedTextures[0]);
        }
        else
        {
            Debug.LogWarning("Nenhuma imagem foi carregada da pasta StreamingAssets.");
        }
    }

    public void NextImage()
    {
        if (loadedTextures.Count == 0) return;

        currentIndex = (currentIndex + 1) % loadedTextures.Count;
        ApplyTexture(loadedTextures[currentIndex]);
    }

    public void PreviousImage()
    {
        if (loadedTextures.Count == 0) return;

        currentIndex = (currentIndex - 1) >= 0 ? (currentIndex - 1) : loadedTextures.Count - 1;
        ApplyTexture(loadedTextures[currentIndex]);
    }

    private void ApplyTexture(Texture2D texture)
    {
        if (sphereRenderer != null)
        {
            sphereRenderer.material.mainTexture = texture;
        }
        else
        {
            Debug.LogWarning("GameManager - Renderer da esfera não está atribuído.");
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

        // Se o thumbstick estiver no centro, reativa a navegação
        if (Mathf.Abs(dir.x) < 0.5f)
        {
            canNavigate = true;
        }

        // Só permite navegar se puder e se o direcional passou do limite
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