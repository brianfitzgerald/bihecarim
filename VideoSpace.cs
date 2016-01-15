using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System;
using System.Text.RegularExpressions;


namespace Videospace
{
    struct VideoLocation
    {
        public List<float> Location;
        public List<float> Rotation;
        public List<float> Scale;
    }

    public static class Utilities
    {

        public static void PlayAudio(string clipFilename)
        {
            Debug.Log("playing soundtrack");
            Debug.Log(clipFilename);
            try
            {
                var clip = Resources.Load(clipFilename) as AudioClip;
                AudioSource.PlayClipAtPoint(clip, Vector3.zero);
            }
            catch
            {
                Debug.Log("Audio not found");
            }
        }

        public static void ActivateScreenLayout(string screenLayoutName)
        {
            Debug.Log(screenLayoutName);

            GameObject screensRoot = GameObject.Find("Screens");
            screensRoot.SetActive(true);
            foreach (Transform child in screensRoot.transform)
            {
                child.gameObject.SetActive(false);
            }

            Transform selectedLayoutRoot = screensRoot.transform.FindChild(screenLayoutName);
            selectedLayoutRoot.gameObject.SetActive(true);
            foreach (Transform child in selectedLayoutRoot)
            {
                child.gameObject.SetActive(true);
                child.gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
        }

    }
    public abstract class Step
    {
        public abstract float StartTime { get; set; }
        public abstract void Run();
        public abstract void Check(float currentTime);
    }

    public class PlayForDuration : Step
    {
        public override float StartTime { get; set; }

        public string VideoSource { get; set; }
        public string DestinationScreenName { get; set; }
        public float Duration { get; set; }

        GameObject targetObject;

        public PlayForDuration(float startTime, float duration, string videoSource, string destinationScreenName)
        {
            StartTime = startTime;
            VideoSource = videoSource;
            DestinationScreenName = destinationScreenName;
            Duration = duration;
            targetObject = GameObject.Find(DestinationScreenName);
        }

        public override void Run()
        {
            Debug.Log(VideoSource + " run");
            targetObject.GetComponent<MeshRenderer>().enabled = true;
            MovieTexture movieTexture = (MovieTexture)Resources.Load("videos/" + VideoSource) as MovieTexture;
            targetObject.GetComponent<Renderer>().material.mainTexture = movieTexture;
            MovieTexture movieComponent = ((MovieTexture)targetObject.GetComponent<Renderer>().material.mainTexture);

            movieComponent.Play();
        }

