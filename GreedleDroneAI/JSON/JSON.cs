using System.Collections.Generic;

namespace IngameScript
{
	public static partial class JSON
	{
		public enum ElementType
		{
			JSONObject,
			JSONArray,
			JSONString,
			JSONNumber,
			JSONBoolean,
			JSONNull,
		}

		public struct Element {
			public ElementType type;

			public Dictionary<string, Element> jsonObject;
			public List<Element> jsonArray;
			public string jsonString;
			public double jsonNumber;
			public bool jsonBoolean;

			public static Element NewObject(Dictionary<string, Element> jsonObject)
			{
				return new Element
				{
					type = ElementType.JSONObject,
					jsonObject = jsonObject
				};
			}

			public static Element NewArray(List<Element> jsonArray)
			{
				return new Element
				{
					type = ElementType.JSONArray,
					jsonArray = jsonArray
				};
			}

			public static Element NewString(string jsonString)
			{
				return new Element
				{
					type = ElementType.JSONString,
					jsonString = jsonString
				};
			}

			public static Element NewNumber(double jsonNumber)
			{
				return new Element
				{
					type = ElementType.JSONNumber,
					jsonNumber = jsonNumber
				};
			}

			public static Element NewBoolean(bool jsonBoolean)
			{
				return new Element
				{
					type = ElementType.JSONBoolean,
					jsonBoolean = jsonBoolean
				};
			}

			public static Element NewNull()
			{
				return new Element
				{
					type = ElementType.JSONNull,
				};
			}
		}
	}
}
