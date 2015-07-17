/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

*******************************************************************************/

using UnityEngine;
using System.Collections;

namespace RSUnityToolkit
{
    /// <summary>
    /// Always True Rule - Returns true
    /// </summary>
    [AddComponentMenu("")]
	[EventTrigger.EventTriggerAtt]
    public class AlwaysTrueRule : BaseRule
    {
        #region Private fields

        #endregion

        #region C'tor
        public AlwaysTrueRule() : base()
        {
            FriendlyName = "Always True";
        }

        #endregion

        #region Public Methods

        protected override bool OnRuleEnabled()
        {
            return true;
        }

        override public string GetIconPath()
        {
            return @"";
        }

        override public string GetRuleDescription()
        {
            return "Fires true at every frame";
        }

        public override bool Process(Trigger trigger)
        {
            trigger.ErrorDetected = false;
            bool success = false;

            if (trigger is EventTrigger)
            {
                EventTrigger specificTrigger = (EventTrigger)trigger;
                specificTrigger.Source = this.name;

                success = true;
            }
            return success;

        }

        #endregion
    }
}
