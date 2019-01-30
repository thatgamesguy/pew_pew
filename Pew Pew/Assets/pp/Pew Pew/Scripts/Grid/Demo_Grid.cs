using UnityEngine;
using System.Collections;

namespace WarpGrid
{
    /// <summary>
    /// Used to demonstrate how to apply a force to an existing grid.
    /// </summary>
    public class Demo_Grid : MonoBehaviour
    {

        /// <summary>
        /// The grid to apply force to.
        /// </summary>
        public WarpingGrid grid;

        void Update()
        {

            if (Input.GetMouseButtonUp(0))
            {
                grid.ApplyDirectedForce(Vector2.up * 10f, Camera.main.ScreenToWorldPoint(Input.mousePosition), 1f);
            }
        }
    }
}