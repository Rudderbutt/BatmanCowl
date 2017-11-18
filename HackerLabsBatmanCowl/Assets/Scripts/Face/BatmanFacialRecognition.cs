using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System;
using System.Net.Security;
using System.Net;
using System.Collections;
using System.Security.Cryptography.X509Certificates;

public class BatmanFacialRecognition : MonoBehaviour
{
    const string faceAPIKey = "f6633cc8206a495199dd7bcb6053afe8";
    const string emotionAPIKey = "default";
    UnityEngine.XR.WSA.WebCam.PhotoCapture photoCaptureObject = null;
   
    Resolution cameraResolution;
    Vector3 cameraPosition = Vector3.zero;
    Quaternion cameraRotation = new Quaternion();
    UnityEngine.XR.WSA.Input.GestureRecognizer recognizer = null;
    public GameObject status = null;
    IEnumerator coroutine;
    void Start()
    {
        PostTestImage();
        status = FindObjectOfType<TextMesh>().gameObject;
        //StartCoroutine(PostFace(File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, "image.jpg"))));
        //StartCoroutine(GetVisionDataFromImages(File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, "image.jpg"))));
        Detect();
        //UnityEngine.XR.WSA.WebCam.PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }


    IEnumerator CoroLoop()
    {
        int secondsInterval = 20;
        while (true)
        {
            //AnalyzeScene();
            capture();//RunComputerVision(UnityEngine.Windows.File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, "image.jpg")));
            yield return new WaitForSeconds(secondsInterval);
        }
    }

    void capture() {
        UnityEngine.XR.WSA.WebCam.PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    async void Detect() {
       CriminalDatabase.Criminal_t criminal =  await FindObjectOfType<CriminalDatabase>().GetCriminalFromImage(File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, "image.jpg")));
        Debug.Log(criminal.CriminalName);
    }

    private void OnEnable()
    {
        ServicePointManager.ServerCertificateValidationCallback = MonoSecurityBypass;
    }


