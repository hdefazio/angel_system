using DilmerGames.Core.Singletons;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapQueryTaskGraph : MonoBehaviour, IMixedRealityInputActionHandler
{
    /*string[,] tasks =
    {
        {"0", "Boil 12 ounces of water"},
        {"1", "Measure 12 ounces of water in the liquid measuring cup"},
        {"1", "Pour the water from the liquid measuring cup into  the electric kettle"},
        {"1", "Turn on the electric kettle by pushing the button underneath the handle"},
        {"1", "Boil the water. The water is done boiling when the button underneath the handle pops up"},

        {"0","Assemble the filter cone"},
        {"1", "While the water is boiling, assemble the filter cone. Place the dripper on top of a coffee mug"},
        {"1", "Prepare the filter insert by folding the paper filter in half to create a semi-circle, and in half again to create a quarter-circle. Place the paper filter in the dripper and spread open to create a cone."},
        {"1", "Take the coffee filter and fold it in half to create a semi-circle"},
        {"1", "Folder the filter in half again to create a quarter-circle"},
        {"1", "Place the folded filter into the dripper such that the the point of the quarter-circle rests in the center of the dripper"},
        {"1", "Spread the filter open to create a cone inside the dripper"},
        {"1", "Place the dripper on top of the mug"},
        
        {"0","Ground the coffee and add to the filter cone"},
        {"1","Weigh the coffee beans and grind until the coffee grounds are the consistency of coarse sand, about 20 seconds. Transfer the grounds to the filter cone."},
        {"1","Turn on the kitchen scale"},

         {"0","Check the water temperature"},
         {"1"," Turn on the thermometer"},
         {"1"," Place the end of the thermometer into the water. The temperature should read 195-205 degrees Fahrenheit or between 91-96 degrees Celsius."},

         {"0","Pour the water over the coffee grounds"},
         
         {"0","Clean up the paper filter and coffee grounds"},
    };*/
    private Logger _logger = null;

    ROSConnection ros;
    public string querytaskgraphTopicName = "TaskUpdates";

    private Timer timerTest;

    private bool actionInProgress = false;

    private int currentTask = 0;

    /// <summary>
    /// Lazy acquire the logger object and return the reference to it.
    /// </summary>
    /// <returns>Logger instance reference.</returns>
    private ref Logger logger()
    {
        if (this._logger == null)
        {
            // TODO: Error handling for null loggerObject?
            this._logger = GameObject.Find("Logger").GetComponent<Logger>();
        }
        return ref this._logger;
    }

    public void Start()
    {
        // Create the QueryTaskGraph subscriber
        ros = ROSConnection.GetOrCreateInstance();
        ROSConnection.GetOrCreateInstance().Subscribe<TaskGraphMsg>(querytaskgraphTopicName, QueryTaskGraphCallback);

        timerTest = new GameObject("TimerTest").AddComponent<Timer>();
    }


    /// <summary>
    /// Listen to Keyevents for debugging (only in the editor)
    /// </summary>
    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            currentTask++;
            AngelARUI.Instance.SetCurrentTaskID(currentTask);
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            currentTask--;
            AngelARUI.Instance.SetCurrentTaskID(currentTask);
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            AngelARUI.Instance.SetTasks(tasks2);
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            AngelARUI.Instance.SetTasks(tasks);
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            AngelARUI.Instance.ToggleTasklist();
        }
    }

    void QueryTaskGraphCallback(TaskGraphMsg msg)
    {
        Logger log = logger();
        log.LogInfo("Got taskGraphMsg: " + msg)
        edges = msg.node_edges
        string[,] tasks = {};
        
        taskIdx = 0;
        curTask = edges[taskIdx];
        tasks.append({"0", msg.task_nodes[curTask]});

        nextTask = edges[taskIdx+1];

        // remove pair form list
        edges.erase(edges.begin() + (taskIdx+1));
        edges.erase(edges.begin() + taskIdx);

        for (int i=0; i<=len(edges)/2; i++) // number of pairs to loop through
        {
            curTask = nextTask;
            taskIdx = std::distance(edges, std::find(edges.begin(), edges.end(), curTask))
            tasks.append({"0", msg.task_nodes[curTask]});

            nextTask = edges[taskIdx+1];

            // remove pair form list
            edges.erase(edges.begin() + (taskIdx+1));
            edges.erase(edges.begin() + taskIdx);
        }
        // Add the last task
        tasks.append({"0", msg.task_nodes[nextTask]});

        // Send tasks to ARUI
        log.LogInfo("Setting task list: " + tasks)
        AngelARUI.Instance.SetTasks(tasks);
    }

    private IEnumerator AddIfHit(BaseInputEventData eventData)
    {
        var result = eventData.InputSource.Pointers[0].Result;
        if (result != null)
        {
            var hitPosition = result.Details.Point;

            if (result.CurrentPointerTarget?.layer == 31)
            {
                AngelARUI.Instance.UpdateDatabase("id", UpdateType.add, hitPosition, Utils.ConvertClassNumToStr((uint)UnityEngine.Random.Range(0, 99)));
            }
        }
        currentTask++;
        AngelARUI.Instance.SetCurrentTaskID(currentTask);

        yield return new WaitForSeconds(1f);
        actionInProgress = false;
    }

    #region tap input registration 
    private void OnEnable()
    {
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputActionHandler>(this);
        AngelARUI.Instance.PringDebugMessage("Generate Test Data using Tap gesture", true);
    }


    private void OnDisable()
    {
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputActionHandler>(this);
    }

    public void OnActionEnded(BaseInputEventData eventData)
    {
        if (eventData != null && eventData.InputSource.SourceType.Equals(InputSourceType.Hand) && !actionInProgress)
        {
            actionInProgress = true;
            StartCoroutine(AddIfHit(eventData));
        }
    }

    public void OnActionStarted(BaseInputEventData eventData) { }


    #endregion

}
