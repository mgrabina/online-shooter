using UnityEngine;

namespace lib.Utils
{
    public abstract class PoolObject<T> : MonoBehaviour{

        public abstract void Initialize(T t);

    }
}