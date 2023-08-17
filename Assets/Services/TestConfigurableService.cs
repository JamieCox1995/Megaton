using System.Collections;
using System.Collections.Generic;

public class TestConfigurableService : ITestConfigurableService
{
    public bool IsConfigured { get; private set; }

    public int Value { get; private set; }

    public void Configure(TestConfiguration configuration)
    {
        if (this.IsConfigured) return;

        this.Value = configuration.Value;
        this.IsConfigured = true;
    }
}
