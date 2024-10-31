using System;
using System.Collections.Generic;

namespace Mirivoice.Commands
{
    public class Memento<T>
    {
        public Stack<T> states { get; }
        public Memento()
        {
            states = new Stack<T>();

        }

        public void Push(T state)
        {
            states.Push(state);
        }

        public void Clear()
        {
            states.Clear();
        }

        public T Pop()
        {
            return states.Pop();
        }

        public bool CanPop => states.Count > 0;

        public int Count => states.Count;
        public override string ToString()
        {
            return String.Join(", ", states);
        }
    }

}