    public bool MonoSecurityBypass(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
{
    bool isOk = true;
    // If there are errors in the certificate chain,
    // look at each error to determine the cause.
    if (sslPolicyErrors != SslPolicyErrors.None)
    {
        for (int i = 0; i < chain.ChainStatus.Length; i++)
        {
            if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown)
            {
                continue;
            }
            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
            chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
            chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
            bool chainIsValid = chain.Build((X509Certificate2)certificate);
            if (!chainIsValid)
            {
                isOk = false;
                break;
            }
        }
    }
    return isOk;
}
private void Update()
    {
        //UnityEngine.XR.WSA.WebCam.PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    void PostTestImage()
    {
        string filePath = "G:\\Desktop\\20171110_190853.jpg";

        //StartCoroutine(PostFace(GetImageAsByteArray(filePath)));
    }

    void OnPhotoCaptureCreated(UnityEngine.XR.WSA.WebCam.PhotoCapture captureObject)
    {
        photoCaptureObject = captureObject;
        cameraResolution = UnityEngine.XR.WSA.WebCam.PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        UnityEngine.XR.WSA.WebCam.CameraParameters c = new UnityEngine.XR.WSA.WebCam.CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = UnityEngine.XR.WSA.WebCam.CapturePixelFormat.PNG;

        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }

    void OnCapturedPhotoToMemory(UnityEngine.XR.WSA.WebCam.PhotoCapture.PhotoCaptureResult result, UnityEngine.XR.WSA.WebCam.PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {
            List<byte> imageBufferList = new List<byte>();
            // Copy the raw IMFMediaBuffer data into our empty byte list.
            photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);

            var cameraToWorldMatrix = new Matrix4x4();
            photoCaptureFrame.TryGetCameraToWorldMatrix(out cameraToWorldMatrix);

            cameraPosition = cameraToWorldMatrix.MultiplyPoint3x4(new Vector3(0, 0, -1));
            cameraRotation = Quaternion.LookRotation(-cameraToWorldMatrix.GetColumn(2), cameraToWorldMatrix.GetColumn(1));

            Matrix4x4 projectionMatrix;
            photoCaptureFrame.TryGetProjectionMatrix(Camera.main.nearClipPlane, Camera.main.farClipPlane, out projectionMatrix);
            Matrix4x4 pixelToCameraMatrix = projectionMatrix.inverse;

            status.GetComponent<TextMesh>().text = "Processing";

            //StartCoroutine(PostFace(imageBufferList.ToArray()));
            StartCoroutine(GetVisionDataFromImages(imageBufferList.ToArray()));

        }
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);

    }

    void OnStoppedPhotoMode(UnityEngine.XR.WSA.WebCam.PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    private void OnPhotoModeStarted(UnityEngine.XR.WSA.WebCam.PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
    }

    string VISIONKEY = "258dced5ef694cb3a1209b44f8b652c6"; // replace with your Computer Vision API Key

    string emotionURL = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/analyze?visualFeatures=Description&language=en";

    static byte[] GetImageAsByteArray(string imageFilePath)
    {
        FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
        BinaryReader binaryReader = new BinaryReader(fileStream);
        return binaryReader.ReadBytes((int)fileStream.Length);
    }

    IEnumerator<object> GetVisionDataFromImages(byte[] image)
    {

        Dictionary<string, string> requestHeaders = new Dictionary<string, string>
        {
            { "Ocp-Apim-Subscription-Key", VISIONKEY },
            {"Content-Type", "application/octet-stream" },
        };

        WWW www = new WWW(emotionURL, image, requestHeaders);

        yield return www;
        string responseData = www.text; // Save the response as JSON string
        Debug.Log(responseData);
        status.GetComponent<TextMesh>().text = responseData;
    }

    IEnumerator<object> PostFace(byte[] imageData)
    {
        string url = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/detect?returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses";
        Dictionary<string, string> requestHeaders = new Dictionary<string, string>
        {
            { "Ocp-Apim-Subscription-Key", faceAPIKey },
            {"Content-Type", "application/octet-stream" },
        };

        WWW request = new WWW(url, imageData, requestHeaders);

        yield return request;

        string response = request.text;

        JSONObject jObj = new JSONObject(response);

        //JSONObject[] jArray = JsonUtility.FromJson<JSONObject[]>(response);


        if(jObj.list.Count == 0)
        {
            status.GetComponent<TextMesh>().text = "No Faces";
            Debug.LogWarning("No face found.");
            yield break;
        }

        string faceRect = string.Empty;
        Dictionary<string, TextMesh> meshes = new Dictionary<string, TextMesh>();

        // Do this later.
        string id = "";
        for (int i = 0; i < jObj.Count; i++)
        {
            id = "\n" + jObj[i].GetField("faceId").ToString();
        }

        //GetComponentInChildren<TextMesh>().text = "Face ID: " + id;
        
        Debug.Log("Face ID: " + id);
    }

    void Awake()
    {
        Camera.main.nearClipPlane = 100.0f;
        recognizer = new UnityEngine.XR.WSA.Input.GestureRecognizer();
        recognizer.TappedEvent += (source, tapCount, ray) =>
        {
            OnScan();
        };

        recognizer.StartCapturingGestures();
    }

    void OnScan()
    {
        OnClear();
        status.GetComponent<TextMesh>().text = "Scanning";
        UnityEngine.XR.WSA.WebCam.PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    void OnClear()
    {
        /*
        //status.GetComponent<TextMesh>().text = "Ready";
O
        //Gamebject[] gameObjects = GameObject.FindGameObjectsWithTag("faceBounds");
        //foreach (GameObject enemy in gameObjects)
          //  Destroy(enemy);

        //gameObjects = GameObject.FindGameObjectsWithTag("faceText");
        foreach (GameObject enemy in gameObjects)
            Destroy(enemy);

        gameObjects = GameObject.FindGameObjectsWithTag("facePicture");
        foreach (GameObject enemy in gameObjects)
            Destroy(enemy);

        gameObjects = GameObject.FindGameObjectsWithTag("emoteText");
        foreach (GameObject enemy in gameObjects)
            Destroy(enemy);*/
    }

    void OnReset()
    {
        Camera.main.nearClipPlane = 100;
    }

    void OnInitiate()
    {
        Camera.main.nearClipPlane = 0.85f;
    }
}
