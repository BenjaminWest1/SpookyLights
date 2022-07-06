using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Text;
using UnityEngine.Windows.Speech;

[RequireComponent(typeof(AudioSource))]
public class VoiceInput : MonoBehaviour
{
    /*void Start ()
    {
        var audio = GetComponent<AudioSource>();
        audio.clip = Microphone.Start("Built-in Microphone", true, 10, 44100);
        audio.loop = true;
        while (!(Microphone.GetPosition(null) > 0))
            audio.Play();
        
    }*/


    /*[SerializeField] private string[] m_Keywords;

    private KeywordRecognizer m_Recognizer; */


    [SerializeField] private string useSpecificDevice = "";

    [SerializeField] private int refreshMicEveryNSeconds = 5;

    string deviceName;

    Color32 objColor;
    Color32 fire;

    public Light lightSource;
    public GameObject sphere;

    float micVariable;

    IEnumerator Start()
    {
        objColor = cube.GetComponent<MeshRenderer>().material.color;
        fire = sphere.GetComponent<MeshRenderer>().material.color;

        /*m_Recognizer = new KeywordRecognizer(m_Keywords);
        m_Recognizer.OnPhraseRecognized += OnPhraseRecognized;
        m_Recognizer.Start();*/

        var audio = GetComponent<AudioSource>();

        if (audio == null || Microphone.devices == null || Microphone.devices.Length == 0) yield break;

        deviceName = Microphone.devices.FirstOrDefault(x =>
            !string.IsNullOrEmpty(x) &&
            (string.IsNullOrEmpty(useSpecificDevice) ? true : x.Contains(useSpecificDevice))
        );
        if (string.IsNullOrEmpty(deviceName)) yield break;

        int minFreq, maxFreq;
        Microphone.GetDeviceCaps(deviceName, out minFreq, out maxFreq);
        int freq = maxFreq;

        foreach (var item in Microphone.devices)
            Debug.Log("Microphone device: " + item);
        Debug.Log("Using: " + deviceName);

        audio.loop = false;
        audio.clip = Microphone.Start(deviceName, true, 10, freq);


        /*while (Microphone.devices.Any(x => x == deviceName))
        {
            while (!Microphone.IsRecording(deviceName) || !audio.isPlaying)
            {

                refreshMicEveryNSeconds = Mathf.Clamp(refreshMicEveryNSeconds, 1, 60 * 60); // unity requires this to be between 1 second and an hour
                audio.clip = Microphone.Start(deviceName, true, refreshMicEveryNSeconds, freq);

                while (Microphone.GetPosition(deviceName) == 0)
                    yield return null;

                audio.Play();
                yield return null;
            }
            yield return null;
        }
        yield return null;*/
    }

    /*private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0} ({1}){2}", args.text, args.confidence, Environment.NewLine);
        builder.AppendFormat("\tTimestamp: {0}{1}", args.phraseStartTime, Environment.NewLine);
        builder.AppendFormat("\tDuration: {0} seconds{1}", args.phraseDuration.TotalSeconds, Environment.NewLine);
        Debug.Log(builder.ToString());

        if (args.text == "fireball")
        {
            Debug.Log("Ignition");
        }
        if (args.text == "lightning")
        {
            Debug.Log("Shazam");
        }
        if (args.text == "water")
        {
            Debug.Log("Tsunami");
        }
        if (args.text == "wind")
        {
            Debug.Log("Hurricane");
        }
        if (args.text == "earth")
        {
            Debug.Log("Quake");
        }
    }
    */

        public static float MicLoudness;

        public GameObject cube;

        void StopMicrophone()
        {
            Microphone.End(deviceName);
        }
     
 
        int _sampleWindow = 128;
     
        //get data from microphone into audioclip
        float  LevelMax()
        {
            float levelMax = 0;
            float[] waveData = new float[_sampleWindow];
            int micPosition = Microphone.GetPosition(null)-(_sampleWindow+1); // null means the first microphone
            if (micPosition < 0) return 0;
            GetComponent<AudioSource>().clip.GetData(waveData, micPosition);
            // Getting a peak on the last 128 samples
            for (int i = 0; i < _sampleWindow; i++) {
                float wavePeak = waveData[i] * waveData[i];
                if (levelMax < wavePeak) {
                    levelMax = wavePeak;
                }
            }
            return levelMax;
        }

        void FixedUpdate()
        {
            // levelMax equals to the highest normalized value power 2, a small number because < 1
            // pass the value to a static var so we can access it from anywhere
            MicLoudness = LevelMax ();
            
            if (MicLoudness * 100 > micVariable)
            {
                micVariable = 100 * MicLoudness;
            } else 
            {
                micVariable /= 1.25f;
            }

            /*objColor.g = (byte) ((micVariable * 255) %256);

            if (objColor.g > 30)
            {
                cube.GetComponent<Renderer>().material.SetColor("_Color", objColor);
                Debug.Log((byte) (micVariable * 255));
            }
            */

            if (micVariable < 0.1f) 
            {
                micVariable = 0.1f;
            }

            lightSource.intensity = micVariable;

            sphere.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.red * (micVariable / 4));
        }
     
        //stop mic when loading a new level or quit application
        void OnDisable()
        {
            StopMicrophone();
        }
     
        void OnDestroy()
        {
            StopMicrophone();
        }
}