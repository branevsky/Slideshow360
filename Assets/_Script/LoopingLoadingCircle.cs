using UnityEngine;
using UnityEngine.UI;

public class LoopingLoadingCircle : MonoBehaviour
{
    public Image loadingImage;
    public float cycleDuration = 1f; // tempo total de preenchimento ou esvaziamento

    private bool filling = true;

    void Update()
    {
        if (loadingImage == null) return;

        float speed = 1f / cycleDuration;

        if (filling)
        {
            loadingImage.fillAmount += speed * Time.deltaTime;
            if (loadingImage.fillAmount >= 1f)
            {
                loadingImage.fillAmount = 1f;
                filling = false;
                loadingImage.fillClockwise = !loadingImage.fillClockwise;
            }
        }
        else
        {
            loadingImage.fillAmount -= speed * Time.deltaTime;
            if (loadingImage.fillAmount <= 0f)
            {
                loadingImage.fillAmount = 0f;
                filling = true;
                loadingImage.fillClockwise = !loadingImage.fillClockwise;
            }
        }
    }
}