using System;
using System.Collections.Generic;

namespace RocketChatPCL
{
	/// <summary>
	/// Represents a difference of a collection of objects.
	/// </summary>
	public class CollectionDiff<T>
	{
		/// <summary>
		/// List of objects that have been added.
		/// </summary>
		/// <value>The added.</value>
		public List<T> Added { get;  }
		/// <summary>
		/// List of objects that have been removed.
		/// </summary>
		/// <value>The removed.</value>
		public List<T> Removed { get; }
		/// <summary>
		/// List of objects that have been updated.
		/// </summary>
		/// <value>The updated.</value>
		public List<T> Updated { get; }

		public CollectionDiff()
		{
			Added = new List<T>();
			Removed = new List<T>();
			Updated = new List<T>();
		}
	}
}
