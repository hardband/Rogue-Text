using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct roomLoc{
    public Vector3 pos;
    public Room room;
}

public class Parser : MonoBehaviour {

    public InputField inText;
    public Text outText;
    public string helpText;

    public Room currentRoom;

    public List<roomLoc> editorMap;

    public Vector3 currentPos;

    private List<Interactable> inventory;

    private bool commandFlag = false;

    public Room[,,] roomMap;

    public List<ItemCombination> combinations;

    // Use this for initialization
    void Start() {
        resetFields();
        inventory = new List<Interactable>();
        helpText = helpText.Replace("/n", "\n");
        makeMapFromEditor();
        if (roomMap[(int)currentPos.x,(int)currentPos.y,(int)currentPos.z] != null)
        {
            currentRoom = roomMap[(int)currentPos.x, (int)currentPos.y, (int)currentPos.z];
            writeToScreen(currentRoom.getRoomText());
        }
    }

    // Update is called once per frame
    void Update() {

    }

    //This is only here for testing purposes
    public void makeMapFromEditor()
    {
        roomMap = new Room[10,10,10];

        foreach(roomLoc place in editorMap)
        {
            Vector3 pos = place.pos;
            roomMap[(int)pos.x, (int)pos.y, (int)pos.z] = place.room;
        }
    }

    public void parseInput(string text)
    {

        if (text != "")
        {

            commandFlag = false;

            outText.text = outText.text + "\n" + text;

            text = text.ToLower();

            checkHelp(text);
            checkLook(text);
            checkUse(text);
            checkMove(text);
            checkExamine(text);
            checkQuit(text);
            checkTake(text);
            checkInventory(text);

            if (!commandFlag)
            {
                writeToScreen("\"" + text + "\" was not understood");
            }
        }

        resetFields();

    }

    //Each of the following check functions checks whether the command specified has been used and if so tries to handle it
    private void checkLook(string text)
    {
        if (text == "look")
        {
            commandFlag = true;
            writeToScreen(currentRoom.getRoomText());
        }
    }

    private void checkHelp(string text)
    {
        if (text == "help")
        {
            commandFlag = true;
            writeToScreen(helpText);
        }
    }

    private void checkUse(string text)
    {
        string[] words = text.Split();

        if (words[0] == "use")
        {
            commandFlag = true;

            if (words.Length == 1)
            {
                writeToScreen("No item specified to use");
                return;
            }

            //Checks whether it is possible to be multiple items
            bool singleUse = words.Length < 4;

            int onPos = 0;

            if (!singleUse)
            {

                bool onFlag = false;

                for(int i = 2; i < words.Length - 1 && !onFlag; i++)
                {
                    if (words[i] == "on")
                    {
                        onPos = i;
                        onFlag = true;
                    }
                }

                singleUse = !onFlag;
            }

            //Checks whether it could only possibly be one item being used
            if (singleUse)
            {
                string[] itemStr = new string[1];
                itemStr[0] = getItemString(words);
                bool available = checkAvailable(itemStr[0]);

                if (available)
                {
                    if (currentRoom.attemptPuzzleSolve(itemStr))
                    {
                        writeToScreen(currentRoom.getLastSolveText());
                    }
                    else
                    {
                        writeToScreen("\"" + itemStr[0] + "\" can not be used");
                    }
                }
                else
                {
                    writeToScreen("\"" + itemStr[0] + "\" is not an available item");
                    return;
                }
                
            }
            //Handles when multiple items are being considered
            else
            {
                string[] itemStr = new string[2];

                itemStr[0] = getItemString(words, 1, onPos - 1);
                itemStr[1] = getItemString(words, onPos + 1, words.Length - 1);

                bool[] itemsAvailable = new bool[2];

                itemsAvailable[0] = checkAvailable(itemStr[0]);
                itemsAvailable[1] = checkAvailable(itemStr[1]);

                //Handles output based on whether the items are available
                if (itemsAvailable[0] && itemsAvailable[1])
                {
                    if (currentRoom.attemptPuzzleSolve(itemStr))
                    {
                        writeToScreen(currentRoom.getLastSolveText());
                    }
                    else
                    {

                        //Check whether items can combine

                        foreach (ItemCombination comb in combinations)
                        {

                            string[] combItems = comb.getCombineItems();

                            if((itemStr[0] == combItems[0] && itemStr[1] == combItems[1]) || (itemStr[0] == combItems[1] && itemStr[1] == combItems[0] && comb.reversible))
                            {

                                //Adds new item
                                inventory.Add(comb.getReturnItem());

                                //Removes old items
                                if (inInventory(itemStr[0]))
                                {
                                    removeFromInventory(itemStr[0]);
                                }
                                else
                                {
                                    currentRoom.removeItem(itemStr[0]);
                                }

                                if (inInventory(itemStr[1]))
                                {
                                    removeFromInventory(itemStr[1]);
                                }
                                else
                                {
                                    currentRoom.removeItem(itemStr[1]);
                                }

                                writeToScreen(comb.combineDescription);

                                return;

                            }


                        }
                        
                        writeToScreen("\"" + itemStr[0] + "\" and \"" + itemStr[1] + "\" can not be used together like that");
                    }
                }
                else {
                    if (itemsAvailable[0])
                    {
                        writeToScreen("\"" + itemStr[1] + "\" is not an available item");
                        return;
                    }
                    else if (itemsAvailable[1])
                    {
                        writeToScreen("\"" + itemStr[0] + "\" is not an available item");
                        return;
                    }
                    else
                    {
                        writeToScreen("\"" + itemStr[0] + "\" and \"" + itemStr[1] +"\" are not available items");
                    }
                }
            }                    

        }
    }

