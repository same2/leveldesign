// SJM Tech
// www.sjmtech3d.com
//
// Unofficial First Person Camera AddOn for Invector Basic/Melee/Shooter Template.
//
// rev. 2.15.1
//           
// use:
// 1 - drop the inVector Controller prefab on your scene ad add this script on it.
// 2 - remove all ThirdCamera by invector in scene; add a new camera and set it as mainCamera (or add a custom camera in "PC Camera" field.
// 3 - use the ContextMenu to set the camera in the right position.
//
//

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Invector;
using Invector.CharacterController;

[RequireComponent(typeof(vHeadTrack))]
[vClassHeader(" First person Camera Add-on", "Use context Menu > Set Camera position to initialize", iconName = "FPCameraIcon")]
public class vFirstPersonCamera : vMonoBehaviour {

	[Header("Camera Position Settings")]
	[Space(5)]
	[Tooltip("Set the camera used in FPCamera mode. If empty the MainCamera will be used")]
	public Camera fpCamera;
	[Tooltip("Set the FPCamera Near Plane")]
	public float cameraNearClip=0.01f;
	[Tooltip("Set the FPCamera Y offset from the head bone")]
	public float cameraYOffset = 0.1f;
	[Tooltip("Set the FPCamera Z offset from the head bone")]
	public float cameraZOffset = 0.02f;
	[Space(5)]

	[Tooltip("enable head collision to prevent camera goes into the objects")]
	public bool enableHeadCollider=true;
	[vHideInInspector("enableHeadCollider")]
	[Tooltip("show head collision Gizmos")]
	public bool showGizmos=true;
	[vHideInInspector("enableHeadCollider")]
	[Tooltip("head collision radius")]
	public float colliderRadius=0.12f;
	[vHideInInspector("enableHeadCollider")]
	[Tooltip("head collision center")]
	public Vector3 colliderCenter=new Vector3(0,0.1f,0.04f);
	[Space(5)]

	[Header("Camera Rotation Settings")]
	[Space(5)]
	[Tooltip("the speed of the camera rotation")]
	[Range(0.5f,3f)]
	public float cameraRotationSpeed = 1f;
	[Tooltip("enable the camera extra smoothing")]
	public bool useExtraSmoothing=false;
	[vHideInInspector("useExtraSmoothing")]
	[Tooltip("the smoothing of the camera rotation")]
	public float smoothingValue=5f;
	[Space(5)]
	[Tooltip("the maximum vertical camera rotation angle")]
	[Range(40f,80f)]
	public float upAngleLimit = 50f;
	[Tooltip("the minimum vertical camera rotation angle")]
	[Range(40f,85f)]
	public float downAngleLimit = 85f;
	[Space(5)]
	[Tooltip("the Horizontal clamp angle for head look during actions")]
	[Range(0f,90f)]
	public float actionHAngleLimit = 90f;
	[Tooltip("the Down Clamp angle for head look during actions")]
	[Range(40f,85f)]
	public float actionDownAngleLimit = 55f;
	[Space(5)]
	[Tooltip("should be enabled to add Shooter AimCanvas at runtime")]
	public bool usingShooter=false;
	[vHideInInspector("usingShooter")]
	[Tooltip("sway and recoil amount")]
	public float swayRecoilAmount=2;
	[Tooltip("align melee actions to the camera direction")]
	public bool usingMelee=false;
	[vHideInInspector("usingMelee")]
	[Tooltip("set melee week attack key used")]
	public KeyCode weekAttackKey=KeyCode.Mouse0;
	[vHideInInspector("usingMelee")]
	[Tooltip("set melee strong attack key used")]
	public KeyCode strongAttackKey=KeyCode.Alpha1;

	[Header("Body Rotation Settings")]
	[Space(5)]
	[Tooltip("Set the default Animator Update Mode")]
	public AnimatorUpdateMode animatorUpdateMode=AnimatorUpdateMode.AnimatePhysics;
	[Space(5)]
	[Tooltip("Set the body IK reactivity respect head rotation")]
	[Range(0f,1f)]
	public float bodyIKWeight=0.43f;
	[Space(5)]
	[Tooltip("the threshold angle between head and body beyond which the rotation begins")]
	[Range(0f,70f)]
	public float RotationThld = 60f;
	[Space(5)]

