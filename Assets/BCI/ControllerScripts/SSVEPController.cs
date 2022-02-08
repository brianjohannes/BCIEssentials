using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSVEPController : Controller
{
    public float[] setFreqFlash;
    public float[] realFreqFlash;

    private int[] frames_on;
    private int[] frames_off;
    private int[] frame_count;
    private int[] period;
    private int[] frame_off_count;
    private int[] frame_on_count;


    public override void populateObjectList(string popMethod)
    {
        // Remove everything from the existing list
        objectList.Clear();

        print("populating the list using method " + popMethod);
        //Collect objects with the BCI tag
        if (popMethod == "tag")
        {
            try
            {
                //Add game objects to list by tag "BCI"
                //GameObject[] objectList = GameObject.FindGameObjectsWithTag("BCI");
                GameObject[] objectArray = GameObject.FindGameObjectsWithTag("BCI");
                for (int i = 0; i < objectArray.Length; i++)
                {
                    objectList.Add(objectArray[i]);
                }
                //objectList.Add(GameObject.FindGameObjectsWithTag("BCI"));

                //The object list exists
                listExists = true;
            }
            catch
            {
                //the list does not exist
                print("Unable to create a list based on 'BCI' object tag");
                listExists = false;
            }

        }

        //List is predefined
        else if (popMethod == "predefined")
        {
            if (listExists == true)
            {
                print("The predefined list exists");
            }
            if (listExists == false)
            {
                print("The predefined list doesn't exist, try a different pMethod");
            }
        }

        // Collect by children ??
        else if (popMethod == "children")
        {
            Debug.Log("Populute by children is not yet implemented");
        }

        // Womp womp
        else
        {
            print("No object list exists");
        }

        // Remove from the list any entries that have includeMe set to false
        foreach (GameObject thisObject in objectList)
        {
            if (thisObject.GetComponent<SPO>().includeMe == false)
            {
                objectsToRemove.Add(thisObject);
            }
        }
        foreach (GameObject thisObject in objectsToRemove)
        {
            objectList.Remove(thisObject);
        }
        objectsToRemove.Clear();

        realFreqFlash = new float[objectList.Count];

        //for (int i = 0; i < objectList.Count; i++)
        //{
        //    frames_on[i] = 0;
        //    frame_count[i] = 0;
        //    period = (float)refreshRate / (float)setFreqFlash[i];
        //    // could add duty cycle selection here, but for now we will just get a duty cycle as close to 0.5 as possible
        //    frame_off_count[i] = (int)Math.Ceiling(period / 2);
        //    frame_on_count[i] = (int)Math.Floor(period / 2);
        //    realFreqFlash[i] = (refreshRate / (float)(frame_off_count[i] + frame_on_count[i]));
        //    print("frequency " + (i + 1).ToString() + " : " + realFreqFlash[i].ToString());
        //}
    }

    public override IEnumerator sendMarkers(int trainingIndex = 99)
    {
        // Make the marker string, this will change based on the paradigm
        while (stimOn)
        {
            // Desired format is: [windowLength, training frequencies, trainingIndex] 

            string markerString = windowLength.ToString();
            for (int i = 0; i < realFreqFlash.Length; i++)
            {
                markerString = markerString + "," + realFreqFlash[i].ToString();
            }

            if (trainingIndex <= objectList.Count)
            {
                markerString = markerString + "," + trainingIndex.ToString();
            }

            // Send the marker
            marker.Write(markerString);

            // Wait the window length + the inter-window interval
            yield return new WaitForSecondsRealtime(windowLength + interWindowInterval);


        }
    }

    //public override IEnumerator stimulus()
    //{

    //}
}
