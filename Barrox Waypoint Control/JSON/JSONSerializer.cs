using System;
using System.Linq;

namespace IngameScript
{
	public static partial class JSON
	{
		public static string Stringify(Element element)
		{
			switch (element.type)
			{
				case ElementType.JSONNull:
					return "null";
				case ElementType.JSONBoolean:
					return element.jsonBoolean ? "true" : "false";
				case ElementType.JSONString:
					return string.Format("\"{0}\"", element.jsonString);
				case ElementType.JSONNumber:
					return element.jsonNumber.ToString();
				case ElementType.JSONArray:
					return string.Format("[{0}]", element.jsonArray
						.Select(arrayElement => Stringify(arrayElement))
						.Aggregate("", (arrayText,elementText) => arrayText == "" ? string.Format("{0}", elementText) : string.Format("{0},{1}", arrayText, elementText)));
				case ElementType.JSONObject:
					{
						var propertiesText = element.jsonObject
							.Select(kvp => string.Format("{0}:{1}", $"\"{kvp.Key}\"", Stringify(kvp.Value)))
							.Aggregate("", (objectText, propertyText) => objectText == "" ? string.Format("{0}", propertyText) : string.Format("{0},{1}", objectText, propertyText));
						return $"{{{propertiesText}}}";
					}
			}

			throw new Exception("Unknown JSON Element type");
		}
		public static string PrettyStringify(Element element, int intendationLevel = 0)
		{
			var intendation = intendationLevel == 0 ? "" : new String(' ', intendationLevel*2);
			switch (element.type)
			{
				case ElementType.JSONNull:
					return "null";
				case ElementType.JSONBoolean:
					return (element.jsonBoolean ? "true" : "false");
				case ElementType.JSONString:
					return string.Format("\"{0}\"", element.jsonString);
				case ElementType.JSONNumber:
					return element.jsonNumber.ToString();
				case ElementType.JSONArray:
					{
						if (element.jsonArray.Count == 0)
							return "[]";
						return string.Format("[\n{0}{1}\n{0}]", intendation, element.jsonArray
							.Select(arrayElement => PrettyStringify(arrayElement, intendationLevel + 1))
							.Aggregate("", (arrayText, elementText) => arrayText == "" ?
								string.Format("{0}", elementText) :
								string.Format("{1},\n{0}{2}", intendation, elementText, arrayText)
							));
					}
				case ElementType.JSONObject:
					{
						if (element.jsonObject.Keys.Count == 0)
							return "{}";
						var propertiesText = element.jsonObject
							.Select(kvp => string.Format("{0}: {1}", $"\"{kvp.Key}\"", PrettyStringify(kvp.Value, intendationLevel + 1)))
							.Aggregate("", (objectText, propertyText) => objectText == "" ?
								string.Format("{0}{1}", intendation, propertyText) :
								string.Format("{1},\n{0}{2}", intendation, objectText, propertyText)
							);
						return $"{{\n{propertiesText}\n{intendation}}}";
					}
			}

			throw new Exception("Unknown JSON Element type");
		}
	}
}
