using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoItemDoorPuzzle : Puzzle
{

    public Interactable firstUseItem;
    public Interactable secondUseItem;
    public Room unlockRoom;
    public direction unlockDir;
    public string solvedText;
    public bool reversible = true;

    public override bool attemptSolvePuzzle(string[] items, Room room)
    {

        if (items.Length == 2)
        {
            //Checks that the items match and if not reversible that they are in the correct order
            if ((items[0] == firstUseItem.itemName.ToLower() && items[1] == secondUseItem.itemName.ToLower()) 
                || (items[1] == firstUseItem.itemName.ToLower() && items[0] == secondUseItem.itemName.ToLower() && reversible))
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
