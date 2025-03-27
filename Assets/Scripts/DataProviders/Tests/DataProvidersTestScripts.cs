using NUnit.Framework;
using UnityEngine;
using FluentAssertions;
using Assets.Scripts.DataProviders;

namespace Assets.Scripts.DataProviders.Tests
{
    public class DataProvidersTestScripts
    {
        private class TestDataProvider : DataProviderBase
        {
            public int PublicStringToNumber(string input, string description)
            {
                return StringToNumberProtected(input, description);
            }
        }

        private TestDataProvider _dataProvider;

        [SetUp]
        public void Setup()
        {
            _dataProvider = new TestDataProvider();
        }


        [Test]
        [TestCase("123", "Valid number", 123)]
        [TestCase("0", "Zero", 0)]
        [TestCase("-456", "Negative number", -456)]
        [TestCase("", "Empty string", 0)]
        [TestCase("abc", "Invalid string", 0)]
        [TestCase("12.34", "Decimal string", 0)]
        [TestCase(null, "Null string", 0)]
        public void StringToNumberProtected_HandlesVariousInputs(string input, string description, int expectedResult)
        {
            // Act
            var result = _dataProvider.PublicStringToNumber(input, description);

            // Assert
            result.Should().Be(expectedResult, $"because converting '{input}' with description '{description}' should result in {expectedResult}");
        }

        [TearDown]
        public void Cleanup()
        {
            _dataProvider = null;
        }
    }
}
