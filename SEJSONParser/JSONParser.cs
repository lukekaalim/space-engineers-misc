using System.Collections.Generic;
using System;

namespace IngameScript
{
	public static partial class JSON
	{
		enum JSONKeyword
		{
			Null,
			True,
			False
		}

		public enum ParserStatusState
		{
			Initializing,
			Working,
			Error,
			Done
		}
		public struct ParserStatus<T>
		{
			public bool HasWork()
			{
				return state == ParserStatusState.Working || state == ParserStatusState.Initializing;
			}

			public readonly ParserStatusState state;
			public readonly int charactersProcessed;
			public readonly string errorMessage;
			public readonly T result;

			public ParserStatus(int charactersProcessed)
			{
				this.state = ParserStatusState.Working;
				this.charactersProcessed = charactersProcessed;
				this.result = default(T);
				this.errorMessage = "";
			}

			public ParserStatus(int charactersProcessed, T result)
			{
				this.state = ParserStatusState.Done;
				this.charactersProcessed = charactersProcessed;
				this.result = result;
				this.errorMessage = "";
			}

			public ParserStatus(int charactersProcessed, string errorMessage)
			{
				this.state = ParserStatusState.Error;
				this.charactersProcessed = charactersProcessed;
				this.result = default(T);
				this.errorMessage = errorMessage;
			}
		}

		public static Element SyncParse(string source)
		{
			var enumerator = Parse(source).GetEnumerator();
			enumerator.MoveNext();

			while (enumerator.Current.HasWork())
				enumerator.MoveNext();

			if (enumerator.Current.state == ParserStatusState.Error)
				throw new Exception($"There was an Error with the Parser @{enumerator.Current.charactersProcessed} :\n" + enumerator.Current.errorMessage);

			return enumerator.Current.result.Value;
		}

		public static IEnumerable<ParserStatus<Element?>> Parse(string source)
		{
			return ParseElement(source, 0);
		}

		static IEnumerable<ParserStatus<Element?>> ParseElement(string source, int startingIndex = 0)
		{
			for (int i = startingIndex; i < source.Length; i++)
			{
				var currentCharacter = source[i];
				IEnumerator<ParserStatus<Element?>> enumerator = null;
				if (char.IsDigit(currentCharacter) || currentCharacter == '.' || currentCharacter == '-')
				{
					enumerator = ParseNumber(source, i).GetEnumerator();
				} else
				if (char.IsWhiteSpace(currentCharacter))
				{
					yield return new ParserStatus<Element?>(i);
					continue;
				} else
				if (currentCharacter == '[')
				{
					enumerator = ParseArray(source, i + 1).GetEnumerator();
				} else
				if (currentCharacter == '{')
				{
					enumerator = ParseObject(source, i + 1).GetEnumerator();
				} else
				if (currentCharacter == '"')
				{
					enumerator = ParseString(source, i + 1).GetEnumerator();
				} else
				if (currentCharacter == 'n' || currentCharacter == 't' || currentCharacter == 'f')
				{
					enumerator = ParseKeyword(source, i).GetEnumerator();
				}
				if (enumerator != null)
				{
					enumerator.MoveNext();
					while (enumerator.Current.HasWork())
					{
						yield return enumerator.Current;
						enumerator.MoveNext();
					}
					yield return enumerator.Current;
					yield break;
				}
			}
			yield return new ParserStatus<Element?>(source.Length - 1, "Unexpected end of JSON");
			yield break;
		}

		static IEnumerable<ParserStatus<Element?>> ParseObject(string source, int initalIndex)
		{
			var dict = new Dictionary<string, Element>();

			for (int i = initalIndex; i < source.Length; i++)
			{
				var currentChar = source[i];
				if (currentChar == '}')
				{
					// And it's the end of the object!
					yield return new ParserStatus<Element?>(i, Element.NewObject(dict));
					yield break;
				} else 
				if (char.IsWhiteSpace(currentChar))
				{
					yield return new ParserStatus<Element?>(i);
					continue;
				} else if (currentChar == ',')
				{
					yield return new ParserStatus<Element?>(i);
					continue;
				}
				// If it's not the end of the object, then there must be a property
				var result = ParseProperty(source, i).GetEnumerator();
				result.MoveNext();
				while (result.Current.HasWork())
				{
					yield return new ParserStatus<Element?>(result.Current.charactersProcessed);
					result.MoveNext();
				}
				if (result.Current.state == ParserStatusState.Error)
				{
					yield return new ParserStatus<Element?>(
						i,
						"There was an issue parsing this object:\n" + result.Current.errorMessage
					);
					yield break;
				}

				i = result.Current.charactersProcessed;
				var kvp = result.Current.result.Value;
				currentChar = source[i];

				dict.Add(kvp.Key, kvp.Value);
			}

			yield return new ParserStatus<Element?>(source.Length, "Unexpect end of object, expecting \"}\"");
			yield break;
		}

