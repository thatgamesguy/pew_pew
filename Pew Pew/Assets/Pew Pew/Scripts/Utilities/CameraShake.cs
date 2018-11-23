using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Camera shake based on perlin noise.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraShake : MonoBehaviour
    {
        /// <summary>
        /// The global magnitude dampener. Any requested camera shake magnitude is multipled by this.
        /// </summary>
        public float globalMagDampener = 0.8f;

        /// <summary>
        /// The global duration dampener. Any requested camera shake duration is multipled by this.
        /// </summary>
        public float globalDurDampener = 1f;

        private Camera cameraToShake;

        void Awake()
        {
            cameraToShake = GetComponent<Camera>();
        }

        /// <summary>
        /// Begin this instance. Moves the camera round based on perlin noise, centred around cameras current position.
        /// </summary>
        public void Begin(float duration, float magnitude)
        {
            StartCoroutine(DoShake(duration * globalDurDampener, magnitude * globalMagDampener));
        }

        private IEnumerator DoShake(float duration, float magnitude)
        {
            float elapsed = 0.0f;

            Vector3 originalCamPos = cameraToShake.transform.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float percentComplete = elapsed / duration;
                float damper = 1.0f - Mathf.Clamp(4.0f * percentComplete - 3.0f, 0.0f, 1.0f);

                // map value to [-1, 1]
                float x = Random.value * 2.0f - 1.0f;
                float y = Random.value * 2.0f - 1.0f;
                x *= magnitude * damper;
                y *= magnitude * damper;

                cameraToShake.transform.position = new Vector3(x, y, originalCamPos.z);

                yield return null;
            }

            cameraToShake.transform.position = originalCamPos;
        }
    }
}