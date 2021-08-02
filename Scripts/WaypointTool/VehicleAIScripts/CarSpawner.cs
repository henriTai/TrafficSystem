using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Author: Henri Tainio

/// <summary>
/// Manages car traffic and spawning cars. This manager is rather a test version than a finished system.
/// </summary>
public class CarSpawner : MonoBehaviour
{
    /// <summary>
    /// An array of available prefabs for spawning cars.
    /// </summary>
    public GameObject[] carPrefabs;
    /// <summary>
    /// An array of all spawned cars.
    /// </summary>
    private GameObject[] carObjects;
    /// <summary>
    /// An array of label texts attached to the spawned cars.
    /// </summary>
    private Text[] carLabels;
    /// <summary>
    /// The AI of the currently selected car.
    /// </summary>
    private CarAIMain currentAI;
    /// <summary>
    /// An array of cameras attached to the spawned cars.
    /// </summary>
    private Camera[] carCameras;
    /// <summary>
    /// An array of all nodes.
    /// </summary>
    private Nodes[] allNodes;
    /// <summary>
    /// The index of the current selected car / camera.
    /// </summary>
    public int cameraIndex = 0;
    /// <summary>
    /// Number of cars that will be spawned.
    /// </summary>
    public int carCount;
    /// <summary>
    /// UI text element for dispalying speed information.
    /// </summary>
    public Text speedText;
    /// <summary>
    /// UI text for displaying the name of the current car.
    /// </summary>
    public Text carNameText;
    /// <summary>
    /// The name of the current selected car.
    /// </summary>
    private string carName;
    /// <summary>
    /// This bool was used in testing when random positions were not used.
    /// </summary>
    [Header("TestPositions")]
    public bool testPositionsOn = false;
    /// <summary>
    /// A list of node positions used when testing with set start positions.
    /// </summary>
    public List<Nodes> carPosNodes;
    /// <summary>
    /// A test parameter. This was used when driving a player controlled vehicle to disable AI camera display.
    /// </summary>
    [Header("Player")]
    public bool playerCameraOn = false;
    /// <summary>
    /// A player controlled vehicle's camera (for test purpose).
    /// </summary>
    public Camera playerCamera;
    /// <summary>
    /// A color atlas texture used as a color map of all different car colors.
    /// </summary>
    public Texture2D colorAtlas;
    /// <summary>
    /// A timer for updating speed UI text.
    /// </summary>
    private float textTimer = 0f;
    /// <summary>
    /// Is bird eye view camera on?
    /// </summary>
    private bool upViewOn = false;
    /// <summary>
    /// Is bird eye view camera locked to the currently selected car?
    /// </summary>
    private bool viewLockedToACar = true;
    /// <summary>
    /// Bird eye view camera.
    /// </summary>
    public Camera upviewCamera;
    /// <summary>
    /// Min and max x-coordinate boundaries for moving the bird eye view camera.
    /// </summary>
    public Vector2 upCameraXBoundaries;
    /// <summary>
    /// Min and max z-coordinate boundaries for moving the bird eye view camera.
    /// </summary>
    public Vector2 upCameraZBoundaries;
    /// <summary>
    /// Min and max y-coordinate boundaries for moving the bird eye view camera.
    /// </summary>
    public Vector2 upCameraYBoundaries;
    /// <summary>
    /// Is information UI panel on?
    /// </summary>
    bool infoOn = true;
    /// <summary>
    /// Are car label texts on?
    /// </summary>
    bool labelsOn = true;
    /// <summary>
    /// UI info panel's gameobject.
    /// </summary>
    public GameObject infoPanel;
    /// <summary>
    /// UI text displaying the key to open the info panel when the info panel is closed.
    /// </summary>
    public Text infoText;

