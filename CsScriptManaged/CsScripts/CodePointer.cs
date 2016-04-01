﻿using System;
using System.Linq;

namespace CsScripts
{
    /// <summary>
    /// Extension specialization functions for CodePointer.
    /// </summary>
    public static class CodePointerExtensions
    {
        /// <summary>
        /// Reads the string from CodePointer.
        /// </summary>
        /// <param name="codePointer">The code pointer.</param>
        /// <param name="length">The length in characters. If length is -1, string is null terminated</param>
        public static string ReadString(this CodePointer<char> codePointer, int length = -1)
        {
            return UserType.ReadString(codePointer.GetCodeType().Module.Process, codePointer.GetPointerAddress(), (int)codePointer.GetCodeType().ElementType.Size, length);
        }

        /// <summary>
        /// Reads the string from CodePointer.
        /// </summary>
        /// <param name="codePointer">The code pointer.</param>
        /// <param name="length">The length in characters.</param>
        public static string ReadString(this CodePointer<char> codePointer, uint length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadString(codePointer, (int)length);
        }

        /// <summary>
        /// Reads the string from CodePointer.
        /// </summary>
        /// <param name="codePointer">The code pointer.</param>
        /// <param name="length">The length in characters.</param>
        public static string ReadString(this CodePointer<char> codePointer, long length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadString(codePointer, (int)length);
        }

        /// <summary>
        /// Reads the string from CodePointer.
        /// </summary>
        /// <param name="codePointer">The code pointer.</param>
        /// <param name="length">The length in characters.</param>
        public static string ReadString(this CodePointer<char> codePointer, ulong length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadString(codePointer, (int)length);
        }

        /// <summary>
        /// Reads the string from CodePointer.
        /// </summary>
        /// <param name="codePointer">The code pointer.</param>
        /// <param name="length">The length in bytes. If length is -1, string is null terminated</param>
        public static string ReadStringByteLength(this CodePointer<char> codePointer, int length = -1)
        {
            int charSize = (int)codePointer.GetCodeType().ElementType.Size;

            if (length % charSize != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return UserType.ReadString(codePointer.GetCodeType().Module.Process, codePointer.GetPointerAddress(), charSize, length / charSize);
        }

        /// <summary>
        /// Reads the string from CodePointer.
        /// </summary>
        /// <param name="codePointer">The code pointer.</param>
        /// <param name="length">The length in bytes.</param>
        public static string ReadStringByteLength(this CodePointer<char> codePointer, uint length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadStringByteLength(codePointer, (int)length);
        }

        /// <summary>
        /// Reads the string from CodePointer.
        /// </summary>
        /// <param name="codePointer">The code pointer.</param>
        /// <param name="length">The length in bytes.</param>
        public static string ReadStringByteLength(this CodePointer<char> codePointer, long length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadStringByteLength(codePointer, (int)length);
        }

        /// <summary>
        /// Reads the string from CodePointer.
        /// </summary>
        /// <param name="codePointer">The code pointer.</param>
        /// <param name="length">The length in bytes.</param>
        public static string ReadStringByteLength(this CodePointer<char> codePointer, ulong length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadStringByteLength(codePointer, (int)length);
        }
    }

    /// <summary>
    /// Wrapper class that represents a pointer.
    /// </summary>
    /// <typeparam name="T">The type of element this pointer points to.</typeparam>
    public class CodePointer<T> : Variable
    {
        /// <summary>
        /// The element
        /// </summary>
        private UserMember<T> element;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodePointer{T}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public CodePointer(Variable variable)
            : base(variable)
        {
            if (!GetCodeType().IsPointer)
            {
                throw new Exception("Wrong code type of passed variable " + variable.GetCodeType().Name);
            }

            element = UserMember.Create(() => variable.DereferencePointer().CastAs<T>());
        }

