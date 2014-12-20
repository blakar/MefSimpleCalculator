namespace SimpleCalculator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.IO;

    public interface ICalculator
    {
        string Calculate(string input);
    }

    public interface IOperation
    {
        int Operate(int left, int right);
    }

    public interface IOperationData
    {
        char Symbol { get; }
    }

    [Export(typeof (IOperation))]
    [ExportMetadata("Symbol", '+')]
    internal class Add : IOperation
    {
        public int Operate(int left, int right)
        {
            return left + right;
        }
    }

    [Export(typeof (IOperation))]
    [ExportMetadata("Symbol", '-')]
    internal class Subtract : IOperation
    {
        public int Operate(int left, int right)
        {
            return left - right;
        }
    }

    [Export(typeof (ICalculator))]
    internal class MySimpleCalculator : ICalculator
    {
        [ImportMany] private IEnumerable<Lazy<IOperation, IOperationData>> operations;

        public string Calculate(string input)
        {
            int left;
            int right;
            char operation;
            int fn = FindFirstNonDigit(input); //finds the operator
            if (fn < 0) return "Could not parse command.";

            try
            {
                //separate out the operands
                left = int.Parse(input.Substring(0, fn));
                right = int.Parse(input.Substring(fn + 1));
            }
            catch
            {
                return "Could not parse command.";
            }

            operation = input[fn];

            foreach (Lazy<IOperation, IOperationData> i in operations)
            {
                if (i.Metadata.Symbol.Equals(operation)) return i.Value.Operate(left, right).ToString();
            }

            return "Operation Not Found!";
        }

        private int FindFirstNonDigit(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (!(char.IsDigit(s[i]))) return i;
            }
            return -1;
        }
    }

    internal class Program
    {
        private CompositionContainer _container;

        [Import(typeof (ICalculator))] public ICalculator calculator;

        private Program()
        {
            // Dynamically figure out where the extension folder is located relative to the 
            // application installation directory
            var baseDirectory = typeof (Program).Assembly.Location;
            var extensionDirectory = Path.Combine(Path.GetDirectoryName(baseDirectory), "Extensions");

            // Ensure that the extension directory always exists
            if (!Directory.Exists(extensionDirectory))
            {
                Directory.CreateDirectory(extensionDirectory);
            }

            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();
            //Adds all the parts found in the same assembly as the Program class
            catalog.Catalogs.Add(new AssemblyCatalog(typeof (Program).Assembly));
            catalog.Catalogs.Add(new DirectoryCatalog(extensionDirectory));

            //Create the CompositionContainer with the parts in the catalog
            _container = new CompositionContainer(catalog);

            //Fill the imports of this object
            try
            {
                this._container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                Console.WriteLine(compositionException.ToString());
            }
        }

        private static void Main(string[] args)
        {
            Program p = new Program(); //Composition is performed in the constructor
            string s;
            Console.WriteLine("Enter Command:");
            while (true)
            {
                s = Console.ReadLine();
                Console.WriteLine(p.calculator.Calculate(s));
            }
        }
    }
}