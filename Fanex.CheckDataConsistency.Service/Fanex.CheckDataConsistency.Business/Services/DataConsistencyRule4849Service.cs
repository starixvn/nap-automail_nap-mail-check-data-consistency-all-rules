using Fanex.CheckDataConsistency.Business.Model;
using System;
using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface DataConsistencyRule4849Service
{
    List<MessageRule4849Model> QueryData(DateTime currentTime);
}