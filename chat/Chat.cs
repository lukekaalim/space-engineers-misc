using System;
using System.Linq;
using IngameScript;
using static IngameScript.Program;

public class Chat
{
	public void Init()
	{
		var msg = Rial.Binary.Write(
			Rial.Array()
				.Entry(Rial.Object()
					.Prop("Hello", "World"))
				.Entry("Luke")
				.Entry(10)
				.Entry(10f)
				.Entry(new Rial.Element[]
				{
					10f,
					"What",
					Rial.Object()
						.Prop("cowsay", Rial.Object()
							.Prop("prop", new Rial.Element[] { "AAA", "AAA" }))
				})
		).ToArray();

		var elements = Rial.Binary.Read(msg);
		var greeting = elements.Array
			.Value[0]
				.Object.Get("Hello")
					.String;
		var number = elements.Array
			.Value[3].Float;

		Console.WriteLine($"{elements}");
		Console.WriteLine($"{greeting}");
		Console.WriteLine($"{number}");
	}
}


public class Entry {
  static public void Main() {
    var chat = new Chat();
    chat.Init();

  }
}