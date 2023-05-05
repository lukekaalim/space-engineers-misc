using System;

public class Chat
{
  public void Init() {
    var msg = Rial.Binary.Write(
      new Rial.ArrayElement() { Value = new Rial.Element[] {
        new Rial.StringElement() { Value = "Hello" },
        new Rial.StringElement() { Value = "World" },
      } }
    );

    var elements = Rial.Binary.Read(new Rial.Binary.ReadingHead() { Bytes = msg, Position = 0 });

    Console.WriteLine($"{elements}");
  }
}

public class Program {
  static public void Main() {
    var chat = new Chat();
    chat.Init();

  }
}