using UnityEngine;
using System.Collections;
using System.IO;


public class VideoAndScreenshot : MonoBehaviour
{
     public void TakeScreenshot()
    {
        string path = Path.Combine(Application.persistentDataPath, "AR_Screenshot_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png");
        ScreenCapture.CaptureScreenshot(path);
        Debug.Log("Saved screenshot to: " + path);
    }
}
