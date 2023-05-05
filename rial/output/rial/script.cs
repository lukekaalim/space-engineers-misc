static class ý{public abstract class ü{}public abstract class ü<û>:ü{public û ú;public override string ToString(){return
ú.ToString();}};public class ù:ü<string>{public override string ToString(){return$"\"{ú}\"";}};public class ø:ü<int>{};
public class ö:ü<float>{};public class õ:ü<byte>{};public class ô:ü{};public class ó:ü<ü[]>{public override string ToString(){
return$"[{string.Join(",",ú.Select(ò=>ò.ToString()))}]";}};public class ñ:ü<Tuple<string,ü>>{};public static class ð{public
enum ï{î=0,í=1,ì=2,ë=3,ê=4,é=5,þ=6,}public class è{public int ÿ=0;public byte[]Ĕ;public ï Ē(){return(ï)Ĕ[ÿ++];}public byte[]
đ(int Đ){var ď=new byte[Đ];for(int ć=0;ć<Đ;ć++)ď[ć]=Ĕ[ć+ÿ];ÿ+=Đ;return ď;}public int Ď(){var č=BitConverter.ToInt32(Ĕ,ÿ);
ÿ+=4;return č;}}static UTF8Encoding Č=new UTF8Encoding();public static ü ċ(è Ċ){var ē=Ċ.Ē();switch(ē){case ï.ê:return new
ô();case ï.ë:return new õ(){ú=Ċ.đ(1)[0]};case ï.é:var ĉ=Ċ.Ď();var Ĉ=new ü[ĉ];for(int ć=0;ć<ĉ;ć++){Ĉ[ć]=ċ(Ċ);}return new ó
(){ú=Ĉ};case ï.î:var Ć=Ċ.Ď();var ą=Č.GetString(Ċ.đ(Ć));return new ù(){ú=ą};default:throw new NotImplementedException(
"Ooops");}}public static byte[]Ą(ü ă){if(ă is ù stringElement){var Ă=Č.GetByteCount(stringElement.ú);var ç=new byte[Ă+1+4];ç[0]
=(byte)ï.î;var ā=BitConverter.GetBytes(Ă);Array.Copy(ā,0,ç,1,4);Č.GetBytes(stringElement.ú,0,stringElement.ú.Length,ç,5);
return ç;}if(ă is ó arrayElement){var ā=BitConverter.GetBytes(arrayElement.ú.Length);var Ā=arrayElement.ú.SelectMany(ò=>Ą(ò)).
ToArray();var ç=new byte[1+4+Ā.Length];ç[0]=(byte)ï.é;Array.Copy(ā,0,ç,1,4);Array.Copy(Ā,0,ç,5,Ā.Length);return ç;}throw new
NotImplementedException("Ooops");}}}
}static class Ú{public abstract class Í{}public abstract class Í<Ì>:Í{public Ì Ë;public override string ToString(){return
Ë.ToString();}};public class Ê:Í<string>{public override string ToString(){return$"\"{Ë}\"";}};public class É:Í<int>{};
public class È:Í<float>{};public class Ç:Í<byte>{};public class Æ:Í{};public class Å:Í<Í[]>{public override string ToString(){
return$"[{string.Join(",",Ë.Select(Ä=>Ä.ToString()))}]";}};public class Ã:Í<Tuple<string,Í>>{};public static class Â{public
enum Á{À=0,º=1,µ=2,ª=3,z=4,y=5,x=6,}public class Î{public int w=0;public byte[]Ï;public Á æ(){return(Á)Ï[w++];}public byte[]
ä(int ã){var â=new byte[ã];for(int Ù=0;Ù<ã;Ù++)â[Ù]=Ï[Ù+w];w+=ã;return â;}public int á(){var à=BitConverter.ToInt32(Ï,w);
w+=4;return à;}}static UTF8Encoding ß=new UTF8Encoding();public static Í Þ(Î Ý){var Ü=Ý.æ();switch(Ü){case Á.z:return new
Æ();case Á.ª:return new Ç(){Ë=Ý.ä(1)[0]};case Á.y:var å=Ý.á();var Û=new Í[å];for(int Ù=0;Ù<å;Ù++){Û[Ù]=Þ(Ý);}return new Å
(){Ë=Û};case Á.À:var Ø=Ý.á();var Ö=ß.GetString(Ý.ä(Ø));return new Ê(){Ë=Ö};default:throw new NotImplementedException(
"Ooops");}}public static byte[]Õ(Í Ô){if(Ô is Ê stringElement){var Ó=ß.GetByteCount(stringElement.Ë);var Ð=new byte[Ó+1+4];Ð[0]
=(byte)Á.À;var Ò=BitConverter.GetBytes(Ó);Array.Copy(Ò,0,Ð,1,4);ß.GetBytes(stringElement.Ë,0,stringElement.Ë.Length,Ð,5);
return Ð;}if(Ô is Å arrayElement){var Ò=BitConverter.GetBytes(arrayElement.Ë.Length);var Ñ=arrayElement.Ë.SelectMany(Ä=>Õ(Ä)).
ToArray();var Ð=new byte[1+4+Ñ.Length];Ð[0]=(byte)Á.y;Array.Copy(Ò,0,Ð,1,4);Array.Copy(Ñ,0,Ð,5,Ñ.Length);return Ð;}throw new
NotImplementedException("Ooops");}