        /// <summary>
        /// Gets the element.
        /// </summary>
        public T Element
        {
            get
            {
                return element.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is null.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is null; otherwise, <c>false</c>.
        /// </value>
        public bool IsNull
        {
            get
            {
                return IsNullPointer();
            }
        }

        /// <summary>
        /// Converts pointer to array of specified length.
        /// </summary>
        /// <param name="length">The length.</param>
        public CodeArray<T> ToCodeArray(uint length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ToCodeArray((int)length);
        }

        /// <summary>
        /// Converts pointer to array of specified length.
        /// </summary>
        /// <param name="length">The length.</param>
        public CodeArray<T> ToCodeArray(int length)
        {
            return new CodeArray<T>(this, length);
        }

        /// <summary>
        /// Converts pointer to array of specified length.
        /// </summary>
        /// <param name="length">The length.</param>
        public T[] ToArray(uint length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ToArray((int)length);
        }

        /// <summary>
        /// Converts pointer to array of specified length.
        /// </summary>
        /// <param name="length">The length.</param>
        public T[] ToArray(int length)
        {
            return ToCodeArray(length).ToArray();
        }

        /// <summary>
        /// Gets the &lt;T&gt; at the specified index.
        /// </summary>
        /// <param name="index">The array index.</param>
        public T this[int index]
        {
            get
            {
                return GetArrayElement(index).CastAs<T>();
            }
        }

        /// <summary>
        /// Reads the ANSI string from CodePointer.
        /// </summary>
        /// <param name="length">The length in characters. If length is -1, string is null terminated</param>
        public string ReadAnsiString(int length = -1)
        {
            return UserType.ReadString(GetCodeType().Module.Process, GetPointerAddress(), 1, length);
        }

        /// <summary>
        /// Reads the ANSI string from CodePointer.
        /// </summary>
        /// <param name="length">The length in characters.</param>
        public string ReadAnsiString(uint length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadAnsiString((int)length);
        }

        /// <summary>
        /// Reads the ANSI string from CodePointer.
        /// </summary>
        /// <param name="length">The length in characters.</param>
        public string ReadAnsiString(long length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadAnsiString((int)length);
        }

        /// <summary>
        /// Reads the ANSI string from CodePointer.
        /// </summary>
        /// <param name="length">The length in characters.</param>
        public string ReadAnsiString(ulong length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadAnsiString((int)length);
        }

        /// <summary>
        /// Reads the Unicode string from CodePointer.
        /// </summary>
        /// <param name="length">The length in characters. If length is -1, string is null terminated</param>
        public string ReadUnicodeString(int length = -1)
        {
            return UserType.ReadString(GetCodeType().Module.Process, GetPointerAddress(), 2, length);
        }

        /// <summary>
        /// Reads the Unicode string from CodePointer.
        /// </summary>
        /// <param name="length">The length in characters.</param>
        public string ReadUnicodeString(uint length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadUnicodeString((int)length);
        }

        /// <summary>
        /// Reads the Unicode string from CodePointer.
        /// </summary>
        /// <param name="length">The length in characters.</param>
        public string ReadUnicodeString(long length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadUnicodeString((int)length);
        }

        /// <summary>
        /// Reads the Unicode string from CodePointer.
        /// </summary>
        /// <param name="length">The length in characters.</param>
        public string ReadUnicodeString(ulong length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadUnicodeString((int)length);
        }

        /// <summary>
        /// Reads the Unicode string from CodePointer.
        /// </summary>
        /// <param name="length">The length in bytes. If length is -1, string is null terminated</param>
        public string ReadUnicodeStringByteLength(int length)
        {
            if ((length & 1) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadUnicodeString(length / 2);
        }

        /// <summary>
        /// Reads the Unicode string from CodePointer.
        /// </summary>
        /// <param name="length">The length in bytes.</param>
        public string ReadUnicodeStringByteLength(uint length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadUnicodeStringByteLength((int)length);
        }

        /// <summary>
        /// Reads the Unicode string from CodePointer.
        /// </summary>
        /// <param name="length">The length in bytes.</param>
        public string ReadUnicodeStringByteLength(long length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadUnicodeStringByteLength((int)length);
        }

        /// <summary>
        /// Reads the Unicode string from CodePointer.
        /// </summary>
        /// <param name="length">The length in bytes.</param>
        public string ReadUnicodeStringByteLength(ulong length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadUnicodeStringByteLength((int)length);
        }
    }
}
