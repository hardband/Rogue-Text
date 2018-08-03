using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Puzzle : MonoBehaviour {

    protected bool complete = false;

    //Shouls take in some amount of items and the current room and return whether the puzzle was solved as well as changing anything that needs to be changed
    public abstract bool attemptSolvePuzzle(string[] items, Room room);

    public abstract string getSolvedText();
    
    public bool isComplete()
    {
        return complete;
    }


}
