//
// In order to convert some functionality to Visual C#, the Java Language Conversion Assistant
// creates "support classes" that duplicate the original functionality.  
//
// Support classes replicate the functionality of the original code, but in some cases they are 
// substantially different architecturally. Although every effort is made to preserve the 
// original architecture of the application in the converted project, the user should be aware that 
// the primary goal of these support classes is to replicate functionality, and that at times 
// the architecture of the resulting solution may differ somewhat.
//

using System;
using System.Collections;
using System.Collections.Specialized;

	/// <summary>
	/// This interface should be implemented by any class whose instances are intended 
	/// to be executed by a thread.
	/// </summary>
	public interface IThreadRunnable
	{
		/// <summary>
		/// This method has to be implemented in order that starting of the thread causes the object's 
		/// run method to be called in that separately executing thread.
		/// </summary>
		void Run();
	}

/// <summary>
/// Contains conversion support elements such as classes, interfaces and static methods.
/// </summary>
public class SupportClass
{
	/// <summary>
	/// SupportClass for the Stack class.
	/// </summary>
	public class StackSupport
	{
		/// <summary>
		/// Removes the element at the top of the stack and returns it.
		/// </summary>
		/// <param name="stack">The stack where the element at the top will be returned and removed.</param>
		/// <returns>The element at the top of the stack.</returns>
		public static Object Pop(ArrayList stack)
		{
			Object obj = stack[stack.Count - 1];
			stack.RemoveAt(stack.Count - 1);

			return obj;
		}
	}


	/*******************************/
	/// <summary>
	/// This class contains static methods to manage TreeViews.
	/// </summary>
	public class TreeSupport
	{
		/// <summary>
		/// Creates a new TreeView from the provided HashTable.
		/// </summary> 
		/// <param name="hashTable">HashTable</param>		
		/// <returns>Returns the created tree</returns>
		public static System.Windows.Forms.TreeView CreateTreeView(Hashtable hashTable)
		{
			System.Windows.Forms.TreeView tree = new System.Windows.Forms.TreeView();
			return SetTreeView(tree,hashTable);
		}

		/// <summary>
		/// Sets a TreeView with data from the provided HashTable.
		/// </summary> 
		/// <param name="treeView">Tree.</param>
		/// <param name="hashTable">HashTable.</param>
		/// <returns>Returns the set tree.</returns>		
		public static System.Windows.Forms.TreeView SetTreeView(System.Windows.Forms.TreeView treeView, Hashtable hashTable)
		{
			foreach (DictionaryEntry myEntry in hashTable)
			{				
				System.Windows.Forms.TreeNode root = new System.Windows.Forms.TreeNode();

				if (myEntry.Value is ArrayList)
				{
					root.Text = "[";
					FillNode(root, (ArrayList)myEntry.Value);	
					root.Text = root.Text + "]";				
				}
				else if (myEntry.Value is Object[])
				{
					root.Text = "[";
					FillNode(root,(Object[])myEntry.Value);
					root.Text = root.Text + "]";
				}
				else if (myEntry.Value is Hashtable)
				{
					root.Text = "[";
					FillNode(root,(Hashtable)myEntry.Value);
					root.Text = root.Text + "]";
				}
				else
					root.Text = myEntry.ToString();

				treeView.Nodes.Add(root);					
			}
			return treeView;
		}
		

		/// <summary>
		/// Creates a new TreeView from the provided ArrayList.
		/// </summary> 
		/// <param name="arrayList">ArrayList.</param>		
		/// <returns>Returns the created tree.</returns>
		public static System.Windows.Forms.TreeView CreateTreeView(ArrayList arrayList)
		{
			System.Windows.Forms.TreeView tree = new System.Windows.Forms.TreeView();
			return SetTreeView(tree, arrayList);
		}