    private void checkMove(string text)
    {
        //Checks and handles each direction
        if (text == "north" || text == "n")
        {
            commandFlag = true;
            if (currentPos.y + 1 < roomMap.GetLength(1))
            {
                Room checkRoom = roomMap[(int)currentPos.x, (int)currentPos.y + 1, (int)currentPos.z];

                if (checkRoom != null)
                {
                    if (currentRoom.getState(direction.NORTH) == connectionState.OPEN)
                    {
                        currentPos.y = currentPos.y + 1;
                        currentRoom = roomMap[(int)currentPos.x, (int)currentPos.y, (int)currentPos.z];
                        writeToScreen("You head north");
                        writeToScreen(currentRoom.getRoomText());
                        return;
                    }
                    else if (currentRoom.getState(direction.NORTH) == connectionState.LOCKED)
                    {
                        writeToScreen("The route to the north is locked");
                        return;
                    }
                }
            }

            writeToScreen("There is no way to head north");
        }
        if (text == "east" || text == "e")
        {
            commandFlag = true;

            if (currentPos.x + 1 < roomMap.GetLength(0))
            {
                Room checkRoom = roomMap[(int)currentPos.x + 1, (int)currentPos.y, (int)currentPos.z];

                if (checkRoom != null)
                {
                    if (currentRoom.getState(direction.EAST) == connectionState.OPEN)
                    {
                        currentPos.x = currentPos.x + 1;
                        currentRoom = roomMap[(int)currentPos.x, (int)currentPos.y, (int)currentPos.z];
                        writeToScreen("You head east");
                        writeToScreen(currentRoom.getRoomText());
                        return;
                    }
                    else if (currentRoom.getState(direction.EAST) == connectionState.LOCKED)
                    {
                        writeToScreen("The route to the east is locked");
                        return;
                    }
                }
            }

            writeToScreen("There is no way to head east");
        }
        if (text == "south" || text == "s")
        {
            commandFlag = true;

            if (currentPos.y - 1 > 0)
            {
                Room checkRoom = roomMap[(int)currentPos.x, (int)currentPos.y - 1, (int)currentPos.z];

                if (checkRoom != null)
                {
                    if (currentRoom.getState(direction.SOUTH) == connectionState.OPEN)
                    {
                        currentPos.y = currentPos.y - 1;
                        currentRoom = roomMap[(int)currentPos.x, (int)currentPos.y, (int)currentPos.z];
                        writeToScreen("You head south");
                        writeToScreen(currentRoom.getRoomText());
                        return;
                    }
                    else if (currentRoom.getState(direction.SOUTH) == connectionState.LOCKED)
                    {
                        writeToScreen("The route to the south is locked");
                        return;
                    }
                }
            }

            writeToScreen("There is no way to head south");
        }
        if (text == "west" || text == "w")
        {
            commandFlag = true;

            if (currentPos.x - 1 > 0)
            {
                Room checkRoom = roomMap[(int)currentPos.x - 1, (int)currentPos.y, (int)currentPos.z];

                if (checkRoom != null)
                {
                    if (currentRoom.getState(direction.WEST) == connectionState.OPEN)
                    {
                        currentPos.x = currentPos.x - 1;
                        currentRoom = roomMap[(int)currentPos.x, (int)currentPos.y, (int)currentPos.z];
                        writeToScreen("You head west");
                        writeToScreen(currentRoom.getRoomText());
                        return;
                    }
                    else if (currentRoom.getState(direction.WEST) == connectionState.LOCKED)
                    {
                        writeToScreen("The route to the west is locked");
                        return;
                    }
                }
            }

            writeToScreen("There is no way to head west");
        }
        if (text == "up" || text == "u")
        {
            commandFlag = true;

            if (currentPos.z + 1 < roomMap.GetLength(2))
            {
                Room checkRoom = roomMap[(int)currentPos.x, (int)currentPos.y, (int)currentPos.z + 1];

                if (checkRoom != null)
                {
                    if (currentRoom.getState(direction.UP) == connectionState.OPEN)
                    {
                        currentPos.z = currentPos.z + 1;
                        currentRoom = roomMap[(int)currentPos.x, (int)currentPos.y, (int)currentPos.z];
                        writeToScreen("You head up");
                        writeToScreen(currentRoom.getRoomText());
                        return;
                    }
                    else if (currentRoom.getState(direction.UP) == connectionState.LOCKED)
                    {
                        writeToScreen("The route upwards is locked");
                        return;
                    }
                }
            }

            writeToScreen("There is no way to head up");
        }
        if (text == "down" || text == "d")
        {
            commandFlag = true;

            if (currentPos.z - 1 > 0)
            {
                Room checkRoom = roomMap[(int)currentPos.x, (int)currentPos.y, (int)currentPos.z - 1];

                if (checkRoom != null)
                {
                    if (currentRoom.getState(direction.DOWN) == connectionState.OPEN)
                    {
                        currentPos.z = currentPos.z - 1;
                        currentRoom = roomMap[(int)currentPos.x, (int)currentPos.y, (int)currentPos.z];
                        writeToScreen("You head down");
                        writeToScreen(currentRoom.getRoomText());
                        return;
                    }
                    else if (currentRoom.getState(direction.DOWN) == connectionState.LOCKED)
                    {
                        writeToScreen("The route downwards is locked");
                        return;
                    }
                }
            }

            writeToScreen("There is no way to head down");
        }
    }