        public override void Check(float currentTime)
        {
            if (currentTime.Equals(this.Duration + this.StartTime))
            {
                GameObject targetToDisable = GameObject.Find(this.DestinationScreenName);
                MovieTexture movieComponent = ((MovieTexture)targetToDisable.GetComponent<Renderer>().material.mainTexture);
                movieComponent.Stop();
                targetToDisable.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }


    public class PlayUntilFinished : Step
    {
        public override float StartTime { get; set; }

        public string VideoSource { get; set; }
        public string DestinationScreenName { get; set; }
        public float Duration { get; set; }
        public string AudioOption { get; set; }

        GameObject targetObject;
        MovieTexture movieTexture;

        public PlayUntilFinished(float startTime, string videoSource, string destinationScreenName, string audioOption)
        {
            StartTime = startTime;
            VideoSource = videoSource;
            AudioOption = audioOption;
            DestinationScreenName = destinationScreenName;
        }

        public override void Run()
        {
            targetObject = GameObject.Find(DestinationScreenName);
            Debug.Log(VideoSource);
            Debug.Log(targetObject);


            targetObject.GetComponent<MeshRenderer>().enabled = true;
            movieTexture = (MovieTexture) Resources.Load("videos/" + VideoSource) as MovieTexture;
            this.Duration = movieTexture.duration * 100;
            targetObject.GetComponent<Renderer>().material.mainTexture = movieTexture;
            MovieTexture movieComponent = ((MovieTexture)targetObject.GetComponent<Renderer>().material.mainTexture);
            movieComponent.Play();

            if (this.AudioOption != null)
            {
                if (this.AudioOption.Equals("internal"))
                {
                    AudioSource aud = targetObject.AddComponent<AudioSource>();
                    aud.clip = movieTexture.audioClip;
                    aud.Play();
                }
            }

        }

        public override void Check(float currentTime)
        {
            if (currentTime.Equals(this.Duration + this.StartTime))
            {
                Debug.Log("PUF clip stopped");
                MovieTexture movieComponent = ((MovieTexture)targetObject.GetComponent<Renderer>().material.mainTexture);
                movieComponent.Stop();
                targetObject.GetComponent<MeshRenderer>().enabled = false;
            }
        }

    }


    public class PlayInSequence : Step
    {
        public override float StartTime { get; set; }

        List<string> VideoSources;
        public string DestinationScreenName { get; set; }
        public float CurrentVideoEndTime { get; set; }

        GameObject targetObject;
        int currentVideoIndex;

        public PlayInSequence(float startTime, List<string> videoSources, string destinationScreenName)
        {
            StartTime = startTime;
            DestinationScreenName = destinationScreenName;
            currentVideoIndex = 0;
            VideoSources = videoSources;

        }
        public override void Run()
        {
            targetObject = GameObject.Find(this.DestinationScreenName);
            Debug.Log("sequence run");
            Debug.Log(targetObject);
            targetObject.GetComponent<Renderer>().enabled = true;
            MovieTexture firstTexture = (MovieTexture)Resources.Load("videos/" + VideoSources[currentVideoIndex]) as MovieTexture;
            this.CurrentVideoEndTime = firstTexture.duration * 100;

            targetObject.GetComponent<Renderer>().material.mainTexture = firstTexture;

            ((MovieTexture)targetObject.GetComponent<Renderer>().material.mainTexture).Play();

        }
        public override void Check(float currentTime)
        {
            Debug.Log(this.StartTime.ToString());
            if (currentTime.Equals(this.StartTime))
            {
                Debug.Log("sequence play!");
                ((MovieTexture)targetObject.GetComponent<Renderer>().material.mainTexture).Pause();
                currentVideoIndex++;
                MovieTexture nextTexture = (MovieTexture)Resources.Load("videos/" + VideoSources[currentVideoIndex]) as MovieTexture;
                CurrentVideoEndTime = nextTexture.duration * 100;

                targetObject.GetComponent<Renderer>().material.mainTexture = nextTexture;
                ((MovieTexture)targetObject.GetComponent<Renderer>().material.mainTexture).Play();
            }
        }
    }

    // wip
    public class PlayAndRepeat : Step
    {
        public override float StartTime { get; set; }

        public string VideoSource { get; set; }
        public string DestinationScreenName { get; set; }
        public int TimesRepeated { get; set; }
        public float Duration { get; set; }


        GameObject targetObject;
        MovieTexture movieComponent;

        public PlayAndRepeat(float startTime, int timesRepeated, string videoSource, string destinationScreenName)
        {
            StartTime = startTime;
            TimesRepeated = timesRepeated;
            VideoSource = videoSource;
            DestinationScreenName = destinationScreenName;
            targetObject = GameObject.Find(DestinationScreenName);
        }

        public override void Run()
        {
            Debug.Log(VideoSource + " run");
            targetObject.GetComponent<MeshRenderer>().enabled = true;
            MovieTexture movieTexture = (MovieTexture)Resources.Load("videos/" + VideoSource) as MovieTexture;
            this.Duration = movieTexture.duration * 100;
            movieTexture.loop = true;
            targetObject.GetComponent<Renderer>().material.mainTexture = movieTexture;
            movieComponent = ((MovieTexture)targetObject.GetComponent<Renderer>().material.mainTexture);
            movieComponent.Play();
        }

        public override void Check(float currentTime)
        {
            if (currentTime.Equals(this.StartTime + (this.Duration * this.TimesRepeated)))
            {
                Debug.Log("PAR clip stopped");
                Debug.Log(this.Duration * this.TimesRepeated);
                movieComponent.Play();
                targetObject.GetComponent<MeshRenderer>().enabled = false;
            }

        }
    }
}

