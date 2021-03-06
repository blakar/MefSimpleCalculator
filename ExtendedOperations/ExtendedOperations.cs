﻿namespace ExtendedOperations
{
    ﻿using SimpleCalculator;
    using System.ComponentModel.Composition;
    
    [Export(typeof(IOperation))]
    [ExportMetadata("Symbol", '%')]
    public class Mod : IOperation
    {
        public int Operate(int left, int right)
        {
            return left % right;
        }
    }

}