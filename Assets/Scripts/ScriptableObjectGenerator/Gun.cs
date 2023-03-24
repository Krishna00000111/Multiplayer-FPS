using UnityEngine;

namespace Com.Kawaiisun.SimpleHostile
{
    [CreateAssetMenu(fileName = "New Gun", menuName = "Gun")]
    public class Gun : ScriptableObject
    {
        public string name;
        public float fireRate;
        public int damage;
        public int ammo;
        public int burst; // 0 = semi, 1 = auto, 2 = burst
        public int pellete;
        public int clipsize;
        public float aimSpeed;
        public float bloom;
        public float reloadTime;
        public float recoil_x;
        public float recoil_y;
        public float kickBack;
        [Range(0, 1)] public float mainFOV;
        [Range(0, 1)] public float weaponFOV;

        public AudioClip gunShotSpund;
        public float pitchRandomizer;
        public float volume;
        public bool recovery;
        public GameObject prefab;


        private int stash; //current ammo
        private int clip; //current clip

        public void Initialize()
        {
            stash = ammo;
            clip = clipsize;
        }

        public bool FireBullet()
        {
            if (clip > 0)
            {
                clip = clip - 1;
                return true;
            }
            else return false;
        }

        public void Reload()
        {
            stash += clip;
            clip = Mathf.Min(clipsize, stash);
            stash -= clip;
        }

        public int GetStash()
        { return stash; }

        public int GetClip()
        { return clip; }
    }
}