	[Header("Controller Options")]
	[Space(5)]
	[Tooltip("use cinematic camera during DEFAULT actions")]
	public bool cinematicOnActions=false;
	[Tooltip("use cinematic camera by external calls")]
	public bool cinematicOnRequest=true;
	[Tooltip("lock and hide the mouse cursor")]
	public bool lockMouseCursor=true;
	[Tooltip("add Crosshair UI prefab at start")]
	public bool addCrosshair=true; 			// Crosshair UI spawning
	[Tooltip("add ExtraCams prefab at start")]
	public bool addExtraCams=false; 			// Extra cams spawning

	private bool  lastSettings=false;
	private GameObject extraCamsPrefab;
	private GameObject crosshairPrefab;
	private GameObject crosshairInstance;
	private GameObject extraCamsInstance;

	private Vector2 offsetMouse;
	private Vector2 recoilMouse;
	private bool isAction=false;				//Force action status (Integrations)
	private bool isCinematic; 					//disable mouse user input (Cinematic)

	private  bool isUpdateModeNormal=false;
	private bool stateDone=false;

	// Shooter requisite
	private GameObject aimCanvas;
	private GameObject aimInstance;

	// inVector references
	private vThirdPersonInput vInput;
	private vHeadTrack vHeadT;

	// Animator and Bones
	private Animator animator;
	private Transform headBone;
	private GameObject headBoneRef;
	private GameObject headBoneRotCorrection;
	//private SphereCollider headCollider;

	// Player Rotation Correction
	private float x=0;
	private float y=0;
	private Vector3 originalRotation;
	private Vector3 fpCameraLocapPosition;
	private bool lateUpdateSync;
	private GameObject headCollider;
	private bool headColliderStatus=true;

    // camera transition
	private bool startCameraTransition=false;
	private Vector3 startCameraLocalPosition;
	private Transform cameraTransform;
	private Vector3 endCameraPosition;
	private float startTime;
	private float transitionSpeed;

	void OnDisable(){

		if (headBoneRotCorrection!=null){
			headBoneRotCorrection.SetActive(false);

			if(headColliderStatus){
				enableHeadCollider=false;
				headCollider.SetActive(false);
			}

			if(extraCamsInstance!=null)
			extraCamsInstance.SetActive(false);

			if(crosshairInstance!=null)
			crosshairInstance.SetActive(false);
		}
	}

	void OnEnable(){

		if (headBoneRotCorrection!=null){
			//x=transform.eulerAngles.y;
			//x=headBone.transform.eulerAngles.y;
			//y=transform.eulerAngles.x;
			//y=headBone.transform.eulerAngles.x;
			headBoneRotCorrection.transform.rotation=Quaternion.Euler(new Vector3(0,transform.eulerAngles.y,0));
			headBoneRotCorrection.SetActive(true);
			headCollider.SetActive(headColliderStatus);
			vInput.cc.locomotionType=vThirdPersonMotor.LocomotionType.OnlyStrafe;

			if(extraCamsInstance!=null) 
			extraCamsInstance.SetActive(true);

			if(crosshairInstance!=null)
			crosshairInstance.SetActive(true);
		}
	}

	void Awake()
	{    
		if(usingShooter&&aimInstance==null){
			aimCanvas=Resources.Load("AimCanvas") as GameObject;
			aimInstance=Instantiate(aimCanvas);
			aimInstance.name="AimCanvas";	
		}
		if(addExtraCams&&extraCamsInstance==null){
			extraCamsPrefab=Resources.Load("ExtraCams") as GameObject;
			extraCamsInstance=Instantiate(extraCamsPrefab);
			extraCamsInstance.name="ExtraCams";
			//fpCamera.rect = new Rect(0,-0.25f,1,1);
		}
		if(addCrosshair&&crosshairInstance==null){
			crosshairPrefab=Resources.Load("CrosshairUI") as GameObject;
			crosshairInstance=Instantiate(crosshairPrefab,GameObject.FindWithTag("PlayerUI").transform);
			crosshairInstance.name="CrosshairUI";
			//fpCamera.rect = new Rect(0,-0.25f,1,1);
		}
	}

