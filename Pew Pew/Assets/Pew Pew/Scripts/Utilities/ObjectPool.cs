using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GameCore
{
    /// <summary>
    /// Generic object pool.
    /// </summary>
    public class ObjectPool<T> where T : MonoBehaviour
    {
        private List<T> m_PooledItems = new List<T>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPool`1"/> class.
        /// </summary>
        /// <param name="prefab">Prefab.</param>
        /// <param name="numToPool">Number to pool.</param>
        public ObjectPool(GameObject prefab, int numToPool)
            : this(prefab, numToPool, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPool`1"/> class.
        /// </summary>
        /// <param name="prefab">Prefab.</param>
        /// <param name="numToPool">Number to pool.</param>
        /// <param name="owner">Owner.</param>
        public ObjectPool(GameObject prefab,
            int numToPool, Transform owner)
        {
            for (int i = 0; i < numToPool; i++)
            {
                var itemToPool = ((GameObject)GameObject.Instantiate(prefab)).GetComponent<T>();
                itemToPool.gameObject.SetActive(false);
                if (owner != null)
                {
                    itemToPool.transform.SetParent(owner);
                }
                m_PooledItems.Add(itemToPool);
            }
        }

        /// <summary>
        /// Returns object from pool. If no object is found, null is returned.
        /// </summary>
        /// <returns>The object.</returns>
        public T GetObject()
        {
            if (m_PooledItems.Count > 0)
            {
                foreach (var pooledItem in m_PooledItems)
                {
                    if (!pooledItem.gameObject.activeSelf)
                    {
                        pooledItem.gameObject.SetActive(true);
                        return pooledItem;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Disables object and adds to pool.
        /// </summary>
        /// <param name="obj">Object to pool.</param>
        public void PoolObject(T obj)
        {
            obj.gameObject.SetActive(false);

            if (!m_PooledItems.Contains(obj))
            {
                m_PooledItems.Add(obj);
            }
        }

        /// <summary>
        /// Returns list of active objects. These objects have are currently in use.
        /// </summary>
        /// <returns>The active.</returns>
        public List<T> GetActive()
        {
            var active = new List<T>();

            foreach (var pooledItem in m_PooledItems)
            {
                if (pooledItem.gameObject.activeSelf)
                {
                    active.Add(pooledItem);
                }
            }

            return active;
        }
    }
}