using UnityEngine;
using SpaceFusion.SF_Portals.Scripts;

public class Chapter2PuzzleManager : MonoBehaviour
{
    [Header("Portal Screens")]
    [SerializeField] private GameObject[] MazePortalScreens;
    [SerializeField] private GameObject[] BridgePortalScreens;
    [SerializeField] private GameObject[] HallwayPortalScreens;
    [SerializeField] private GameObject[] OrbPortalScreens;

    [Space]
    [Header("Portal Components")]
    [SerializeField] private Portal MazePortalComponent;
    [SerializeField] private Portal BridgePortalComponent;
    [SerializeField] private Portal HallwayPortalComponent;
    [SerializeField] private Portal OrbPortalComponent;

    [Space]
    [Header("Portals")]
    [SerializeField] private GameObject MazePortal;
    [SerializeField] private GameObject BridgePortal;
    [SerializeField] private GameObject HallwayPortal;
    [SerializeField] private GameObject OrbPortal;
    [SerializeField] private GameObject Chapter3Portal;
    [SerializeField] private GameObject Chapter3;
    [SerializeField] private GameObject InfiniteRoomDoors;
    
    [Space]
    [Header("Rooms/Triggers")]
    [SerializeField] private GameObject OrbRoom;
    [SerializeField] private GameObject BridgeRoom;
    [SerializeField] private GameObject HallwayRoom;
    [SerializeField] private GameObject MazeRoom;
    [SerializeField] private GameObject SunTrigger;


    private int itemsPlaced = 0;


    private void Start()
    {
        InitScreens();
        Chapter3Portal.SetActive(false);
        Chapter3.SetActive(false);
        InfiniteRoomDoors.SetActive(false);
    }

    public void RegisterItemPlaced()
    {
        itemsPlaced++;

        switch (itemsPlaced)
        {
            case 1:
                DestroyPortals(OrbPortal);
                Destroy(OrbRoom);
                HallwayRoom.SetActive(true);
                HallwayPortalComponent.enabled = true;
                SetScreensOn(HallwayPortalScreens);
                break;
            case 2:
                DestroyPortals(HallwayPortal);
                Destroy(HallwayRoom);
                BridgeRoom.SetActive(true);
                BridgePortalComponent.enabled = true;
                SetScreensOn(BridgePortalScreens);
                break;
            case 3:
                DestroyPortals(BridgePortal);
                Destroy(BridgeRoom);
                MazeRoom.SetActive(true);
                MazePortalComponent.enabled = true;
                SetScreensOn(MazePortalScreens);
                break;
            case 4:
                DestroyPortals(MazePortal);
                Destroy(MazeRoom);
                SunTrigger.SetActive(true);
                Chapter3Portal.SetActive(true);
                Chapter3.SetActive(true);
                break;
            default:
                break;
        }
    }

    private void InitScreens()
    {
        SetScreensOn(OrbPortalScreens);
        SetScreensOff(HallwayPortalScreens);
        SetScreensOff(BridgePortalScreens);
        SetScreensOff(MazePortalScreens);
        OrbPortalComponent.enabled = true;
        HallwayPortalComponent.enabled = false;
        BridgePortalComponent.enabled = false;
        MazePortalComponent.enabled = false;
    }

    private void SetScreensOff(GameObject[] screens)
    {
        foreach (var screen in screens)
        {
            screen.SetActive(false);
        }
    }

    private void SetScreensOn(GameObject[] screens)
    {
        foreach (var screen in screens)
        {
            screen.SetActive(true);
        }
    }

    private void DestroyPortals(GameObject portal)
    {
        Destroy(portal);
    }
}