		static IEnumerable<ParserStatus<KeyValuePair<string, Element>?>> ParseProperty(string source, int initalIndex)
		{
			string key = "";
			Element value;

			int i = initalIndex;
			for (; i < source.Length; i++)
			{
				var currentChar = source[i];
				if (char.IsWhiteSpace(currentChar))
				{
					yield return new ParserStatus<KeyValuePair<string, Element>?>(i);
					continue;
				}
				// The key is starting!
				if (currentChar == '"')
				{
					var keyStatus = ParseString(source, i + 1).GetEnumerator();
					keyStatus.MoveNext();
					while (keyStatus.Current.HasWork())
					{
						yield return new ParserStatus<KeyValuePair<string, Element>?>(keyStatus.Current.charactersProcessed);
						keyStatus.MoveNext();
					}
					// make sure nothing is wrong with the key
					if (keyStatus.Current.state == ParserStatusState.Error)
					{
						yield return new ParserStatus<KeyValuePair<string, Element>?>(
							keyStatus.Current.charactersProcessed,
							"There was an issue parding the key of an object:\n" + keyStatus.Current.errorMessage
						);
						yield break;
					}

					// yeah, technically I'm allocating a JSON object where I don't need to be
					// fuck it
					key = keyStatus.Current.result.Value.jsonString;
					i = keyStatus.Current.charactersProcessed;
					break;
				}
				yield return new ParserStatus<KeyValuePair<string, Element>?>(
					i,
					$"Unexpected token ({currentChar}), was expecting either the start of a key or the closing brace of the object"
				);
				yield break;
			}
			i++;

			// check to see if the source code hasnt ended
			if (i == source.Length - 1)
			{
				yield return new ParserStatus<KeyValuePair<string, Element>?>(
					i,
					"Unexpected end of JSON after key, was expecting \":\""
				);
				yield break;
			}

			for (; i < source.Length; i++)
			{
				var currentChar = source[i];
				if (char.IsWhiteSpace(currentChar))
				{
					yield return new ParserStatus<KeyValuePair<string, Element>?>(i);
					continue;
				}
				if (currentChar == ':')
				{
					yield return new ParserStatus<KeyValuePair<string, Element>?>(i);
					// and let head on over to the next character;
					break;
				}
				yield return new ParserStatus<KeyValuePair<string, Element>?>(
					i,
					$"Unexpected token ({currentChar}), was expecting \":\""
				);
				yield break;
			}
			i++;

			if (i == source.Length - 1)
			{
				yield return new ParserStatus<KeyValuePair<string, Element>?>(
					i,
					"Unexpected end of JSON after key and \":\", was expecting a property value"
				);

				yield break;
			}

			// parse element
			var valueStatus = ParseElement(source, i).GetEnumerator();
			valueStatus.MoveNext();
			while (valueStatus.Current.HasWork())
			{
				yield return new ParserStatus<KeyValuePair<string, Element>?>(valueStatus.Current.charactersProcessed);
				valueStatus.MoveNext();
			}
			if (valueStatus.Current.state == ParserStatusState.Error)
			{
				yield return new ParserStatus<KeyValuePair<string, Element>?>(
					i,
					$"Error parsing property value of key \"{key}\":\n" + valueStatus.Current.errorMessage
				);
				yield break;
			}

			value = valueStatus.Current.result.Value;

			yield return new ParserStatus<KeyValuePair<string, Element>?>(
				valueStatus.Current.charactersProcessed,
				new KeyValuePair<string, Element>(key, value)
			);
			yield break;
		}

