using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Com.Kawaiisun.SimpleHostile
{
    public class Player : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region Variables

        //Variables
        public float speed;

        public float sprintModifier;
        public float crouchModifier;
        public float jumpForce;
        public int maxHealth;
        private int currentHealth;
        public int mannualDamage;

        private Transform ui_HealthBar;
        private Text ui_Ammo;
        private TextMeshProUGUI ui_username;

        public Transform groundDetect;
        public LayerMask ground;

        [HideInInspector]public ProfileData playerProfile;
        public TextMeshPro playerUser;

        //Rigidbody
        private Rigidbody rb;

        //Animator
        private Animator anim;

        //FOV adjustment variable
        private float baseFov;

        private float sprintFOVmodifier = 1.35f;

        //Getting player's camera
        public Camera normalCam;
        public Camera weaponCam;
        private Vector3 originCam;

        public GameObject camParent;
        public Transform weaponParent;

        private Vector3 weaponParentOrigin;
        private Vector3 targetWeaponBobPosition;
        private Vector3 weaponParentcurrentPos;

        private float movementCounter;
        private float idleCounter;

        private Manager manager;
        private Weapon weapon;

        private float aimAngle;
        private bool isAiming;

        private Vector3 normalCamTarget;
        private Vector3 weaponCamTarget;

        public SkinnedMeshRenderer playerMeshRender;

        #endregion Variables

        #region PhotonCallbacks
        public void OnPhotonSerializeView(PhotonStream p_stream, PhotonMessageInfo p_message)
        {
            if (p_stream.IsWriting)
            {
                p_stream.SendNext((int)(weaponParent.transform.localEulerAngles.x * 100f));
            }
            else
            {
                 aimAngle = (int)p_stream.ReceiveNext() / 100f;
            }
        }
        #endregion
        
       

        #region MonoBehaviour CallBacks

        private void Start()
        {
            camParent.SetActive(photonView.IsMine);
            if (!photonView.IsMine) gameObject.layer = 9;

            if (Camera.main) Camera.main.enabled = false;
            rb = GetComponent<Rigidbody>();
            baseFov = normalCam.fieldOfView;
            weaponParentOrigin = weaponParent.localPosition;
            weaponParentcurrentPos = weaponParentOrigin;
            originCam = normalCam.transform.localPosition;

            //assigning current health to max health at the start of the game
            currentHealth = maxHealth;

            manager = GameObject.Find("Manager").GetComponent<Manager>();
            weapon = GetComponent<Weapon>();

            if (!photonView.IsMine)
            {
                gameObject.layer = 9;
                playerMeshRender.enabled = true;
            }

            if (photonView.IsMine)
            {
                ui_HealthBar = GameObject.Find("HUD/Health/Bar").transform;
                ui_Ammo = GameObject.Find("HUD/Ammo/Text").GetComponent<Text>();
                ui_username = GameObject.Find("HUD/Username/txt").GetComponent<TextMeshProUGUI>();
                ui_username.text = Launcher.myProfileData.username;

                photonView.RPC("SyncProfile", RpcTarget.All, Launcher.myProfileData.username, Launcher.myProfileData.level, Launcher.myProfileData.xp);


                RefreshHealthBar();
                weapon.RefreshAmmo(ui_Ammo);

                anim = GetComponent<Animator>();

                playerMeshRender.enabled = false;
            }
        }

        

        private void Update()
        {
            if (!photonView.IsMine)
            {
                RefreshMultiplayerState();
                return;
            }

           
            //Axes Inputs From User
            float t_hmove = Input.GetAxisRaw("Horizontal");
            float t_vmove = Input.GetAxisRaw("Vertical");

            //Controls
            bool t_sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool t_jump = Input.GetKeyDown(KeyCode.Space);
            bool pause = Input.GetKeyDown(KeyCode.Escape);

            //States
            bool isGrounded = Physics.Raycast(groundDetect.position, Vector3.down, 0.15f, ground);
            bool isJumping = t_jump && isGrounded;
            bool isSprinting = t_sprint && t_vmove > 0 && !isJumping && isGrounded;


            //Jumping
            if (isJumping)
            {
                rb.AddForce(jumpForce * Vector3.up);
            }

            if (Input.GetKeyDown(KeyCode.K)) TakeDamage(mannualDamage);

            //HeadBob

            if (t_hmove == 0 && t_vmove == 0)
            {
                HeadBob(idleCounter, 0.01f, 0.01f);
                idleCounter += Time.deltaTime;
                weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f * 0.2f);
            }
            else if (!isSprinting )
            {
                //walking
                HeadBob(movementCounter, 0.04f, 0.04f);
                movementCounter += Time.deltaTime * 6f;
                weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6 * 0.2f);
            }
           
            else
            {
                //sprinting
                HeadBob(movementCounter, 0.06f, 0.06f);
                movementCounter += Time.deltaTime * 8f;
                weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f * 0.2f);
            }

            //UI refreshes
            RefreshHealthBar();
            weapon.RefreshAmmo(ui_Ammo);
        }

        private void FixedUpdate()
        {
            if (!photonView.IsMine) return;

            //Axes Inputs From User
            float t_hmove = Input.GetAxisRaw("Horizontal");
            float t_vmove = Input.GetAxisRaw("Vertical");

            //Controls
            bool t_sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool t_jump = Input.GetKeyDown(KeyCode.Space);
            bool aim = Input.GetMouseButton(1);


            //States
            bool isGrounded = Physics.Raycast(groundDetect.position, Vector3.down, 0.1f, ground);
            bool isJumping = t_jump && isGrounded;
            bool isSprinting = t_sprint && t_vmove > 0 && !isJumping && isGrounded;
            isAiming = aim && !isSprinting;

            //Movements


            Vector3 t_direction = Vector3.zero;
            float t_adjustedSpeed = speed;

            t_direction = new Vector3(t_hmove, 0, t_vmove);
            t_direction.Normalize();
            t_direction = transform.TransformDirection(t_direction);
            //increasing the speed while sprinting
            if (isSprinting)
            {
                t_adjustedSpeed *= sprintModifier;
            }


            //Adding Velocity TO player
            Vector3 t_targetVelocity = t_direction * t_adjustedSpeed * Time.deltaTime;
            t_targetVelocity.y = rb.velocity.y;
            rb.velocity = t_targetVelocity;

            //aiming 
            weapon.Aim(isAiming);

            //Camera Stuff


            if (isSprinting)
            {
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFov * sprintFOVmodifier, Time.deltaTime * 2f);
                weaponCam.fieldOfView = Mathf.Lerp(weaponCam.fieldOfView, baseFov * sprintFOVmodifier, Time.deltaTime * 2f);
            }
            else if (isAiming)
            {
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFov * weapon.currentGunData.mainFOV, Time.deltaTime * 6f);
                weaponCam.fieldOfView = Mathf.Lerp(weaponCam.fieldOfView, baseFov * weapon.currentGunData.mainFOV, Time.deltaTime * 6f);
            }

            else
            {
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFov, Time.deltaTime * 5f);
                weaponCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFov, Time.deltaTime * 5f);

            } 


            normalCamTarget = Vector3.MoveTowards(normalCam.transform.localPosition, originCam, Time.deltaTime );
            weaponCamTarget = Vector3.MoveTowards(weaponCam.transform.localPosition, originCam, Time.deltaTime );


            //animationss

            float t_anim_horizontal = 0f;
            float t_anim_vertical = 0f;

            if (isGrounded)
            {
                t_anim_horizontal = t_direction.x;
                t_anim_vertical = t_direction.z;
            }

            anim.SetFloat("Horizontal", t_anim_vertical);
            anim.SetFloat("Vertical", t_anim_horizontal);
        }

        private void LateUpdate()
        {
            normalCam.transform.localPosition = normalCamTarget;
            weaponCam.transform.localPosition = weaponCamTarget;
        }

        #endregion MonoBehaviour CallBacks

        #region Private Methods

        void RefreshMultiplayerState()
        {
            float cacheEuly = weaponParent.localEulerAngles.y;
            Quaternion targetRotation = Quaternion.identity * Quaternion.AngleAxis(aimAngle, Vector3.right);
            weaponParent.rotation = Quaternion.Slerp(weaponParent.rotation, targetRotation, Time.deltaTime * 8f);

            Vector3 finalRotation = weaponParent.localEulerAngles;
            finalRotation.y = cacheEuly;

            weaponParent.localEulerAngles = finalRotation;
        }

        private void HeadBob(float p_z, float p_x_intensity, float p_y_intensity)
        {
            float t_aimAdj = 1f;
            if (isAiming) t_aimAdj = 0.2f;
            targetWeaponBobPosition = weaponParentcurrentPos + new Vector3(Mathf.Cos(p_z) * p_x_intensity * t_aimAdj , Mathf.Sin(p_z * 2) * p_y_intensity * t_aimAdj, 0);
        }

        private void RefreshHealthBar()
        {
            float t_health_ratio = (float)currentHealth / (float)maxHealth;
            ui_HealthBar.localScale = Vector3.Lerp(ui_HealthBar.localScale, new Vector3(t_health_ratio, 1, 1), Time.deltaTime * 8f);
        }

        [PunRPC]
        private void SyncProfile(string p_username, int p_level, int p_xp)
        {
            playerProfile = new ProfileData (p_username, p_level, p_xp);
            playerUser.text = playerProfile.username;
        }

        #endregion Private Methods
        #region Photon CallBAcks
        public void TakeDamage(int p_damage)
        {
            if (photonView.IsMine)
            {
                currentHealth -= p_damage;
                Debug.Log(currentHealth);

                RefreshHealthBar();

                if (currentHealth <= 0)
                {
                    manager.Spawn();
                    Debug.Log("You Died Fool");
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }
        #endregion
    }
}