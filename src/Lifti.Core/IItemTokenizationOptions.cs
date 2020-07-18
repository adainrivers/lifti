﻿using Lifti.ItemTokenization;
using System.Collections.Generic;

namespace Lifti
{
    internal interface IItemTokenizationOptions
    {
        IEnumerable<IFieldTokenization> GetConfiguredFields();
    }
}
