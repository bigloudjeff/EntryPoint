﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using EntryPoint;
using EntryPointTests.Helpers;

namespace EntryPointTests.AppOptionModels {
    public class OperandNonContiguousArgsModel : BaseApplicationOptions {
        [Operand(1)]
        public string Name { get; set; } = "NoName";

        // Gap here
        //[Operand(2)]
        //public string Gender { get; set; }

        [Operand(3)]
        public bool BoolValue { get; set; }

        [Operand(4)]
        public Enum1 Enum { get; set; }
    }
}