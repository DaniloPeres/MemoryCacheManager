using MemoryCacheManager;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MemoryCacheManagerTests
{
    [TestFixture]
    public class MemoryCacheManagerTests
    {
        [Test]
        public void AddCacheTest()
        {
            // Arrange
            const string cacheKey = "MyCache";
            const int value = 123;

            // Act
            Cache.AddCache(cacheKey, value);

            // Assert
            Assert.IsTrue(Cache.TryGetCache(cacheKey, out int cachedValue));
            Assert.AreEqual(value, cachedValue);
        }

        [Test]
        public void UpdateCacheTest()
        {
            // Arrange
            const string cacheKey = "MyCache";
            var value = 123;

            // Act
            Cache.AddCache(cacheKey, value);
            Assert.IsTrue(Cache.TryGetCache(cacheKey, out int cachedValue));
            Assert.AreEqual(value, cachedValue);

            // Update
            value = 321;
            Cache.AddCache(cacheKey, value);
            Assert.IsTrue(Cache.TryGetCache(cacheKey, out cachedValue));
            Assert.AreEqual(value, cachedValue);
        }

        [Test]
        public void AddCacheWithParametersTest()
        {
            // Arrange
            const string cacheKey = "MyCache";
            const string parameter1 = "parameter";
            const int parameter2 = 999;
            var value = 123;

            // Act
            Cache.AddCache(cacheKey, value, parameter1, parameter2);

            // Assert
            Assert.IsTrue(Cache.TryGetCache(cacheKey, out int cachedValue, parameter1, parameter2));
            Assert.AreEqual(value, cachedValue);
        }

        [Test]
        public void UpdateCacheWithParametersTest()
        {
            // Arrange
            const string cacheKey = "MyCache";
            const string parameter1 = "parameter";
            const int parameter2 = 999;
            var value = 123;

            // Act
            Cache.AddCache(cacheKey, value, parameter1, parameter2);
            Assert.IsTrue(Cache.TryGetCache(cacheKey, out int cachedValue, parameter1, parameter2));
            Assert.AreEqual(value, cachedValue);

            // Update
            value = 321;
            Cache.AddCache(cacheKey, value, parameter1, parameter2);
            Assert.IsTrue(Cache.TryGetCache(cacheKey, out cachedValue, parameter1, parameter2));
            Assert.AreEqual(value, cachedValue);
        }

        [Test]
        public void AddMultipleCacheTest()
        {
            // Arrange
            var items = new List<(string key, object value, object[] parameters)>
            {
                {("key1", "value1", new object[]{ "param1", 123, false }) },
                {("key2", "valueFalse", new object[]{ false }) },
                {("key3", "valueTrue", new object[]{ true }) },
                {("key4", false, new object[]{ }) },
                {("key5", true, new object[]{ false, 321 }) }
            };

            // Act
            items.ForEach(item => Cache.AddCache(item.key, item.value, item.parameters));

            // Assert
            items.ForEach(item =>
            {
                Assert.IsTrue(Cache.TryGetCache<object>(item.key, out var cachedValue, item.parameters));
                Assert.AreEqual(item.value, cachedValue);
            });
        }

        [Test]
        public void UpdateMultipleCacheTest()
        {
            // Arrange
            var items = new Dictionary<string, (object value, object[] parameters)>
            {
                { "key1", ("value1", new object[]{ "param1", 123, false }) },
                { "key2", ("valueFalse", new object[]{ false }) },
                { "key3", ("valueTrue", new object[]{ true }) },
                { "key4", (false, new object[]{ }) },
                { "key5", (true, new object[]{ false, 321 }) },
                { "key6", (11, new object[]{ "my info" }) }
            };

            Action<string, object> updateItem = (key, value) =>
            {
                var itemTemp = items[key];
                itemTemp.value = value;
                items[key] = itemTemp;
            };

            // Act
            items.Keys.ToList().ForEach(key => Cache.AddCache(key, items[key].value, items[key].parameters));
            items.Keys.ToList().ForEach(key =>
            {
                Assert.IsTrue(Cache.TryGetCache<object>(key, out var cachedValue, items[key].parameters));
                Assert.AreEqual(items[key].value, cachedValue);
            });

            //update 
            updateItem("key1", "value2");
            updateItem("key2", "valueTrue");
            updateItem("key3", "valueFalse");
            updateItem("key4", true);
            updateItem("key5", false);
            updateItem("key6", 22);

            items.Keys.ToList().ForEach(key => Cache.AddCache(key, items[key].value, items[key].parameters));

            // Arrange
            items.Keys.ToList().ForEach(key =>
            {
                Assert.IsTrue(Cache.TryGetCache<object>(key, out var cachedValue, items[key].parameters));
                Assert.AreEqual(items[key].value, cachedValue);
            });
        }

        [Test]
        public async Task AddCacheExpiresTimeTest()
        {
            // Arrange
            const string cacheKey = "MyCache";
            const int value = 123;
            var expirationTime = new TimeSpan(0, 0, 1);

            // Act
            Cache.AddCacheExpirationTime(cacheKey, value, expirationTime);
            Assert.IsTrue(Cache.TryGetCache(cacheKey, out int cachedValue));
            Assert.AreEqual(value, cachedValue);

            await Task.Delay(2_000).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(Cache.TryGetCache<int>(cacheKey, out _));
        }

        [Test]
        public async Task UpdateCacheExpiresTimeTest()
        {
            // Arrange
            const string cacheKey = "MyCache";
            int value = 123;

            // Act
            Cache.AddCacheExpirationTime(cacheKey, value, new TimeSpan(0, 0, 1));
            Assert.IsTrue(Cache.TryGetCache(cacheKey, out int cachedValue));
            Assert.AreEqual(value, cachedValue);

            value = 321;
            Cache.AddCacheExpirationTime(cacheKey, value, new TimeSpan(0, 0, 3));


            await Task.Delay(2_000).ConfigureAwait(false);

            // Assert
            Assert.IsTrue(Cache.TryGetCache(cacheKey, out cachedValue));
            Assert.AreEqual(value, cachedValue);
        }

        [Test]
        public void RemoveCacheTest()
        {
            // Arrange
            const string cacheKey = "MyCache";
            const int value = 123;

            // Act
            Cache.AddCache(cacheKey, value);
            Assert.IsTrue(Cache.TryGetCache(cacheKey, out int cachedValue));
            Assert.AreEqual(value, cachedValue);
            Cache.RemoveCache(cacheKey);

            // Assert
            Assert.IsFalse(Cache.TryGetCache<int>(cacheKey, out _));
        }

        [Test]
        public void GetCacheOrRunAndCacheTest()
        {
            // Arrange
            const string cacheKey = "MyCacheOrRun";
            const int value = 123;
            int valueResult = default;
            const int runFunctionTimes = 5;
            bool processed = false;

            // Act
            for (var i = 0; i < runFunctionTimes; i++)
            {
                valueResult = Cache.GetCacheOrRunAndCache(cacheKey, () =>
                {
                    if (processed)
                        Assert.Fail("The runner has been processed 2 times");

                    processed = true;
                    return value;
                });
            }

            // Assert
            Assert.AreEqual(value, valueResult);
        }

        [Test]
        public async Task GetCacheOrRunAndCacheWithExpirationTimeExpiresTime()
        {
            // Arrange
            const string cacheKey = "MyCacheOrRunExpires";
            const int value = 123;
            bool processed = false;
            var expirationTime = new TimeSpan(0, 0, 1);

            // Act
            var valueResult = Cache.GetCacheOrRunAndCacheWithExpirationTime(cacheKey, expirationTime, () =>
            {
                if (processed)
                    Assert.Fail("The runner has been processed 2 times");

                processed = true;
                return value;
            });

            Assert.AreEqual(value, valueResult);

            await Task.Delay(2_000).ConfigureAwait(false);

            processed = false;
            Cache.GetCacheOrRunAndCacheWithExpirationTime(cacheKey, expirationTime, () =>
            {
                processed = true;
                return value;
            });

            // Assert
            Assert.IsTrue(processed);
        }

        [Test]
        public async Task GetCacheOrRunAndCacheAsyncTest()
        {
            // Arrange
            const string cacheKey = "MyCacheOrRunAsync";
            const int value = 123;
            int valueResult = default;
            const int runFunctionTimes = 5;
            bool processed = false;

            // Act
            for (var i = 0; i < runFunctionTimes; i++)
            {
                valueResult = await Cache.GetCacheOrRunAndCacheAsync(cacheKey, async () =>
                {
                    if (processed)
                        Assert.Fail("The runner has been processed 2 times");

                    // used to make this function async with some delay
                    await Task.Delay(10).ConfigureAwait(false);

                    processed = true;
                    return value;
                }).ConfigureAwait(false);
            }

            // Assert
            Assert.AreEqual(value, valueResult);
        }
    }
}
