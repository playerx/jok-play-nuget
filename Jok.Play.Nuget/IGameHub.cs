﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jok.Play
{
    public interface IGameHub
    {
        int ConnectionsCount { get; }
        int TablesCount { get; }
    }
}
