using UnityEngine;
using System.Collections;
using Invector.CharacterController;
using UnityEngine.Events;

namespace Invector.CharacterController.Actions
{
    /// <summary>
    /// vSwimming Add-on
    /// On this Add-on we're locking the tpInput along with the tpMotor, tpAnimator & tpController methods to handle the Swimming behaviour.
    /// We can still access those scripts and methods, and call just what we need to use for example the FreeMovement, CameraInput, StaminaRecovery and UpdateHUD methods    
    /// This way the add-on become modular and plug&play easy to modify without changing the core of the controller. 
    /// </summary>

    [vClassHeader("Swimming Action")]
    public class vSwimming : vActionListener
    {
        #region Swimming Variables      

        [Header("Animations Clips & Tags")]
        [Tooltip("Name of the tag assign into the Water object")]
        public string waterTag = "Water";
        [Tooltip("Name of the animation clip that will play when you enter the Water")]
        public string swimmingClip = "Swimming";
        [Tooltip("Name of the animation clip that will play when you enter the Water")]
        public string diveClip = "Dive";
        [Tooltip("Name of the tag assign into the Water object")]
        public string exitWaterTag = "Action";
        [Tooltip("Name of the animation clip that will play when you exit the Water")]
        public string exitWaterClip = "QuickClimb";

        [Header("Speed & Extra Options")]
        [Tooltip("Uncheck if you don't want to go under water")]
        public bool swimUpAndDown = true;
        [Tooltip("Speed to swim forward")]
        public float swimForwardSpeed = 3f;
        [Tooltip("Speed to rotate the character")]
        public float swimRotationSpeed = 3f;
        [Tooltip("Speed to swim up")]
        public float swimUpSpeed = 1.5f;
        [Tooltip("Increase the radius of the capsule collider to avoid enter walls")]
        public float colliderRadius = .5f;
        [Tooltip("Height offset to match the character Y position")]
        public float heightOffset = .3f;
        [Tooltip("Create a limit for the camera before affects the rotation Y of the character")]
        public float cameraRotationLimit = .65f;

        [Header("Stamina Consuption")]
        [Tooltip("Leave with 0 if you don't want to use stamina consuption")]
        public float stamina = 15f;

        [Header("Particle Effects")]
        public GameObject impactEffect;
        [Tooltip("Check the Rigibody.Y of the character to trigger the ImpactEffect Particle")]
        public float velocityToImpact = -4f;
        public GameObject waterRingEffect;
        [Tooltip("Frequency to instantiate the WaterRing effect while standing still")]
        public float waterRingFrequencyIdle = .8f;
        [Tooltip("Frequency to instantiate the WaterRing effect while swimming")]
        public float waterRingFrequencySwim = .15f;
        [Tooltip("Instantiate a prefab when exit the water")]
        public GameObject waterDrops;
        [Tooltip("Y Offset based at the capsule collider")]
        public float waterDropsYOffset = 1.6f;

        [Tooltip("Debug Mode will show the current behaviour at the console window")]
        public bool debugMode;

        [Header("Inputs")]
        [Tooltip("Input to make the character go up")]
        public GenericInput swimUpInput = new GenericInput("Space", "X", "X");
        [Tooltip("Input to make the character go down")]
        public GenericInput swimDownInput = new GenericInput("LeftShift", "Y", "Y");
        [Tooltip("Input to exit the water by triggering a climb animation")]
        public GenericInput exitWaterInput = new GenericInput("E", "A", "A");

        private vThirdPersonInput tpInput;
        private vGetTransform exitWaterTrigger;
        private vGetTransform _tempExitWaterTrigger;
        private float originalColliderRadius;
        private float speed;
        private float timer;
        private float waterHeightLevel;
        private float originalRotationSpeed;
        private float waterRingSpawnFrequency;
        private bool inTheWater;
        private bool isExitingWater;

        // bools to trigger a method once on a update
        private bool triggerSwimState;
        private bool triggerExitSwim;
        private bool triggerUnderWater;
        private bool triggerAboveWater;

        #endregion

        public UnityEvent OnAboveWater;
        public UnityEvent OnUnderWater;

