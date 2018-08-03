using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RoomPair
{
    Room firstRoom;
    Room secondRoom;
}

public class MapGenerator : MonoBehaviour {

    public int maxWidth;
    public int maxLength;
    public int maxHeight;
    public int minimumRooms;
    public int maxRooms;

    [Range(0,100)]
    public float puzzleRoomChance;

    [Range(0, 100)]
    public float rareRoomChance;

    public List<RoomPair> startAndEndRooms;
    public List<Room> genericRoomSets;
    public List<RoomPackage> puzzleRoomSets;
    public List<RoomPackage> rarePuzzleRoomSets;

    private Room[,,] generatedMap;
    private Vector3 startPos = new Vector3(0, 0, 0);
    private int seed;

    public void newRandomSeed()
    {
        seed = Random.Range(0,int.MaxValue);
    }

    public void setSeed(int seed)
    {
        this.seed = seed;
    }

    public void generateNewMap()
    {
        generatedMap = new Room[maxWidth,maxLength,maxHeight];
        Random.InitState(seed);

        //TODO Actually generate the map
    }

    public Room[,,] getGeneratedMap()
    {
        return generatedMap;
    }

    public Vector3 getStartLoc()
    {
        return startPos;
    }


}
