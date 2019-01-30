using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// C# extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Returns new colour with specified alpha.
        /// </summary>
        /// <returns>Colour with alpha.</returns>
        /// <param name="c">R, G, B values are copied.</param>
        /// <param name="alpha">Alpha.</param>
        public static Color WithAlpha(this Color c, float alpha)
        {
            return new Color(c.r, c.g, c.b, alpha);
        }

        /// <summary>
        /// Flashes the SpriteRenderes sprite on and off to signify that damage has occured.
        /// </summary>
        /// <param name="rend">Spriterenderes sprite to flash.</param>
        public static void DoDamageFlash(this SpriteRenderer rend)
        {
            var coHandle = rend.gameObject.GetComponent<CoroutineHandler>();

            if (coHandle == null)
            {
                coHandle = rend.gameObject.AddComponent<CoroutineHandler>();
            }

            coHandle.RunCoroutine(FlashSprite(rend));
        }

        private static IEnumerator FlashSprite(SpriteRenderer rend)
        {
            const float alphaStep = 10f;
            float alpha = 1f;

            while (alpha > 0.2f)
            {
                alpha -= alphaStep * Time.deltaTime;
                rend.color = rend.color.WithAlpha(alpha);
                yield return null;
            }

            rend.color = rend.color.WithAlpha(1f);
        }
    }
}