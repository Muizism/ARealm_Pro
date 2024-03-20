using System.Collections.Generic;
using System.Drawing;
using Unity.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ZXing;
using static UnityEngine.XR.ARSubsystems.XRCpuImage;

public class QrCodeRecenter : MonoBehaviour
{

    [SerializeField]
    private ARSession session;
    [SerializeField]
    private XROrigin sessionOrigin;
    [SerializeField]
    private ARCameraManager cameraManager;
    [SerializeField]
    private List<Target> navigationTargetObjects = new List<Target>();
    private Texture2D cameraImageTexture;
    private IBarcodeReader reader = new BarcodeReader(); // create a barcode reader instance

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetQrCodeRecenterTarget("office");
        }
    }
    private void OnEnable()
    {
        cameraManager.frameReceived += OnCameraFrameReceived;
    }


    private void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrameReceived;
    }



    private void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            return;
        }
        var conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, image.width, image.height),

            outputDimensions = new Vector2Int(image.width / 2, image.height / 2),

            outputFormat = TextureFormat.RGBA32,

            transformation = XRCpuImage.Transformation.MirrorY
        };


        int size = image.GetConvertedDataSize(conversionParams);

        var buffer = new NativeArray<byte>(size, Allocator.Temp);

        image.Convert(conversionParams, buffer);
        image.Dispose();
        // At this point, you can process the image, pass it to a computer vision algorithm, etc.
        // In this example, you apply it to a texture to visualize it.
        // You've got the data; let's put it into a texture so you can visualize it.
        cameraImageTexture = new Texture2D(
        conversionParams.outputDimensions.x,
        conversionParams.outputDimensions.y,
        conversionParams.outputFormat,
        false);
        cameraImageTexture.LoadRawTextureData(buffer);
        cameraImageTexture.Apply();
        // Done with your temporary data, so you can dispose it.
        buffer.Dispose();
        // Detect and decode the barcode inside the bitmap
        var result = reader.Decode(cameraImageTexture.GetPixels32(), cameraImageTexture.width, cameraImageTexture.height);
        // Do something with the result
        // Do something with the result
        if (result != null)
        {
            SetQrCodeRecenterTarget(result.Text);
        }
    }
    private void SetQrCodeRecenterTarget(string targetText)
    {
        Target currentTarget = navigationTargetObjects.Find(x => x.Name.ToLower().Equals(targetText.ToLower()));
        if (currentTarget != null)
        {
            // Reset position and rotation of ARSession
            session.Reset();
            // Add offset for recentering
            sessionOrigin.transform.position = currentTarget.positionObject.transform.position;
            sessionOrigin.transform.rotation = currentTarget.positionObject.transform.rotation;

        }
    }
}








    