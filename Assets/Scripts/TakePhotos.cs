using System;
using UnityEngine;

using System. Collections;
using System.Collections. Generic;

public class TakePhotos : MonoBehaviour
{
    public void TakePhoto()
    {
        Camera camera = Camera.main;
        int width = Screen.width;
        int height = Screen.height;
        RenderTexture rt = new RenderTexture(width, height, 24);
         camera.targetTexture = rt;

        var currentRT = RenderTexture.active;
          RenderTexture.active = rt;

        camera.Render();

       Texture2D image = new Texture2D(width, height);
          image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply();
        camera.targetTexture = null;

        RenderTexture.active = currentRT;
        byte[] bytes = image.EncodeToPNG();
}

}

