using UnityEngine;
using UnityEngine.Networking;

public class PlayerBuild : NetworkBehaviour
{
    public GameObject Wall;
    public GameObject PhantomWall;

    [SerializeField] private int _maxBricks = 100;
    [SyncVar] private int _currentNbBricks;

    GameObject BuildingWall;
    public Transform WallSpawn;
    bool isBuilding;
    private PlayerUi _ui;

    public bool InputDisabled = false;

    void Start()
    {
        _currentNbBricks = 0;
        isBuilding = false;
    }

    public bool PickUpBrick()
    {
        if (_currentNbBricks < _maxBricks)
        {
            _currentNbBricks += 10;
            UpdateUI();
            return true;
        }

        return false;
    }

    public void SetPlayerUi(PlayerUi ui)
    {
        if (ui == null)
        {
            Debug.LogError("PlayerBuild: No PlayerUi component on Player UI prefab");
        }
        else
        {
            _ui = ui;
        }
    }

    void UpdateUI()
    {
        if (isLocalPlayer)
        {
            _ui.SetCurrentAmountClipContent(_currentNbBricks);
        }
    }

// Update is called once per frame
    void Update()
    {
        if (!InputDisabled)
        {
            HandleBuilding();

            if (Input.GetKeyDown(KeyCode.W))
            {
                if (isBuilding)
                {
                    //cancel building
                    Destroy(BuildingWall);
                    isBuilding = !isBuilding;
                }
                else if (_currentNbBricks >= 40) //want to build
                {
                    BuildingWall = Instantiate(PhantomWall, WallSpawn.position, transform.rotation);
                    isBuilding = !isBuilding;
                }
                else
                {
                    //display that he has not enough bricks ? Or too much information on the screen ?
                }
            }
        }
    }

    void HandleBuilding()
    {
        if (isBuilding)
        {
            //What we can do : set the y depending on the height map and not just in front of us, as you want
            BuildingWall.transform.position = WallSpawn.position;
            BuildingWall.transform.rotation = transform.rotation;
            if (Input.GetButtonDown("Fire2") && (Physics.OverlapBox(BuildingWall.transform.position,
                                                         new Vector3(1.5f, 0.8f, 0.25f),
                                                         BuildingWall.transform.rotation)
                                                     .Length <= 1))
            {
                isBuilding = false;
                _currentNbBricks -= 40;
                UpdateUI();
                Destroy(BuildingWall);
                Instantiate(Wall, WallSpawn.position, transform.rotation);
            }
        }
    }
}