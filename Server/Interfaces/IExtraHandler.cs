﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public interface IExtraHandler
    {
        int GetEffectId();
        Extra GetExtra();

    }
}
