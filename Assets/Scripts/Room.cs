using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum connectionState
{
    OPEN,
    LOCKED,
    NILL
}

public enum direction
{
    NORTH,
    EAST,
    SOUTH,
    WEST,
    UP,
    DOWN,
    NUM_DIRECTIONS
}



public class Room : MonoBehaviour {

    public string startRoomText = "Write stuff here. Use <ix> to start and end blocks only used when items available. Use (ix) to put item name in text.";

    public Puzzle[] puzzles;

    public connectionState[] exitStates = new connectionState[(int)direction.NUM_DIRECTIONS];

    public List<Interactable> roomObjects;

    //private Room[] connections = new Room[(int)direction.NUM_DIRECTIONS];
    private string actualRoomText;

    private int maxObjects;
    private Puzzle lastSolved;

    // Use this for initialization
    void Start () {
        correctRoomText();
        maxObjects = roomObjects.Count;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //Tries to add an object to the room, if it does it will return true
    public bool addItem(Interactable obj)
    {

        if (roomObjects.Count < maxObjects)
        {
            roomObjects.Add(obj);
            correctRoomText();
            return true;
        }
        else if (roomObjects.Count == maxObjects)
        {
            for (int i = 0; i < roomObjects.Count; i++)
            {
                if (roomObjects[i] == null)
                {
                    roomObjects[i] = obj;
                    correctRoomText();
                    return true;
                }
            }
        }

        return false;
    }

    public void removeItem(string itemName)
    {

        for (int i = 0; i < roomObjects.Count; i++)
        {
            if (roomObjects[i] != null && roomObjects[i].itemName.ToLower() == itemName)
            {
                roomObjects[i] = null;
                correctRoomText();
                return;
            }
        }

    }

    //Returns the display text of the room
    public string getRoomText()
    {
        return actualRoomText;
    }

    //Returns true if an item with the specified name exists in the room
    public bool itemExists(string itemText)
    {

        foreach(Interactable item in roomObjects)
        {
            if (item != null && item.itemName.ToLower() == itemText)
            {
                return true;
            }
        }

        return false;
    }

    public bool canTakeItem(string itemText)
    {

        foreach (Interactable item in roomObjects)
        {
            if (item != null && item.itemName.ToLower() == itemText)
            {
                return item.isCollectable;
            }
        }

        return false;
    }


    public Interactable takeItem(string itemText)
    {
        //foreach (Interactable item in roomObjects)
        for(int i = 0; i < roomObjects.Count; i++)
        {
            if (roomObjects[i] != null && roomObjects[i].itemName.ToLower() == itemText)
            {
                Interactable item = roomObjects[i];
                roomObjects[i] = null;
                correctRoomText();
                return item;
            }
        }
        return null;
    }

    public string getItemDesc(string itemText)
    {
        foreach (Interactable item in roomObjects)
        {
            if (item != null && item.itemName.ToLower() == itemText)
            {
                return item.description;
            }
        }

        return null;
    }

    public connectionState getState(direction dir)
    {
        return exitStates[(int)dir];
    }

    public void unlockDoor(direction dir)
    {
        if (exitStates[(int)dir] == connectionState.LOCKED)
        {
            exitStates[(int)dir] = connectionState.OPEN;
        }
    }

    public bool attemptPuzzleSolve(string[] items)
    {
        foreach (Puzzle p in puzzles){
            if (!p.isComplete())
            {
                if (p.attemptSolvePuzzle(items, this))
                {
                    lastSolved = p;
                    return true;
                }
            }
        }

        return false;
    }

    public string getLastSolveText()
    {
        return lastSolved.getSolvedText();
    }

    //Puts the correct items into the room text
    private void correctRoomText()
    {

        char[] textChars = startRoomText.ToCharArray();

        string finalString = "";

        bool write = true;

        int waitNum = -1;

        //Checks through the text to parse any parts that need to be replaced
        for (int i = 0; i < textChars.Length; i++)
        {
            if (textChars[i].Equals('<'))
            {
                string commandString = "";
                int j = i + 1;

                while (!textChars[j].Equals('>') && j < textChars.Length)
                {
                    commandString = commandString + textChars[j];
                    j++;
                }

                //If was a part of the text which should only be displayed if there is an item in the room
                if (textChars[j].Equals('>'))
                {
                    char[] commandChars = commandString.ToCharArray();

                    if (commandChars[0].Equals('i'))
                    {
                        string numString = "";

                        for (int k = 1; k < commandChars.Length; k++)
                        {
                            numString = numString + commandChars[k];
                        }

                        int num = int.Parse(numString);


                        if (waitNum == -1){
                            if (num < roomObjects.Count)
                            {
                                if (roomObjects[num] == null)
                                {
                                    write = false;
                                    waitNum = num;
                                }
                            }
                        }
                        else if (waitNum == num)
                        {
                            write = true;
                            waitNum = -1;
                        }
                    }
                }

                i = j + 1;
            }

            if (write && i < textChars.Length)
            {
                //Checks for item name replacement
                if (textChars[i].Equals('('))
                {
                    string commandString = "";
                    int j = i + 1;

                    while (!textChars[j].Equals(')') && j < textChars.Length)
                    {
                        commandString = commandString + textChars[j];
                        j++;
                    }

                    if (textChars[j].Equals(')'))
                    {
                        char[] commandChars = commandString.ToCharArray();

                        if (commandChars[0].Equals('i'))
                        {

                            string numString = "";

                            for (int k = 1; k < commandChars.Length; k++)
                            {
                                numString = numString + commandChars[k];
                            }

                            int num = int.Parse(numString);

                            if(roomObjects[num] != null)
                            {
                                finalString = finalString + roomObjects[num].itemName;
                            }
                        }

                        i = j;
                    }
                }
                else
                {

                    finalString = finalString + textChars[i];
                    
                }
            }

            
            
        }
        actualRoomText = finalString;
    }

    //Return the number of items in room
    private int getItemSlotsUsed()
    {
        int count = 0;

        for (int i = 0; i < maxObjects; i++)
        {
            if (roomObjects[i] != null)
            {
                count++;
            }
        }

        return count;
    }

}