		/// <summary>
		/// Sets a TreeView with data from the provided ArrayList.
		/// </summary> 
		/// <param name="treeView">Tree.</param>
		/// <param name="arrayList">ArrayList.</param>
		/// <returns>Returns the set tree.</returns>
		public static System.Windows.Forms.TreeView SetTreeView(System.Windows.Forms.TreeView treeView, ArrayList arrayList)		
		{
			foreach (Object arrayEntry in arrayList)
			{
				System.Windows.Forms.TreeNode root = new System.Windows.Forms.TreeNode();
		
				if (arrayEntry is ArrayList)
				{
					root.Text = "[";
					FillNode(root, (ArrayList)arrayEntry);
					root.Text = root.Text + "]";
				}
				else if (arrayEntry is Hashtable)
				{
					root.Text = "[";
					FillNode(root,(Hashtable)arrayEntry);
					root.Text = root.Text + "]";
				}
				else if (arrayEntry is Object[])	
				{
					root.Text = "[";
					FillNode(root,(Object[])arrayEntry);
					root.Text = root.Text + "]";
				}
				else
					root.Text = arrayEntry.ToString();
				
		
				treeView.Nodes.Add(root);					
			}
			return treeView;
		}		
		
		/// <summary>
		/// Creates a new TreeView from the provided Object Array.
		/// </summary> 
		/// <param name="objectArray">Object Array.</param>		
		/// <returns>Returns the created tree.</returns>
		public static System.Windows.Forms.TreeView CreateTreeView(Object[] objectArray)
		{
			System.Windows.Forms.TreeView tree = new System.Windows.Forms.TreeView();
			return SetTreeView(tree, objectArray);
		}

		/// <summary>
		/// Sets a TreeView with data from the provided Object Array.
		/// </summary> 
		/// <param name="treeView">Tree.</param>
		/// <param name="objectArray">Object Array.</param>
		/// <returns>Returns the created tree.</returns>
		public static System.Windows.Forms.TreeView SetTreeView(System.Windows.Forms.TreeView treeView, Object[] objectArray)
		{
			foreach (Object arrayEntry in objectArray)
			{		
				System.Windows.Forms.TreeNode root = new System.Windows.Forms.TreeNode();

				if (arrayEntry is ArrayList)
				{
					root.Text = "[";
					FillNode(root,(ArrayList)arrayEntry);
					root.Text = root.Text + "]";
				}
				else if (arrayEntry is Hashtable)				
				{
					root.Text = "[";
					FillNode(root,(Hashtable)arrayEntry);						
					root.Text = root.Text + "]";		
				}
				else if (arrayEntry is Object[])
				{
					root.Text = "[";
					FillNode(root,(Object[])arrayEntry);					
					root.Text = root.Text + "]";
				}
				else
					root.Text = arrayEntry.ToString();

				treeView.Nodes.Add(root);
			}		
			return treeView;
		}		

		/// <summary>
		/// Creates a new TreeView with the provided TreeNode as root.
		/// </summary> 
		/// <param name="root">Root.</param>		
		/// <returns>Returns the created tree.</returns>
		public static System.Windows.Forms.TreeView CreateTreeView(System.Windows.Forms.TreeNode root)
		{
			System.Windows.Forms.TreeView tree = new System.Windows.Forms.TreeView();
			return SetTreeView(tree, root);
		}

		/// <summary>
		/// Sets a TreeView with the provided TreeNode as root.
		/// </summary>
		/// <param name="treeView">Tree</param>
		/// <param name="root">Root</param>
		/// <returns>Returns the created tree</returns>
		public static System.Windows.Forms.TreeView SetTreeView(System.Windows.Forms.TreeView treeView, System.Windows.Forms.TreeNode root)
		{
			if (root != null)
				treeView.Nodes.Add(root);
			return treeView;
		}
			
		/// <summary>
		/// Sets a TreeView with the provided TreeNode as root.
		/// </summary> 
		/// <param name="model">Root data model.</param>
		public static void SetModel(System.Windows.Forms.TreeView tree, System.Windows.Forms.TreeNode model)
		{
			tree.Nodes.Clear();
			tree.Nodes.Add(model);
		}
			
		/// <summary>
		/// Get the root TreeNode from a TreeView.
		/// </summary> 
		/// <param name="tree">Tree.</param>
		public static System.Windows.Forms.TreeNode GetModel(System.Windows.Forms.TreeView tree)
		{
			if (tree.Nodes.Count > 0 )
				return tree.Nodes[0];
			else
				return null;
		}

