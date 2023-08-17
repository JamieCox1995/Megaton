using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TotalDistraction.ServiceLocation;

public interface ITestConfigurableService : IUnityService<TestConfiguration>
{
    int Value { get; }
}
