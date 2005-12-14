using System;
using System.Collections;

namespace antlr.stringtemplate.language
{
	
	/// <summary>Represents the name of a formal argument
	/// defined in a template:
	/// 
	/// group test;
	/// test(a,b) : "$a$ $b$"
	/// t() : "blort"
	/// 
	/// Each template has a set of these formal arguments or uses
	/// a placeholder object: UNKNOWN (indicating that no arguments
	/// were specified such as when a template is loaded from a file.st).
	/// 
	/// Note: originally, I tracked cardinality as well as the name of an
	/// attribute.  I'm leaving the code here as I suspect something may come
	/// of it later.  Currently, though, cardinality is not used.
	/// </summary>
	public class FormalArgument
	{
		// the following represent bit positions emulating a cardinality bitset.
		public const int OPTIONAL = 1; // a?
		public const int REQUIRED = 2; // a
		public const int ZERO_OR_MORE = 4; // a*
		public const int ONE_OR_MORE = 8; // a+
		
		public static readonly String[] suffixes = new String[]{null, "?", "", null, "*", null, null, null, "+"};
		
		/// <summary>When template arguments are not available such as when the user
		/// uses "new StringTemplate(...)", then the list of formal arguments
		/// must be distinguished from the case where a template can specify
		/// args and there just aren't any such as the t() template above.
		/// </summary>
		public static IDictionary UNKNOWN = new Hashtable();
		
		protected internal String name;
		//protected int cardinality = REQUIRED;

		// If they specified name="value", store the template here 
		public StringTemplate defaultValueST;
		
		public FormalArgument(String name)
		{
			this.name = name;
		}

		public FormalArgument(String name, StringTemplate defaultValueST) 
		{
			this.name = name;
			this.defaultValueST = defaultValueST;
		}
				
		public static String getCardinalityName(int cardinality)
		{
			switch (cardinality)
			{
				
				case OPTIONAL:  return "optional";
				
				case REQUIRED:  return "exactly one";
				
				case ZERO_OR_MORE:  return "zero-or-more";
				
				case ONE_OR_MORE:  return "one-or-more";
				
				default:  return "unknown";
				
			}
		}
		
		public override String ToString()
		{
			if ( defaultValueST!=null ) 
			{
				return name+"="+defaultValueST;
			}
			return name;
		}
	}
}