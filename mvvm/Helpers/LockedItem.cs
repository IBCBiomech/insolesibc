using OpenCvSharp;
using System;
using System.Collections.Generic;
namespace mvvm.Helpers
{
    public class LockedItem<T>
    {
        public LockedItem(T item)
        {
            Item = item;
        }

        public readonly object lockObject = new object();
        private T item;
        public T Item
        {
            get
            {
                lock (lockObject)
                {
                    return item;
                }
            }
            set
            {
                lock (lockObject)
                {
                    item = value;
                }
            }
        }
    }
}
