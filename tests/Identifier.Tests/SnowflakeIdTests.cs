using Identifier.Snowflake;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Identifier.Tests
{
    public class SnowflakeIdTests
    {
        private readonly ITestOutputHelper _TestOutputHelper;
        private readonly int _GeneratorId = 5;
        private readonly SnowflakeId _Id;

        public SnowflakeIdTests(ITestOutputHelper testOutputHelper)
        {
            _TestOutputHelper = testOutputHelper;
            
            DateTime epoch = new DateTime(
                2000,
                1,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc);

            SnowflakeConfig config = new SnowflakeConfig(42, 6, 15, epoch);

            _Id = new SnowflakeId(_GeneratorId, config);
            
            // Let's ask the config how many milliseconds we could use 
            // in this setup (42 bits)
            _TestOutputHelper.WriteLine("Max. milliseconds     : {0}", config.MaxMilliseconds);
            
            // Let's ask the config how many generators we could instantiate 
            // in this setup (6 bits)
            _TestOutputHelper.WriteLine("Max. generators       : {0}", config.MaxGenerators);

            // Let's ask the config how many sequential Id's we could generate 
            // in a single ms in this setup (15 bits)
            _TestOutputHelper.WriteLine("Id's/ms per generator : {0}", config.MaxSequenceIds);

            // Let's calculate the number of Id's we could generate, per ms, should we use
            // the maximum number of generators
            _TestOutputHelper.WriteLine("Id's/ms total         : {0}", config.MaxGenerators * config.MaxSequenceIds);


            // Let's ask the config configuration for how long we could generate Id's before
            // we experience a 'wraparound' of the timestamp
            _TestOutputHelper.WriteLine("Wraparound interval   : {0}", config.WraparoundInterval());

            // And finally: let's ask the config when this wraparound will happen
            // (we'll have to tell it the generator's epoch)
            _TestOutputHelper.WriteLine("Wraparound date       : {0}", config.WraparoundDate());
        }

        [Fact]
        public void NewId()
        {
            long id = _Id.NewId();
            int generator = _Id.GetGenerator(id);
            
            Assert.Equal(_GeneratorId, generator);
        }
        
        [Fact]
        public void NewIdWithProvidedTime()
        {
            DateTime time = new DateTime(
                2000,
                1,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc); 
            long id = _Id.NewId(time);

            DateTime decodedTime = _Id.GetTimestamp(id);
            int generator = _Id.GetGenerator(id);
            
            Assert.Equal(time, decodedTime);
            Assert.Equal(_GeneratorId, generator);
        }
    }
}