    /// <summary>
    /// Unity's built-in awake function.
    /// </summary>
    private void Awake()
    {
        if (testPositionsOn)
        {
            InitTestPositions();
        }
        else
        {
            InitRandomPositionedCars();
        }
        if (playerCameraOn && playerCamera != null)
        {
            foreach (Camera c in carCameras)
            {
                c.gameObject.SetActive(false);
            }
        }
        else if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// Instantiates and initializes cars in test mode.
    /// </summary>
    private void InitTestPositions()
    {
        if (carPosNodes == null || carPosNodes.Count == 0)
        {
            return;
        }
        int count = 0;
        for (int i = 0; i < carPosNodes.Count; i++)
        {
            if (carPosNodes[i] != null)
            {
                count++;
            }
        }
        carObjects = new GameObject[count];
        carCameras = new Camera[count];
        int index = 0;
        for (int i = 0; i < carPosNodes.Count; i++)
        {
            if (carPosNodes[i] != null)
            {
                int ind = Random.Range(0, carPrefabs.Length);

                GameObject g = Instantiate(carPrefabs[ind]);
                g.name = "Car_" + index;
                CarAIMain carAI = g.GetComponent<CarAIMain>();

                Nodes n = carPosNodes[i];
                g.transform.position = GetHeightAtPoint(n.transform.position);
                carAI.carData.previousNode = n;

                Vector3 dir = (n.OutNode.transform.position - n.transform.position).normalized;
                g.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
                carCameras[index] = g.GetComponentInChildren<Camera>();
                if (index > 0)
                {
                    carCameras[index].gameObject.SetActive(false);
                }
                carObjects[index] = g;
            }
            index++;
        }
        currentAI = carObjects[0].GetComponent<CarAIMain>();
        carName = carObjects[0].name;
        carCameras[0].tag = "MainCamera";
    }
    /// <summary>
    /// Instantiates and and initializes randomly positioned cars.
    /// </summary>
    private void InitRandomPositionedCars()
    {
        Nodes[] startNodes = new Nodes[carCount];
        carObjects = new GameObject[carCount];
        carCameras = new Camera[carCount];
        GameObject canvas = FindObjectOfType<Canvas>().gameObject;
        carLabels = new Text[carCount];

        for (int i = 0; i < carCount; i++)
        {
            bool nodeAdded = false;
            while (!nodeAdded)
            {
                bool duplicate = false;
                Nodes node = RandomStartNode();
                for (int j = 0; j < i; j++)
                {
                    // don't spawn in intersection
                    if (node.ParentLane.laneType != LaneType.ROAD_LANE)
                    {
                        duplicate = true;
                        break;
                    }
                    Nodes other = startNodes[j];
                    if (other == node || Vector3.Distance(other.transform.position, node.transform.position) < 5f)
                    {
                        duplicate = true;
                        break;
                    }
                }
                if (!duplicate)
                {
                    startNodes[i] = node;
                    nodeAdded = true;
                }
            }

        }
        for (int i = 0; i < carCount; i++)
        {
            int ind = Random.Range(0, carPrefabs.Length);
            GameObject g = Instantiate(carPrefabs[ind]);
            g.name = "Car_" + i;

            CarAIMain carAI = g.GetComponent<CarAIMain>();
            Nodes n = startNodes[i];
            g.transform.position = GetHeightAtPoint(n.transform.position);
            carAI.carData.previousNode = n;

            Vector3 dir = (n.OutNode.transform.position - n.transform.position).normalized;
            g.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            carCameras[i] = g.GetComponentInChildren<Camera>();
            if (i > 0)
            {
                carCameras[i].gameObject.SetActive(false);
            }
            carAI.spawner = this;
            carObjects[i] = g;
            MeshRenderer rend = g.GetComponentInChildren<MeshRenderer>();
            int x = Random.Range(0, 17);
            int y = Random.Range(0, 17);
            Color c = colorAtlas.GetPixel(x, y);
            carAI.carData.carControl.SetCarColor(c);

            GameObject textObj = new GameObject("car_text_" + i);
            textObj.transform.SetParent(canvas.transform);
            carLabels[i] =  textObj.AddComponent<Text>();
            carLabels[i].font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            carLabels[i].text = "Car_" + i;
            carLabels[i].color = Color.red;

        }
        currentAI = carObjects[0].GetComponent<CarAIMain>();
        carName = carObjects[0].name;
        carCameras[0].tag = "MainCamera";
    }
    /// <summary>
    /// Unity's built-in update function. Monitors key inputs and updates UI texts.
    /// </summary>
    void Update()
    {
        if (labelsOn)
        {
            for (int i = 0; i < carLabels.Length; i++)
            {
                if (upViewOn)
                {
                    carLabels[i].transform.position = upviewCamera.WorldToScreenPoint(carObjects[i].transform.position);
                }
                else
                {
                    carLabels[i].transform.position = carCameras[cameraIndex].WorldToScreenPoint(carObjects[i].transform.position);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            ToggleLabelsOn();
        }
        if (upViewOn && viewLockedToACar)
        {
            upviewCamera.transform.position = new Vector3(carObjects[cameraIndex].transform.position.x,
                upviewCamera.transform.position.y, carObjects[cameraIndex].transform.position.z);
        }
        textTimer += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.N))
        {
            ChangeCamera(-1);
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            ChangeCamera(1);
        }
        if (speedText != null && textTimer > 0.5f)
        {
            textTimer = 0f;
            speedText.text = "Speed (km/h): " + (Mathf.Round(currentAI.carData.carSpeed * 3.6f * 100f) / 100f);
        }
        if (carNameText != null)
        {
            carNameText.text = carName;
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            currentAI.TryChangeLane(IntersectionDirection.Left);
            Debug.Log("Requested car number " + cameraIndex + " to change to left lane.");
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            currentAI.TryChangeLane(IntersectionDirection.Straight);
            Debug.Log("Reseted car number " + cameraIndex + " lane change request.");
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            currentAI.TryChangeLane(IntersectionDirection.Right);
            Debug.Log("Requested car number " + cameraIndex + " to change to right lane.");
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            ToggleBirdEyeView();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            LockBirdEyeView();
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                float newY = Mathf.Clamp(upviewCamera.transform.position.y -
                    Time.deltaTime * 5f * upviewCamera.transform.position.y, upCameraYBoundaries.x,
                    upCameraYBoundaries.y);
                upviewCamera.transform.position = new Vector3(upviewCamera.transform.position.x, newY,
                    upviewCamera.transform.position.z);
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                float newY = Mathf.Clamp(upviewCamera.transform.position.y +
                    Time.deltaTime * 5f * upviewCamera.transform.position.y, upCameraYBoundaries.x,
                    upCameraYBoundaries.y);
                upviewCamera.transform.position = new Vector3(upviewCamera.transform.position.x, newY,
                    upviewCamera.transform.position.z);
            }
        }
        if (!viewLockedToACar)
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                float newX = Mathf.Clamp(upviewCamera.transform.position.x - Time.deltaTime * upviewCamera.transform.position.y,
                    upCameraXBoundaries.x, upCameraXBoundaries.y);
                upviewCamera.transform.position = new Vector3(newX, upviewCamera.transform.position.y,
                    upviewCamera.transform.position.z);
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                float newX = Mathf.Clamp(upviewCamera.transform.position.x + Time.deltaTime * upviewCamera.transform.position.y,
                    upCameraXBoundaries.x, upCameraXBoundaries.y);
                upviewCamera.transform.position = new Vector3(newX, upviewCamera.transform.position.y,
                    upviewCamera.transform.position.z);
            }
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    float newZ = Mathf.Clamp(upviewCamera.transform.position.z + Time.deltaTime * upviewCamera.transform.position.y,
                    upCameraZBoundaries.x, upCameraZBoundaries.y);
                    upviewCamera.transform.position = new Vector3(upviewCamera.transform.position.x,
                        upviewCamera.transform.position.y, newZ);
                }
                else if (Input.GetKey(KeyCode.DownArrow))
                {
                    float newZ = Mathf.Clamp(upviewCamera.transform.position.z - Time.deltaTime * upviewCamera.transform.position.y,
                    upCameraZBoundaries.x, upCameraZBoundaries.y);
                    upviewCamera.transform.position = new Vector3(upviewCamera.transform.position.x,
                        upviewCamera.transform.position.y, newZ);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInfoOn();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
    /// <summary>
    /// This is used when car is returned from the traffic to the garage to wait to be spawned back in.
    /// </summary>
    /// <param name="returnedCar">Gameonject of the returned car.</param>
    public void ReturnCarToCarage(GameObject returnedCar)
    {
        ResetCarPosition(returnedCar);
    }
    /// <summary>
    /// Calculates terrain height at the given position so the car is spawned to a correct height.
    /// </summary>
    /// <param name="point">The point where in these xz-coordinates the terrain's height is measured.</param>
    /// <returns>The height of the terrain at the given xz-coordinates.</returns>
    public Vector3 GetHeightAtPoint(Vector3 point)
    {
        Ray r = new Ray(new Vector3(point.x, 50f, point.z), Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(r, out hit, float.MaxValue))
        {
            return hit.point;
        }
        else
        {
            return point;
        }
    }
    /// <summary>
    /// Resets car and sends it back to the traffic.
    /// </summary>
    /// <param name="car">Car's gameobject.</param>
    private void ResetCarPosition(GameObject car)
    {
        CarAIMain carAI = car.GetComponent<CarAIMain>();
        Nodes n = null;
        while (true)
        {
            int r = Random.Range(0, allNodes.Length);
            n = allNodes[r];
            Vector2 p0 = new Vector2(n.transform.position.x, n.transform.position.z);
            if (n.ParentLane.laneType != LaneType.ROAD_LANE)
            {
                continue;
            }
            bool isOk = true;
            for (int i = 0; i < carObjects.Length; i++)
            {
                Vector2 p1 = new Vector2(carObjects[i].transform.position.x, carObjects[i].transform.position.z);
                if (Vector2.Distance(p0, p1) < 5f)
                {
                    isOk = false;
                    break;
                }
            }
            if (isOk)
            {
                Vector3 pos = new Vector3(n.transform.position.x, 0f, n.transform.position.z);
                car.transform.position = pos;
                Vector3 dir = (n.OutNode.transform.position - n.transform.position).normalized;
                car.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
                carAI.carData.previousNode = n;
                CarDataInitializer.ResetCar(carAI.carData);

                break;
            }
        }

    }
    /// <summary>
    /// Toggles bird eye view camera on and off.
    /// </summary>
    private void ToggleBirdEyeView()
    {
        if (upViewOn)
        {
            upViewOn = false;
            carCameras[cameraIndex].gameObject.SetActive(true);
            upviewCamera.gameObject.SetActive(false);
        }
        else
        {
            upViewOn = true;
            upviewCamera.gameObject.SetActive(true);
            carCameras[cameraIndex].gameObject.SetActive(false);
            if (viewLockedToACar)
            {
                upviewCamera.transform.position = new Vector3(carObjects[cameraIndex].transform.position.x,
                    upviewCamera.transform.position.y, carObjects[cameraIndex].transform.position.z);
            }
        }
    }
    /// <summary>
    /// Locks / unlocks bird eye view camera to follow the current active car.
    /// </summary>
    private void LockBirdEyeView()
    {
        if (viewLockedToACar)
        {
            viewLockedToACar = false;
        }
        else
        {
            viewLockedToACar = true;
            upviewCamera.transform.position = new Vector3(carObjects[cameraIndex].transform.position.x,
                upviewCamera.transform.position.y, carObjects[cameraIndex].transform.position.z);
        }
    }
    /// <summary>
    /// Toggles the info UI panel on and off.
    /// </summary>
    private void ToggleInfoOn()
    {
        if (infoOn)
        {
            infoOn = false;
            infoPanel.SetActive(false);
            infoText.gameObject.SetActive(true);
        }
        else
        {
            infoOn = true;
            infoPanel.SetActive(true);
            infoText.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// Toggles car labels on and off.
    /// </summary>
    private void ToggleLabelsOn()
    {
        if (labelsOn)
        {
            labelsOn = false;
            for (int i = 0; i < carLabels.Length; i++)
            {
                carLabels[i].gameObject.SetActive(false);
            }
        }
        else
        {
            labelsOn = true;
            for (int i = 0; i < carLabels.Length; i++)
            {
                carLabels[i].gameObject.SetActive(true);
            }
        }
    }
    /// <summary>
    /// Changes camera to the next or previous camera.
    /// </summary>
    /// <param name="i">-1 = previous camera, 1 = next camera.</param>
    private void ChangeCamera(int i)
    {
        if (playerCameraOn)
        {
            return;
        }
        int nextIndex = 0;
        if (i == -1)
        {
            if (cameraIndex == 0)
            {
                nextIndex = carCount - 1;
            }
            else
            {
                nextIndex = cameraIndex - 1;
            }
        }
        if (i == 1)
        {
            if (cameraIndex == carCount - 1)
            {
                nextIndex = 0;
            }
            else
            {
                nextIndex = cameraIndex + 1;
            }
        }
        if (!upViewOn)
        {
            carCameras[nextIndex].gameObject.SetActive(true);
            carCameras[cameraIndex].gameObject.SetActive(false);
        }
        cameraIndex = nextIndex;
        currentAI = carObjects[cameraIndex].GetComponent<CarAIMain>();
        carName = carObjects[cameraIndex].name;
    }
    /// <summary>
    /// Finds an available start node.
    /// </summary>
    /// <returns>Returns a random available start node.</returns>
    private Nodes RandomStartNode()
    {
        Nodes n = null;
        if (allNodes == null)
        {
            allNodes = FindObjectsOfType<Nodes>();
        }
        while (true)
        {
            int i = Random.Range(0, allNodes.Length - 1);
            if (!allNodes[i].LaneStartNode)
            {
                if (allNodes[i].OutNode != null)
                {
                    n = allNodes[i];
                    break;
                }
            }
        }

        return n;
    }
}