        private void Start()
        {
            tpInput = GetComponent<vThirdPersonInput>();
        }

        protected virtual void Update()
        {
            if (!inTheWater) return;

            ExitWaterAnimation();

            if (isExitingWater) return;

            UnderWaterBehaviour();
            SwimmingBehaviour();
        }

        private void SwimmingBehaviour()
        {
            // trigger swim behaviour only if the water level matches the player height + offset
            if (tpInput.cc._capsuleCollider.bounds.center.y + heightOffset < waterHeightLevel)
            {
                if (tpInput.cc.currentHealth > 0)
                {
                    if (!triggerSwimState) EnterSwimState();        // call once the swim behaviour
                    SwimForwardInput();                             // input to swin forward
                    SwimUpOrDownInput();                            // input to swin up or down
                    ExitWaterInput();                               // input to exit the water if inside the exitTrigger
                    tpInput.cc.FreeMovement();                      // update the free movement so we can rotate the character
                    tpInput.cc.StaminaRecovery();
                }
                else
                {
                    ExitSwimState();
                }
                tpInput.CameraInput();                          // update the camera input
                tpInput.UpdateHUD();                            // update hud graphics                
            }
            else
            {
                ExitSwimState();
            }
        }

        private void UnderWaterBehaviour()
        {
            if (isUnderWater)
            {
                StaminaConsumption();

                if (!triggerUnderWater)
                {
                    tpInput.cc._capsuleCollider.radius = colliderRadius;
                    triggerUnderWater = true;
                    triggerAboveWater = false;
                    OnUnderWater.Invoke();
                    tpInput.cc.animator.CrossFadeInFixedTime(diveClip, 0.25f);
                    tpInput.cc.animator.SetInteger("ActionState", 2);
                }
            }
            else
            {
                WaterRingEffect();
                if (!triggerAboveWater && triggerSwimState)
                {
                    triggerUnderWater = false;
                    triggerAboveWater = true;
                    OnAboveWater.Invoke();
                    tpInput.cc.animator.CrossFadeInFixedTime(swimmingClip, 0.25f);
                    tpInput.cc.animator.SetInteger("ActionState", 1);
                }
            }
        }

        private void StaminaConsumption()
        {
            if (tpInput.cc.currentStamina <= 0)
            {
                tpInput.cc.currentHealth -= 1f;
            }
            else
            {
                tpInput.cc.ReduceStamina(stamina, true);        // call the ReduceStamina method from the player
                tpInput.cc.currentStaminaRecoveryDelay = 0.25f;    // delay to start recovery stamina           
            }
        }

        public override void OnActionEnter(Collider other)
        {
            if (other.gameObject.CompareTag(waterTag))
            {
                if (debugMode) Debug.Log("Player enter the Water");
                inTheWater = true;
                waterHeightLevel = other.transform.position.y;
                originalColliderRadius = tpInput.cc._capsuleCollider.radius;
                originalRotationSpeed = tpInput.cc.freeSpeed.rotationSpeed;

                if (tpInput.cc.verticalVelocity <= velocityToImpact)
                {
                    var newPos = new Vector3(transform.position.x, other.transform.position.y, transform.position.z);
                    Instantiate(impactEffect, newPos, tpInput.transform.rotation);
                }
            }

            if (other.gameObject.CompareTag(exitWaterTag))
            {
                exitWaterTrigger = other.GetComponent<vGetTransform>();
                _tempExitWaterTrigger = exitWaterTrigger;
            }
        }

        public override void OnActionExit(Collider other)
        {
            if (other.gameObject.CompareTag(waterTag))
            {
                if (debugMode) Debug.Log("Player left the Water");
                if (isExitingWater) return;
                inTheWater = false;
                ExitSwimState();
                if (waterDrops)
                {
                    var newPos = new Vector3(transform.position.x, transform.position.y + waterDropsYOffset, transform.position.z);
                    GameObject myWaterDrops = Instantiate(waterDrops, newPos, tpInput.transform.rotation) as GameObject;
                    myWaterDrops.transform.parent = transform;
                }
            }

            if (other.gameObject.CompareTag(exitWaterTag) && !isExitingWater)
            {
                exitWaterTrigger = null;
            }
        }

