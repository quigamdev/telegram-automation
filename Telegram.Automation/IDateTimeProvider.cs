﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram.Automation;
public interface IDateTimeProvider
{
    DateTime Now { get; }
    DateTime UtcNow { get; }

}
