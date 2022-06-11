using UnityEngine;
using System.Collections;

public class GrabDropScript : MonoBehaviour, InteractionListenerInterface
{
	[Tooltip("Material used to outline the currently selected object.")]
	public Material selectedObjectMaterial;
	
	[Tooltip("Drag speed of the selected object.")]
	public float dragSpeed = 3.0f;

	[Tooltip("Minimum Z-position of the dragged object, when moving forward and back.")]
	public float minZ = 0f;

	[Tooltip("Maximum Z-position of the dragged object, when moving forward and back.")]
	public float maxZ = 5f;

	// public options (used by the Options GUI)
	[Tooltip("Whether the objects obey gravity when released, or not. Used by the Options GUI-window.")]
	public bool useGravity = true;
	[Tooltip("Whether the objects should be put in their original positions. Used by the Options GUI-window.")]
	public bool resetObjects = false;

	[Tooltip("Camera used for screen ray-casting. This is usually the main camera.")]
	public Camera screenCamera;

	[Tooltip("UI-Text used to display information messages.")]
	public UnityEngine.UI.Text infoGuiText;

	[Tooltip("Interaction manager instance, used to detect hand interactions. If left empty, it will be the first interaction manager found in the scene.")]
	private InteractionManager interactionManager;

    [Tooltip("Index of the player, tracked by the respective InteractionManager. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
    public int playerIndex = 0;

    [Tooltip("Whether the left hand interaction is allowed by the respective InteractionManager.")]
	public bool leftHandInteraction = false;

	[Tooltip("Whether the right hand interaction is allowed by the respective InteractionManager.")]
	public bool rightHandInteraction = true;


	// hand interaction variables
	//private bool isLeftHandDrag = false;
	private InteractionManager.HandEventType lastHandEvent = InteractionManager.HandEventType.None;

	// currently dragged object and its parameters
	private GameObject draggedObject;
	//private float draggedObjectDepth;
	private Vector3 draggedObjectOffset;
	private Material draggedObjectMaterial;
	private float draggedNormalZ;

	// initial objects' positions and rotations (used for resetting objects)
	private Vector3[] initialObjPos;
	private Quaternion[] initialObjRot;

	// normalized and pixel position of the cursor
	private Vector3 screenNormalPos = Vector3.zero;
	private Vector3 screenPixelPos = Vector3.zero;
	private Vector3 newObjectPos = Vector3.zero;

    private ModelGestureListener gestureListener;
    public bool rightClick = false;
    public bool leftClick = false;


    // choose whether to use gravity or not
    public void SetUseGravity(bool bUseGravity)
	{
		this.useGravity = bUseGravity;
	}

	// request resetting of the draggable objects
	public void RequestObjectReset()
	{
		resetObjects = true;
	}


	void Start()
	{
		// by default set the main-camera to be screen-camera
		if (screenCamera == null) 
		{
			screenCamera = Camera.main;
		}

		// get the interaction manager instance
		if(interactionManager == null)
		{
            //interactionManager = InteractionManager.Instance;
            interactionManager = GetInteractionManager();
        }

        // get the gestures listener
        gestureListener = ModelGestureListener.Instance;
    }


    // tries to locate a proper interaction manager in the scene
    private InteractionManager GetInteractionManager()
    {
        // find the proper interaction manager
        MonoBehaviour[] monoScripts = FindObjectsOfType(typeof(MonoBehaviour)) as MonoBehaviour[];

        foreach (MonoBehaviour monoScript in monoScripts)
        {
            if ((monoScript is InteractionManager) && monoScript.enabled)
            {
                InteractionManager manager = (InteractionManager)monoScript;

                if (manager.playerIndex == playerIndex && manager.rightHandInteraction == rightHandInteraction)
                {
                    return manager;
                }
            }
        }

        // not found
        return null;
    }


    void Update() 
	{
		if(interactionManager != null && interactionManager.IsInteractionInited())
		{
			bool bRightHandIntAllowed = (rightHandInteraction && interactionManager.IsRightHandPrimary());

            // check if there is an underlying object to be selected
            if (lastHandEvent == InteractionManager.HandEventType.Grip && bRightHandIntAllowed)
			{
                if(!leftClick)
                    rightClick = true;
			}
            else if (lastHandEvent == InteractionManager.HandEventType.Release && bRightHandIntAllowed)
            {
                rightClick = false;
            }
        }
	}


	void OnGUI()
	{
		if(infoGuiText != null && interactionManager != null && interactionManager.IsInteractionInited())
		{
			string sInfo = string.Empty;
			
			long userID = interactionManager.GetUserID();
			if(userID != 0)
			{
                if(rightClick)
                {
                    //sInfo = "Right click";
                }
                else if(leftClick)
                {
                    //sInfo = "Left click";
                }
			}
			else
			{
				KinectManager kinectManager = KinectManager.Instance;

				if(kinectManager && kinectManager.IsInitialized())
				{
					sInfo = "Waiting for Users...";
				}
				else
				{
					sInfo = "Kinect is not initialized. Check the log for details.";
				}
			}
			
			infoGuiText.text = sInfo;
		}
	}


	public void HandGripDetected(long userId, int userIndex, bool isRightHand, bool isHandInteracting, Vector3 handScreenPos)
	{
		if (!isHandInteracting || !interactionManager)
			return;
		if (userId != interactionManager.GetUserID())
			return;

		lastHandEvent = InteractionManager.HandEventType.Grip;
		//isLeftHandDrag = !isRightHand;
		screenNormalPos = handScreenPos;
	}

	public void HandReleaseDetected(long userId, int userIndex, bool isRightHand, bool isHandInteracting, Vector3 handScreenPos)
	{
		if (!isHandInteracting || !interactionManager)
			return;
		if (userId != interactionManager.GetUserID())
			return;

		lastHandEvent = InteractionManager.HandEventType.Release;
		//isLeftHandDrag = !isRightHand;
		screenNormalPos = handScreenPos;
	}

	public bool HandClickDetected(long userId, int userIndex, bool isRightHand, Vector3 handScreenPos)
	{
		return true;
	}


}