        private void EnterSwimState()
        {
            if (debugMode) Debug.Log("Player is Swimming");

            triggerSwimState = true;
            tpInput.enabled = false;
            ResetPlayerValues();
            tpInput.cc.isStrafing = false;
            tpInput.cc.customAction = true;
            tpInput.cc.animator.CrossFadeInFixedTime(swimmingClip, 0.25f);
            tpInput.cc.freeSpeed.rotationSpeed = swimRotationSpeed;
            tpInput.cc._rigidbody.useGravity = false;
            tpInput.cc._rigidbody.drag = 10f;
        }

        private void ExitSwimState()
        {
            if (!triggerSwimState) return;
            if (debugMode) Debug.Log("Player Stop Swimming");

            triggerSwimState = false;
            tpInput.enabled = true;
            tpInput.cc.customAction = false;
            tpInput.cc.animator.SetInteger("ActionState", 0);
            tpInput.cc.colliderRadius = originalColliderRadius;
            tpInput.cc.freeSpeed.rotationSpeed = originalRotationSpeed;
            tpInput.cc._rigidbody.useGravity = true;
            tpInput.cc._rigidbody.drag = 0f;
        }

        private void ExitWaterAnimation()
        {
            tpInput.cc.LayerControl();                              // update the verification of the layers 
            tpInput.cc.ActionsControl();                            // update the verifications of actions 

            if (_tempExitWaterTrigger == null) return;

            // verify if the exit water animation is playing
            isExitingWater = tpInput.cc.baseLayerInfo.IsName(exitWaterClip);
            if (isExitingWater)
            {
                tpInput.CameraInput();                              // update the camera input
                tpInput.cc.DisableGravityAndCollision();            // disable gravity and collision so the character can make the animation using root motion                
                tpInput.cc.isGrounded = true;                       // ground the character so that we can run the root motion without any issues
                tpInput.cc.animator.SetBool("IsGrounded", true);    // also ground the character on the animator so that he won't float after finishes the climb animation

                if (_tempExitWaterTrigger.matchTarget != null)
                {
                    if (debugMode) Debug.Log("Match Target...");
                    // use match target to match the Y and Z target 
                    tpInput.cc.MatchTarget(_tempExitWaterTrigger.matchTarget.transform.position, _tempExitWaterTrigger.matchTarget.transform.rotation, _tempExitWaterTrigger.avatarTarget,
                        new MatchTargetWeightMask(_tempExitWaterTrigger.matchTargetMask, 0), _tempExitWaterTrigger.startMatchTarget, _tempExitWaterTrigger.endMatchTarget);
                }

                if (_tempExitWaterTrigger.useTriggerRotation)
                {
                    if (debugMode) Debug.Log("Rotate to Target...");
                    // smoothly rotate the character to the target
                    transform.rotation = Quaternion.Lerp(transform.rotation, _tempExitWaterTrigger.transform.rotation, tpInput.cc.animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
                }

                // after playing the animation we reset some values
                if (tpInput.cc.animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1 >= .8f)
                {
                    tpInput.cc.EnableGravityAndCollision(0f);       // enable again the gravity and collision 
                    exitWaterTrigger = null;                        // reset the exitWaterTrigger to null
                    isExitingWater = false;                         // reset the bool isExitingWater so we can exit again
                    inTheWater = false;                             // reset the bool saying that we're not on water anymore
                    ExitSwimState();                                // run the method exit swim state
                }
            }
        }

        private void SwimForwardInput()
        {
            // get input access from player
            tpInput.cc.input.x = tpInput.horizontalInput.GetAxis();
            tpInput.cc.input.y = tpInput.verticallInput.GetAxis();
            speed = Mathf.Abs(tpInput.cc.input.x) + Mathf.Abs(tpInput.cc.input.y);
            speed = Mathf.Clamp(speed, 0, 1f);
            // update input values to animator 
            tpInput.cc.animator.SetFloat("InputVertical", speed, 0.5f, Time.deltaTime);
            // extra rigibody forward force 
            var velY = transform.forward * swimForwardSpeed * speed;
            velY.y = tpInput.cc._rigidbody.velocity.y;
            tpInput.cc._rigidbody.velocity = velY;
            tpInput.cc._rigidbody.AddForce(transform.forward * (swimForwardSpeed * speed) * Time.deltaTime, ForceMode.VelocityChange);
        }

        private void SwimUpOrDownInput()
        {
            if (tpInput.cc.customAction) return;
            var upConditions = (((tpInput.cc._capsuleCollider.bounds.center.y + heightOffset) - waterHeightLevel) < -.2f);

            if (!swimUpAndDown)
            {
                var newPos = new Vector3(transform.position.x, waterHeightLevel, transform.position.z);
                if (upConditions) tpInput.cc.transform.position = Vector3.Lerp(transform.position, newPos, 0.5f * Time.deltaTime);
                return;
            }

            // extra rigibody up velocity                 
            if (swimUpInput.GetButton() && upConditions)
            {
                var vel = tpInput.cc._rigidbody.velocity;
                vel.y = swimUpSpeed;
                tpInput.cc._rigidbody.velocity = vel;
                tpInput.cc.animator.PlayInFixedTime("DiveUp", 0, tpInput.cc.input.magnitude > 0.1f ? 0.5f : 0.1f);
            }
            else if (swimDownInput.GetButtonDown() && !upConditions)
            {
                var vel = tpInput.cc._rigidbody.velocity;
                vel.y = -swimUpSpeed;
                tpInput.cc._rigidbody.velocity = vel;
                tpInput.cc.animator.CrossFadeInFixedTime("DiveDown", tpInput.cc.input.magnitude > 0.1f ? 0.5f : 0.1f);
            }
            else
            {
                // swim up or down based at the camera forward
                float inputGravityY = (Camera.main.transform.forward.y) * speed;
                var vel = tpInput.cc._rigidbody.velocity;
                vel.y = inputGravityY;
                if (vel.y > 0 && !upConditions)
                    vel.y = 0f;
                if (inputGravityY > cameraRotationLimit || inputGravityY < -cameraRotationLimit)
                {
                    tpInput.cc._rigidbody.velocity = vel;
                }
            }
        }

        private void ExitWaterInput()
        {
            if (exitWaterTrigger == null) return;

            if (exitWaterInput.GetButtonDown())
            {
                tpInput.cc._rigidbody.drag = 0f;
                OnAboveWater.Invoke();
                tpInput.cc.animator.CrossFadeInFixedTime(exitWaterClip, 0.1f);
            }
        }

        private void WaterRingEffect()
        {
            // switch between waterRingFrequency for idle and swimming
            if (tpInput.cc.input != Vector2.zero) waterRingSpawnFrequency = waterRingFrequencySwim;
            else waterRingSpawnFrequency = waterRingFrequencyIdle;

            // counter to instantiate the waterRingEffects using the current frequency
            timer += Time.deltaTime;
            if (timer >= waterRingSpawnFrequency)
            {
                var newPos = new Vector3(transform.position.x, waterHeightLevel, transform.position.z);
                Instantiate(waterRingEffect, newPos, tpInput.transform.rotation);
                timer = 0f;
            }
        }

        private void ResetPlayerValues()
        {
            tpInput.cc.isJumping = false;
            tpInput.cc.isSprinting = false;
            tpInput.cc.animator.SetFloat("InputHorizontal", 0);
            tpInput.cc.animator.SetFloat("InputVertical", 0);
            tpInput.cc.animator.SetInteger("ActionState", 1);
            tpInput.cc.isGrounded = true;                       // ground the character so that we can run the root motion without any issues
            tpInput.cc.animator.SetBool("IsGrounded", true);    // also ground the character on the animator so that he won't float after finishes the climb animation
            tpInput.cc.verticalVelocity = 0f;
        }

        bool isUnderWater
        {
            get
            {
                if (tpInput.cc._capsuleCollider.bounds.max.y >= waterHeightLevel + 0.25f)
                    return false;
                else
                    return true;
            }
        }
    }
}