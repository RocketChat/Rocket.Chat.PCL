using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MeteorPCL;
using Newtonsoft.Json.Linq;
using RocketChatPCL;

namespace RocketChatPCL
{
	public abstract class AbstractCollection<T> : IRCollection<T>
	{
		protected Dictionary<string, T> _items = new Dictionary<string, T>();
		protected IMeteor _meteor;
		protected HashSet<string> _collections = new HashSet<string>();

		public AbstractCollection(IMeteor meteor, params string[] collectionNames)
		{
			_meteor = meteor;	
			_meteor.ItemAdded += ProcessItemAdded;
			_meteor.ItemUpdated += ProcessItemUpdated;
			_meteor.ItemDeleted += ProcessItemDeleted;

			foreach (string collection in collectionNames)
				_collections.Add(collection);
		}

		public T this[string key]
		{
			get
			{
				return _items[key];
			}
		}

		public int Count
		{
			get
			{
				return _items.Count;
			}
		}

		public IEnumerable<string> Keys
		{
			get
			{
				return _items.Keys;
			}
		}

		public IEnumerable<T> Values
		{
			get
			{
				return _items.Values;
			}
		}

		public bool ContainsKey(string key)
		{
			return _items.ContainsKey(key);
		}

		public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
		{
			return _items.GetEnumerator();
		}

		public bool TryGetValue(string key, out T value)
		{
			return _items.TryGetValue(key, out value);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _items.GetEnumerator();
		}

		private void ProcessItemAdded(string id, string collection, JObject obj)
		{
			if (_collections.Contains(collection))
			{
				OnItemAdded(id, collection, obj);
			}
		}

		private void ProcessItemUpdated(string id, string collection, JObject obj)
		{
			if (_collections.Contains(collection))
			{
				OnItemUpdated(id, collection, obj);
			}
		}

		private void ProcessItemDeleted(string id, string collection)
		{
			if (_collections.Contains(collection))
			{
				OnItemDeleted(id, collection);
			}
		}

		protected abstract void OnItemAdded(string id, string collection, JObject obj);
		protected abstract void OnItemUpdated(string id, string collection, JObject obj);
		protected abstract void OnItemDeleted(string id, string collection);
	}
}
