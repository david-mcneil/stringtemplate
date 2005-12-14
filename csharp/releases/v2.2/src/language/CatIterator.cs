using System;
using System.Collections;
using System.Text;

namespace antlr.stringtemplate.language
{
	/// <summary>Given a list of iterators, return the combined elements one by one.</summary>
	public class CatIterator : IEnumerator
	{
		/// <summary>
		/// List of iterators to cat together 
		/// </summary>
		protected IList iterators;
		protected object curobj;

		public CatIterator(IList iterators) 
		{
			this.iterators = iterators;
		}

		public bool MoveNext() 
		{
			bool hasnext=false;
			for (int i=0;i<iterators.Count;i++)
			{
				IEnumerator it=(IEnumerator)iterators[i];
				hasnext=it.MoveNext();
				if (hasnext)
				{
					curobj=it.Current;
					break;
				}
			}
			return hasnext;
		}

		public object Current
		{
			get { return curobj; }
		}

		public void Reset()
		{
			foreach (IEnumerator it in iterators)
				it.Reset();
		}

		/// <summary>
		/// The result of asking for the string of an iterator is the list of elements
		/// and so this is just the cat'd list of both elements.  This is destructive
		/// in that the iterator cursors have moved to the end after printing.
		/// </summary>
		public override string ToString() 
		{
			StringBuilder buf = new StringBuilder();
			while (MoveNext())
			{
				buf.Append(Current);
			}
			return buf.ToString();
		}
	}
}