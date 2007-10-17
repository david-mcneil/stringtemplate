namespace Antlr.StringTemplate.Language
{
	using System;
	using ICollection	= System.Collections.ICollection;
	using IEnumerator	= System.Collections.IEnumerator;
	using ArrayList		= System.Collections.ArrayList;
	using StringBuilder = System.Text.StringBuilder;

	
	/// <summary>
	/// An ICollection implementation that skips over it's first element.
	/// </summary>
	internal sealed class RestCollection : ICollection
	{
		#region Helper classes

		/// <summary>
		/// An IEnumerator implementation that skips over the first elements in 
		/// it's collection.
		/// </summary>
		internal sealed class RestCollectionIterator : IEnumerator
		{
			/// <summary>The collection to be enumerated</summary>
			private ICollection collection;
			private IEnumerator inner;
			private bool beforeMoveNext;

			public RestCollectionIterator(RestCollection collection)
			{
				this.collection = collection._inner;
				Reset();
			}
		
			#region IEnumerator Members

			public void Reset()
			{
				if (inner == null)
				{
					inner = collection.GetEnumerator();
				}
				inner.Reset();
				inner.MoveNext();		// skip first
				beforeMoveNext = true;
			}

			public object Current
			{
				get
				{
					if (beforeMoveNext)
					{
						throw new InvalidOperationException("Enumeration has either not started or has already finished.");
					}
					return inner.Current;
				}
			}

			public bool MoveNext()
			{
				beforeMoveNext = false;
				return inner.MoveNext();
			}

			#endregion
		
			/// <summary>
			/// The result of asking for the string of an iterator is the list of elements
			/// and so this is just the cat'd list of both elements.  This is destructive
			/// in that the iterator cursors have moved to the end after printing.
			/// </summary>
			public override string ToString()
			{
				StringBuilder buf = new StringBuilder();
				//this.Reset();
				while (this.MoveNext())
				{
					buf.Append(this.Current);
				}
				return buf.ToString();
			}
		}

		#endregion

		private ICollection _inner;
		private int _count = 0;
		
		#region Constructors

		internal RestCollection(ICollection collection)
		{
			if ( (collection == null) || (collection.Count < 2) )
				_inner = new object[0];
			else
			{
				_inner = collection;
				_count = _inner.Count - 1;
			}
		}

		internal RestCollection(IEnumerator enumerator)
		{
			if (enumerator == null)
				this._inner = new object[0];
			else
			{
				ArrayList list = new ArrayList();
				//try
				//{
				//    object o = enumerator.Current;
				//    list.Add(o);
				//}
				//catch (InvalidOperationException)
				//{
				//    // Just means there's no current object
				//}

				while (enumerator.MoveNext())
				{
					list.Add(enumerator.Current);
				}
				this._inner = list;
				_count = _inner.Count-1;
			}
		}

		#endregion
		
		public override bool Equals(object o)
		{
			if (o is RestCollection)
			{
				RestCollection other = (RestCollection) o;
				if ((Count == 0) && (other.Count == 0))
					return true;
				else if (Count == other.Count)
				{
					IEnumerator myIter = GetEnumerator();
					IEnumerator otherIter = other.GetEnumerator();
					while ( myIter.MoveNext() )
					{
						if ( !otherIter.MoveNext() || !myIter.Current.Equals(otherIter.Current) )
							return false;
					}
					return !otherIter.MoveNext();
				}
			}
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#region ICollection Members

		public bool IsSynchronized
		{
			get { return _inner.IsSynchronized; }
		}

		public int Count
		{
			get { return _count; }
		}

		public void CopyTo(Array array, int index)
		{
			_inner.CopyTo(array, index);
		}

		public object SyncRoot
		{
			get { return _inner.SyncRoot; }
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return new RestCollectionIterator(this);
		}

		#endregion
	}
}