		/// <summary>
		/// Sets a TreeNode with data from the provided ArrayList instance.
		/// </summary> 
		/// <param name="treeNode">Node.</param>
		/// <param name="arrayList">ArrayList.</param>
		/// <returns>Returns the set node.</returns>
		public static System.Windows.Forms.TreeNode FillNode(System.Windows.Forms.TreeNode treeNode, ArrayList arrayList)
		{		
			foreach (Object arrayEntry in arrayList)
			{
				System.Windows.Forms.TreeNode root = new System.Windows.Forms.TreeNode();				

				if (arrayEntry is ArrayList)
				{
					root.Text = "[";
					FillNode(root, (ArrayList)arrayEntry);
					root.Text = root.Text + "]";
					treeNode.Nodes.Add(root);
					treeNode.Text = treeNode.Text + ", " + root.Text;
				}
				else if (arrayEntry is Object[])
				{					
					root.Text = "[";
					FillNode(root,(Object[])arrayEntry);	
					root.Text = root.Text + "]";
					treeNode.Nodes.Add(root);	
					treeNode.Text = treeNode.Text + ", " + root.Text;
				}
				else if (arrayEntry is Hashtable)
				{
					root.Text = "[";
					FillNode(root,(Hashtable)arrayEntry);	
					root.Text = root.Text + "]";
					treeNode.Nodes.Add(root);	
					treeNode.Text = treeNode.Text + ", " + root.Text;
				}
				else
				{
					treeNode.Nodes.Add(arrayEntry.ToString());
					if (!(treeNode.Text.Equals("")))
					{
						if (treeNode.Text[treeNode.Text.Length-1].Equals('['))
							treeNode.Text = treeNode.Text + arrayEntry.ToString();
						else
							treeNode.Text = treeNode.Text + ", " + arrayEntry.ToString();
					}
					else
						treeNode.Text = treeNode.Text + ", " + arrayEntry.ToString();
				}
			}
			return treeNode;
		}
		

		/// <summary>
		/// Sets a TreeNode with data from the provided ArrayList.
		/// </summary> 
		/// <param name="treeNode">Node.</param>
		/// <param name="objectArray">Object Array.</param>
		/// <returns>Returns the set node.</returns>
		
		public static System.Windows.Forms.TreeNode FillNode(System.Windows.Forms.TreeNode treeNode, Object[] objectArray)
		{
			foreach (Object arrayEntry in objectArray)
			{
				System.Windows.Forms.TreeNode root = new System.Windows.Forms.TreeNode();

				if (arrayEntry is ArrayList)
				{
					root.Text = "[";
					FillNode(root,(ArrayList)arrayEntry);									
					root.Text = root.Text + "]";
				}
				else if (arrayEntry is Hashtable)				
				{
					root.Text = "[";
					FillNode(root,(Hashtable)arrayEntry);
					root.Text = root.Text + "]";				
				}
				else
				{
					root.Nodes.Add(objectArray.ToString());
					root.Text = root.Text + ", " + objectArray.ToString();
				}
				treeNode.Nodes.Add(root);
				treeNode.Text = treeNode.Text + root.Text;
			}
			return treeNode;
		}
		
		/// <summary>		
		/// Sets a TreeNode with data from the provided Hashtable.		
		/// </summary> 		
		/// <param name="treeNode">Node.</param>		
		/// <param name="hashTable">Hash Table Object.</param>		
		/// <returns>Returns the set node.</returns>		
		public static System.Windows.Forms.TreeNode FillNode(System.Windows.Forms.TreeNode treeNode, Hashtable hashTable)
		{		
			foreach (DictionaryEntry myEntry in hashTable)
			{
				System.Windows.Forms.TreeNode root = new System.Windows.Forms.TreeNode();				

				if (myEntry.Value is ArrayList)
				{
					FillNode(root, (ArrayList)myEntry.Value);
					treeNode.Nodes.Add(root);
				}
				else if (myEntry.Value is Object[])
				{
					FillNode(root,(Object[])myEntry.Value);	
					treeNode.Nodes.Add(root);	
				}
				else
					treeNode.Nodes.Add(myEntry.Key.ToString());
			}
			return treeNode;
		}
	}
	/*******************************/
	/// <summary>
	/// The class performs token processing in strings
	/// </summary>
	public class Tokenizer: IEnumerator
	{
		/// Position over the string
		private long currentPos = 0;

		/// Include demiliters in the results.
		private bool includeDelims = false;

		/// Char representation of the String to tokenize.
		private char[] chars = null;
			
