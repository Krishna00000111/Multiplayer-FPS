using Photon.Pun;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Com.Kawaiisun.SimpleHostile
{
    public class Weapon : MonoBehaviourPunCallbacks
    {
        #region Variable

        public Gun[] loadout;


        [HideInInspector] public Gun currentGunData;

        public Transform weaponParent;

        private int currentIndex;

        private float currentCoolDown;

        private GameObject currentWeapon;
        public GameObject bulletHolePrefabs;
        public LayerMask canBeShot;
        public AudioSource sfx;
        public AudioClip hitMarkerSound;

        private bool isReloading;
        public bool isAiming;

        private Image hitMarker;
        private float hitMarkerWait;

        #endregion Variable

        #region MonoBehaviour Callbacks

        private void Start()
        {
            hitMarker = GameObject.Find("HUD/Hitmarker/Img").GetComponent<Image>();
            hitMarker.color = new Color(1, 1, 1, 0);

           foreach (Gun a in loadout) a.Initialize();
            Equip(0);
        }

        private void Update()
        {
            

            if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha1)) { photonView.RPC("Equip", RpcTarget.All, 0); }
            if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha2)) { photonView.RPC("Equip", RpcTarget.All, 1); }
            if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha3)) { photonView.RPC("Equip", RpcTarget.All, 2); }

                if (currentWeapon != null)
                {
                if (photonView.IsMine)
                {
                    //Aim(Input.GetMouseButton(1));

                    if (loadout[currentIndex].burst != 1)
                    {
                        if (Input.GetMouseButtonDown(0) && currentCoolDown <= 0 && isReloading == false)
                        {
                            if (loadout[currentIndex].FireBullet()) photonView.RPC("Shoot", RpcTarget.All);
                            else { StartCoroutine(Reload(loadout[currentIndex].reloadTime)); }
                        }
                    }
                    else
                    {
                        if (Input.GetMouseButton(0) && currentCoolDown <= 0 && isReloading == false)
                        {
                            if (loadout[currentIndex].FireBullet()) photonView.RPC("Shoot", RpcTarget.All);
                            else { StartCoroutine(Reload(loadout[currentIndex].reloadTime)); }
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.R) && isReloading == false )
                    {
                        photonView.RPC("ReloadRPC", RpcTarget.All);
                    }

                    //weapon position elasticity
                    currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 6.7f);

                    //cooldown weapon
                    if (currentCoolDown > 0) currentCoolDown -= Time.deltaTime;
                }
            }

            if (photonView.IsMine)
            {
                if(hitMarkerWait >= 0)
                {
                    hitMarkerWait -= Time.deltaTime;
                }

                else
                {
                    hitMarker.color = Color.Lerp(hitMarker.color, new Color(1, 1, 1, 0), Time.deltaTime * 4.5f);
                }
            }

            /*Debug.Log("Current Stash " + loadout[currentIndex].stash);
            Debug.Log("Current clip " + loadout[currentIndex].clip);*/

            if (!photonView.IsMine)

                if (currentWeapon != null)
                {
                    currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 6.7f); return;
                }
        }

        #endregion MonoBehaviour Callbacks

        #region Private Methods

        [PunRPC]
        private void ReloadRPC()
        {
            StartCoroutine(Reload(loadout[currentIndex].reloadTime));

        }

        IEnumerator Reload(float p_wait)
        {
            isReloading = true;


            if (currentWeapon.GetComponent<Animator>())
                currentWeapon.GetComponent<Animator>().Play("Reload", 0, 0);


            else
                currentWeapon.SetActive(false);


            yield return new WaitForSeconds(p_wait);
            loadout[currentIndex].Reload();
            currentWeapon.SetActive(true);
            isReloading = false;
        }
        
        [PunRPC]
        private void Equip(int p_ind)
        {
            if (currentWeapon != null) 
            {
                if (isReloading) StartCoroutine(Reload(loadout[currentIndex].reloadTime));
                Destroy(currentWeapon); 
            }

            currentIndex = p_ind;

            GameObject t_newEquipment = Instantiate(loadout[p_ind].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
            t_newEquipment.transform.localPosition = Vector3.zero;
            t_newEquipment.transform.localEulerAngles = Vector3.zero;
            t_newEquipment.GetComponent<Sway>().isOurs = photonView.IsMine;

            if (photonView.IsMine) ChangeLayerRecrusively(t_newEquipment, 8);
            else ChangeLayerRecrusively(t_newEquipment, 0);

            t_newEquipment.GetComponent<Animator>().Play("Equip", 0, 0);



            currentWeapon = t_newEquipment;
            currentGunData = loadout[p_ind];
        }

        private void ChangeLayerRecrusively(GameObject p_target, int p_layer)
        {
            p_target.layer = p_layer;
            foreach (Transform a in p_target.transform)
            {
                ChangeLayerRecrusively(a.gameObject, p_layer);
            }
        }

       
        public void Aim(bool p_isAiming)
        {
            if (!currentWeapon) return;

            isAiming = p_isAiming;
            Transform t_anchor = currentWeapon.transform.Find("Anchor");
            Transform t_State_Ads = currentWeapon.transform.Find("States/ADS");
            Transform t_State_hips = currentWeapon.transform.Find("States/Hips");

            if (p_isAiming)
            {
                //aim
                t_anchor.position = Vector3.Lerp(t_anchor.position, t_State_Ads.position, Time.deltaTime * loadout[currentIndex].aimSpeed);
            }
            if(!p_isAiming)
            {
                //hips
                t_anchor.position = Vector3.Lerp(t_anchor.position, t_State_hips.position, Time.deltaTime * loadout[currentIndex].aimSpeed);
            }
        }

        [PunRPC]
        private void Shoot()
        {
            Transform t_spawn = transform.Find("Camera/Normal Camera");


            //cooldown
            currentCoolDown = loadout[currentIndex].fireRate;

            for (int i = 0; i < Mathf.Max(1, currentGunData.pellete); i++)
            {

                //Bloom

                Vector3 t_Bloom = t_spawn.position + t_spawn.forward * 1000f;
                t_Bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * t_spawn.up;
                t_Bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * t_spawn.right;
                t_Bloom -= t_spawn.position;
                t_Bloom.Normalize();



                //Raycast
                RaycastHit t_hit = new RaycastHit();

                //Debug.DrawRay(t_spawn.position, t_spawn.forward, Color.red);

                if (Physics.Raycast(t_spawn.position, t_Bloom, out t_hit, 1000f, canBeShot))
                {
                    GameObject _newBulletHole = Instantiate(bulletHolePrefabs, t_hit.point + t_hit.normal * .001f, Quaternion.identity) as GameObject;
                    _newBulletHole.transform.LookAt(t_hit.point + t_hit.normal);
                    Destroy(_newBulletHole, 2f);

                    if (photonView.IsMine)
                    {
                        //if we are shooting other player
                        if (t_hit.collider.gameObject.layer == 9)
                        {
                            //RPC call to damage player goes here......
                            t_hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, loadout[currentIndex].damage);
                            //show hitmarker
                            hitMarker.color = Color.white;
                            sfx.PlayOneShot(hitMarkerSound);
                            hitMarkerWait = 0.5f;
                        }
                    }
                }

            }
            

            //sound fx
            sfx.Stop();
            sfx.clip = currentGunData.gunShotSpund;
            sfx.pitch = 1 - currentGunData.pitchRandomizer + Random.Range(-currentGunData.pitchRandomizer, currentGunData.pitchRandomizer);
            sfx.Play();

            //gun fx
            currentWeapon.transform.Rotate(Random.Range(-loadout[currentIndex].recoil_x, loadout[currentIndex].recoil_x), 0 , 0);
            currentWeapon.transform.Rotate(0, Random.Range(-loadout[currentIndex].recoil_y, loadout[currentIndex].recoil_y), 0);

            currentWeapon.transform.position -= currentWeapon.transform.forward * loadout[currentIndex].kickBack;
            if (isAiming)
            {
                currentWeapon.transform.Rotate(0, Random.Range(-loadout[currentIndex].recoil_y , loadout[currentIndex].recoil_y ) * 0.06f, 0);
                currentWeapon.transform.Rotate(Random.Range(-loadout[currentIndex].recoil_x  , loadout[currentIndex].recoil_x ) * 0.06f, 0, 0);

            }

            if (currentGunData.recovery) currentWeapon.GetComponent<Animator>().Play("Recovery", 0, 0);


        }

        [PunRPC]
        private void TakeDamage(int p_damage)
        {
            GetComponent<Player>().TakeDamage(p_damage);
        }

        #endregion Private Methods

        #region Public Methods

        public void RefreshAmmo(Text p_text)
        {
            int t_clip = loadout[currentIndex].GetClip();
            int t_stash = loadout[currentIndex].GetStash();

            p_text.text = t_clip.ToString("D2") + " / " + t_stash.ToString("D2");
        }
        #endregion
    }
}