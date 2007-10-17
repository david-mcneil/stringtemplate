namespace Antlr.StringTemplate.Language
{
	using System;
	using IList			= System.Collections.IList;
	using IEnumerator	= System.Collections.IEnumerator;
	using StringBuilder = System.Text.StringBuilder;
	
	/// <summary>
	/// Given a list of iterators, return the combined elements one by one.
	/// </summary>
	public sealed class CatIterator : IEnumerator
	{
		/// <summary>List of iterators to cat together </summary>
		private IList iterators;
		private object currentObject;
		private int currentIteratorIndex = 0;
		private bool hasCurrent = false;

		public CatIterator(IList iterators)
		{
			this.iterators = iterators;
		}
		
		public object Current
		{
			get 
			{
				if (!hasCurrent)
					throw new InvalidOperationException("Enumeration not started or already finished.");
				return currentObject; 
			}
		}

		public bool MoveNext()
		{
			if (currentIteratorIndex < iterators.Count)
			{
				IEnumerator iter = (IEnumerator) iterators[currentIteratorIndex];
				if (iter.MoveNext())
				{
					currentObject = iter.Current;
					return (hasCurrent = true);
				}
				else
				{
					currentIteratorIndex++;
					for ( ; currentIteratorIndex < iterators.Count; currentIteratorIndex++)
					{
						iter = (IEnumerator) iterators[currentIteratorIndex];
						if (iter.MoveNext())
						{
							currentObject = iter.Current;
							return (hasCurrent = true);
						}
					}
				}
			}
			return (hasCurrent = false);
		}
		
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

		public void  Reset()
		{
			currentIteratorIndex = 0;
			currentObject = null;
			hasCurrent = false;
			foreach (IEnumerator it in iterators)
				it.Reset();
		}
	}
}