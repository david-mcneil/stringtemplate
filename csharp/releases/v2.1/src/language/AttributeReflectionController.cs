using System;
using System.Collections;
using StringTemplate = antlr.stringtemplate.StringTemplate;
namespace antlr.stringtemplate.language
{
	
	/// <summary>This class knows how to recursively walk a StringTemplate and all
	/// of its attributes to dump a type tree out.  STs contain attributes
	/// which can contain multiple values.  Those values could be
	/// other STs or have types that are aggregates (have properties).  Those
	/// types could have properties etc...
	/// 
	/// I am not using ST itself to print out the text for $attributes$ because
	/// it kept getting into nasty self-recursive loops that made my head really
	/// hurt.  Pretty hard to get ST to print itselt out.  Oh well, it was
	/// a cool thought while I had it.  I just dump raw text to an output buffer
	/// now.  Easier to understand also.
	/// </summary>
	public class AttributeReflectionController
	{
		/// <summary>To avoid infinite loops with cyclic type refs, track the types </summary>
		protected internal IDictionary typesVisited;
		
		/// <summary>Build up a string buffer as you walk the nested data structures </summary>
		protected internal System.Text.StringBuilder output;
		
		/// <summary>Can't use ST to output so must do our own indentation </summary>
		protected internal int indentation = 0;
		
		/// <summary>If in a list, itemIndex=1..n; used to print ordered list in indent().
		/// Use a stack since we have nested lists.
		/// </summary>
		protected internal ArrayList itemIndexStack = new ArrayList();
		
		protected internal StringTemplate st;
		
		public AttributeReflectionController(StringTemplate st)
		{
			this.st = st;
		}
		
		public override String ToString()
		{
			typesVisited = new Hashtable();
			output = new System.Text.StringBuilder(1024);
			saveItemIndex();
			walkStringTemplate(st);
			restoreItemIndex();
			typesVisited = null;
			return output.ToString();
		}
		
		public virtual void walkStringTemplate(StringTemplate st)
		{
			// make sure the exact ST instance has not been visited
			if (typesVisited.Contains(st))
			{
				return ;
			}
			typesVisited.Add(st, null);
			indent();
			output.Append("Template ");
			output.Append(st.getName());
			output.Append(":");
			output.Append(Environment.NewLine);
			indentation++;
			walkAttributes(st);
			indentation--;
		}
		
		/// <summary>Walk all the attributes in this template, spitting them out. </summary>
		public virtual void walkAttributes(StringTemplate st)
		{
			if (st == null || st.getAttributes() == null)
			{
				return ;
			}

			ICollection keys = st.getAttributes().Keys;
			saveItemIndex();

			for (IEnumerator iterator = keys.GetEnumerator(); iterator.MoveNext(); )
			{
				String name = (String) iterator.Current;

				if (name.Equals(ASTExpr.REFLECTION_ATTRIBUTES))
				{
					continue;
				}

				Object value = st.getAttributes()[name];
				incItemIndex();
				indent();
				output.Append("Attribute ");
				output.Append(name);
				output.Append(" values:");
				output.Append(Environment.NewLine);
				indentation++;
				walkAttributeValues(value);
				indentation--;
			}
			restoreItemIndex();
		}
		
		public virtual void walkAttributeValues(Object attributeValue)
		{
			saveItemIndex();
			if (attributeValue is IList)
			{
				IList values = (IList) attributeValue;
				for (int i = 0; i < values.Count; i++)
				{
					Object value = values[i];
					System.Type type = value.GetType();
					incItemIndex();
					walkValue(value, type);
				}
			}
			else
			{
				System.Type type = attributeValue.GetType();
				incItemIndex();
				walkValue(attributeValue, type);
			}
			restoreItemIndex();
		}
		
		/// <summary>Get the list of properties by looking for get/isXXX methods.
		/// The value is the instance of "type" we are concerned with.
		/// Return null if no properties
		/// </summary>
		public virtual void walkPropertiesList(Object aggregateValue, System.Type type)
		{
			if (typesVisited.Contains(type))
			{
				return ;
			}
			typesVisited.Add(type, null);
			saveItemIndex();
			System.Reflection.MethodInfo[] methods = type.GetMethods(
				System.Reflection.BindingFlags.Instance 
				| System.Reflection.BindingFlags.Public 
				| System.Reflection.BindingFlags.DeclaredOnly);

			for (int i = 0; i < methods.Length; i++)
			{
				System.Reflection.MethodInfo m = methods[i];
				String methodName = m.Name;
				String propName = null;

				if (methodName.Length > 4 && methodName.StartsWith("get_")) 
				{
					propName = System.Char.ToLower(methodName[4]) + methodName.Substring(5, (methodName.Length) - (5));
				}
				else if (methodName.Length > 3 && methodName.StartsWith("get"))
				{
					propName = System.Char.ToLower(methodName[3]) + methodName.Substring(4, (methodName.Length) - (4));
				}
				else if (methodName.Length > 2 && methodName.StartsWith("is"))
				{
					propName = System.Char.ToLower(methodName[2]) + methodName.Substring(3, (methodName.Length) - (3));
				}
				else
				{
					continue;
				}
				incItemIndex();
				indent();
				output.Append("Property ");
				output.Append(propName);
				output.Append(" : ");
				output.Append(terseType(m.ReturnType.FullName));
				output.Append(Environment.NewLine);
				// get actual value for STs or for Maps; otherwise type is enough
				if (m.ReturnType == typeof(StringTemplate))
				{
					indentation++;
					walkStringTemplate((StringTemplate) getRawValue(aggregateValue, m));
					indentation--;
				}
				else if (ASTExpr.isValidReturnTypeMapInstance(m.ReturnType))
				{
					IDictionary rawMap = (IDictionary) getRawValue(aggregateValue, m);
					// if valid map instance, show key/values
					// otherwise don't descend
					if (rawMap != null && ASTExpr.isValidMapInstance(rawMap.GetType()))
					{
						indentation++;
						walkValue(rawMap, rawMap.GetType());
						indentation--;
					}
				}
				else if (!isAtomicType(m.ReturnType))
				{
					indentation++;
					walkValue(null, m.ReturnType);
					indentation--;
				}
			}
			restoreItemIndex();
		}
		
