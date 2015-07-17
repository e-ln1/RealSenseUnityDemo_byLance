/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

******************************************************************************/

using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

namespace RSUnityToolkit
{	
	/// <summary>
	/// Sense toolkit speech manager.
	/// </summary>
	[Serializable]
	public class SenseToolkitSpeechManager
	{
		public int tey;
		
		#region Public Fields
			
		/// <summary>
		/// Gets or sets the active source.
		/// </summary>		
		public int ActiveSource {
			get { return _activeSource;}
			set { 
				_activeSource = value;
				Reset();
			}
		}
			
		/// <summary>
		/// Gets or sets the active module.
		/// </summary>		
		public int ActiveModule {
			get { return _activeModule;}
			set { 
				_activeModule = value;
				Reset();
			}
		}
		
		/// <summary>
		/// Gets or sets the active language.
		/// </summary>		
		public int ActiveLanguage {
			get { return _activeLanguage;}
			set { 
				_activeLanguage = value;
				Reset();
			}
		}
		
		/// <summary>
		/// The speech module mode (dictation / command-control).
		/// </summary>
		public SpeechModuleModeType SpeechModuleMode;
		
		/// <summary>
		/// The audio sources.
		/// </summary>
		public string[] AvailableSources;
		
		/// <summary>
		/// The speech modules.
		/// </summary>
		public string[] AvailableModules;
		
		/// <summary>
		/// The supported languages.
		/// </summary>
		[SerializeField]
		public string[] AvailableLanguages;
		
		/// <summary>
		/// Is initialized indicator.
		/// </summary>
		public bool IsInitialized
		{
			get { return _isInitialized; }
		}
		
		/// <summary>
		/// Gets a value indicating whether this instance is running.
		/// </summary>		
		public bool IsRunning
		{
			get { return running; }
		}
				
		
		/// <summary>
		/// Voice recognition commands
		/// </summary>
		public string[] Commands;
		
		/// <summary>
		/// Error detected indicator.
		/// </summary>
		public SpeechManagerErrorType ErrorDetected;
		
		/// <summary>
		/// Alert Detected
		/// </summary>
		public string AlertDetected = "No Alert";
		
		#endregion
		
		#region Private Fields
		
		private PXCMSession session;		
		private bool _isInitialized = false;
		
		private PXCMAudioSource.DeviceInfo[] sourceDeviceInfo;
		private int[] modulesIuID;

		
		private PXCMAudioSource source;
		private PXCMSpeechRecognition sr;
		
		private bool running = false;
		private bool stop = true;
		private bool reset = false;
				
		private Dictionary<string,int> commanndsRecognized = new Dictionary<string, int>();
		private List<string> sentencesRecognized = new List<string>();

		[SerializeField]
		private int _activeSource = 0;
		[SerializeField]
		private int _activeModule = 0;
		[SerializeField]
		private int _activeLanguage = 0;
		
		#endregion
		
		#region Public Methods
		
