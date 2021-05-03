using System;

namespace Identifier.Snowflake
{
    public class SnowflakeConfig
    {
        /// <summary>
        /// Gets number of bits to use for the timestamp part of the Id's to generate.
        /// </summary>
        public DateTime Epoch { get; }

        /// <summary>
        /// Gets number of bits to use for the timestamp part of the Id's to generate.
        /// </summary>
        public byte TimestampBits { get; }

        /// <summary>
        /// Gets number of bits to use for the generator-id part of the Id's to generate.
        /// </summary>
        public byte GeneratorIdBits { get; }

        /// <summary>
        /// Gets number of bits to use for the sequence part of the Id's to generate.
        /// </summary>
        public byte SequenceBits { get; }

        /// <summary>
        /// Gets the total number of bits for the <see cref="SnowflakeConfig"/>.
        /// </summary>
        public int TotalBits => TimestampBits + GeneratorIdBits + SequenceBits;

        /// <summary>
        /// Returns the maximum number of intervals for this mask configuration.
        /// </summary>
        public long MaxMilliseconds => (1L << TimestampBits);

        /// <summary>
        /// Returns the maximum number of generators available for this mask configuration.
        /// </summary>
        public long MaxGenerators => (1L << GeneratorIdBits);

        /// <summary>
        /// Returns the maximum number of sequential Id's for a time-interval (e.g. max. number of Id's generated 
        /// within a single interval).
        /// </summary>
        public long MaxSequenceIds => (1L << SequenceBits);

        /// <summary>
        /// Returns the default epoch.
        /// </summary>
        public static readonly DateTime DefaultEpoch = new DateTime(
            2015,
            1,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);

        /// <summary>
        /// Gets a default <see cref="SnowflakeConfig"/> with 41 bits for the timestamp part, 10 bits for the generator-id 
        /// part and 12 bits for the sequence part of the id.
        /// </summary>
        public static SnowflakeConfig Default => new SnowflakeConfig(41, 10, 12, DefaultEpoch);

        /// <summary>
        /// Initializes a bitmask configuration for <see cref="SnowflakeId"/>s.
        /// </summary>
        /// <param name="timestampBits">Number of bits to use for the timestamp-part of Id's.</param>
        /// <param name="generatorIdBits">Number of bits to use for the generator-id of Id's.</param>
        /// <param name="sequenceBits">Number of bits to use for the sequence-part of Id's.</param>
        /// <param name="epoch">The epoch start for the <see cref="SnowflakeId"/></param>
        public SnowflakeConfig(
            byte timestampBits,
            byte generatorIdBits,
            byte sequenceBits,
            DateTime epoch)
        {
            if (generatorIdBits > 31)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(generatorIdBits),
                    "GeneratorId cannot have more than 31 bits");
            }

            if (sequenceBits > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(sequenceBits), "Sequence cannot have more than 31 bits");
            }

            if (timestampBits + generatorIdBits + sequenceBits != 63)
            {
                throw new InvalidOperationException("Number of bits used to generate Id's is not equal to 63");
            }

            if (epoch > DateTime.UtcNow)
            {
                throw new ArgumentOutOfRangeException(nameof(epoch), "Epoch in future");
            }

            TimestampBits = timestampBits;
            GeneratorIdBits = generatorIdBits;
            SequenceBits = sequenceBits;
            Epoch = epoch;
        }

        /// <summary>
        /// Calculates the last date for an Id before a 'wrap around' will occur in the timestamp-part of an Id for the
        /// given <see cref="SnowflakeConfig"/>.
        /// </summary>
        /// <returns>The last date for an Id before a 'wrap around' will occur in the timestamp-part of an Id.</returns>
        public DateTime WraparoundDate()
        {
            return Epoch.AddMilliseconds(MaxMilliseconds);
        }

        /// <summary>
        /// Calculates the interval at which a 'wrap around' will occur in the timestamp-part of an Id for the given
        /// <see cref="SnowflakeConfig"/>.
        /// </summary>
        /// <returns>
        /// The interval at which a 'wrap around' will occur in the timestamp-part of an Id for the given
        /// <see cref="SnowflakeId"/>.
        /// </returns>
        public TimeSpan WraparoundInterval()
        {
            return TimeSpan.FromMilliseconds(MaxMilliseconds);
        }
    }
}
