using System;
using System.Text;
using System.Linq;

using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
  partial class Program : MyGridProgram
  {
    public static class Rial
    {
      public abstract class Element {}

      public abstract class Element<T> : Element {
        public T Value;
        public override string ToString() { return Value.ToString(); }
      };

      public class StringElement  : Element<string> {
        public override string ToString() { return $"\"{Value}\""; }
      };
      public class IntElement     : Element<int> {};
      public class FloatElement   : Element<float> {};
      public class ByteElement    : Element<byte> {};

      public class NullElement    : Element {};
      public class ArrayElement   : Element<Element[]> {
        public override string ToString() { return $"[{string.Join(",", Value.Select(v => v.ToString()))}]"; }
      };
      public class ObjectElement  : Element<Tuple<string, Element>> {};


      public static class Binary {

        public enum Tag {
          String = 0,
          Int = 1,
          Float = 2,
          Byte = 3,
          Null = 4,
          Array = 5,
          Object = 6,
        }

        public class ReadingHead {
          public int Position = 0;
          public byte[] Bytes;

          public Tag ReadTag() {
            return (Tag)Bytes[Position++];
          }
          
          public byte[] ReadBytes(int count) {
            var slice = new byte[count];
            for (int i = 0; i < count; i++)
              slice[i] = Bytes[i + Position];
            Position += count;
            return slice;
          }

          public int ReadInt() {
            var value = BitConverter.ToInt32(Bytes, Position);
            Position += 4;
            return value;
          }
        }
        readonly static UTF8Encoding utf8 = new UTF8Encoding();

        public static Element Read(ReadingHead head) {
          var tag = head.ReadTag();

          switch (tag) {
            case Tag.Null:
              return new NullElement();
            case Tag.Byte:
              return new ByteElement() { Value = head.ReadBytes(1)[0] };
            case Tag.Array:
              var elementCount = head.ReadInt();
              var elements = new Element[elementCount];
              for (int i = 0; i < elementCount; i++) {
                elements[i] = Read(head);
              }
              return new ArrayElement() { Value = elements };
            case Tag.String:
              var stringLength = head.ReadInt();
              var stringContent = utf8.GetString(head.ReadBytes(stringLength));
              return new StringElement() { Value = stringContent };
            default:
              throw new NotImplementedException("Ooops");
          }
        }

        public static byte[] Write(Element element) {
          if (element is StringElement stringElement)
          {
            var length = utf8.GetByteCount(stringElement.Value);
            var bytes = new byte[length + 1 + 4];
            bytes[0] = (byte)Tag.String;
            var lengthBytes = BitConverter.GetBytes(length);
            Array.Copy(lengthBytes, 0, bytes, 1, 4);
            utf8.GetBytes(stringElement.Value, 0, stringElement.Value.Length, bytes, 5);
            return bytes;
          }
          if (element is ArrayElement arrayElement)
          {
            var lengthBytes = BitConverter.GetBytes(arrayElement.Value.Length);
            var elementBytes = arrayElement.Value
              .SelectMany(v => Write(v))
              .ToArray();

            var bytes = new byte[1 + 4 + elementBytes.Length];
            bytes[0] = (byte)Tag.Array;
            Array.Copy(lengthBytes, 0, bytes, 1, 4);
            Array.Copy(elementBytes, 0, bytes, 5, elementBytes.Length);
            return bytes;
          }
            throw new NotImplementedException("Ooops");
        }
      }
    }
  }
}