	void Start () {

		// if there is no custom camera ... use the main camera.
		if (fpCamera == null) {
			fpCamera = Camera.main.gameObject.GetComponent<Camera>();
		}

		fpCamera.gameObject.tag="MainCamera";
		fpCamera.gameObject.layer=15;

		// set the optimal near clip plane and depth
		fpCamera.GetComponent<Camera>().nearClipPlane = cameraNearClip;
		fpCamera.GetComponent<Camera>().depth = 2;

		// set vTPC reference
		vInput=GetComponent<vThirdPersonInput>();
		vInput.cc.locomotionType=vThirdPersonMotor.LocomotionType.OnlyStrafe;
		vHeadT=GetComponent<vHeadTrack>();
		vHeadT.strafeBodyWeight=bodyIKWeight;

		// find the head bone
		animator = GetComponent<Animator>();
		headBone = animator.GetBoneTransform(HumanBodyBones.Head);

		animator.updateMode=animatorUpdateMode;

		if(animator.updateMode!=AnimatorUpdateMode.AnimatePhysics) {
			isUpdateModeNormal=true;
			//if(extraCamsInstance!=null) extraCamsInstance.SendMessage("ForceUpdateMode", false);
		} else { 
			isUpdateModeNormal=false;
			//if(extraCamsInstance!=null) extraCamsInstance.SendMessage("ForceUpdateMode", true);
		}

		// create head collision object
		headColliderStatus=enableHeadCollider;
		if(enableHeadCollider){
			headCollider=new GameObject("HeadCollision");
			headCollider.AddComponent<vFPCameraHeadCollider>();
			headCollider.layer=2;
			headCollider.tag="Player";
			headCollider.AddComponent<SphereCollider>();
			headCollider.GetComponent<SphereCollider>().radius=colliderRadius;
			headCollider.GetComponent<SphereCollider>().center=colliderCenter;
			headCollider.transform.parent=this.transform;
			headCollider.transform.localRotation=Quaternion.identity;
		}

		// create bones reference
		headBoneRef = new GameObject("HeadRef");
		headBoneRotCorrection = new GameObject("FPCameraRoot");
		headBoneRotCorrection.AddComponent<vFPCameraRoot>();

		// position bones reference
		headBoneRef.transform.position = headBone.transform.position;
		headBoneRotCorrection.transform.position = headBone.transform.position;
		headBoneRotCorrection.transform.rotation = headBone.transform.root.rotation;
		headBoneRef.transform.rotation = headBone.transform.rotation;
		headBoneRef.transform.parent = headBoneRotCorrection.transform;

		// camera position
		fpCamera.transform.position = headBoneRotCorrection.transform.position + (transform.root.forward * cameraZOffset) + (transform.root.up * cameraYOffset);
		fpCamera.transform.parent = headBoneRotCorrection.transform;
		fpCamera.transform.localRotation=Quaternion.identity;
		fpCameraLocapPosition=fpCamera.transform.localPosition;
		endCameraPosition=fpCamera.transform.localPosition;

		// set initial horizontal and vertical axes
		originalRotation = transform.eulerAngles;
		x = originalRotation.y;
		y = originalRotation.x;

		// find and stop "UnderBody" animator layer to reduce lags during camera free look
		for(int i = 0; i < animator.layerCount; i++)
		{
			if(animator.GetLayerName(i)=="UnderBody"){
				animator.SetLayerWeight(i, 0);
			}
		}
	}

	void FixedUpdate () {

		lateUpdateSync = true;

	}