		//The tokenizer uses the default delimiter set: the space character, the tab character, the newline character, and the carriage-return character and the form-feed character
		private string delimiters = " \t\n\r\f";		

		/// <summary>
		/// Initializes a new class instance with a specified string to process
		/// </summary>
		/// <param name="source">String to tokenize</param>
		public Tokenizer(String source)
		{			
			this.chars = source.ToCharArray();
		}

		/// <summary>
		/// Initializes a new class instance with a specified string to process
		/// and the specified token delimiters to use
		/// </summary>
		/// <param name="source">String to tokenize</param>
		/// <param name="delimiters">String containing the delimiters</param>
		public Tokenizer(String source, String delimiters):this(source)
		{			
			this.delimiters = delimiters;
		}


		/// <summary>
		/// Initializes a new class instance with a specified string to process, the specified token 
		/// delimiters to use, and whether the delimiters must be included in the results.
		/// </summary>
		/// <param name="source">String to tokenize</param>
		/// <param name="delimiters">String containing the delimiters</param>
		/// <param name="includeDelims">Determines if delimiters are included in the results.</param>
		public Tokenizer(String source, String delimiters, bool includeDelims):this(source,delimiters)
		{
			this.includeDelims = includeDelims;
		}	


		/// <summary>
		/// Returns the next token from the token list
		/// </summary>
		/// <returns>The string value of the token</returns>
		public String NextToken()
		{				
			return NextToken(this.delimiters);
		}

		/// <summary>
		/// Returns the next token from the source string, using the provided
		/// token delimiters
		/// </summary>
		/// <param name="delimiters">String containing the delimiters to use</param>
		/// <returns>The string value of the token</returns>
		public String NextToken(String delimiters)
		{
			//According to documentation, the usage of the received delimiters should be temporary (only for this call).
			//However, it seems it is not true, so the following line is necessary.
			this.delimiters = delimiters;

			//at the end 
			if (this.currentPos == this.chars.Length)
				throw new System.ArgumentOutOfRangeException();
			//if over a delimiter and delimiters must be returned
			else if (   (System.Array.IndexOf(delimiters.ToCharArray(),chars[this.currentPos]) != -1)
				     && this.includeDelims )                	
				return "" + this.chars[this.currentPos++];
			//need to get the token wo delimiters.
			else
				return nextToken(delimiters.ToCharArray());
		}

		//Returns the nextToken wo delimiters
		private String nextToken(char[] delimiters)
		{
			string token="";
			long pos = this.currentPos;

			//skip possible delimiters
			while (System.Array.IndexOf(delimiters,this.chars[currentPos]) != -1)
				//The last one is a delimiter (i.e there is no more tokens)
				if (++this.currentPos == this.chars.Length)
				{
					this.currentPos = pos;
					throw new System.ArgumentOutOfRangeException();
				}
			
			//getting the token
			while (System.Array.IndexOf(delimiters,this.chars[this.currentPos]) == -1)
			{
				token+=this.chars[this.currentPos];
				//the last one is not a delimiter
				if (++this.currentPos == this.chars.Length)
					break;
			}
			return token;
		}

				
		/// <summary>
		/// Determines if there are more tokens to return from the source string
		/// </summary>
		/// <returns>True or false, depending if there are more tokens</returns>
		public bool HasMoreTokens()
		{
			//keeping the current pos
			long pos = this.currentPos;
			
			try
			{
				this.NextToken();
			}
			catch (System.ArgumentOutOfRangeException)
			{				
				return false;
			}
			finally
			{
				this.currentPos = pos;
			}
			return true;
		}

		/// <summary>
		/// Remaining tokens count
		/// </summary>
		public int Count
		{
			get
			{
				//keeping the current pos
				long pos = this.currentPos;
				int i = 0;
			
				try
				{
					while (true)
					{
						this.NextToken();
						i++;
					}
				}
				catch (System.ArgumentOutOfRangeException)
				{				
					this.currentPos = pos;
					return i;
				}
			}
		}

		/// <summary>
		///  Performs the same action as NextToken.
		/// </summary>
		public Object Current
		{
			get
			{
				return this.NextToken();
			}		
		}		
		
		/// <summary>
		//  Performs the same action as HasMoreTokens.
		/// </summary>
		/// <returns>True or false, depending if there are more tokens</returns>
		public bool MoveNext()
		{
			return this.HasMoreTokens();
		}
		
