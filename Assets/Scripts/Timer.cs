using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DefaultNamespace
{
    public class Timer : MonoBehaviour
    {

        public float timeLeft = 60;

        public PoundSpawner poundSpawner;
        public BottleController bottleController;

        public TextMeshProUGUI timerUi;
        public TextMeshProUGUI earnedAmount;

        private void Start()
        {
            timerUi.text = timeLeft.ToString();
            timerUi.gameObject.SetActive(true);
        }

        private IEnumerator End(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            var scene = SceneManager.GetActiveScene();

            SceneManager.LoadScene(scene.buildIndex);
        }

        private void Update()
        {
            timeLeft -= Time.deltaTime;
            timerUi.text = ((int)timeLeft).ToString();

            if (timeLeft <= 0)
            {
                timerUi.gameObject.SetActive(false);
                earnedAmount.gameObject.SetActive(true);
                earnedAmount.text = $"You earned: £{poundSpawner.count}";
                bottleController.enabled = false;

                StartCoroutine(End(10));
            }
        }
    }
}