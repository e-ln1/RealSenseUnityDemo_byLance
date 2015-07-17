/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

******************************************************************************/

using UnityEngine;
using System.Collections.Generic;

namespace RSUnityToolkit
{	
	public static class MCTTypes
	{
		#region Gestures
		
		private static Dictionary<RSUnityToolkitGestures,string> _gesturesEnumToString;		
		
		#region C'tors
		
		static MCTTypes()
		{
			_gesturesEnumToString = new Dictionary<RSUnityToolkitGestures, string>();
			_gesturesEnumToString.Add(RSUnityToolkitGestures.FingersSpread,"spreadfingers");
			_gesturesEnumToString.Add(RSUnityToolkitGestures.TwoFingersPinch,"two_fingers_pinch_open");
			_gesturesEnumToString.Add(RSUnityToolkitGestures.V_Sign,"v_sign");
			_gesturesEnumToString.Add(RSUnityToolkitGestures.ThumbUp,"thumb_up");
			_gesturesEnumToString.Add(RSUnityToolkitGestures.ThumbDown,"thumb_down");			
			_gesturesEnumToString.Add(RSUnityToolkitGestures.Grab,"fist");	
			_gesturesEnumToString.Add(RSUnityToolkitGestures.Tap,"tap");	
			_gesturesEnumToString.Add(RSUnityToolkitGestures.Wave,"wave");	
			_gesturesEnumToString.Add(RSUnityToolkitGestures.Swipe,"swipe");	
			_gesturesEnumToString.Add(RSUnityToolkitGestures.FullPinch,"full_pinch");	
			_gesturesEnumToString.Add(RSUnityToolkitGestures.None,"");
		}
		
		#endregion
		
		#region Public Methods
		
		/// <summary>
		/// Gets the gesture's name from the enum value.
		/// </summary>
		/// <param name='gesture'>
		/// Gesture's enum value
		/// </param>
		public static string GetGesture(RSUnityToolkitGestures gesture)
		{
			if (_gesturesEnumToString.ContainsKey(gesture))
			{
				return _gesturesEnumToString[gesture];
			}
			return "";
		}
		
		/// <summary>
		/// Gets the gesture enum value from its' name. If does not exist, reutnr NONE value
		/// </summary>
		/// <param name='gesture'>
		/// Gesture's name
		/// </param>
		public static RSUnityToolkitGestures GetGesture(string gesture)
		{
			if (_gesturesEnumToString.ContainsValue(gesture))
			{
				foreach (KeyValuePair<RSUnityToolkitGestures,string> kvp in _gesturesEnumToString)
				{
					if (kvp.Value.Equals(gesture))
					{
						return kvp.Key;
					}
				}
			}
			return RSUnityToolkitGestures.None;
		}
		
		#region Nested Types
		
		/// <summary>
		/// Internal gestures enum for convenience purposes
		/// </summary>
		public enum RSUnityToolkitGestures
		{
			FingersSpread = 0,
			TwoFingersPinch,
			V_Sign,
			Grab,			
			ThumbUp,
			ThumbDown,
			Tap,
			Wave,
			Swipe,
			FullPinch,
			None
		}
		#endregion
		
		#endregion
		
		#endregion
		/// <summary>
		/// Describe the input (live/file) or if to record to a file
		/// </summary>
        public enum RunModes
        {
            LiveStream = 0,
            PlayFromFile,
            RecordToFile,
        }
		
		public enum RGBQuality
		{
			VGA = 1,
			HalfHD,
			HD,
			FullHD
		}
	}
}