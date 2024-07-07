using Generator;
using Generator.Attributes;

namespace TesterConsole
{
    [Autogen]
    public class MyClass
    {
    }

    [Autogen]
    public class Test { }
    public static class Program
    {
        public static void Main()
        {
            // Accessing the generated class to trigger the static constructor
           // MyClassAutoGenConsole.print();
            Console.WriteLine("Test");
        }
    }


}


