using System;


namespace Mirivoice.Mirivoice.Core.Utils
{
    public abstract class SingletonBase<T> where T : class
    {
        private static readonly Lazy<T> instance = new Lazy<T>(
            () => (T)Activator.CreateInstance(typeof(T), true),
            isThreadSafe: true);
        public static T Instance => instance.Value;
    }
}
