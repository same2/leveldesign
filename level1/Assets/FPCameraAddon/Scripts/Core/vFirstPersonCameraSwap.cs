// SJM Tech
// www.sjmtech3d.com
//
// Simple First/Third Camera Swap
//
// assign the ThrdCamera prefab from project tab
//

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Invector;
using Invector.CharacterController;

[RequireComponent(typeof(vFirstPersonCamera))]
[vClassHeader(" First person Camera SWAP Add-on", "Assign your Third Camera prefab", iconName = "FPCameraSwapIcon")]
public class vFirstPersonCameraSwap : vMonoBehaviour {

	[Space(1)]
	[Header("Settings")]
	[Tooltip("Assign thrdCamera prefab from project tab")]
	public GameObject thrdCameraPrefab;
	[Tooltip("Swap transition duration")]
	public float swapTime=0.1f;
	[Tooltip("Assign keyboard button for camera swap camera mode")]
	public KeyCode swapKey=KeyCode.Backspace;

	[Space(1)]
	[Header("Default Mode")]
	[Tooltip("Set thrdCamera as default camera on start")]
	public bool thrdCameraMode=false;
	[Tooltip("Set strafe as default thrdCamera mode")]
	public bool thrdCameraDefaultStrafe=false;

	private GameObject thrdCameraObject;
	private vFirstPersonCamera fPCamera;
	private bool thrdCameraModeLast;
	private vThirdPersonInput vInput;

	void Start(){
		vInput=GetComponent<vThirdPersonInput>();
		fPCamera=GetComponent<vFirstPersonCamera>();
	}

	void Update () {
		if(thrdCameraModeLast!=thrdCameraMode){
			if (thrdCameraMode){
				//vInput.cc.RotateToTarget(Camera.main.transform);
				fPCamera.enabled=false;			
				thrdCameraObject=Instantiate(thrdCameraPrefab,transform.position, transform.rotation);
				thrdCameraObject.name="vThirdCamera";
				//gameObject.SendMessage("CameraSwap",SendMessageOptions.DontRequireReceiver);
				vInput.cc.locomotionType=vThirdPersonMotor.LocomotionType.FreeWithStrafe;


				if(thrdCameraDefaultStrafe){
					vInput.cc.isStrafing=true;
				} else 	{
					vInput.cc.isStrafing=false;
				}

				thrdCameraModeLast=thrdCameraMode;

			} else {
				Vector3 thirdCamPos=thrdCameraObject.transform.position;
				//vInput.cc.RotateToDirection(cameraTransform.forward,true);
				vInput.cc.RotateToDirection(Camera.main.transform.forward,true);

				if(thrdCameraObject!=null){
					Destroy(thrdCameraObject);	
				}

				fPCamera.enabled=true;
				fPCamera.CameraTransition(thrdCameraObject.transform,swapTime);
				thrdCameraModeLast=thrdCameraMode;
			}
		}

			// swap by key
		if(Input.GetKeyDown(swapKey)){
			thrdCameraMode=!thrdCameraMode;
		}
	}
}
