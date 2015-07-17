using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections;

public class SpeechTesting : MonoBehaviour {


    private PXCMSenseManager _pxcmSenseManager;

    private static PXCMSpeechSynthesis tts;
    private static AudioClip clip;
    private new static PXCMAudio audio;


	// Use this for initialization
    void Start()
    {
        _pxcmSenseManager = PXCMSenseManager.CreateInstance();

        if (_pxcmSenseManager == null)
        {
            Debug.LogError("SenseManager Initialization Failed");
        }
        else
        {
            int nbuffers;
            int nsamples;
            AudioSource aud = GetComponent<AudioSource>();
            String name = "test";
            Int32 sid = 1;

            _pxcmSenseManager.session.CreateImpl<PXCMSpeechSynthesis>(out tts);
            PXCMSpeechSynthesis.ProfileInfo pinfo;
            tts.QueryProfile(0, out pinfo);
            pinfo.language = PXCMSpeechSynthesis.LanguageType.LANGUAGE_US_ENGLISH;
            tts.SetProfile(pinfo);
            tts.BuildSentence(1, "The year is 2098.  Interstellar space travel was made possible in 2045.  But space travel is dangerous.  Human life support systems for interdimensional transport are costly and high risk.  The majority of trade ships are unmanned and piloted remotely using hyper dimensional virtual reality.  Use the voice command initialize to begin, or click the play button.");

            nsamples = tts.QuerySampleNum(sid);

            if (nsamples != 0)
            {
                tts.QueryProfile(out pinfo);
                clip = AudioClip.Create(
                    name,
                    nsamples * pinfo.outputs.nchannels,
                    pinfo.outputs.nchannels,
                    pinfo.outputs.sampleRate,
                    false
                );

                nbuffers = tts.QueryBufferNum(sid);

                for (int i = 0, offset = 0; i < nbuffers; i++)
                {
                    PXCMAudio audio = tts.QueryBuffer(sid, i);
                    PXCMAudio.AudioData data;
                    pxcmStatus sts = audio.AcquireAccess(
                        PXCMAudio.Access.ACCESS_READ,
                        PXCMAudio.AudioFormat.AUDIO_FORMAT_IEEE_FLOAT,
                        out data
                    );

                    if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR) break;

                    float[] samples = data.ToFloatArray();

                    clip.SetData(data.ToFloatArray(), offset);

                    offset += samples.Length;

                    audio.ReleaseAccess(data);
                }

                aud.clip = clip;
                aud.Play();
            }
        }

        tts.Dispose();
    }
	
}