		/// <summary>
		/// Initializes the speech module.
		/// </summary>		
		public bool InitalizeSpeech()
		{
			if (_isInitialized)
			{
				return true;
			}
			
			ErrorDetected = SpeechManagerErrorType.NoError;
			SpeechModuleMode = SpeechModuleModeType.CommandControl;
			
			_isInitialized = false;
			
			session = PXCMSession.CreateInstance();
			
			//Get Sources
			source = session.CreateAudioSource();
			if (source == null)
            {
				SetError(SpeechManagerErrorType.CreateAudioSourceFailed);				
				return false;;
			}
			
			int numOfDevices = 0;
			for (int i=0; ; i++)
            {
                PXCMAudioSource.DeviceInfo dinfo;
                if (source.QueryDeviceInfo(i, out dinfo) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
				numOfDevices++;
			}
			
			if (numOfDevices == 0) 
			{
				Debug.Log("No Audio Device Found!");
				SetError(SpeechManagerErrorType.NoAudioDeviceFound);	
				return false;
			}
						
            source.ScanDevices();
			AvailableSources = new string[numOfDevices];		
			sourceDeviceInfo = new PXCMAudioSource.DeviceInfo[numOfDevices];
			
            for (int i=0; i<numOfDevices ; i++)
            {
                PXCMAudioSource.DeviceInfo dinfo;
                if (source.QueryDeviceInfo(i, out dinfo) < pxcmStatus.PXCM_STATUS_NO_ERROR) 
				{	
					AvailableSources[i] = "FailedToQueryDeviceInfo";
					sourceDeviceInfo[i] = null;
					Debug.Log("QueryDeviceInfo Failed for Index " + i.ToString());
					SetError(SpeechManagerErrorType.QueryDeviceInfoFailed);	
				}
				else 
				{
					sourceDeviceInfo[i] = dinfo;
					AvailableSources[i] = dinfo.name;
				}				
			}			
            source.Dispose();
            
			//Get Modules
			PXCMSession.ImplDesc desc = new PXCMSession.ImplDesc();			
			int NumOfModules = 0;
			
			PXCMSession.ImplDesc desc1;
            desc.cuids[0] = PXCMSpeechRecognition.CUID;
            for (int i = 0; ; i++)
            {                
				if (session.QueryImpl(desc, i, out desc1) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
				NumOfModules++;
			}
			
			if (NumOfModules == 0)
			{
				Debug.Log("No Audio Modules Found!");
				SetError(SpeechManagerErrorType.NoAudioModulesFound);	
				return false;
			}
			
			AvailableModules = new string[NumOfModules];
			modulesIuID = new int[NumOfModules];
			
			for (int i=0; i<NumOfModules ; i++)
			{
                if (session.QueryImpl(desc, i, out desc1) < pxcmStatus.PXCM_STATUS_NO_ERROR) 
				{
					AvailableModules[i] = "FailedToQueryModuleInfo";
					Debug.Log("QueryImpl Failed for Index " + i.ToString());
					SetError(SpeechManagerErrorType.QueryImplFailed);
				}
				else
				{
					AvailableModules[i] = desc1.friendlyName;					
					modulesIuID[i] = desc1.iuid;
				}                
            }
			
			//Get Languages
			//PXCMSession.ImplDesc desc = new PXCMSession.ImplDesc();
            //desc.cuids[0] = PXCMSpeechRecognition.CUID;
			
			desc.iuid = -1;
			for (int i=0; i<NumOfModules ; i++)
			{
				if (!AvailableModules[i].Equals("FailedToQueryModuleInfo")) {
					desc.iuid=modulesIuID[i];
					break;
				}
			}
			
			if (desc.iuid == -1)
			{
				Debug.Log("No Valid Module Found!");
				SetError(SpeechManagerErrorType.NoValidModuleFound);
				return false;
			}
            

            PXCMSpeechRecognition vrec;
            if (session.CreateImpl<PXCMSpeechRecognition>(desc, out vrec) < pxcmStatus.PXCM_STATUS_NO_ERROR) 
			{
				Debug.Log("CreateImpl for Languages Failed!");
				SetError(SpeechManagerErrorType.CreateImplFailed);				
				return false;
			}
    
			int NumOfLanguages = 0;
            for (int i=0; ; i++)
            {
                PXCMSpeechRecognition.ProfileInfo pinfo;
                if (vrec.QueryProfile(i,out pinfo) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
				NumOfLanguages++;                
            }
			
			AvailableLanguages = new string[NumOfLanguages];
			for (int i=0; i<NumOfLanguages ; i++)
            {
                PXCMSpeechRecognition.ProfileInfo pinfo;
                if (vrec.QueryProfile(i,out pinfo) < pxcmStatus.PXCM_STATUS_NO_ERROR) 
				{
					AvailableLanguages[i] = "FailedToQueryProfile";
					Debug.Log("QueryProfile for Languages Failed!");
					SetError(SpeechManagerErrorType.QueryProfileFailed);						
				}
				else
				{
					AvailableLanguages[i] = LanguageToString(pinfo.language);
				}
            }
            vrec.Dispose();
			
			_isInitialized = true;
			
			return true;
		}
		
		/// <summary>
		/// Starts the voice recognition thread
		/// </summary>		
		public bool Start()
		{
			if (!_isInitialized) 
			{
				Debug.Log("Module Not Initalized!");
				SetError(SpeechManagerErrorType.NotInitalized);
				return false;
			}
			
			stop = false;
			reset = false;
			
			System.Threading.Thread thread = new System.Threading.Thread(DoVoiceRecognition);
            thread.Start();
            System.Threading.Thread.Sleep(5);
			return true;
		}		
		
		/// <summary>
		/// Resets voice recognition
		/// </summary>
		public void Reset()
		{
			reset = true;
		}	
		
		/// <summary>
		/// Stop voice recognition
		/// </summary>
		public void Stop()
		{
			stop = true;	
		}
		
		/// <summary>
		/// Releases all resource used by the <see cref="RSUnityToolkit.SenseToolkitSpeechManager"/> object.
		/// </summary>
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
				
		/// <summary>
		/// Queries the recognized commands.
		/// </summary>		
		public bool QueryRecognizedCommands(out Dictionary<string,int> CommanndsRecognized)
		{
			if (SpeechModuleMode == SpeechModuleModeType.Dictation)
			{
				CommanndsRecognized = null;
				return false;
			}
			CommanndsRecognized = new Dictionary<string, int>(commanndsRecognized);
			commanndsRecognized.Clear();
			return true;
		}
		
		/// <summary>
		/// Queries the recognized sentences.
		/// </summary>		
		public bool QueryRecognizedSentences(out List<string> SentencesRecognized)
		{
			if (SpeechModuleMode == SpeechModuleModeType.CommandControl)
			{
				SentencesRecognized = null;
				return false;
			}
			SentencesRecognized = new List<string>(sentencesRecognized);
			sentencesRecognized.Clear();
			return true;
		}
		
		/// <summary>
		/// Sets the source.
		/// </summary>		
		public bool SetSource(string Source)
		{
			for (int i=0; i<AvailableSources.Length; i++)
			{
				if (AvailableSources[i].Equals(Source))
				{
					_activeSource = i;
					return true;
				}
			}
			return false;
		}
		
		/// <summary>
		/// Sets the Module.
		/// </summary>		
		public bool SetModule(string Module)
		{
			for (int i=0; i<AvailableModules.Length; i++)
			{
				if (AvailableModules[i].Equals(Module))
				{
					_activeModule = i;
					return true;
				}
			}
			return false;
		}
		
		/// <summary>
		/// Sets the Language.
		/// </summary>		
		public bool SetLanguage(string Language)
		{
			for (int i=0; i<AvailableLanguages.Length; i++)
			{
				if (AvailableLanguages[i].Equals(Language))
				{
					_activeLanguage = i;
					return true;
				}
			}
			return false;
		}
		
		#endregion
		
		#region Private Methods
		
		private bool SetError(SpeechManagerErrorType error)
		{
			if ((ErrorDetected == SpeechManagerErrorType.NoError) || (ErrorDetected == SpeechManagerErrorType.AlertDetected))
			{
				ErrorDetected = error;
				return true;
			}
			return false;
		}
		
		private string LanguageToString(PXCMSpeechRecognition.LanguageType language)
        {
            switch (language)
            {
                case PXCMSpeechRecognition.LanguageType.LANGUAGE_US_ENGLISH: return "US English";
                case PXCMSpeechRecognition.LanguageType.LANGUAGE_GB_ENGLISH: return "British English";
                case PXCMSpeechRecognition.LanguageType.LANGUAGE_DE_GERMAN: return "Deutsch";
                case PXCMSpeechRecognition.LanguageType.LANGUAGE_IT_ITALIAN: return "italiano";
                case PXCMSpeechRecognition.LanguageType.LANGUAGE_BR_PORTUGUESE: return "PORTUGUÊS";
                case PXCMSpeechRecognition.LanguageType.LANGUAGE_CN_CHINESE: return "中文";
                case PXCMSpeechRecognition.LanguageType.LANGUAGE_FR_FRENCH: return "Français";
                case PXCMSpeechRecognition.LanguageType.LANGUAGE_JP_JAPANESE: return "日本語";
                case PXCMSpeechRecognition.LanguageType.LANGUAGE_US_SPANISH: return "español";
               // case PXCMSpeechRecognition.LanguageType.LANGUAGE_RU_RUSSIAN: return "Русский";
            }
            return null;
        }		
		
		/// <summary>
		/// Runs on a different thread
		/// </summary>
		private void DoVoiceRecognition()
        {
			if (running)
			{
				Debug.Log("Failed to start voice recognition - already running");
				SetError(SpeechManagerErrorType.VoiceThreadError_AlreadyRunning);
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
                if (sts>=pxcmStatus.PXCM_STATUS_NO_ERROR) 
				{
                    while (!stop) 
					{
						if (reset)
						{
							sr.StopRec();							
							ConfigureSpeechRecognition();
							sts = sr.StartRec(source, handler);
			                if (sts<pxcmStatus.PXCM_STATUS_NO_ERROR) 
							{
								Debug.Log("VoiceThreadError - ResetFailed - StartRec!");
		        				SetError(SpeechManagerErrorType.VoiceThreadError_ResetFailed_StartRec);
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
		        	SetError(SpeechManagerErrorType.VoiceThreadError_InitFailed_StartRec);
                }
	        } 
			
            CleanUp();
	        
			running = false;
        }
        
		/// <summary>
		/// Configures the speech recognition.
		/// </summary>		
		private bool ConfigureSpeechRecognition()
		{
			/* Create the AudioSource instance */
            source=session.CreateAudioSource();

            /* Set audio volume to 0.2 */
            source.SetVolume(0.2f);

        	/* Set Audio Source */
	        source.SetDevice(sourceDeviceInfo[_activeSource]);			

	        /* Set Module */
            PXCMSession.ImplDesc mdesc = new PXCMSession.ImplDesc();
            mdesc.iuid=modulesIuID[_activeModule];
			
			pxcmStatus sts = session.CreateImpl<PXCMSpeechRecognition>(out sr);
            if (sts >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                /* Configure */
                PXCMSpeechRecognition.ProfileInfo pinfo;
                sr.QueryProfile(_activeLanguage, out pinfo);
                sr.SetProfile(pinfo);
                
                /* Set Command/Control or Dictation */
                if (SpeechModuleMode == SpeechModuleModeType.CommandControl)
                {
                    string[] cmds = Commands;
                    if (cmds != null && cmds.GetLength(0) != 0)
                    {
                        // voice commands available, use them
				        sr.BuildGrammarFromStringList(1, cmds, null);
                        sr.SetGrammar(1);
                    } 
					else 
					{						
						Debug.Log("Speech Command List Empty!");
						SetError(SpeechManagerErrorType.VoiceThreadError_CommandsListEmpty);
						
						//Switch to dictaction mode
						//SpeechModuleMode = SpeechModuleModeType.Dictation;
                        //sr.SetDictation();
                    }
                }
                else
                {                    
                    sr.SetDictation();
                }
			}
			else 
			{
				Debug.Log("VoiceThreadError - InitFailed - CreateImpl!");
		        SetError(SpeechManagerErrorType.VoiceThreadError_InitFailed_CreateImpl);
				return false;
        	}
			return true;
		}
				
		/// <summary>
		/// On word recognized
		/// </summary>		
		private void OnRecognition(PXCMSpeechRecognition.RecognitionData data)
        {
            if (data.scores[0].label < 0)
            {
				//sentencesRecognized.Add(data.scores[0].sentence);
                //if (data.scores[0].tags.Length > 0)
				//{
					//tagsRecognized.Add(data.scores[0].tags);
				//}
            }
            else
            {                
                for (int i = 0; i < PXCMSpeechRecognition.NBEST_SIZE; i++)
                {
                    int label = data.scores[i].label;
                    int confidence = data.scores[i].confidence;
                    if (label < 0 || confidence == 0) continue;
					
					if (Commands.GetLength(0) < label)
					{
						Debug.Log("VoiceThreadError - UnrecognizedLabel");
						SetError(SpeechManagerErrorType.VoiceThreadError_UnrecognizedLabel);
						continue;
					}					
										
					if (commanndsRecognized.ContainsKey(Commands[label]))
					{
						commanndsRecognized.Remove(Commands[label]);
					}
                    commanndsRecognized.Add(Commands[label],confidence);
                }
                 
            }
        }
		
		/// <summary>
		/// On Alert
		/// </summary>		
        private void OnAlert(PXCMSpeechRecognition.AlertData data)
        {			
			AlertDetected = data.label.ToString();
			SetError(SpeechManagerErrorType.AlertDetected);
        }
		
		/// <summary>
		/// Cleans up.
		/// </summary>
	 	private void CleanUp() {
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
		
		#endregion
			
		#region Nested Types
		
		/// <summary>
		/// Error Indication of the speech manager 
		/// </summary>
		public enum SpeechManagerErrorType {
			NoError,
			AlertDetected,
			NotInitalized,
			CreateAudioSourceFailed,
			NoAudioDeviceFound,
			QueryDeviceInfoFailed,
			NoAudioModulesFound,
			QueryImplFailed,
			NoValidModuleFound,
			CreateImplFailed,
			QueryProfileFailed,
			VoiceThreadError_InitFailed_StartRec,
			VoiceThreadError_InitFailed_CreateImpl,
			VoiceThreadError_CommandsListEmpty,
			VoiceThreadError_AlreadyRunning,
			VoiceThreadError_ResetFailed_StartRec,
			VoiceThreadError_UnrecognizedLabel
		};
		
		/// <summary>
		/// Speech module mode type.
		/// </summary>
		public enum SpeechModuleModeType {
			CommandControl,
			Dictation
		}
		
		#endregion
	}
}