	void LateUpdate(){

		if(isUpdateModeNormal){
			lateUpdateSync = true;	
		}

		if (lateUpdateSync){

			lateUpdateSync = false;

			if(!vInput.cc.ragdolled && Time.timeScale!=0){

				if(cinematicOnActions){
					if(vInput.cc.actions){
						isCinematic=true;
						stateDone=false;
					} else if(!vInput.cc.actions&&!stateDone){
						isCinematic=false;
						stateDone=true;
					}
				}

				CameraHeadBonePosition();

				if(isCinematic){
					y=0;
					x= headBoneRotCorrection.transform.eulerAngles.y;
					headBoneRotCorrection.transform.rotation = headBone.transform.rotation;
					fpCamera.transform.localRotation=Quaternion.identity;
				} else {

					CharacterRotation();
					FaceToCamera();
					CameraLook();					
					CameraHeadBoneRotation();
				}

			} else {

				// no user input during rag doll
				headBoneRotCorrection.transform.position = headBone.transform.position;
				headBoneRotCorrection.transform.rotation = headBone.transform.rotation;
				fpCamera.transform.localRotation=Quaternion.identity;
				fpCamera.transform.localPosition= fpCameraLocapPosition;
			}
		}

		if(startCameraTransition){
			
			float timeSinceStart=Time.time-startTime;
			float transitionComplete=timeSinceStart/transitionSpeed;
			startCameraLocalPosition.y=0;
			fpCamera.transform.localPosition=Vector3.Lerp(startCameraLocalPosition,endCameraPosition,transitionComplete);
			if(transitionComplete>=1f){
				startCameraTransition=false;	
			}
		}

	}

	void Update(){

		if (lockMouseCursor) {
			Cursor.lockState = CursorLockMode.Locked;
		}
		if(animator.updateMode!=AnimatorUpdateMode.AnimatePhysics) {
			isUpdateModeNormal=true;
			//if(extraCamsInstance!=null) extraCamsInstance.SendMessage("ForceUpdateMode", false);
		} else { 
			isUpdateModeNormal=false;
			//if(extraCamsInstance!=null) extraCamsInstance.SendMessage("ForceUpdateMode", true);
		}
	}

	void CameraHeadBonePosition(){

		headBoneRotCorrection.transform.position = headBone.transform.position;
		if(enableHeadCollider)
		headCollider.transform.position=headBone.transform.position;

	}

	void CameraHeadBoneRotation(){

		headBone.rotation = headBoneRef.transform.rotation;
		if(enableHeadCollider)
		headCollider.transform.rotation = headBone.transform.rotation;

	}

	void CameraLook() {

		y += vInput.rotateCameraYInput.GetAxis() * cameraRotationSpeed;

		if(vInput.cc.actions||isAction){
			if(vInput.rotateCameraXInput.GetAxis()!=0){
				x += vInput.rotateCameraXInput.GetAxis() * cameraRotationSpeed;		
			} else {

				x= headBoneRotCorrection.transform.eulerAngles.y;
			}
		} else {
			x += vInput.rotateCameraXInput.GetAxis() * cameraRotationSpeed;
		}

		float minX = transform.eulerAngles.y + (-actionHAngleLimit);
		float maxX = transform.eulerAngles.y + actionHAngleLimit;

		float tempx=Mathf.Clamp(x, minX, maxX);

		if(x>tempx+100){
			x-=360;
		} else if(x<tempx-100){
			x+=360;
		} 

		if(vInput.cc.actions||isAction){

			x = Mathf.Clamp(x, minX, maxX);
			y = Mathf.Clamp(y,-actionDownAngleLimit,upAngleLimit);

		} else  {
			y = Mathf.Clamp(y,-downAngleLimit,upAngleLimit);
		}

			// Head rotation
		Quaternion defRotation = Quaternion.Euler(-y+offsetMouse.x*swayRecoilAmount+recoilMouse.x*swayRecoilAmount,x+offsetMouse.y*swayRecoilAmount+recoilMouse.y*swayRecoilAmount,0f);
		if(!useExtraSmoothing){
			//headBoneRotCorrection.transform.rotation = defRotation; // no smoothing
			headBoneRotCorrection.transform.rotation = Quaternion.Lerp(headBoneRotCorrection.transform.rotation,defRotation,cameraRotationSpeed);
		} else {
			//headBoneRotCorrection.transform.rotation = Quaternion.Lerp(headBoneRotCorrection.transform.rotation,defRotation,smoothingValue*Time.deltaTime); 		// smoothing by time
			headBoneRotCorrection.transform.rotation = Quaternion.Lerp(headBoneRotCorrection.transform.rotation,defRotation,1/smoothingValue); 	// smoothing by frame
		}

	}

