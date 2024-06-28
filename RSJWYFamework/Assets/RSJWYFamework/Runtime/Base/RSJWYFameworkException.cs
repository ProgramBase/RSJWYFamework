using UnityEngine;

namespace RSJWYFamework.Runtime.Base
{
    public class RSJWYFameworkException:UnityException
    {
        public RSJWYFameworkException( string message) : base($"{message}")
        {
        }
    }
}