using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assets.Scripts.DataProviders;
using FluentAssertions;

public class DataProvidersTestScripts
{
    [Test]
    public void GetSquareFaceRegionRectInt_HappyPath()
    {
        // Arrange
        var provider = new PrimaryThumbnailForPersonFromDigiKam("dummy.db", "dummy_digikam.db");
        var texture = new Texture2D(1000, 1000);
        var faceRegion = new Rect(400, 400, 100, 100); // Already square, no cropping needed
        var expectedResult = new RectInt(400, 400, 100, 100); // Expected result should be the same as the input

        // Act
        RectInt result = provider.GetSquareFaceRegionRectInt(texture, faceRegion);

      // Assert and show which values are different by name x, y, width, height
        result.x.Should().Be(expectedResult.x);
        result.y.Should().Be(expectedResult.y);
        result.width.Should().Be(expectedResult.width);
        result.height.Should().Be(expectedResult.height); 
    }

    [Test]
    public void GetSquareFaceRegionRectInt_OnlyCroppingNeeded()
    {
        // Arrange
        var provider = new PrimaryThumbnailForPersonFromDigiKam("dummy.db", "dummy_digikam.db");
        var texture = new Texture2D(1000, 1000);
        var faceRegion = new Rect(400, 400, 100, 200); // Tall face region in center
        var expectedResult = new RectInt(400, 400, 200, 200);

        // Act
        RectInt result = provider.GetSquareFaceRegionRectInt(texture, faceRegion);

        // Assert and show which values are different by name x, y, width, height
        result.Should().BeEquivalentTo(expectedResult);
    }


    [Test]
    public void GetSquareFaceRegionRectInt_FullImageRegion_ReturnsFullSquare()
    {
        // Arrange
        var provider = new PrimaryThumbnailForPersonFromDigiKam("dummy.db", "dummy_digikam.db");
        var texture = new Texture2D(1000, 1000);
        var faceRegion = new Rect(0, 0, 1, 1); // Full image region
         var expectedResult = new RectInt(400, 400, 200, 200);

        // Act
        RectInt result = provider.GetSquareFaceRegionRectInt(texture, faceRegion);

      // Assert and show which values are different by name x, y, width, height
        result.x.Should().Be(expectedResult.x);
        result.y.Should().Be(expectedResult.y);
        result.width.Should().Be(expectedResult.width);
        result.height.Should().Be(expectedResult.height); 
    }

    [TearDown]
    public void Cleanup()
    {
      
 
    }
}
