using UnityEngine;
using System.Collections.Generic;

namespace RSUnityToolkit
{	

	public class SenseOption
	{
		#region Nested Types
		
		public enum SenseOptionID
        {
            None = 0,
            All = 0xFFFF,
            VideoColorStream = 1,
            VideoDepthStream = 2,
            Hand = 4,
            Face = 8,
			VideoIRStream = 16,
			Speech,
			PointCloud,
			UVMap,
			Object,
			VideoSegmentation
        }
		
		#endregion
	
		#region Private Fields
		
		private SenseOptionID _id;
		
		#endregion
		
		#region Public Fields
		
		public int RefCounter;
		public int ModuleCUID;
		public bool Enabled = false;
		public bool Initialized = false;
		
		#endregion
		
		#region C'tor
		
		public SenseOption(SenseOptionID id)
		{
			_id = id;
			ModuleCUID = -1;
		}
			
		#endregion
		
		#region Public Properties
		
		public SenseOptionID ID
		{
			get
			{
				return _id;
			}
			private set
			{
				_id = value;
			}
		}
		
		#endregion
		
	}
}
