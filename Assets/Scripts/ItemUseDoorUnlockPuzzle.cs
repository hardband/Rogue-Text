using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemUseDoorUnlockPuzzle : Puzzle {

    public Interactable useItem;
    public Room unlockRoom;
    public direction unlockDir;
    public string solvedText;

    //Checks whether the item being used unlocks the door
    public override bool attemptSolvePuzzle(string[] items, Room room)
    {
        if (items.Length == 1)
        {
            if(items[0] == useItem.itemName)
            {
                complete = true;

                unlockRoom.unlockDoor(unlockDir);


                return true;
            }
        }

        return false;
    }

    public override string getSolvedText()
    {
        return solvedText;
    }
}