    private void checkExamine(string text)
    {
        string[] words = text.Split();

        if (words[0] == "examine")
        {
            commandFlag = true;
            //If no item name specified
            if (words.Length == 1)
            {
                writeToScreen("No item specified to examine");
                return;
            }

            string itemName = getItemString(words);

            foreach (Interactable item in inventory)
            {
                if (itemName == item.itemName.ToLower())
                {
                    writeToScreen(item.description);
                    return;
                }
            }

            if (currentRoom.itemExists(itemName))
            {
                writeToScreen(currentRoom.getItemDesc(itemName));
                return;
            }

            writeToScreen("\"" + itemName + "\" not in inventory or room");

        }
    }

    private void checkQuit(string text)
    {
        //TODO
    }

    private void checkTake(string text)
    {
        string[] words = text.Split();

        if (words[0] == "take")
        {

            commandFlag = true;
            //If no item name specified
            if (words.Length == 1)
            {
                writeToScreen("No item specified to take");
                return;
            }

            string itemName = getItemString(words);

            if (currentRoom.itemExists(itemName))
            {
                if (currentRoom.canTakeItem(itemName))
                {
                    inventory.Add(currentRoom.takeItem(itemName));
                    writeToScreen("\"" + itemName + "\" was taken");
                }
                else
                {
                    writeToScreen("\"" + itemName + "\" can not be taken");
                }
            }
            else
            {
                writeToScreen("\"" + itemName + "\" is not an item in the room");
            }
        }
    }

    private void checkInventory(string text)
    {
        if (text == "inventory")
        {
            commandFlag = true;
            if (inventory.Count == 0)
            {
                writeToScreen("Inventory is empty");
            }
            else
            {
                string invString = "Inventory: ";

                foreach (Interactable item in inventory)
                {
                    invString = invString + item.itemName;
                    
                    if (item != inventory[inventory.Count - 1])
                    {
                        invString = invString + ", ";
                    }
                }

                writeToScreen(invString);

            }
        }
    }

    private void writeToScreen(string text)
    {
        outText.text = outText.text + "\n" + text;
    }

    private void resetFields()
    {
        inText.text = "";
        inText.GetComponent<InputField>().ActivateInputField();
    }

    //Concatenates strings to get an item assuming everything after the first word is the item
    private string getItemString(string[] words)
    {
        return getItemString(words, 1, words.Length - 1);
    }

    //Get's the items between two points in an array of words
    private string getItemString(string[] words, int startPos, int endPos)
    {
        string itemName = "";

        if (words.Length - 1 < endPos)
        {
            itemName = "ERROR";
            return itemName;
        }

        for (int i = startPos; i <= endPos; i++)
        {
            itemName = itemName + words[i];

            if(i != endPos)
            {
                itemName = itemName + " ";
            }
        }

        return itemName;
    }

    //Checks whether an item fo the specified name is in the players inventory
    private bool inInventory(string itemName)
    {

        foreach (Interactable item in inventory)
        {

            if (item.itemName.ToLower() == itemName.ToLower())
            {
                return true;
            }
        }

        return false;
    }

    //Checks whether an item name is in the inventory or room
    private bool checkAvailable(string itemName)
    {
        return inInventory(itemName) || currentRoom.itemExists(itemName);
    }

    //Removes an item from the inventory based on it's name
    private void removeFromInventory(string itemName)
    {
       for(int i = 0; i < inventory.Count; i++)
        {
            if(inventory[i].itemName.ToLower() == itemName)
            {
                inventory.Remove(inventory[i]);
                return;
            }
        }
    }
}
