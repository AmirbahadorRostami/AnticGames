using System.Collections.Generic;

namespace TacticalGame.Utilities
{
    public static class ListPool<T>
    {
        private static readonly Stack<List<T>> pool = new Stack<List<T>>();
        private static readonly object lockObject = new object();

        public static List<T> Get()
        {
            lock (lockObject)
            {
                if (pool.Count > 0)
                {
                    return pool.Pop();
                }
            }
            
            return new List<T>();
        }

        public static void Release(List<T> list)
        {
            if (list == null) return;
            
            list.Clear();
            
            lock (lockObject)
            {
                pool.Push(list);
            }
        }
    }
    
    public static class ListExtensions
    {
        public static void ReturnToPool<T>(this List<T> list)
        {
            ListPool<T>.Release(list);
        }
    }
}
