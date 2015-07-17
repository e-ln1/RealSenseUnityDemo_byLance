using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class VoiceRecognitionTesting : MonoBehaviour
{
    private PXCMSession session;
    private pxcmStatus status;

    private PXCMSpeechRecognition sr;

    private delegate void OnRecognitionDelegate(PXCMSpeechRecognition data);

    private static List<PXCMAudioSource.DeviceInfo> devices = new List<PXCMAudioSource.DeviceInfo>();
    private PXCMAudioSource source;
    private PXCMAudioSource.DeviceInfo[] sourceDeviceInfo;
    private PXCMSpeechRecognition.Handler handler;

    private bool running = false;
    private bool stop = true;
    private bool reset = false;
    private bool _isInitialized = false;

    [SerializeField]
    private int _activeSource = 0;
    
    [SerializeField]
    private int _activeLanguage = 0;
    
    public string[] AvailableSources;
    public string AlertDetected = "No Alert";

    void Start()
    {
        session = PXCMSession.CreateInstance();
        source = session.CreateAudioSource();
        if (source == null)
        {
            Debug.Log("Error Creating Audio Source");
        }

        int numOfDevices = 0;
        for (int i = 0; ; i++)
        {
            PXCMAudioSource.DeviceInfo dinfo;
            if (source.QueryDeviceInfo(i, out dinfo) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
            numOfDevices++;
        }

        if (numOfDevices == 0)
        {
            Debug.Log("No Audio Device Found!");
        }

        source.ScanDevices();
        AvailableSources = new string[numOfDevices];
        sourceDeviceInfo = new PXCMAudioSource.DeviceInfo[numOfDevices];

        for (int i = 0; i < numOfDevices; i++)
        {
            PXCMAudioSource.DeviceInfo dinfo;
            if (source.QueryDeviceInfo(i, out dinfo) < pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                AvailableSources[i] = "FailedToQueryDeviceInfo";
                sourceDeviceInfo[i] = null;
                Debug.Log("QueryDeviceInfo Failed for Index " + i.ToString());
            }
            else
            {
                sourceDeviceInfo[i] = dinfo;
                AvailableSources[i] = dinfo.name;
            }
        }
        source.Dispose();
        _isInitialized = true;
        stop = false;
        reset = false;
    }

    void Update()
    {
        if (!_isInitialized)
            return;
        System.Threading.Thread thread = new System.Threading.Thread(DoVoiceRecognition);
        thread.Start();
        System.Threading.Thread.Sleep(5);
    }

    //runs on another thread
    private void DoVoiceRecognition()
    {
        if (running)
        {
            //Debug.Log("Failed to start voice recognition - already running");
            return;
        }

        running = true;

        if (ConfigureSpeechRecognition())
        {
            /* Initialization */
            PXCMSpeechRecognition.Handler handler = new PXCMSpeechRecognition.Handler();
            handler.onRecognition = OnRecognition;
            handler.onAlert = OnAlert;

            pxcmStatus sts = sr.StartRec(source, handler);
            if (sts >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                while (!stop)
                {
                    if (reset)
                    {
                        sr.StopRec();
                        ConfigureSpeechRecognition();
                        sts = sr.StartRec(source, handler);
                        if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR)
                        {
                            Debug.Log("VoiceThreadError - ResetFailed - StartRec!");
                            stop = true;
                            CleanUp();
                            running = false;
                            return;
                        }

                        reset = false;
                    }

                    System.Threading.Thread.Sleep(50);
                }

                sr.StopRec();
            }
            else
            {
                Debug.Log("VoiceThreadError - InitFailed - StartRec!");
            }
        }

        CleanUp();

        running = false;
    }

    /// </summary>
    private void CleanUp()
    {
        if (sr != null)
        {
            sr.Dispose();
            sr = null;
        }
        if (source != null)
        {
            source.Dispose();
            source = null;
        }
    }

    public void Reset()
    {
        reset = true;
    }

    public void Stop()
    {
        stop = true;
    }

    public void Dispose()
    {
        if (running)
        {
            Stop();
        }
        while (running)
        {
            System.Threading.Thread.Sleep(5);
        }
    }

    private bool ConfigureSpeechRecognition()
    {
        /* Create the AudioSource instance */
        source = session.CreateAudioSource();

        /* Set audio volume to 0.2 */
        source.SetVolume(0.2f);

        /* Set Audio Source */
        source.SetDevice(sourceDeviceInfo[_activeSource]);

        pxcmStatus sts = session.CreateImpl<PXCMSpeechRecognition>(out sr);
        if (sts >= pxcmStatus.PXCM_STATUS_NO_ERROR)
        {
            /* Configure */
            PXCMSpeechRecognition.ProfileInfo pinfo;
            sr.QueryProfile(_activeLanguage, out pinfo);
            sr.SetProfile(pinfo);

            /* Set Command/Control or Dictation */
            string[] cmds = new String[4] { "Create", "Save", "Load", "Run" };
            if (cmds != null && cmds.GetLength(0) != 0)
            {
                // voice commands available, use them
                sr.BuildGrammarFromStringList(1, cmds, null);
                sr.SetGrammar(1);
            }
        }
        else
        {
            Debug.Log("VoiceThreadError - InitFailed - CreateImpl!");
            return false;
        }
        return true;
    }

    private void OnAlert(PXCMSpeechRecognition.AlertData data)
    {
        AlertDetected = data.label.ToString();
    }

    public static PXCMAudioSource.DeviceInfo GetCheckedSource()
    {
        UnityEngine.Debug.Log("SELECTED : " + devices[0].name);
        return devices[0];
    }

    public bool SetSource(string Source)
    {
        for (int i = 0; i < AvailableSources.Length; i++)
        {
            if (AvailableSources[i].Equals(Source))
            {
                _activeSource = i;
                return true;
            }
        }
        return false;
    }


    static void OnRecognition(PXCMSpeechRecognition.RecognitionData data)
    {

        UnityEngine.Debug.Log("RECOGNIZED sentence : " + data.scores[0].sentence);
        UnityEngine.Debug.Log("RECOGNIZED tags : " + data.scores[0].tags);

        if (data.scores[0].sentence == "Create")
            UnityEngine.Debug.Log("Call Create Function");
        if (data.scores[0].sentence == "Save")
            UnityEngine.Debug.Log("Call Save Function");
        if (data.scores[0].sentence == "Load")
            UnityEngine.Debug.Log("Call Load Function");
        if (data.scores[0].sentence == "Run")
            UnityEngine.Debug.Log("Call Run Function");
    }

}