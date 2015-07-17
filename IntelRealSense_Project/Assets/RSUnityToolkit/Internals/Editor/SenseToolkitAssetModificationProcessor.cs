/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

*******************************************************************************/
using UnityEditor;
using UnityEngine;

using System.Collections.Generic;
using RSUnityToolkit;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

public class SenseToolkitAssetModificationProcessor : UnityEditor.AssetModificationProcessor
{
	
    public static string[] OnWillSaveAssets(string[] paths)
    { 
 		Resources.UnloadUnusedAssets();	
		
		foreach( string asset in paths )
        {
            // Check prefabs
            if ( asset.EndsWith(".prefab") )
            {               
                Object[] allObjs = AssetDatabase.LoadAllAssetsAtPath( asset );
				foreach (Object obj in allObjs)
				{
					
					if (obj is HiddenBehaviour)
					{
					 	if ( ((HiddenBehaviour)obj).ActionOwner == null )
						{
                			GameObject.DestroyImmediate( obj, true );
						}
						else if (((HiddenBehaviour)obj).ActionOwner.gameObject != ((HiddenBehaviour)obj).gameObject)
						{
							GameObject.DestroyImmediate( obj, true );
						}
						else if (obj is Trigger)
						{
							bool found = false;
							foreach( Trigger trigger in ((HiddenBehaviour)obj).ActionOwner.SupportedTriggers )
							{
								if (trigger == obj)
								{
									found = true;
									break;
								}
							}
							if (!found)
							{
								GameObject.DestroyImmediate( obj, true );
							}
						}
						else if (obj is BaseRule)
						{
							bool found = false;
							foreach( Trigger trigger in ((HiddenBehaviour)obj).ActionOwner.SupportedTriggers )
							{
								foreach( BaseRule rule in trigger.Rules )
								{
									if (rule == obj)
									{
										found = true;
										break;
									}
								}	
								if (found)
								{
									break;
								}
							}
							if (!found)
							{
								GameObject.DestroyImmediate( obj, true );
							}
						}
					}
				}
				
            }//Check level files
			else if ( asset.EndsWith(".unity") )
            {
                HiddenBehaviour[] allObjs = GameObject.FindObjectsOfType( typeof(HiddenBehaviour) ) as HiddenBehaviour[];
                foreach( var obj in allObjs )
		        {
		            if ( obj.ActionOwner == null )
					{
		                GameObject.DestroyImmediate( obj, true );
					}
					else if (obj.ActionOwner.gameObject != obj.gameObject) 
						//in case we copy/pasted or duplicated game object. we make sure that the owner of the script is the same owner as the ActionOwner field
					{
						GameObject.DestroyImmediate( obj, true );
					}
					else if (obj is Trigger)
					{
						bool found = false;
						foreach( Trigger trigger in obj.ActionOwner.SupportedTriggers )
						{
							if (trigger == obj)
							{
								found = true;
								break;
							}
						}
						if (!found)
						{
							GameObject.DestroyImmediate( obj, true );
						}
					}
					else if (obj is BaseRule)
					{
						bool found = false;
						foreach( Trigger trigger in obj.ActionOwner.SupportedTriggers )
						{
							foreach( BaseRule rule in trigger.Rules )
							{
								if (rule == obj)
								{
									found = true;
									break;
								}
							}	
							if (found)
							{
								break;
							}
						}
						if (!found)
						{
							GameObject.DestroyImmediate( obj, true );
						}
					}
		        }
            }
        }
		
		Resources.UnloadUnusedAssets();	
		
        return paths;
    }

}
