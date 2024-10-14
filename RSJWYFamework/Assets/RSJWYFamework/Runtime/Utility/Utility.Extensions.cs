using System;

namespace RSJWYFamework.Runtime.Utility.Extensions
{
    public static partial class Utility
    {
        /// <summary>
        /// 从数组中获取指定位置长度的memory
        /// </summary>
        /// <param name="array">数组</param>
        /// <param name="startIndex">起始位</param>
        /// <param name="length">终止位</param>
        public static Memory<byte> GetFromByteArrToMemory(this byte[] array, int startIndex, int length)
        {
            // 假设 `bytes` 是一个 byte[] 或 Memory<byte>
            Memory<byte> bytesMemory = array.AsMemory();
            // 使用 Slice 方法来获取特定的部分，而不需要复制它们
            return bytesMemory.Slice(startIndex, length);
        }
    }
}