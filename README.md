# Identifier

A collection of Id algorithms for c#

# Snowflake

Snowflake is a versatile algorithm to create 64 bit id's composed of a time, a generator and a sequence part.
This implementation allows to configure the amount of bits which ca be used for the specific parts of the id.

```lang=c#
using Identifier.Snowflake;
using System;

class Program
{
    static void Main(string[] args)
    {
        // Let's say we take 1st january 2020 as our epoch
        DateTime epoch = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            
        // Create an ID with 42 bits for timestamp, 6 for generator-id and 15 for sequence
        SnowflakeConfig config = new SnowflakeConfig(42, 6, 15, epoch);
            
        // Create an IdGenerator with it's generator-id set to 0 and our custom snowflake config
        SnowflakeId generator = new SnowflakeId(0, config);

        // Let's ask the config how many milliseconds we could use 
        // in this setup (42 bits)
        Console.WriteLine("Max. milliseconds     : {0}", config.MaxMilliseconds);
        
        // Let's ask the config how many generators we could instantiate 
        // in this setup (6 bits)
        Console.WriteLine("Max. generators       : {0}", config.MaxGenerators);

        // Let's ask the config how many sequential Id's we could generate 
        // in a single ms in this setup (15 bits)
        Console.WriteLine("Id's/ms per generator : {0}", config.MaxSequenceIds);

        // Let's calculate the number of Id's we could generate, per ms, should we use
        // the maximum number of generators
        Console.WriteLine("Id's/ms total         : {0}", config.MaxGenerators * config.MaxSequenceIds);

        // Let's ask the config configuration for how long we could generate Id's before
        // we experience a 'wraparound' of the timestamp
        Console.WriteLine("Wraparound interval   : {0}", config.WraparoundInterval());

        // And finally: let's ask the config when this wraparound will happen
        // (we'll have to tell it the generator's epoch)
        Console.WriteLine("Wraparound date       : {0}", config.WraparoundDate());
    }
}
```

Output:
```
Max. milliseconds     : 4398046511104
Max. generators       : 64
Id's/ms per generator : 32768
Id's/ms total         : 2097152
Wraparound interval   : 50903.07:35:11.1040000
Wraparound date       : 15.05.2139 07:35:11
```