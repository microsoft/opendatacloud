// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace Msr.Odr.Admin
{
	/// <summary>
	/// String conversion helpers.
	/// </summary>
	internal static class StringExtensions
    {
		/// <summary>
		/// The global namespace for name generation
		/// </summary>
		public static readonly Guid GlobalNamespaceId = new Guid("6938602c-b606-40b9-809f-ba812d7f92c4");

		/// <summary>
		/// Converts the string to a stream of bytes
		/// </summary>
		/// <param name="content">The contents</param>
		/// <returns>The stream</returns>
		public static Stream ToStream(this string content)
		{
			return new MemoryStream(Encoding.UTF8.GetBytes(content));
		}

		/// <summary>
		/// Converts a string into a secure string.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <returns>The secure string.</returns>
		public static SecureString ToSecureString(this string source)
		{
			var secure = new SecureString();
			foreach (var c in source)
			{
				secure.AppendChar(c);
			}

			secure.MakeReadOnly();
			return secure;
		}

		/// <summary>
		/// Creates a GUID from a name
		/// </summary>
		/// <param name="qualifiedName">The full name</param>
		/// <returns>The Guid</returns>
		public static Guid GetGuidFromName(this string qualifiedName)
		{
			return GetGuidFromName(qualifiedName, GlobalNamespaceId);
		}

		/// <summary>
		/// Creates a GUID from a name
		/// </summary>
		/// <param name="qualifiedName">The full name</param>
		/// <param name="namespaceIdentifier">The namespace GUID</param>
		/// <returns>The Guid</returns>
		public static Guid GetGuidFromName(this string qualifiedName, Guid namespaceIdentifier)
		{
			String invariantName = qualifiedName.ToUpperInvariant();
			var invariantBytes = Encoding.BigEndianUnicode.GetBytes(invariantName);
			Byte[] buffer = new byte[invariantBytes.Length + 16];
			namespaceIdentifier.ToByteArray().CopyTo(buffer, 0);
			invariantBytes.CopyTo(buffer, 16);

			var hash = SHA1.Create();
			var data = hash.ComputeHash(buffer).Take(16).ToArray();
			// Add the version.
			data[7] &= 0x0F; // Mask the version bits
			data[7] |= 0x50; // Add the SHA-1 version/v5 (0x30=MD5)

			//Add the variant
			data[9] &= 0x3F; // Mask the variant bits
			data[9] |= 0x80; // Variant 10x- (standard)

			return new Guid(data);
		}
	}
}
