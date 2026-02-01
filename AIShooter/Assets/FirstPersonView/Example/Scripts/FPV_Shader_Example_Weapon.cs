using UnityEngine;
using System.Collections;

namespace FirstPersonView.Example
{
    public class FPV_Shader_Example_Weapon : MonoBehaviour
    {
        public Rigidbody wRigidbody;
        public Collider wCollider;

        public Transform spawnpoint;

        public ParticleSystem _particleSystem;

        public IFPV_Object fpvObject { get; private set; }

        private bool isOnPlayer;

        public Transform sightCameraPlacement;

        private int layer = 1 << 0; //Default layer

        public GameObject hitObjectPrefab;

        private bool _isAiming;

        public void Setup()
        {
            fpvObject = GetComponent<IFPV_Object>();

            isOnPlayer = false;
        }

        public void SetWeaponOnPlayer(Transform player)
        {
            transform.SetParent(player);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            wRigidbody.isKinematic = true;
            wCollider.enabled = false;

            isOnPlayer = true;

            fpvObject.SetAsFirstPersonObject();
        }

        public void SetWeaponOnWorld()
        {
            transform.parent = null;

            wRigidbody.isKinematic = false;
            wCollider.enabled = true;

            isOnPlayer = false;

            fpvObject.RemoveAsFirstPersonObject();
        }

        public bool IsOnPlayer()
        {
            return isOnPlayer;
        }

        public void Aim(bool aim, Transform camera)
        {
            _isAiming = aim;

            if (aim)
            {
                Vector3 vector = camera.position - sightCameraPlacement.position;

                transform.position = Vector3.Lerp(transform.position, transform.position + vector, Time.deltaTime * 4);
            }
            else
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, Time.deltaTime * 4);
            }
        }

        public void Fire()
        {
            Vector3 point;
            Vector3 direction;

            if (_isAiming) {
                //Get the Position and direction of the spawnpoint of the weapon from First Person Prespective into World
                FPV.FPVToWorld(sightCameraPlacement, out point, out direction);
            }
            else {
                //Get the Position and direction of the spawnpoint of the weapon from First Person Prespective into World
                FPV.FPVToWorld(spawnpoint, out point, out direction);
            }

            RaycastHit hit;
            if(Physics.Raycast(point, direction, out hit, 1000, layer))
            {
                GameObject.Instantiate(hitObjectPrefab, hit.point, Quaternion.identity);
            }

            _particleSystem.Play();
        }
    }
}