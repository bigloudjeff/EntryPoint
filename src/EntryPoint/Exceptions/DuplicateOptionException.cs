﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EntryPoint.Exceptions {
    public class DuplicateOptionException : Exception {
        public DuplicateOptionException(string message) : base(message) { }
    }
}
