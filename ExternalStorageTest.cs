using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

public class ExternalStorageTest : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        // Get the external storage directory
        AndroidJavaClass environment = new AndroidJavaClass("android.os.Environment");

        bool isExternalStorageManager = environment.CallStatic<bool>("isExternalStorageManager");

        if(!isExternalStorageManager)
            OpenStorageSettings();
    }

    /// <summary>
    /// Open screen to change allow access to manage all files permission
    /// </summary>
    void OpenStorageSettings()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent");
        intent.Call<AndroidJavaObject>("setAction", "android.settings.MANAGE_ALL_FILES_ACCESS_PERMISSION");

        currentActivity.Call("startActivity", intent);
    }

    /// <summary>
    /// Read files from usb
    /// </summary>
    public void GetFileFromUsbStorage( out string readedData)
    {
        readedData = "";
        // Get the current Android activity
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        // Get the StorageManager class
        AndroidJavaClass storageManagerClass = new AndroidJavaClass("android.os.storage.StorageManager");

        // Get the StorageManager instance
        AndroidJavaObject storageManager = currentActivity.Call<AndroidJavaObject>("getSystemService", "storage");

        AndroidJavaObject volumes = storageManager.Call<AndroidJavaObject>("getStorageVolumes");
        int num = volumes.Call<int>("size");

        try
        {
            AndroidJavaObject storageVolume = volumes.Call<AndroidJavaObject>("get", 1);
            AndroidJavaObject directory = storageVolume.Call<AndroidJavaObject>("getDirectory");

            string filePath = directory.Call<string>("getAbsolutePath");
            Debug.Log(filePath);

            AndroidJavaObject[] files = directory.Call<AndroidJavaObject[]>("listFiles");

            foreach (var file in files)
            {
                string path = file.Call<string>("getAbsolutePath");
                Debug.Log(path);

                if (path.Contains(".crc"))
                {
                    GetCRC(path, out readedData);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("No device connected");
        }
    }

    private bool GetCRC(string filePath, out string crc)
    {
        crc = "";
        // var fileName = $"{_filePattern}{_fileVersion}.crc";
        // string filePath = Path.Combine(filepath, fileName);

        if (File.Exists(filePath))
        {
            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        crc = reader.ReadToEnd();
                        crc = crc.Replace("\n", "");
                        Debug.Log("crc: " + crc);
                        return true;
                    }
                }
            }
            catch (IOException e)
            {
                Debug.LogError("Error reading file: " + e.Message);
                return false;
            }
        }
        else
        {
            Debug.LogWarning("File does not exist at path: " + filePath);
            return false;
        }
    }

    public void SetPermissions()
    {
        Permission.RequestUserPermission(Permission.ExternalStorageRead);
        Permission.RequestUserPermission(Permission.ExternalStorageWrite);
    }

    public void GetFileFromInternalStorage()
    {
        try
        {
            // Get the external storage directory
            AndroidJavaClass environment = new AndroidJavaClass("android.os.Environment");
            //AndroidJavaObject externalStorageDir = environment.CallStatic<AndroidJavaObject>("getExternalStorageDirectory");
            AndroidJavaObject externalStorageDir = environment.CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory", environment.GetStatic<string>("DIRECTORY_DOWNLOADS"));
            // Specify the file path
            string filePath = externalStorageDir.Call<string>("getAbsolutePath");
            Debug.Log(filePath);

           /* string filePath2 = externalStorageDir.Call<string>("getPath");
            debugText.text += filePath2 + "\n";*/

            AndroidJavaObject[] files = externalStorageDir.Call<AndroidJavaObject[]>("listFiles");

           // debugText.text += files.ToString() + files.Length.ToString() + "\n";

            foreach (var file in files)
            {
                string path = file.Call<string>("getAbsolutePath");
                Debug.Log(path);

                if (path.Contains(".crc"))
                {
                    GetCRC(path, out string data);
                    Debug.Log(data);
                }
            }
        }
        catch(Exception e)
        {
            Debug.Log(e.ToString());
        }

    }
}

