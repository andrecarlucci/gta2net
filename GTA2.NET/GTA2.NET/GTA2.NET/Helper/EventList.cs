//15.02.2010

using System;
using System.Collections.Generic;

namespace Hiale.GTA2NET.Helper
{
    public class EventList<T> : List<T>
    {
        public event EventHandler<GenericEventArgs<T>> ItemAdded;
        public event EventHandler<GenericEventArgs<T>> ItemRemoved;

        public new void Add(T item)
        {
            base.Add(item);
            if (ItemAdded != null)
                ItemAdded(this, new GenericEventArgs<T>(item));
        }

        public new void Remove(T item)
        {
            base.Remove(item);
            if (ItemRemoved != null)
                ItemRemoved(this, new GenericEventArgs<T>(item));
        }
    }

    public class GenericEventArgs<T> : EventArgs
    {
        public T Item { get; set; }

        public GenericEventArgs(T item)
        {
            this.Item = item;
        }
    }
}
