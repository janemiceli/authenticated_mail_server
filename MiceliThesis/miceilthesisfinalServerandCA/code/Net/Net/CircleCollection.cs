using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net
{
    /// <summary>
    /// Circle collection. Elements will be circled clockwise.
    /// </summary>
    public class CircleCollection<T>
    {
        private Queue<T> m_pItems = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CircleCollection()
        {
            m_pItems = new Queue<T>();
        }


        #region methd Add

        /// <summary>
        /// Adds specified items to the collection.
        /// </summary>
        /// <param name="items">Items to add.</param>
        public void Add(T[] items)
        {
            foreach(T item in items){
                Add(item);
            }
        }

        /// <summary>
        /// Adds specified item to collection.
        /// </summary>
        /// <param name="item">Item to add.</param>
        public void Add(T item)
        {
            m_pItems.Enqueue(item);
        }

        #endregion

        /*
        #region method Remove

        /// <summary>
        /// Removes specified item from collection.
        /// </summary>
        /// <param name="item"></param>
        public void Remove(T item)
        {
           m_pItems. 
        }

        #endregion
*/

        #region method Clear

        /// <summary>
        /// Clears all items from collection.
        /// </summary>
        public void Clear()
        {
            m_pItems.Clear();
        }

        #endregion

        #region method Next

        /// <summary>
        /// Gets next item.
        /// </summary>
        public T Next()
        {
            T item = m_pItems.Dequeue();
            m_pItems.Enqueue(item);

            return item;
        }

        #endregion


        #region Properties Implementation

        /// <summary>
        /// Gets number of items in queue.
        /// </summary>
        public int Count
        {
            get{ return m_pItems.Count; }
        }

        #endregion
    }
}
