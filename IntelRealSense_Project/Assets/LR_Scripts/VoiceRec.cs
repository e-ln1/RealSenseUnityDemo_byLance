using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class VoiceRec : MonoBehaviour
{
	
	private PXCMAudioSource source;
	private PXCMSpeechRecognition sr;
	private PXCMSession session;
	
	private static List<PXCMAudioSource.DeviceInfo> devices = new List<PXCMAudioSource.DeviceInfo>();
	private PXCMAudioSource.DeviceInfo device = new PXCMAudioSource.DeviceInfo();
	
	[Range(0.2f, 0.8f)]
	public float setVolume = 0.2f;

    public static bool isReadyToLoadLevel { get; set; }
	public static bool isReadyToRestartLevel { get; set; }

	
	void Start()
	{
		session = PXCMSession.CreateInstance();
		audioSourceCheck();
		initSession(session);
	}

    void Update()
    {
        if (isReadyToLoadLevel)
        {
            Application.LoadLevel("RealSense_Testing");
            sr.StopRec();
			isReadyToLoadLevel = false;
            Debug.Log("Cleaning up in Update whilst Initializing ApplicationLoadLevel");
            sr.Dispose();
        }
		if (isReadyToRestartLevel)
		{
			Application.LoadLevel("SettingUpVoiceTests");
			sr.StopRec();
			isReadyToRestartLevel = false;
			Debug.Log("Cleaning up in Update whilst Restart for ApplicationLoadLevel");
			sr.Dispose();
		}
    }
	
	void audioSourceCheck()
	{
		/* Create the AudioSource instance */
		source = session.CreateAudioSource();
		
		if (source != null)
		{
			source.ScanDevices();
			
			for (int i = 0; ; i++)
			{
				PXCMAudioSource.DeviceInfo dinfo;
				if (source.QueryDeviceInfo(i, out dinfo) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
				
				devices.Add(dinfo);
				UnityEngine.Debug.Log("Device : " + dinfo.name);
			}
		}
	}
	
	void OnAlert(PXCMSpeechRecognition.AlertData data)
	{
		Debug.Log(data.label);
	}
	
	public static void OnRecognition(PXCMSpeechRecognition.RecognitionData data)
	{
		
		UnityEngine.Debug.Log("RECOGNIZED sentence : " + data.scores[0].sentence);
		UnityEngine.Debug.Log("RECOGNIZED tags : " + data.scores[0].tags);

        if (data.scores[0].sentence == "Initialize")
        {
            UnityEngine.Debug.Log("Initialize Link Start Function");
            isReadyToLoadLevel = true;
        }
        if (data.scores[0].sentence == "Restart")
        {
            UnityEngine.Debug.Log("Restart Function");
			isReadyToRestartLevel = true;
        }
		//if (data.scores[0].sentence == "Load")
        //{
        //    UnityEngine.Debug.Log("Call Load Function");
        //} 
        //if (data.scores[0].sentence == "Run")
        //{
        //    UnityEngine.Debug.Log("Call Run Function");
        //}
    }
	
	void initSession(PXCMSession session)
	{
		if (source == null)
		{
			Debug.Log("Source was null!  No audio device?");
			return;
		}
		
		// Set audio volume to 0.2 
		source.SetVolume(setVolume);
		
		// Set Audio Source 
		Debug.Log("Using device: " + device.name);
		source.SetDevice(device);
		
		// Set Module 
		PXCMSession.ImplDesc mdesc = new PXCMSession.ImplDesc();
		mdesc.iuid = 0;
		
		pxcmStatus sts = session.CreateImpl<PXCMSpeechRecognition>(out sr);
		
		if (sts >= pxcmStatus.PXCM_STATUS_NO_ERROR)
		{
			// Configure 
			PXCMSpeechRecognition.ProfileInfo pinfo;
			// Language
			sr.QueryProfile(0, out pinfo);
			Debug.Log(pinfo.language);
			sr.SetProfile(pinfo);
			
			// Set Command/Control or Dictation 
			sr.SetDictation();
			
			// Initialization 
			Debug.Log("Init Started");
			PXCMSpeechRecognition.Handler handler = new PXCMSpeechRecognition.Handler();
			handler.onRecognition = OnRecognition;
			handler.onAlert = OnAlert;
			
			sts = sr.StartRec(source, handler);
			
			if (sts >= pxcmStatus.PXCM_STATUS_NO_ERROR)
			{
				Debug.Log("Voice Rec Started");
			}
			else
			{
				Debug.Log("Voice Rec Start Failed");
			}
		}
		else
		{
			Debug.Log("Voice Rec Session Failed");
		}
	}
	/*
	public void OnDisable()
	{
		sr.StopRec();
		Debug.Log("Clean up using OnApplicationQuit");
		sr.Dispose();
	}
	*/
	public void OnApplicationQuit()
	{
		sr.StopRec();
		Debug.Log("Clean up using OnApplicationQuit");
		sr.Dispose();
	}

}