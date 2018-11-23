using UnityEngine; 
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Exposes sorting layer of MeshRenderer.
    /// </summary>
    public class SortingLayerExposer : MonoBehaviour
    {
        /// <summary>
        /// The name of the layer to set.
        /// </summary>
        public string sortingLayerName = "Default";

        /// <summary>
        /// The sorting order to set.
        /// </summary>
        public int sortingOrder = 0;

        void Awake()
        {
            gameObject.GetComponent<MeshRenderer>().sortingLayerName = sortingLayerName;
            gameObject.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        }
    }
}