#region Copyright
//
// Copyright 2015 Underscore Research LLC.  
// ALL RIGHTS RESERVED.
//
// UNDERSCORE RESEARCH LLC. MAKES NO REPRESENTATIONS OR
// WARRANTIES ABOUT THE SUITABILITY OF THE SOFTWARE, EITHER
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE, OR NON-INFRINGEMENT. DELL SHALL
// NOT BE LIABLE FOR ANY DAMAGES SUFFERED BY LICENSEE
// AS A RESULT OF USING, MODIFYING OR DISTRIBUTING
// THIS SOFTWARE OR ITS DERIVATIVES.
//
// Authored by Henrik Johnson
//
#endregion

using System;
using System.Collections.Generic;

namespace Underscore.SoapParser.Test
{
  /// <summary>
  /// Byte array comparer.
  /// </summary>
  public class ByteArrayComparer : IComparer<byte[]>
  {
    private static readonly ByteArrayComparer comparer = new ByteArrayComparer();

    /// <summary>
    /// Preallocated instance
    /// </summary>
    public static ByteArrayComparer Comparer
    {
      get { return comparer; }
    }

    #region Implementation of IComparer<in byte[]>

    /// <summary>
    /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
    /// </summary>
    /// <returns>
    /// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, as shown in the following table.Value Meaning Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.
    /// </returns>
    /// <param name="x">The first object to compare.</param><param name="y">The second object to compare.</param>
    public int Compare(byte[] x, byte[] y)
    {
      int min = Math.Min(x.Length, y.Length);
      for (int i = 0; i < min; i++)
        if (x[i] != y[i])
          return (int)x[i] - (int)y[i];
      return x.Length - y.Length;
    }

    #endregion
  }

}
