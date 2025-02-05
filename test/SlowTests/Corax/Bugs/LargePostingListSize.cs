using System;
using FastTests.Voron;
using Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SlowTests.Corax.Bugs;

public class LargePostingListSize : StorageTest
{
    public LargePostingListSize(ITestOutputHelper output) : base(output)
    {
    }

    [RavenMultiplatformFact(RavenTestCategory.Corax, RavenArchitecture.AllX64)]
    public void WillProduceProperTree()
    {
        using (var tw = Env.WriteTransaction())
        {
            var pl = tw.OpenPostingList("Test");

            var r = new Random(3292);
            for (int i = 0; i < 15_000_000; i++)
            {
                pl.Add(r.NextInt64(int.MaxValue, (long)int.MaxValue * 50) << 2);
            }
            tw.Commit();
        }

        using (var tr = Env.WriteTransaction())
        {
            var pl = tr.OpenPostingList("Test");
            Assert.Equal(3, pl.State.Depth);
        }
    }

}
