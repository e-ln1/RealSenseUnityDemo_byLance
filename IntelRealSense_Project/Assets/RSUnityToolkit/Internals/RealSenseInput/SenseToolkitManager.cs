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
    /// Sense toolkit manager - this componenet is responsible for initializing the Sense SDK with the requested capabilities, according to the Options set.
    /// There must be one and only one instance of this script per scene.
    /// </summary>
    /// <exception cref='Exception'>
    /// Is thrown when the more than one instance is added to the scene
    /// </exception>
    public class SenseToolkitManager : MonoBehaviour 
	{
		
		#region Public Static fields

        public static SenseToolkitManager Instance = null;
		
		public static string AssetPrefabFolder = "Assets/RSUnityToolkit/Prefabs/";
	
		public MCTTypes.RGBQuality ColorImageQuality = MCTTypes.RGBQuality.VGA;
		 
		#endregion
		
        #region Public Properties/ fields

        public PXCMSenseManager SenseManager = null;
		
		[SerializeField]
		[HideInInspector]
		public SenseToolkitSpeechManager SpeechManager = null;

        public MCTTypes.RunModes RunMode = MCTTypes.RunModes.LiveStream;
		
        public string FilePath = "";
		
		public int NumberOfDetectedFaces = 2;
		
		public int MaxBlobsToDetect = 2;
		
        [HideInInspector]
        public bool Initialized;


        //-----------------------------------------
        // Output data
        //************

        [HideInInspector]
        public PXCMFaceData FaceModuleOutput = null;

        [HideInInspector]
        public PXCMHandData HandDataOutput = null;		

        [HideInInspector]
        public PXCMImage ImageRgbOutput = null;

        [HideInInspector]
        public PXCMImage ImageDepthOutput = null;

        [HideInInspector]
        public PXCMImage ImageMaskOutout = null;
		
		[HideInInspector]
        public PXCMImage ImageIROutput = null;
				
		[HideInInspector]
        public PXCMImage Image3DSegmentationOutput = null;
		
		[HideInInspector]
		public PXCMPoint3DF32[] PointCloud;
		
		[HideInInspector]
		public PXCMPointF32[] UvMap;		
		
		[HideInInspector]
		public Dictionary<string,int> SpeechOutput = null;
		
		public PXCMProjection Projection;
		public PXCMBlobExtractor BlobExtractor;
        //-----------------------------------------

        #endregion

        #region Private fields
		
				
		private List<SenseOption> _senseOptions = new List<SenseOption>();

		private pxcmStatus _sts;
		private PXCMCapture.Sample _captureSample;	
		private bool _isInitBlob = false;

		private Dictionary<string,int> _speechCommandsRef = new Dictionary<string, int>();
		private bool _speechCommandsChanged = false;
		
		private List<Action> DisposeFunctions = new List<Action>();
        #endregion
		
		#region CTOR
		
		public SenseToolkitManager() : base()			
		{
			/* Create a SpeechManager instance */
			if (SpeechManager == null)
			{				
				SpeechManager = new SenseToolkitSpeechManager();
				
				if (SpeechManager == null)
				{
					print("Unable to create the speech pipeline instance");
				}			
			}

		}
		
		#endregion
		
        #region Unity's overridden methods

        void Awake()
        {
            if (Instance != null)
            {
                throw new UnityException("Only one instance of SenseToolkitManager in a scene is allowed.");
            }
            Instance = this;
			
			// sets default options
			_senseOptions.Add( new SenseOption(SenseOption.SenseOptionID.Face){ ModuleCUID = PXCMFaceModule.CUID } );
			_senseOptions.Add( new SenseOption(SenseOption.SenseOptionID.Hand){ ModuleCUID = PXCMHandModule.CUID } );			
			_senseOptions.Add( new SenseOption(SenseOption.SenseOptionID.Object){ ModuleCUID = PXCMTracker.CUID } );	
			_senseOptions.Add( new SenseOption(SenseOption.SenseOptionID.VideoColorStream));
			_senseOptions.Add( new SenseOption(SenseOption.SenseOptionID.VideoDepthStream));
			_senseOptions.Add( new SenseOption(SenseOption.SenseOptionID.VideoIRStream));			
			_senseOptions.Add( new SenseOption(SenseOption.SenseOptionID.PointCloud));			
			_senseOptions.Add( new SenseOption(SenseOption.SenseOptionID.UVMap));			
			_senseOptions.Add( new SenseOption(SenseOption.SenseOptionID.Speech));	
			_senseOptions.Add( new SenseOption(SenseOption.SenseOptionID.VideoSegmentation){ModuleCUID = PXCM3DSeg.CUID} );		
        }

        void OnEnable()
        {
            Initialized = false;

            /* Create a SenseManager instance */
			SenseManager = PXCMSenseManager.CreateInstance();
			
            if (SenseManager == null)
            {
                print("Unable to create the pipeline instance");
                return;
            }
			
			if (_speechCommandsRef.Count != 0)
			{
				SetSenseOption(SenseOption.SenseOptionID.Speech);
			}

            int numberOfEnabledModalities = 0;

            //Set mode according to RunMode - play from file / record / live stream
            if (RunMode == MCTTypes.RunModes.PlayFromFile)
            {
                //CHECK IF FILE EXISTS
                if (!System.IO.File.Exists(FilePath))
                {
                    Debug.LogWarning("No Filepath Set Or File Doesn't Exist, Run Mode Will Be Changed to Live Stream");
                    RunMode = MCTTypes.RunModes.LiveStream;
                }
                else
                {
                    PXCMCaptureManager cManager = SenseManager.QueryCaptureManager();
                    cManager.SetFileName(FilePath, false);
                    Debug.Log("SenseToolkitManager: Playing from file: " + FilePath);
                }
            }

            if (RunMode == MCTTypes.RunModes.RecordToFile)
            {
                //CHECK IF PATH
                string PathOnly = FilePath;
                while (!PathOnly[PathOnly.Length - 1].Equals('\\'))
                {
                    PathOnly = PathOnly.Remove(PathOnly.Length - 1, 1);
                }

                if (!System.IO.Directory.Exists(PathOnly))
                {
                    Debug.LogWarning("No Filepath Set Or Path Doesn't Exist, Run Mode Will Be Changed to Live Stream");
                    RunMode = MCTTypes.RunModes.LiveStream;
                }
                else
                {
                    PXCMCaptureManager cManager = SenseManager.QueryCaptureManager();
                    cManager.SetFileName(FilePath, true);
                    Debug.Log("SenseToolkitManager: Recording to file: " + FilePath);
                }
            }					
			
            /* Enable modalities according to the set options*/
            if (IsSenseOptionSet(SenseOption.SenseOptionID.Face, true))
            {				            	
				SenseManager.EnableFace();                     			
				_senseOptions.Find( i => i.ID == SenseOption.SenseOptionID.Face).Initialized = true;
				_senseOptions.Find( i => i.ID == SenseOption.SenseOptionID.Face).Enabled = true;
				SetSenseOption(SenseOption.SenseOptionID.VideoColorStream);
                numberOfEnabledModalities++;
            }
						
            if (IsSenseOptionSet(SenseOption.SenseOptionID.Hand, true))
            {
                _sts = SenseManager.EnableHand();
				_senseOptions.Find( i => i.ID == SenseOption.SenseOptionID.Hand).Initialized = true;
				_senseOptions.Find( i => i.ID == SenseOption.SenseOptionID.Hand).Enabled = true;
                numberOfEnabledModalities++;
            }
			
			if (IsSenseOptionSet(SenseOption.SenseOptionID.Object, true))
            {
				_sts = SenseManager.EnableTracker();
				_senseOptions.Find( i => i.ID == SenseOption.SenseOptionID.Object).Initialized = true;		
				_senseOptions.Find( i => i.ID == SenseOption.SenseOptionID.Object).Enabled = true;				
				numberOfEnabledModalities++;				
            }
			
			if (IsSenseOptionSet(SenseOption.SenseOptionID.Speech, true))
            {
				if (!SpeechManager.IsInitialized)
				{
					if (SpeechManager.InitalizeSpeech())
					{				
						_senseOptions.Find( i => i.ID == SenseOption.SenseOptionID.Speech).Initialized = true;	
						_senseOptions.Find( i => i.ID == SenseOption.SenseOptionID.Speech).Enabled = true;
						numberOfEnabledModalities++;				
					}
					else
					{
						UnsetSenseOption(SenseOption.SenseOptionID.Speech);
					}
				}
				else
				{
					_senseOptions.Find( i => i.ID == SenseOption.SenseOptionID.Speech).Initialized = true;	
					_senseOptions.Find( i => i.ID == SenseOption.SenseOptionID.Speech).Enabled = true;
					numberOfEnabledModalities++;		
				}	
            }
			
            if (IsSenseOptionSet(SenseOption.SenseOptionID.VideoDepthStream, true) ||
				IsSenseOptionSet(SenseOption.SenseOptionID.PointCloud, true))
            {
                SenseManager.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_DEPTH, 0, 0, 0);
				_senseOptions.Find( i => i.ID == SenseOption.SenseOptionID.VideoDepthStream).Initialized = true;
				_senseOptions.Find( i => i.ID == SenseOption.SenseOptionID.VideoDepthStream).Enabled = true;
                numberOfEnabledModalities++;
            }
			
			if (IsSenseOptionSet(SenseOption.SenseOptionID.VideoIRStream, true))
            {                
				SenseManager.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_IR, 0, 0, 0);
				_senseOptions.Find( i => i.ID == SenseOption.SenseOptionID.VideoIRStream).Initialized = true;
				_senseOptions.Find( i => i.ID == SenseOption.SenseOptionID.VideoIRStream).Enabled = true;
                numberOfEnabledModalities++;
            }

            if (IsSenseOptionSet(SenseOption.SenseOptionID.VideoColorStream, true))
            {
				if (ColorImageQuality == MCTTypes.RGBQuality.FullHD)
				{
					SenseManager.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_COLOR, 1920, 1080, 0);			
				}
				else if (ColorImageQuality == MCTTypes.RGBQuality.HD)
				{
					SenseManager.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_COLOR, 1280, 720, 0);			
				}
				else if (ColorImageQuality == MCTTypes.RGBQuality.HalfHD)
				{
					SenseManager.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_COLOR, 960, 540, 0);			
				}
				else 
				{
					SenseManager.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_COLOR, 640, 480, 0);			
				}
				_senseOptions.Find( i => i.ID == SenseOption.SenseOptionID.VideoColorStream).Initialized = true;
				_senseOptions.Find( i => i.ID == SenseOption.SenseOptionID.VideoColorStream).Enabled = true;
                numberOfEnabledModalities++;
            } 
			
			if (IsSenseOptionSet(SenseOption.SenseOptionID.VideoSegmentation, true))
            {			
				if (ColorImageQuality == MCTTypes.RGBQuality.FullHD)
				{
					SenseManager.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_COLOR, 1920, 1080, 0);			
				}
				else if (ColorImageQuality == MCTTypes.RGBQuality.HD)
				{
					SenseManager.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_COLOR, 1280, 720, 0);			
				}
				else if (ColorImageQuality == MCTTypes.RGBQuality.HalfHD)
				{
					SenseManager.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_COLOR, 960, 540, 0);			
				}
				else 
				{
					SenseManager.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_COLOR, 640, 480, 0);			
				}
				SenseManager.Enable3DSeg();		
				_senseOptions.Find( i => i.ID == SenseOption.SenseOptionID.VideoSegmentation).Initialized = true;
				_senseOptions.Find( i => i.ID == SenseOption.SenseOptionID.VideoSegmentation).Enabled = true;
                numberOfEnabledModalities++;
            } 

            /* Initialize the execution */
            _sts = SenseManager.Init();
            if (_sts < pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                if (numberOfEnabledModalities > 0)
                {
                    Debug.LogError("Unable to initialize all modalities");
                }
                return;
            } 
			//Set different configurations:
			
			// Face 
            if (IsSenseOptionSet(SenseOption.SenseOptionID.Face, true))
            {
                var faceModule = SenseManager.QueryFace();
                var faceConfiguration = faceModule.CreateActiveConfiguration();
                if (faceConfiguration == null) throw new UnityException("CreateActiveConfiguration returned null");

				faceConfiguration.Update();
                
				faceConfiguration.detection.isEnabled = true;
				faceConfiguration.detection.smoothingLevel = PXCMFaceConfiguration.SmoothingLevelType.SMOOTHING_DISABLED;                
				
            	faceConfiguration.landmarks.isEnabled = true;
				faceConfiguration.landmarks.smoothingLevel = PXCMFaceConfiguration.SmoothingLevelType.SMOOTHING_DISABLED;
				
            	faceConfiguration.pose.isEnabled = true;
				faceConfiguration.pose.smoothingLevel = PXCMFaceConfiguration.SmoothingLevelType.SMOOTHING_DISABLED;				
				
				faceConfiguration.DisableAllAlerts();
				
				faceConfiguration.strategy = PXCMFaceConfiguration.TrackingStrategyType.STRATEGY_APPEARANCE_TIME;
				
				if ((NumberOfDetectedFaces < 1) || (NumberOfDetectedFaces > 15))
				{
					Debug.Log("Ilegal value for Number Of Detected Faces, value is set to 1");
					NumberOfDetectedFaces = 1;
				}
				
				faceConfiguration.detection.maxTrackedFaces = NumberOfDetectedFaces;
				faceConfiguration.landmarks.maxTrackedFaces = NumberOfDetectedFaces;
				faceConfiguration.pose.maxTrackedFaces = NumberOfDetectedFaces;
				
				PXCMFaceConfiguration.ExpressionsConfiguration expressionConfig = faceConfiguration.QueryExpressions();
				expressionConfig.Enable();
				expressionConfig.EnableAllExpressions();
				
				
				faceConfiguration.ApplyChanges();
				faceConfiguration.Dispose();

                FaceModuleOutput = faceModule.CreateOutput();
				
				UnsetSenseOption(SenseOption.SenseOptionID.VideoColorStream);
            }
			
			// Hand
            if (IsSenseOptionSet(SenseOption.SenseOptionID.Hand, true))
            {
                PXCMHandModule handAnalysis = SenseManager.QueryHand();

                PXCMHandConfiguration handConfiguration = handAnalysis.CreateActiveConfiguration();
				if (handConfiguration == null) throw new UnityException("CreateActiveConfiguration returned null");
				                			
				handConfiguration.Update();   
                handConfiguration.EnableAllGestures();
                handConfiguration.EnableStabilizer(true);
				handConfiguration.DisableAllAlerts();
                handConfiguration.EnableSegmentationImage(false);
                handConfiguration.ApplyChanges();                             					
				handConfiguration.Dispose();
				
				HandDataOutput = handAnalysis.CreateOutput();
            }			
			
			if (IsSenseOptionSet(SenseOption.SenseOptionID.Object, true))
            {
				if (_senseOptions.Find( i => i.ID == SenseOption.SenseOptionID.Object).Enabled != true)
				{
					_senseOptions.Find( i => i.ID == SenseOption.SenseOptionID.Object).Enabled = true;
					OnDisable();
		            OnEnable();
				}				
            }
			
			if (IsSenseOptionSet(SenseOption.SenseOptionID.Speech, true))
            {				
				UpdateSpeechCommands();				
				SpeechManager.Start();
			}
			
			// Create an instance for the projection & blob extractor
			
			if (Projection == null)
			{
				Projection = SenseManager.QueryCaptureManager().QueryDevice().CreateProjection();
			}

			if (BlobExtractor == null)
			{
				SenseManager.session.CreateImpl<PXCMBlobExtractor>(out BlobExtractor);
			}			
			
			// Set initialization flag
            Initialized = true;
        }
		
		/// <summary>
		/// Returns true if IvCam is used.
		/// </summary>
        public bool IsIvcam()
        {			
            PXCMCapture.DeviceInfo info;
            SenseManager.QueryCaptureManager().device.QueryDeviceInfo(out info);				
    		
            if (info.model == PXCMCapture.DeviceModel.DEVICE_MODEL_IVCAM)
            {			
				return true;
			}
			return false;
        }

        void Start()
        {
		
        }	
		
        void OnDisable()
        {		
			//Disposses all modules
			
            Initialized = false;
            if (SenseManager == null) return;
			
			DisposeFunctions.ForEach( i=> i.DynamicInvoke());			

            if (FaceModuleOutput != null)
            {
                FaceModuleOutput.Dispose();
                FaceModuleOutput = null;
            }
            if (HandDataOutput != null)
            {
                SenseManager.PauseHand(true);
                HandDataOutput.Dispose();
                HandDataOutput = null;
            }
            if (ImageRgbOutput != null)
            {
                ImageRgbOutput.Dispose();
                ImageRgbOutput = null;
            }
            if (ImageDepthOutput != null)
            {
                ImageDepthOutput.Dispose();
                ImageDepthOutput = null;
            }
			
			if (ImageIROutput != null)
            {
                ImageIROutput.Dispose();
                ImageIROutput = null;
            }
			
			if (Image3DSegmentationOutput != null)
			{
				Image3DSegmentationOutput.Dispose();
				Image3DSegmentationOutput = null;
			}
			
			if (Projection != null)
            {
                Projection.Dispose();
                Projection = null;
            }

			if (BlobExtractor != null)
			{
				BlobExtractor.Dispose();
				BlobExtractor = null;
			}

			UvMap = null;

			PointCloud = null;            

            SenseManager.Dispose();
            SenseManager = null;

        }

        void OnApplicationQuit()
        {
            OnDisable();
			
			if (SpeechManager.IsInitialized)
			{
				SpeechManager.Dispose();						
			}
        }
		
        void Update()
        {

			
            //Dynamically Pause/Enable Modules
			int numberOfEnabledModules = 0;
			foreach (var option in _senseOptions)
			{
				if (option.RefCounter == 0 && option.Enabled )
				{
					if (option.ModuleCUID > 0)
					{
						SenseManager.PauseModule(option.ModuleCUID, true);
					}
					option.Enabled = false;
				}
				else if (option.RefCounter > 0  && !option.Enabled)
				{
					if (!option.Initialized)
					{						
						OnDisable();
		                OnEnable();
		                Start();						
					}
					if (option.ModuleCUID > 0)
					{
						SenseManager.PauseModule(option.ModuleCUID, false);
					}
					option.Enabled = true;
				}								
				
				if (option.Enabled)
				{
					numberOfEnabledModules++;
				}
			}
			
			//Update Speech commands if changed
			if (_speechCommandsChanged)
			{
				UpdateSpeechCommands();
				SpeechManager.Reset();
			}

            // Every frame update all the data
            if (Initialized && numberOfEnabledModules > 0)
            {
				_sts = SenseManager.AcquireFrame(true, 100);
                if (_sts == pxcmStatus.PXCM_STATUS_NO_ERROR)
                {
                    if (_senseOptions.Find(i => i.ID == SenseOption.SenseOptionID.VideoColorStream).Enabled)
                    {
                        if (ImageRgbOutput != null)
                        {
                            ImageRgbOutput.Dispose();
                        }
						
						if (_captureSample == null)
						{
							_captureSample = SenseManager.QuerySample();			
						}
						
						if (_captureSample.color != null)
                        {
                        	ImageRgbOutput = _captureSample.color;		
							ImageRgbOutput.QueryInstance<PXCMAddRef>().AddRef();
						}
                    }
                   	if (_senseOptions.Find(i => i.ID == SenseOption.SenseOptionID.VideoDepthStream).Enabled ||
						_senseOptions.Find(i => i.ID == SenseOption.SenseOptionID.PointCloud).Enabled)
                    {
                        if (ImageDepthOutput != null)
                        {
                            ImageDepthOutput.Dispose();
                        }
						
						if (_captureSample == null)
						{
							_captureSample = SenseManager.QuerySample();			
						}	
						
						if (_captureSample.depth != null)
                        {
                        	ImageDepthOutput = _captureSample.depth;
							ImageDepthOutput.QueryInstance<PXCMAddRef>().AddRef();

							if (!_isInitBlob)
							{
								PXCMImage.ImageInfo info = ImageDepthOutput.QueryInfo();
								BlobExtractor.Init(info);
								BlobExtractor.SetMaxBlobs(MaxBlobsToDetect);
								_isInitBlob = true;
							}


							if (PointCloud == null)
							{
								PointCloud = new PXCMPoint3DF32[ImageDepthOutput.info.width * ImageDepthOutput.info.height];
							}
							
							if (_senseOptions.Find(i => i.ID == SenseOption.SenseOptionID.PointCloud).Enabled)
							{
								if (PointCloud == null) 
								{
									PointCloud = new PXCMPoint3DF32[ImageDepthOutput.info.width * ImageDepthOutput.info.height];
								}
							
								_sts = Projection.QueryVertices(ImageDepthOutput, PointCloud);
							}															
							
							if (_senseOptions.Find(i => i.ID == SenseOption.SenseOptionID.UVMap).Enabled)
							{
								if (UvMap == null)
								{
									UvMap = new PXCMPointF32[ImageDepthOutput.info.width * ImageDepthOutput.info.height];
								}
							
								Projection.QueryUVMap(ImageDepthOutput, UvMap);
							} 															
						}					
                    }					
					if (_senseOptions.Find(i => i.ID == SenseOption.SenseOptionID.VideoIRStream).Enabled)
                    {
                        if (ImageIROutput != null)
                        {
                            ImageIROutput.Dispose();
                        }
						
						if (_captureSample == null)
						{
							_captureSample = SenseManager.QuerySample();		
						}
						
						if (_captureSample.ir != null)
                        {
                        	ImageIROutput = _captureSample.ir;
                        	ImageIROutput.QueryInstance<PXCMAddRef>().AddRef();
						}						
                    }
					
					if (_senseOptions.Find(i => i.ID == SenseOption.SenseOptionID.VideoSegmentation).Enabled)
                    {
                        if (Image3DSegmentationOutput != null)
                        {
                            Image3DSegmentationOutput.Dispose();
                        }
						
						PXCM3DSeg seg = SenseManager.Query3DSeg();
																
						if (seg != null)
						{
							Image3DSegmentationOutput = seg.AcquireSegmentedImage();
						}
                    }
					
                    if (_senseOptions.Find(i => i.ID == SenseOption.SenseOptionID.Face).Enabled)
                    {
                        FaceModuleOutput.Update();
                    }
					
                    if (_senseOptions.Find(i => i.ID == SenseOption.SenseOptionID.Hand).Enabled)
                    {
                        HandDataOutput.Update();
                    }
					
					_captureSample = null;									
        
					SenseManager.ReleaseFrame();
					
				}	
				
				//Speech
				if (_senseOptions.Find(i => i.ID == SenseOption.SenseOptionID.Speech).Enabled)				
				{										
					SpeechManager.QueryRecognizedCommands(out SpeechOutput);
				}
				
            }

        }
		
        #endregion

	
	

        #region Public Methods
		
		/// <summary>
        /// Add the given function to an internal list and this function will be called before calling the dispose on the SenseManager.
        /// </summary>
		public void AddDisposeFunction(Action func)
		{
			DisposeFunctions.Add(func);
		}
		
        /// <summary>
        /// Determines whether the flag is already set (and has at least one reference).
        /// </summary>
        public bool IsSenseOptionSet(SenseOption.SenseOptionID flag)
        {
			return IsSenseOptionSet(flag, false);
        }
				
		/// <summary>
        /// Determines whether the flag is already set (and has at least one reference) or if it is initialized
        /// </summary>
		public bool IsSenseOptionSet(SenseOption.SenseOptionID flag, bool orInitialized)
        {
			var option = _senseOptions.Find(i => i.ID == flag);
			if (option == null)
			{
				return false;
			}
			if (!orInitialized)
			{
				return option.RefCounter > 0;
			}
			else 
			{
				return option.Initialized || option.RefCounter > 0;
			}
            
        }

        /// <summary>
        /// Sets the sense option.
        /// </summary>
        /// <param name='flag'>
        /// Flag.
        /// </param>
        public void SetSenseOption(SenseOption.SenseOptionID flag)
        {
			var option = _senseOptions.Find(i => i.ID == flag);
			if (option == null)
			{
				option = new SenseOption(flag);
				_senseOptions.Add(option);
				
			}
			option.RefCounter++;
        }

        /// <summary>
        /// Unsets the sense option.
        /// </summary>
        /// <param name='flag'>
        /// Flag.
        /// </param>
        public void UnsetSenseOption(SenseOption.SenseOptionID flag)
        {
			var option = _senseOptions.Find(i => i.ID == flag);
			if (option == null)
			{
				option = new SenseOption(flag);
				_senseOptions.Add(option);
			}
			option.RefCounter--;
			if (option.RefCounter < 0)
			{
				option.RefCounter = 0;			
			}
        }
		
		/// <summary>
		/// Adds a speech command.
		/// </summary>		
		public bool AddSpeechCommand(string command)
		{
			if (command.Equals(""))
			{
				return false;
			}
			
			if (_speechCommandsRef.ContainsKey(command))
			{
				_speechCommandsRef[command]++;				
			}
			else
			{
				_speechCommandsRef.Add(command,1);		
				_speechCommandsChanged = true;
			}
			return true;
		}
		
		/// <summary>
		/// Removes a speech command.
		/// </summary>
		public bool RemoveSpeechCommand(string command)
		{
			if (command.Equals("") || !_speechCommandsRef.ContainsKey(command))
			{
				return false;
			}
						
			if (_speechCommandsRef[command] == 1)
			{
				_speechCommandsRef.Remove(command);		
				_speechCommandsChanged = true;
			}
			else
			{
				_speechCommandsRef[command]--;
			}
			return true;
		}
		
        #endregion
		
		#region Private Methods
		
		private void UpdateSpeechCommands()
		{
			SpeechManager.Commands = new string[_speechCommandsRef.Count];
				
			int i=0;
			foreach (KeyValuePair<string,int> cmdPair in _speechCommandsRef)
			{
				SpeechManager.Commands[i++] = cmdPair.Key;								
			}
			_speechCommandsChanged = false;
		}
		
		#endregion
    }
}