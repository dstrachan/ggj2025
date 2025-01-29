using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public class PoundSpawner : MonoBehaviour
    {

        public GameObject pound;

        public float count;


        private void Start()
        {

        }

        public void SpawnPound()
        {
            var position = transform.position;
            var newPos = new Vector3(position.x + Random.Range(0f, 0.04f),
                position.y + Random.Range(0, 0.04f), position.z + Random.Range(0f, 0.04f));

            var spawnedPound = Instantiate(pound, newPos, Quaternion.identity);
            var localRotation = spawnedPound.transform.localRotation;
            localRotation = Quaternion.AngleAxis(Random.Range(-180,180), Vector3.up) * localRotation;
            localRotation = Quaternion.AngleAxis(Random.Range(-180,180), Vector3.forward) * localRotation;
            localRotation = Quaternion.AngleAxis(Random.Range(-180,180), Vector3.right) * localRotation;
            spawnedPound.transform.localRotation = localRotation;
            
            count++;
        }
    }
}