using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FluentAssertions;
using Assets.Scripts.Utilities; 
public class UtilityImageUtilsTestScripts
{
    [Test]
    public void GetSquareFaceRegionRectInt_HappyPath()
    {
        // Arrange
        var texture = new Texture2D(1000, 1000);
        var faceRegion = new Rect(400, 400, 100, 100); // Already square, no cropping needed
        var expectedResult = new RectInt(400, 400, 100, 100); // Expected result should be the same as the input

        // Act
        RectInt result = ImageUtils.GetSquareBoundedRegion(texture, faceRegion);

      // Assert and show which values are different by name x, y, width, height
        result.x.Should().Be(expectedResult.x);
        result.y.Should().Be(expectedResult.y);
        result.width.Should().Be(expectedResult.width);
        result.height.Should().Be(expectedResult.height); 
    }

    [Test]
    public void GetSquareFaceRegionRectInt_OnlyWidthCroppingNeeded()
    {
        // Arrange
        var texture = new Texture2D(1000, 1000);
        var faceRegion = new Rect(400, 400, 100, 200); // Tall face region in center
        var expectedResult = new RectInt(350, 400, 200, 200);

        // Act
        RectInt result = ImageUtils.GetSquareBoundedRegion(texture, faceRegion);

        // Assert and show which values are different by name x, y, width, height
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void GetSquareFaceRegionRectInt_HappyPath_FaceRegionSameSizeAsTexture_AndSquare()
    {
        // Arrange
        var texture = new Texture2D(1000, 1000);
        var faceRegion = new Rect(0, 0, 1000, 1000); // Already square, no cropping needed
        var expectedResult = new RectInt(0, 0, 1000, 1000);

        // Act
        RectInt result = ImageUtils.GetSquareBoundedRegion(texture, faceRegion);

        // Assert and show which values are different by name x, y, width, height
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]  
    public void GetSquareFaceRegionRectInt_HappyPath_FaceRegionSameSizeAsTexture_AndNotSquare_WiderThanTaller()
    {
        // Arrange
        var texture = new Texture2D(1000, 800);
        var faceRegion = new Rect(0, 0, 1000, 800); // Already square, no cropping needed
        var expectedResult = new RectInt(100, 0, 800, 800);

        // Act
        RectInt result = ImageUtils.GetSquareBoundedRegion(texture, faceRegion);

        // Assert and show which values are different by name x, y, width, height
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]  
    public void GetSquareFaceRegionRectInt_HappyPath_FaceRegionSameSizeAsTexture_AndNotSquare_TallerThanWider()
    {
        // Arrange
        var texture = new Texture2D(800, 1000);
        var faceRegion = new Rect(0, 0, 800, 1000); // Already square, no cropping needed
        var expectedResult = new RectInt(0, 100, 800, 800);

        // Act
        RectInt result = ImageUtils.GetSquareBoundedRegion(texture, faceRegion);

        // Assert and show which values are different by name x, y, width, height
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void GetSquareFaceRegionRectInt_OnlyHeightCroppingNeeded()
    {
        // Arrange
        var texture = new Texture2D(1000, 1000);
        var faceRegion = new Rect(400, 400, 200, 100); // Wide face region in center  
        var expectedResult = new RectInt(400, 350, 200, 200);

        // Act
        RectInt result = ImageUtils.GetSquareBoundedRegion(texture, faceRegion);

        // Assert and show which values are different by name x, y, width, height 
        result.Should().BeEquivalentTo(expectedResult);
    }


    [Test]
    public void GetSquareFaceRegionRectInt_FullImageRegion_ReturnsFullSquare()
    {
        // Arrange
        var texture = new Texture2D(1000, 1000);
        var faceRegion = new Rect(0, 0, 1000, 1000); // Full image region
        var expectedResult = new RectInt(0 , 0, 1000, 1000);

        // Act
        RectInt result = ImageUtils.GetSquareBoundedRegion(texture, faceRegion);

      // Assert and show which values are different by name x, y, width, height
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void GetSquareFaceRegionRectInt_TooCloseToRightEdge  ()
    {
        // Arrange
        var texture = new Texture2D(1000, 1000);
        var faceRegion = new Rect(900, 500, 100, 300); // Face region too close to right edge
        var expectedResult = new RectInt(900, 600, 100, 100);

        // Act
        RectInt result = ImageUtils.GetSquareBoundedRegion(texture, faceRegion);

        // Assert and show which values are different by name x, y, width, height
        result.Should().BeEquivalentTo(expectedResult); 
    }

    [Test]
    public void GetSquareFaceRegionRectInt_TooCloseToRightEdge_MoreCentered()
    {
        // Arrange
        var texture = new Texture2D(1000, 1000);
        var faceRegion = new Rect(800, 500, 100, 400); // Face region too close to right edge
        var expectedResult = new RectInt(700, 550, 300, 300);

        // Act
        RectInt result = ImageUtils.GetSquareBoundedRegion(texture, faceRegion);

        // Assert and show which values are different by name x, y, width, height
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void GetSquareFaceRegionRectInt_TooCloseToLeftEdge()
    {
        // Arrange
        var texture = new Texture2D(1000, 1000);    
        var faceRegion = new Rect(0, 500, 100, 200); // Face region too close to left edge
        var expectedResult = new RectInt(0, 550, 100, 100);

        // Act
        RectInt result = ImageUtils.GetSquareBoundedRegion(texture, faceRegion);  

        // Assert and show which values are different by name x, y, width, height
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void GetSquareFaceRegionRectInt_TooCloseToTopEdge()
    {
        // Arrange
        var texture = new Texture2D(1000, 1000);    
        var faceRegion = new Rect(500, 900, 200, 100); // Face region too close to top edge
        var expectedResult = new RectInt(550, 900, 100, 100);

        // Act
        RectInt result = ImageUtils.GetSquareBoundedRegion(texture, faceRegion);

        // Assert and show which values are different by name x, y, width, height
        result.Should().BeEquivalentTo(expectedResult);
        }

    [Test]
    public void GetSquareFaceRegionRectInt_TooCloseToBottomEdge()
    {
        // Arrange
        var texture = new Texture2D(1000, 1000);    
        var faceRegion = new Rect(500, 0, 200, 100); // Face region too close to bottom edge
        var expectedResult = new RectInt(550, 0, 100, 100); 

        // Act
        RectInt result = ImageUtils.GetSquareBoundedRegion(texture, faceRegion);

        // Assert and show which values are different by name x, y, width, height
        result.Should().BeEquivalentTo(expectedResult);
    }

    [TearDown]
    public void Cleanup()
    {
      
 
    }
}
