using System;

namespace Identifier.Snowflake
{
    /// <summary>
    /// Implementation of the twitter snowflake id algorithm
    /// </summary>
    public class SnowflakeId
    {
        private readonly object _GenLock = new object();
        private readonly SnowflakeConfig _Config;
        private readonly int _GeneratorId;

        private readonly long _MaskTime;
        private readonly long _MaskSequence;
        private readonly long _MaskGenerator;

        private readonly int _ShiftTime;
        private readonly int _ShiftGenerator;

        private int _StaticSequence;
        private int _TimeSequence;
        private long _TimeLastGen = -1;

        public SnowflakeId(int generatorId = 0, SnowflakeConfig? config = null)
        {
            _Config = config ?? SnowflakeConfig.Default;

            // calculate value masks
            _MaskTime = GetMask(_Config.TimestampBits);
            _MaskGenerator = GetMask(_Config.GeneratorIdBits);
            _MaskSequence = GetMask(_Config.SequenceBits);

            // calculate bit shift values
            _ShiftTime = _Config.GeneratorIdBits + _Config.SequenceBits;
            _ShiftGenerator = _Config.SequenceBits;

            if (generatorId < 0 || generatorId > _MaskGenerator)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(generatorId),
                    $"GeneratorId must be between 0 and {_MaskGenerator} (inclusive).");
            }

            _GeneratorId = generatorId;
        }

        /// <summary>
        /// Returns a bitmask masking out the desired number of bits; a bitmask of 2 returns 000...000011, a bitmask of
        /// 5 returns 000...011111.
        /// </summary>
        /// <param name="bits">The number of bits to mask.</param>
        /// <returns>Returns the desired bitmask.</returns>
        private static long GetMask(byte bits)
        {
            return (1L << bits) - 1;
        }

        /// <summary>
        /// Returns the amount of milliseconds elapsed since the configured epoch.
        /// </summary>
        /// <param name="timestamp">The timestamp to encode.</param>
        /// <returns>Milliseconds since epoch</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the provided timestamp is before the epoch</exception>
        private long GetTimestamp(DateTime? timestamp = null)
        {
            timestamp ??= DateTime.UtcNow;

            if (timestamp < _Config.Epoch)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(timestamp),
                    $"Timestamp cannot be used before {_Config.Epoch:o}");
            }

            return (long) Math.Floor((timestamp.Value - _Config.Epoch).TotalMilliseconds);
        }

        private long TillNextMillisecond(long lastTimestamp)
        {
            long timestamp = GetTimestamp() & _MaskTime;

            while (timestamp <= lastTimestamp)
            {
                timestamp = GetTimestamp() & _MaskTime;
            }

            return timestamp;
        }

        /// <summary>
        /// Provides the next sequence for regular generated id's
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        private int GetTimeBasedSequence(long timestamp)
        {
            if (timestamp == _TimeLastGen)
            {
                if (_TimeSequence < _MaskSequence)
                    _TimeSequence++;
                else
                {
                    timestamp = TillNextMillisecond(_TimeLastGen);
                    _TimeSequence = 0;
                    _TimeLastGen = timestamp;
                }
            }
            else
            {
                _TimeSequence = 0;
                _TimeLastGen = timestamp;
            }

            return _TimeSequence;
        }

        /// <summary>
        /// Provides the next sequence if a input time is provided otherwise a time based sequence will be used. 
        /// </summary>
        /// <returns>Next sequence</returns>
        private int GetStaticSequence()
        {
            if (_StaticSequence < _MaskSequence)
            {
                _StaticSequence++;
            }
            else
            {
                _StaticSequence = 0;
            }

            return _StaticSequence;
        }

        /// <summary>
        /// Decodes a value from a given id
        /// </summary>
        /// <param name="input">The input value to decode</param>
        /// <param name="mask">The mask for the value.</param>
        /// <param name="shift">The bits to shift</param>
        /// <returns>The decoded value</returns>
        private long DecodeValue(long input, long mask, int shift)
        {
            long value = (input >> shift);

            return value & mask;
        }

        /// <summary>
        /// Encode the values for a new id
        /// </summary>
        /// <param name="timestamp">The timestamp value</param>
        /// <param name="generatorId">The generator id</param>
        /// <param name="sequence">The sequence</param>
        /// <returns>The new id</returns>
        private long EncodeValues(long timestamp, long generatorId, long sequence)
        {
            return ((timestamp & _MaskTime) << _ShiftTime)
                   | ((generatorId & _MaskGenerator) << _ShiftGenerator)
                   | sequence & _MaskSequence;
        }

        /// <summary>
        /// Creates a new Id.
        /// </summary>
        /// <returns>Returns an Id based on the <see cref="SnowflakeId"/>'s epoch, generator id and sequence.</returns>
        public long GenerateId(DateTime? idTime = null)
        {
            lock (_GenLock)
            {
                long timestamp = GetTimestamp(idTime);

                int sequence = idTime == null
                    ? GetTimeBasedSequence(timestamp)
                    : GetStaticSequence();

                return EncodeValues(timestamp, _GeneratorId, sequence);
            }
        }

        /// <summary>
        /// Generates a new id for the current timestamp
        /// </summary>
        /// <returns>The new id</returns>
        public long NewId()
        {
            return GenerateId();
        }

        /// <summary>
        /// Generates a new id for a given timestamp
        /// </summary>
        /// <param name="idTime">The time to encode into the new id</param>
        /// <returns>The new id</returns>
        public long NewId(DateTime idTime)
        {
            return GenerateId(idTime);
        }

        /// <summary>
        /// Return the Timestamp for a given id
        /// </summary>
        /// <param name="id">The id to decode</param>
        /// <returns>The timestamp of the id</returns>
        public DateTime GetTimestamp(long id)
        {
            return _Config.Epoch.AddMilliseconds(DecodeValue(id, _MaskTime, _ShiftTime));
        }

        /// <summary>
        /// Return the generator of a given id
        /// </summary>
        /// <param name="id">The id to decode</param>
        /// <returns>The generator id</returns>
        public int GetGenerator(long id)
        {
            return (int) DecodeValue(id, _MaskGenerator, _ShiftGenerator);
        }

        /// <summary>
        /// Return the sequence of a given id
        /// </summary>
        /// <param name="id">The id to decode</param>
        /// <returns>The sequence</returns>
        public int GetSequence(long id)
        {
            return (int) DecodeValue(id, _MaskSequence, 0);
        }

        /// <summary>
        /// Return the last date for an Id before a 'wrap around' will occur in the timestamp-part of an Id for the
        /// given <see cref="SnowflakeConfig"/>.
        /// </summary>
        /// <returns>The maximum timestamp</returns>
        public DateTime GetWraparoundDate()
        {
            return _Config.WraparoundDate();
        }

        /// <summary>
        /// Returns the interval at which a 'wrap around' will occur in the timestamp-part of an Id for
        /// the given <see cref="SnowflakeConfig"/>
        /// </summary>
        /// <returns></returns>
        public TimeSpan GetWraparoundInterval()
        {
            return _Config.WraparoundInterval();
        }
    }
}
