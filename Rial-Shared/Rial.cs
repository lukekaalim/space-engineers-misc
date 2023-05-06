using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public static class Rial
        {
            public static ObjectElement Object() => new ObjectElement() { Value = new MyTuple<string, Element>[0] };
            public static ArrayElement Array() => new ArrayElement() { Value = new Element[0] };

            public abstract class Element
            {

                public static implicit operator Element(string value) => new StringElement { Value = value };
                public static implicit operator Element(int value) => new IntElement { Value = value };
                public static implicit operator Element(float value) => new FloatElement { Value = value };
                public static implicit operator Element(byte value) => new ByteElement { Value = value };
                public static implicit operator Element(Element[] value) => new ArrayElement { Value = value };
                public ArrayElement Array { get { return this as ArrayElement; } }
                public ObjectElement Object { get { return this as ObjectElement; } }
                public StringElement String { get { return this as StringElement; } }
                public IntElement Int { get { return this as IntElement; } }
                public FloatElement Float { get { return this as FloatElement; } }
                public ByteElement Byte { get { return this as ByteElement; } }
            }

            public abstract class Element<T> : Element
            {
                public T Value;
                public override string ToString() { return Value.ToString(); }

                public static implicit operator T(Element<T> element) => element.Value;
            };

            public class StringElement : Element<string>
            {
                public override string ToString() { return $"\"{Value}\""; }
            };
            public class IntElement : Element<int> { };
            public class FloatElement : Element<float> { };
            public class ByteElement : Element<byte> { };

            public class NullElement : Element { };
            public class ArrayElement : Element<Element[]>
            {
                public override string ToString() { return $"[{string.Join(",", Value.Select(v => v.ToString()))}]"; }
                public ArrayElement Entry(Element value)
                {
                    return new ArrayElement() { Value = Value.Concat(new[] { value }).ToArray() };
                }
            };
            public class ObjectElement : Element<MyTuple<string, Element>[]>
            {
                public override string ToString() { return $"{{ {string.Join(",", Value.Select(v => $"{v.Item1}: {v.Item2}"))} }}"; }
                public ObjectElement Prop(string name, Element value)
                {
                    var el = new MyTuple<string, Element>(name, value);
                    return new ObjectElement() { Value = Value.Concat(new[] { el }).ToArray() };
                }
                public Element Get(string name)
                {
                    var entry = Value.First(e => e.Item1 == name);
                    return entry.Item2;
                }
            };

            public class ByteArrayElement : Element<byte[]> { }


            public static class Binary
            {

                public enum Tag
                {
                    String = 0,
                    Int = 1,
                    Float = 2,
                    Byte = 3,
                    Null = 4,
                    Array = 5,
                    Object = 6,
                    ByteArray = 7,
                }

                public class Decoder
                {
                    public int Position = 0;
                    public byte[] Bytes;

                    public static implicit operator Decoder(byte[] bytes) => new Decoder { Bytes = bytes };
                    public static implicit operator Decoder(ImmutableArray<byte> bytes) => new Decoder { Bytes = (bytes as IEnumerable<byte>).ToArray() };

                    public byte[] ReadBytes(int count)
                    {
                        var slice = new byte[count];
                        for (int i = 0; i < count; i++)
                            slice[i] = Bytes[i + Position];
                        Position += count;
                        return slice;
                    }

                    public int ReadInt()
                    {
                        var value = BitConverter.ToInt32(Bytes, Position);
                        Position += 4;
                        return value;
                    }
                    public float ReadFloat()
                    {
                        var value = BitConverter.ToSingle(Bytes, Position);
                        Position += 4;
                        return value;
                    }
                }
                public class Encoder
                {
                    public static byte[] Tag(Tag tag)
                    {
                        return new byte[] { (byte)tag };
                    }
                    public static IEnumerable<byte> String(string value)
                    {
                        var length = utf8.GetByteCount(value);
                        var bytes = new byte[length];
                        utf8.GetBytes(value, 0, value.Length, bytes, 0);
                        return BitConverter.GetBytes(length)
                          .Concat(bytes);
                    }
                }
                readonly static UTF8Encoding utf8 = new UTF8Encoding();

                public static Element Read(Decoder head)
                {
                    var tag = (Tag)head.ReadBytes(1)[0];

                    switch (tag)
                    {
                        case Tag.Null:
                            return new NullElement();
                        case Tag.Byte:
                            return new ByteElement() { Value = head.ReadBytes(1)[0] };
                        case Tag.Int:
                            return new IntElement() { Value = head.ReadInt() };
                        case Tag.Float:
                            return new FloatElement() { Value = head.ReadFloat() };
                        case Tag.String:
                            {
                                var stringLength = head.ReadInt();
                                var stringContent = utf8.GetString(head.ReadBytes(stringLength));
                                return new StringElement() { Value = stringContent };
                            }
                        case Tag.Array:
                            {
                                var elementCount = head.ReadInt();
                                var elements = new Element[elementCount];
                                for (int i = 0; i < elementCount; i++)
                                {
                                    elements[i] = Read(head);
                                }
                                return new ArrayElement() { Value = elements };
                            }
                        case Tag.Object:
                            {
                                var propertyCount = head.ReadInt();
                                var properties = new MyTuple<string, Element>[propertyCount];
                                for (int i = 0; i < propertyCount; i++)
                                {
                                    var nameLength = head.ReadInt();
                                    var name = utf8.GetString(head.ReadBytes(nameLength));
                                    var value = Read(head);
                                    properties[i] = new MyTuple<string, Element>(name, value);
                                }
                                return new ObjectElement() { Value = properties };
                            }
                        case Tag.ByteArray:
                            {
                                var length = head.ReadInt();
                                return new ByteArrayElement() { Value = head.ReadBytes(length) };
                            }
                        default:
                            throw new Exception("Ooops");
                    }
                }

                public static IEnumerable<byte> Write(Element element)
                {
                    if (element is NullElement)
                        return Encoder.Tag(Tag.Null);

                    var stringElement = element as StringElement;
                    if (stringElement != null)
                        return Encoder.Tag(Tag.String)
                            .Concat(Encoder.String(stringElement.Value));

                    var floatElement = element as FloatElement;
                    if (floatElement != null)
                        return Encoder.Tag(Tag.Float)
                                        .Concat(BitConverter.GetBytes(floatElement.Value));

                    var intElement = element as IntElement;
                    if (intElement != null)
                        return Encoder.Tag(Tag.Int)
                            .Concat(BitConverter.GetBytes(intElement.Value));

                    var byteElement = element as ByteElement;
                    if (byteElement != null)
                        return Encoder.Tag(Tag.Byte)
                            .Concat(BitConverter.GetBytes(byteElement.Value));

                    var objectElement = element as ObjectElement;
                    if (objectElement != null)
                        return Encoder.Tag(Tag.Object)
                           .Concat(BitConverter.GetBytes(objectElement.Value.Length))
                           .Concat(objectElement.Value.SelectMany(property =>
                           {
                               return Encoder.String(property.Item1)
                    .Concat(Write(property.Item2));
                           }));

                    var arrayElement = element as ArrayElement;
                    if (arrayElement != null)
                        return Encoder.Tag(Tag.Array)
                             .Concat(BitConverter.GetBytes(arrayElement.Value.Length))
                             .Concat(arrayElement.Value.SelectMany(entry => Write(entry)));

                    var byteArrayElement = element as ByteArrayElement;
                    if (byteArrayElement != null)
                        return Encoder.Tag(Tag.ByteArray)
                             .Concat(BitConverter.GetBytes(byteArrayElement.Value.Length))
                             .Concat(byteArrayElement.Value);

                    throw new Exception("Ooops");
                }
            }
        }
    }
}