		/// <summary>
		/// Does nothing.
		/// </summary>
		public void Reset()
		{
			;
		}			
	}
	/*******************************/
	/// <summary>
	/// Converts the specified collection to its string representation.
	/// </summary>
	/// <param name="c">The collection to convert to string.</param>
	/// <returns>A string representation of the specified collection.</returns>
	public static String CollectionToString(ICollection c)
	{
		System.Text.StringBuilder s = new System.Text.StringBuilder();
		
		if (c != null)
		{
		
			ArrayList l = new ArrayList(c);

			bool isDictionary = (c is BitArray || c is Hashtable || c is IDictionary || c is NameValueCollection || (l.Count > 0 && l[0] is DictionaryEntry));
			for (int index = 0; index < l.Count; index++) 
			{
				if (l[index] == null)
					s.Append("null");
				else if (!isDictionary)
					s.Append(l[index]);
				else
				{
					isDictionary = true;
					if (c is NameValueCollection)
						s.Append(((NameValueCollection)c).GetKey (index));
					else
						s.Append(((DictionaryEntry) l[index]).Key);
					s.Append("=");
					if (c is NameValueCollection)
						s.Append(((NameValueCollection)c).GetValues(index)[0]);
					else
						s.Append(((DictionaryEntry) l[index]).Value);

				}
				if (index < l.Count - 1)
					s.Append(", ");
			}
			
			if(isDictionary)
			{
				if(c is ArrayList)
					isDictionary = false;
			}
			if (isDictionary)
			{
				s.Insert(0, "{");
				s.Append("}");
			}
			else 
			{
				s.Insert(0, "[");
				s.Append("]");
			}
		}
		else
			s.Insert(0, "null");
		return s.ToString();
	}

	/// <summary>
	/// Tests if the specified object is a collection and converts it to its string representation.
	/// </summary>
	/// <param name="obj">The object to convert to string</param>
	/// <returns>A string representation of the specified object.</returns>
	public static String CollectionToString(Object obj)
	{
		String result = "";

		if (obj != null)
		{
			if (obj is ICollection)
				result = CollectionToString((ICollection)obj);
			else
				result = obj.ToString();
		}
		else
			result = "null";

		return result;
	}
	/*******************************/
	/// <summary>
	/// Writes the exception stack trace to the received stream
	/// </summary>
	/// <param name="throwable">Exception to obtain information from</param>
	/// <param name="stream">Output sream used to write to</param>
	public static void WriteStackTrace(System.Exception throwable, System.IO.TextWriter stream)
	{
		stream.Write(throwable.StackTrace);
		stream.Flush();
	}

	/*******************************/
	/// <summary>
	/// Support class used to handle threads
	/// </summary>
	public class ThreadClass : IThreadRunnable
	{
		/// <summary>
		/// The instance of System.Threading.Thread
		/// </summary>
		private System.Threading.Thread threadField;
	      
		/// <summary>
		/// Initializes a new instance of the ThreadClass class
		/// </summary>
		public ThreadClass()
		{
			threadField = new System.Threading.Thread(new System.Threading.ThreadStart(Run));
		}
	 
		/// <summary>
		/// Initializes a new instance of the Thread class.
		/// </summary>
		/// <param name="Name">The name of the thread</param>
		public ThreadClass(String Name)
		{
			threadField = new System.Threading.Thread(new System.Threading.ThreadStart(Run));
			this.Name = Name;
		}
	      
		/// <summary>
		/// Initializes a new instance of the Thread class.
		/// </summary>
		/// <param name="Start">A ThreadStart delegate that references the methods to be invoked when this thread begins executing</param>
		public ThreadClass(System.Threading.ThreadStart Start)
		{
			threadField = new System.Threading.Thread(Start);
		}
	 
		/// <summary>
		/// Initializes a new instance of the Thread class.
		/// </summary>
		/// <param name="Start">A ThreadStart delegate that references the methods to be invoked when this thread begins executing</param>
		/// <param name="Name">The name of the thread</param>
		public ThreadClass(System.Threading.ThreadStart Start, String Name)
		{
			threadField = new System.Threading.Thread(Start);
			this.Name = Name;
		}
	      
		/// <summary>
		/// This method has no functionality unless the method is overridden
		/// </summary>
		public virtual void Run()
		{
		}
	      