	void FaceToCamera(){

		if(usingMelee){

			if(Input.GetKey(weekAttackKey)||Input.GetKey(strongAttackKey)){

				vInput.cc.RotateToTarget(fpCamera.transform);
			}
		}
	}

	void CharacterRotation() {

		// Get camera forward in the character's rotation space
		Vector3 camRelative = transform.InverseTransformDirection(fpCamera.transform.forward);

		// Get the angle of the camera forward relative to the character forward
		float angle = Mathf.Atan2(camRelative.x, camRelative.z) * Mathf.Rad2Deg;
		float a=0;

		// check the angle threshold
		if (Mathf.Abs(angle) > Mathf.Abs(RotationThld)) {
			a  = angle - RotationThld;
			if (angle < 0)
			a = angle + RotationThld;

			// Body Rotation
			if(isAction||vInput.cc.actions){
				return;
			} else {
				Quaternion newRotation=Quaternion.AngleAxis(a, transform.up)*transform.rotation;
				
				if(!isUpdateModeNormal){
					transform.rotation= Quaternion.Lerp(transform.rotation, newRotation, Time.fixedDeltaTime * vInput.cc.strafeSpeed.rotationSpeed);
				} else {
					transform.rotation= Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * vInput.cc.strafeSpeed.rotationSpeed);
				}
			}
		}
	}

	public void SetOffset(Vector2 value){
		offsetMouse=value; // shooter camera sway
	}
	public void SetRecoil(Vector2 value){
		recoilMouse=value; // shooter camera sway
	}

	// call this to stop rotating body when threshold is reached.
	//  and when you need the camera rotate with body when there is no mouse inputs. (riding, driving...)
	public void IsAction(bool status){

		isAction=status;
		if(isAction==true){
			headBoneRotCorrection.transform.parent = this.transform;	
		} else {
			headBoneRotCorrection.transform.parent = null;
		}
	}

		// call this to use cinematic camera movement.
	public void IsCinematic(bool state){

		if(cinematicOnRequest)isCinematic=state;
	}

	public void OnDrawGizmosSelected() {
		if(fpCamera!=null&&showGizmos&&enableHeadCollider){
			animator = GetComponent<Animator>();
			headBone=animator.GetBoneTransform(HumanBodyBones.Head);
			Gizmos.color = new Color(0,1,1,0.3f);
			Gizmos.DrawSphere(headBone.transform.position+(headBone.transform.forward * colliderCenter.z)+headBone.transform.up * colliderCenter.y+headBone.transform.right*colliderCenter.x, colliderRadius);
		}
	}

	public void CameraTransition(Transform startPos, float tTime){
		//vInput.cc.RotateToTarget(startPos);

		cameraTransform=startPos;
		
		startCameraLocalPosition=fpCamera.transform.InverseTransformPoint(cameraTransform.position);
		
		x=transform.eulerAngles.y;
		y=transform.eulerAngles.x;

		transitionSpeed=tTime;
		startCameraTransition=true;
		startTime=Time.time;
		

		    //x=transform.eulerAngles.y;
		//x=startPos.transform.eulerAngles.y;
			//y=transform.eulerAngles.x;
			//y=headBone.transform.eulerAngles.x;

		
		//vInput.cc.RotateToTarget(fpCamera.transform);
	}

	// manual camera positioning
	[ContextMenu ("FP camera > Set Camera Position")]
	void SetCameraPos() {
		animator = GetComponent<Animator>();
		headBone = animator.GetBoneTransform(HumanBodyBones.Head);
		if (fpCamera == null) {
			fpCamera = Camera.main.gameObject.GetComponent<Camera>();
		}
		fpCamera.GetComponent<Camera>().nearClipPlane = cameraNearClip;
		fpCamera.GetComponent<Camera>().depth = 2;
		fpCamera.transform.position = headBone.position + (transform.root.forward * cameraZOffset) + (transform.root.up * cameraYOffset);
		fpCamera.transform.parent = headBone.transform.root;
		fpCamera.transform.rotation = transform.rotation;
	}
}