		public virtual void walkValue(Object value, System.Type type)
		{
			// ugh, must switch on type here; can't use polymorphism
			if (isAtomicType(type))
			{
				walkAtomicType(type);
			}
			else if (type == typeof(StringTemplate))
			{
				walkStringTemplate((StringTemplate) value);
			}
			else if (ASTExpr.isValidMapInstance(type))
			{
				walkMap((IDictionary) value);
			}
			else
			{
				walkPropertiesList(value, type);
			}
		}
		
		public virtual void walkAtomicType(System.Type type)
		{
			indent();
			output.Append(terseType(type.FullName));
			output.Append(Environment.NewLine);
		}
		
		/// <summary>Walk all the attributes in this template, spitting them out. </summary>
		public virtual void walkMap(IDictionary map)
		{
			if (map == null)
			{
				return ;
			}

			ICollection keys = map.Keys;
			saveItemIndex();

			for (IEnumerator iterator = keys.GetEnumerator(); iterator.MoveNext(); )
			{
				String name = (String) iterator.Current;
				if (name.Equals(ASTExpr.REFLECTION_ATTRIBUTES))
				{
					continue;
				}
				Object value = map[name];
				incItemIndex();
				indent();
				output.Append("Key ");
				output.Append(name);
				output.Append(" : ");
				output.Append(terseType(value.GetType().FullName));
				output.Append(Environment.NewLine);
				if (!isAtomicType(value.GetType()))
				{
					indentation++;
					walkValue(value, value.GetType());
					indentation--;
				}
			}
			restoreItemIndex();
		}
		
		/// <summary>
		/// For now, assume only System stuff is atomic as long as it's not a valid map.
		/// </summary>
		public virtual bool isAtomicType(System.Type type)
		{
			return type.FullName.StartsWith("System.") && type != typeof(Hashtable) && type != typeof(Hashtable);
		}
		
		public static String terseType(String typeName)
		{
			if (typeName == null)
			{
				return null;
			}
			if (typeName.IndexOf("[") >= 0)
			{
				// it's an array, return whole thing
				return typeName;
			}
			int lastDot = typeName.LastIndexOf('.');
			if (lastDot > 0)
			{
				typeName = typeName.Substring(lastDot + 1, (typeName.Length) - (lastDot + 1));
			}
			return typeName;
		}
		
		/// <summary>Normally we don't actually get the value of properties since we
		/// can use normal reflection to get the list of properties
		/// of the return type.  If they as for a value, though, go get it.
		/// Need it for string template return types of properties so we
		/// can get the attribute lists.
		/// </summary>
		protected internal virtual Object getRawValue(Object value, System.Reflection.MethodInfo method)
		{
			Object propertyValue = null;
			try
			{
				propertyValue = method.Invoke(value, (Object[]) null);
			}
			catch (System.Exception e)
			{
				st.error("Can't get property " + method.Name + " value", e);
			}
			return propertyValue;
		}
		
		protected internal virtual void indent()
		{
			for (int i = 1; i <= indentation; i++)
			{
				output.Append("    ");
			}
			int itemIndex = ((System.Int32) itemIndexStack[itemIndexStack.Count - 1]);
			if (itemIndex > 0)
			{
				output.Append(itemIndex);
				output.Append(". ");
			}
		}
		
		protected internal virtual void incItemIndex()
		{
			int itemIndex = ((System.Int32) SupportClass.StackSupport.Pop(itemIndexStack));
			itemIndexStack.Add((System.Int32) (itemIndex + 1));
		}
		
		protected internal virtual void saveItemIndex()
		{
			itemIndexStack.Add(0);
		}
		
		protected internal virtual void restoreItemIndex()
		{
			SupportClass.StackSupport.Pop(itemIndexStack);
		}
	}
}