		/// <summary>
		/// Causes the operating system to change the state of the current thread instance to ThreadState.Running
		/// </summary>
		public virtual void Start()
		{
			threadField.Start();
		}
	      
		/// <summary>
		/// Interrupts a thread that is in the WaitSleepJoin thread state
		/// </summary>
		public virtual void Interrupt()
		{
			threadField.Interrupt();
		}
	      
		/// <summary>
		/// Gets the current thread instance
		/// </summary>
		public System.Threading.Thread Instance
		{
			get
			{
				return threadField;
			}
			set
			{
				threadField = value;
			}
		}
	      
		/// <summary>
		/// Gets or sets the name of the thread
		/// </summary>
		public String Name
		{
			get
			{
				return threadField.Name;
			}
			set
			{
				if (threadField.Name == null)
					threadField.Name = value; 
			}
		}
	      
		/// <summary>
		/// Gets or sets a value indicating the scheduling priority of a thread
		/// </summary>
		public System.Threading.ThreadPriority Priority
		{
			get
			{
				return threadField.Priority;
			}
			set
			{
				threadField.Priority = value;
			}
		}
	      
		/// <summary>
		/// Gets a value indicating the execution status of the current thread
		/// </summary>
		public bool IsAlive
		{
			get
			{
				return threadField.IsAlive;
			}
		}
	      
		/// <summary>
		/// Gets or sets a value indicating whether or not a thread is a background thread.
		/// </summary>
		public bool IsBackground
		{
			get
			{
				return threadField.IsBackground;
			} 
			set
			{
				threadField.IsBackground = value;
			}
		}
	      
		/// <summary>
		/// Blocks the calling thread until a thread terminates
		/// </summary>
		public void Join()
		{
			threadField.Join();
		}
	      
		/// <summary>
		/// Blocks the calling thread until a thread terminates or the specified time elapses
		/// </summary>
		/// <param name="MiliSeconds">Time of wait in milliseconds</param>
		public void Join(long MiliSeconds)
		{
			lock(this)
			{
				threadField.Join(new System.TimeSpan(MiliSeconds * 10000));
			}
		}
	      
		/// <summary>
		/// Blocks the calling thread until a thread terminates or the specified time elapses
		/// </summary>
		/// <param name="MiliSeconds">Time of wait in milliseconds</param>
		/// <param name="NanoSeconds">Time of wait in nanoseconds</param>
		public void Join(long MiliSeconds, int NanoSeconds)
		{
			lock(this)
			{
				threadField.Join(new System.TimeSpan(MiliSeconds * 10000 + NanoSeconds * 100));
			}
		}
	      
		/// <summary>
		/// Resumes a thread that has been suspended
		/// </summary>
		public void Resume()
		{
			threadField.Resume();
		}
	      
		/// <summary>
		/// Raises a ThreadAbortException in the thread on which it is invoked, 
		/// to begin the process of terminating the thread. Calling this method 
		/// usually terminates the thread
		/// </summary>
		public void Abort()
		{
			threadField.Abort();
		}
	      
		/// <summary>
		/// Raises a ThreadAbortException in the thread on which it is invoked, 
		/// to begin the process of terminating the thread while also providing
		/// exception information about the thread termination. 
		/// Calling this method usually terminates the thread.
		/// </summary>
		/// <param name="stateInfo">An object that contains application-specific information, such as state, which can be used by the thread being aborted</param>
		public void Abort(Object stateInfo)
		{
			lock(this)
			{
				threadField.Abort(stateInfo);
			}
		}
	      
		/// <summary>
		/// Suspends the thread, if the thread is already suspended it has no effect
		/// </summary>
		public void Suspend()
		{
			threadField.Suspend();
		}
	      
		/// <summary>
		/// Obtain a String that represents the current Object
		/// </summary>
		/// <returns>A String that represents the current Object</returns>
		public override String ToString()
		{
			return "Thread[" + Name + "," + Priority.ToString() + "," + "" + "]";
		}
	     
		/// <summary>
		/// Gets the currently running thread
		/// </summary>
		/// <returns>The currently running thread</returns>
		public static ThreadClass Current()
		{
			ThreadClass CurrentThread = new ThreadClass();
			CurrentThread.Instance = System.Threading.Thread.CurrentThread;
			return CurrentThread;
		}
	}


}
