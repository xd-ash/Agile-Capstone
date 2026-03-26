using UnityEngine;
using System.Collections.Generic;
using System;
public sealed class GoapWorld
{
    private static readonly GoapWorld instance = new GoapWorld();
    private static WorldStates world;
    //private static Queue<GameObject> patients;
    //private static Queue<GameObject> cubicles;

    static GoapWorld()
    {
        world = new WorldStates();
        Debug.Log("GoapWorld created (largely unimplemented)");
        //patients = new Queue<GameObject>();
        //cubicles = new Queue<GameObject>();

        //GameObject[] cubicleArray = GameObject.FindGameObjectsWithTag("Cubicle");
        //foreach (GameObject c in cubicleArray) 
            //cubicles.Enqueue(c);

        //if (cubicleArray.Length > 0)
            //world.ModifyState("FreeCubicle", cubicleArray.Length);
    }

    public void AddPatient(GameObject p)
    {
        //patients.Enqueue(p);
        throw new NotImplementedException();
    }
    public GameObject RemovePatient()
    {
        //if (patients.Count == 0)
        //return null;
        //else
        //return patients.Dequeue();
        throw new NotImplementedException();
    }
    public void AddCubicle(GameObject c)
    {
        //cubicles.Enqueue(c);
        throw new NotImplementedException();
    }
    public GameObject RemoveCubicle()
    {
        //if (cubicles.Count == 0)
        //return null;
        //else
        //return cubicles.Dequeue();
        throw new NotImplementedException();
    }

    public static GoapWorld Instance
    {
        get { return instance; }
    }
    public WorldStates GetWorld()
    {
        return world;
    }
}
