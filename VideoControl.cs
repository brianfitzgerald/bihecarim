using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System;
using System.Text.RegularExpressions;


public class VideoControl : MonoBehaviour
{
    static float currentTime = 0;
    static float nextStepTime = 0;
    static int nextStepIndex = 0;

    static List<Videospace.Step> videoStepList = new List<Videospace.Step>();
    static List<Videospace.Step> activeVideos = new List<Videospace.Step>();

    static JSONNode JSONProjectObject;
    
    public TextAsset jsonFile;

    void Start()
    {
		LoadJSONFile();
        nextStepTime = videoStepList[0].StartTime;
        Videospace.Utilities.PlayAudio(JSONProjectObject["Audio"][0]);
        Videospace.Utilities.ActivateScreenLayout(JSONProjectObject["Default Screen Setup"]);
        InvokeRepeating("UpdateTime", 0, 0.01f);
    }

    public void UpdateTime()
    {
        if (currentTime.Equals(nextStepTime))
        {
            activeVideos.Add(videoStepList[nextStepIndex]);
            if (nextStepIndex != videoStepList.Count - 1)
            {
                if (videoStepList[nextStepIndex].StartTime.Equals(videoStepList[nextStepIndex + 1].StartTime))
                {
                    Debug.Log("BRAP BRAP DUPLICATE VIDEO TIME DETECTED");
                    Debug.Log(videoStepList[nextStepIndex + 1].StartTime);
                    int latestStepIndex = nextStepIndex;
                    videoStepList[latestStepIndex].Run();
                    while (videoStepList[latestStepIndex].StartTime.Equals(videoStepList[latestStepIndex + 1].StartTime))
                    {
                        latestStepIndex++;
                        videoStepList[latestStepIndex].Run();
                    }
                }
            }

            Debug.Log("step #" + nextStepIndex);
            videoStepList[nextStepIndex].Run();

            if (videoStepList.Count.Equals(nextStepIndex + 1))
            {
                Debug.Log("last step reached");
            }
            else
            {
                nextStepIndex++;
                nextStepTime = videoStepList[nextStepIndex].StartTime;
            }

        }

        for (int i = 0; i < activeVideos.Count; i++)
        {
            activeVideos[i].Check(currentTime);
        }
        currentTime++;
    }

    public static float ParseTimestamp(string timestamp)
    {
        char[] delimiters = { '+' };
        string[] timestampParts = timestamp.Split(delimiters);
        float totalTime = 0;
        for (int i = 0; i < timestampParts.Length; i++)
        {
            char identifier = timestampParts[i][timestamp.Length - 1];
            switch (identifier)
            {
                // seconds
                case 's':
                    string seconds = timestamp.Substring(0, timestamp.Length - 1);
                    totalTime += (float.Parse(seconds) * 100);
                    break;
                case 'm':
                    string minutes = timestamp.Substring(0, timestamp.Length - 1);
                    totalTime += (float.Parse(minutes) * 100 * 60);
                    break;
                case 'f':
                    String frameString = timestamp.Substring(0, timestamp.Length - 1);
                    totalTime += float.Parse(frameString);
                    break;
                default:
                    Debug.Log("goof in the timestamp");
                    return 0;
            }

        }
        return totalTime;

    }


    public void LoadJSONFile()
    {
        Debug.Log("loading json...");
        VideoControl.JSONProjectObject = JSON.Parse(jsonFile.text);
        JSONArray Steps = VideoControl.JSONProjectObject["Steps"].AsArray;
        Debug.Log(Steps);
        for (int i = 0; i < Steps.Count; i++)
        {

            switch (Steps[i]["type"])
            {
                case "play for duration":
                    Debug.Log("added play for duration step");
                    Videospace.PlayForDuration durationStep = new Videospace.PlayForDuration(ParseTimestamp(Steps[i]["params"][1]), ParseTimestamp(Steps[i]["params"][2]), Steps[i]["params"][3], Steps[i]["params"][4]);
                    videoStepList.Add(durationStep);
                    break;
                case "play until finished":
                    Debug.Log(i);
                    Debug.Log("added play until finished step");
                    Videospace.PlayUntilFinished untilFinishedStep = new Videospace.PlayUntilFinished(ParseTimestamp(Steps[i]["params"][0]), Steps[i]["params"][1], Steps[i]["params"][2], Steps[i]["params"][3]);
                    VideoControl.videoStepList.Add(untilFinishedStep);
                    break;
                case "play in sequence":
                    Debug.Log("added play in sequence step");

                    JSONArray sourceJSONArray = Steps[i]["params"][1].AsArray;
                    Debug.Log(sourceJSONArray.ToString());
                    List<string> sourceStringList = new List<string>();

                    for (i = 0; i < sourceJSONArray.Count; i++)
                    {
                        sourceStringList.Add(sourceJSONArray[i]);
                    }

                    Videospace.PlayInSequence inSequenceStep = new Videospace.PlayInSequence(ParseTimestamp(Steps[i]["params"][0]), sourceStringList, Steps[i]["params"][2]);
                    VideoControl.videoStepList.Add(inSequenceStep);
                    break;
                case "play and repeat":
                    Debug.Log("added play and repeat step");
                    Videospace.PlayAndRepeat playAndRepeatStep = new Videospace.PlayAndRepeat(ParseTimestamp(Steps[i]["params"][1]), Steps[i]["params"][2].AsInt, Steps[i]["params"][3], Steps[i]["params"][4]);
                    VideoControl.videoStepList.Add(playAndRepeatStep);
                    break;
                default:
                    Debug.Log("Is there an error in your steps?");
                    break;
            }
        }
    }
}

