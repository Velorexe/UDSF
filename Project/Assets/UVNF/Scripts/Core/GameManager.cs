﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CoroutineManager;

public class GameManager : MonoBehaviour
{
    [Header("UDSF Components")]
    public UVNFCanvas Canvas;
    public AudioManager AudioManager;

    [Header("Story Settings")]
    public StoryContainer CurrentStory;

    public float TimeoutBeforeStart = 0f;
    private float _timeoutTimer = 0f;

    private StoryElement CurrentElement;
    private TaskManager.TaskState _currentTask;

    private List<Tuple<string, string>> _storyLog = new List<Tuple<string, string>>();

    private Dictionary<string, object[]> _eventFlags = new Dictionary<string, object[]>();

    public bool AddEventFlag(string eventFlag, params object[] eventValues)
    {
        if (_eventFlags.ContainsKey(eventFlag))
            return false;
        _eventFlags.Add(eventFlag, eventValues);
        return true;
    }
    
    public bool ReachedEventFlag(string eventFlag)
    {
        return _eventFlags.ContainsKey(eventFlag);
    }

    public object[] GetEventFlagValues(string eventFlag)
    {
        if (!_eventFlags.ContainsKey(eventFlag))
            return null;
        return _eventFlags[eventFlag];
    }

#if UNITY_EDITOR
    public void Update()
    {

        foreach (StoryElement element in CurrentStory.StoryElements)
            element.Active = false;
    }
#endif

    #region StoryElements
    public void Awake()
    {
        Canvas.HideLoadScreen();

        CurrentStory.ConnectStoryElements();
        StartStory();
    }

    public void StartStory()
    {
        CurrentElement = CurrentStory.StoryElements[0];
#if UNITY_EDITOR
        foreach (StoryElement element in CurrentStory.StoryElements)
            element.Active = false;

        CurrentElement.Active = true;
#endif

        _currentTask = TaskManager.CreateTask(CurrentElement.Execute(this, Canvas));
        _currentTask.Finished += AdvanceStory;
        _currentTask.Start();
    }

    public void AdvanceStory(bool manual)
    {
        if (manual)
        {
            Debug.Log("Story Element cancelled manually.");
        }
        else
        {
            if (CurrentElement.Next != null)
            {
#if UNITY_EDITOR
                CurrentElement.Active = false;
                CurrentElement.Next.Active = true;
#endif
                CurrentElement = CurrentElement.Next;

                _currentTask = TaskManager.CreateTask(CurrentElement.Execute(this, Canvas));
                _currentTask.Finished += AdvanceStory;
                _currentTask.Start();
            }
            else
            {
                Debug.Log("Story finished.");
            }
        }
    }

    public void AdvanceStory(StoryElement newStoryPoint)
    {

    }

    public void LogStoryEvent(string characterName, string text)
    {
        _storyLog.Add(new Tuple<string, string>(characterName, text));
    }
    #endregion
}
