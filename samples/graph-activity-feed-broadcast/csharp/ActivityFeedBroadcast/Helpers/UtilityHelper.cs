// <copyright file="UtilityHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All Rights Reserved.
// </copyright>

namespace TabActivityFeed.Helpers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class UtilityHelper
    {
        /// <summary>
        /// Process the data in parallel foreach loop.
        /// </summary>
        /// <typeparam name="T">Type of source data.</typeparam>
        /// <param name="source">Source data.</param>
        /// <param name="partitionCount">Partition count.</param>
        /// <param name="body">Function which process the data.</param>
        /// <returns></returns>
        public static Task ForEachAsync<T>(IEnumerable<T> source, int partitionCount, Func<T, Task> body)
        {
            return Task.WhenAll(
                from partition in Partitioner.Create(source).GetPartitions(partitionCount)
                select Task.Run(async delegate
                {
                    using (partition)
                        while (partition.MoveNext())
                            await body(partition.Current);
                }));
        }

        /// <summary>
        /// Splits a <see cref="List{T}"/> into multiple chunks.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list to be chunked.</param>
        /// <param name="chunkSize">The size of each chunk.</param>
        /// <returns>A list of chunks.</returns>
        public static List<List<T>> SplitIntoChunks<T>(List<T> list, int chunkSize)
        {
            if (chunkSize <= 0)
            {
                throw new ArgumentException("chunkSize must be greater than 0.");
            }

            List<List<T>> chunksList = new List<List<T>>();
            int index = 0;
            while (index < list.Count)
            {
                int count = list.Count - index > chunkSize ? chunkSize : list.Count - index;
                chunksList.Add(list.GetRange(index, count));

                index += chunkSize;
            }

            return chunksList;
        }
    }
}