		static IEnumerable<ParserStatus<Element?>> ParseNumber(string source, int startingIndex)
		{
			Element element;
			for (int i = startingIndex; i < source.Length; i++)
			{
				var currentChar = source[i];
				var isPartOfNumber = char.IsDigit(currentChar) || currentChar == '.' || currentChar == '-';
				if (isPartOfNumber)
				{
					// This character is part of the number
					yield return new ParserStatus<Element?>(i);
					continue;
				} else
				{
					// This character is _not_ part of a number! The number is over
					element = Element.NewNumber(double.Parse(source.Substring(startingIndex, i - startingIndex)));
					// Though don't forget to set out index back by one; since this isn't a number, it needs to be re-parsed;
					yield return new ParserStatus<Element?>(i - 1, element);
					yield break;
				}
			}
			// We're at the end of the source, the current index MUST be the final character
			element = Element.NewNumber(double.Parse(source.Substring(startingIndex, source.Length - startingIndex)));
			yield return new ParserStatus<Element?>(source.Length - 1, element);
			yield break;
		}

		static IEnumerable<ParserStatus<Element?>> ParseKeyword(string source, int initalIndex)
		{
			var keyword = source.Substring(initalIndex, 4);
			int i = initalIndex + 3;

			switch (keyword)
			{
				case "true":
					yield return new ParserStatus<Element?>(i, Element.NewBoolean(true));
					yield break;
				case "fals":
					{
						if (source[i + 1] == 'e')
						{
							i++;
							yield return new ParserStatus<Element?>(i, Element.NewBoolean(false));
							yield break;
						}
						yield return new ParserStatus<Element?>(i, $"Unexpected keyword ({source.Substring(initalIndex, 5)}), was expect true, false, or null");
						yield break;
					}
				case "null":
					yield return new ParserStatus<Element?>(i, Element.NewNull());
					yield break;
			}
			yield return new ParserStatus<Element?>(i, $"Unexpected keyword ({source.Substring(initalIndex, 4)}), was expect true, false, or null");
			yield break;
		}

		static IEnumerable<ParserStatus<Element?>> ParseArray(string source, int initalIndex)
		{
			var list = new List<JSON.Element>();
			Element arrayElement;

			for (int i = initalIndex; i < source.Length; i++)
			{
				var currentChar = source[i];
				if (char.IsWhiteSpace(currentChar))
				{
					// skip white space
					yield return new ParserStatus<Element?>(i);
					continue;
				}
				if (currentChar == ']')
				{
					// and the array is over.
					arrayElement = Element.NewArray(list);
					yield return new ParserStatus<Element?>(i, arrayElement);
					yield break;
				}
				if (currentChar == ',')
				{
					// And skip over to the next element if it's a comma. We're a lax parser, so we don't need to be 100% sure
					yield return new ParserStatus<Element?>(i);
					continue;
				}
				// In all other cases, it looks like we need to parse an element since it's not an array control character
				var status = ParseElement(source, i).GetEnumerator();
				status.MoveNext();
				while (status.Current.HasWork())
				{
					yield return status.Current;
					status.MoveNext();
				}
				// if theres an issue with the child element
				if (status.Current.state == ParserStatusState.Error)
				{
					// elevate the problem
					yield return new ParserStatus<Element?>(
						i,
						$"There was an error parsing the {list.Count - 1}nth element of this array:\n" + status.Current.errorMessage
					);
					yield break;
				}
				list.Add(status.Current.result.Value);
				i = status.Current.charactersProcessed;
			}

			yield return new ParserStatus<Element?>(source.Length - 1, "Unexpected end of JSON, was expecting a ']' token to end the array.");
			yield break;
		}

		static IEnumerable<ParserStatus<Element?>> ParseString(string source, int initialIndex)
		{
			for (int i = initialIndex; i < source.Length; i++)
			{
				var currentChar = source[i];
				if (currentChar == '"')
				{
					var isEscaped = i != 0 && source[i - 1] == '\\';
					if (!isEscaped)
					{
						yield return new ParserStatus<Element?>(i, Element.NewString(source.Substring(initialIndex, i - initialIndex)));
						yield break;
					}
				}
				yield return new ParserStatus<Element?>(i);
			}
			yield return new ParserStatus<Element?>(source.Length - 1, "Unexpected end of JSON, was expecting a (\") token to close the string");
			yield break;
		}
	}
}
