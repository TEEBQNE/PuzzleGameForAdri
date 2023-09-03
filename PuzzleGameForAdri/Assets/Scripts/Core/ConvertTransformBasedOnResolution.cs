using UnityEngine;

public static class TransformExtensions
{
    // tweak this value
    private const float MAX_POSITION_DIFF = 10.0f;

    private const float MAX_SCALE_DIFF = 10.0f;

    // ToDo TJC: Need to do this for both position AND scale - - does this even do anything right now? Need to compare

    /*
        Need to have an offset to my camera and all objects to make the bottom left of my screen the (0,0) point

        After this, when converting the scale and position will be a percentage base of the goalResolution compared to the currentResolution
        
        When calculate / saving resolution however, the bounds need to be calculated in world space so we have a true measurement of the screen in game space
        
        Then our position is merely a percent of savedPosition.x / goalResolution.x where this is now a % of how far from our origin (0,0) in the X and Y the object will be
        
        Similarly for our scale, we need to standardize shapes (or not as it is relative to the screen?)
        
        If not standardizing a unit of a shape to 1:1 game space, where a 1x1 cube is taking up 1x1 of the screen, then we can do a relative screen change where the larger of the two changes takes priority to maintain aspect ratio where we take:
        
        currentScreenResolution / goalScreenResolution * objectScale.x/y (ignore z)
     */

    public static void UpdateScaleToFitResolution(this Transform transform, Camera cam, Vector2 goalResolution, Vector2 currentResolution)
    {
        //float goalXPos = transform.position.x * goalResolution.x;
        //float goalYpos = transform.position.y * goalResolution.y;
        //float goalPosRatio = goalXPos / goalYpos;

        //float goalXScale = transform.lossyScale.x * goalResolution.x;
        //float goalYScale = transform.lossyScale.y * goalResolution.y;
        //float goalScaleRatio = goalXScale / goalYScale;

        float currentXPos = transform.position.x * currentResolution.x;
        float currentYPos = transform.position.y * currentResolution.y;
        //float currentPosRatio = currentXPos / currentYPos;

        float currentXScale = transform.lossyScale.x * currentResolution.x;
        float currentYScale = transform.lossyScale.y * currentResolution.y;
        //float currentScaleRatio = currentXScale / currentYScale;

        // calculate the % difference - if the difference is too large, we need to calculate the position as a relative aspect ratio
        // to maintain relative positioning instead
        //float diffXPos = Mathf.Abs(goalXPos - currentXPos) / ((goalXPos + currentXPos) / 2) * 100;
        //float diffYPos = Mathf.Abs(goalYpos - currentYPos) / ((goalYpos + currentYPos) / 2) * 100;
        //float diffRatioPos = Mathf.Abs(goalPosRatio - currentPosRatio) / ((goalPosRatio + currentPosRatio) / 2) * 100;
        //
        //float diffXScale = Mathf.Abs(goalXScale - currentXScale) / ((goalXScale + currentYScale) / 2) * 100;
        //float diffYScale = Mathf.Abs(goalYScale - currentYScale) / ((currentYScale + currentXScale) / 2) * 100;
        //float diffRatioScale = Mathf.Abs(goalScaleRatio - currentScaleRatio) / ((goalScaleRatio + currentScaleRatio) / 2) * 100;

        // ToDo TJC: Need to make it so when we have a aspect ratio change, we need to make the current screen fit the goal aspect
        // by spawning in black bars on the top / bottom so we maintain the exact same aspect ratio
        // then, we can scale the position / scale by the factor of the aspect change i.e. 4:3 -> 8:6 for example
        // we shouldn't need to do any of the ratio stuff - just scale by % and 'screen size' but that 'screen size'
        // will be the modified screen size with the correct bounds
        // make it so that the (0,0) is the bottom of the window where these black bars start

        //if(diffRatioPos >= MAX_POSITION_DIFF)
        //{
        //    // need to adjust pos based on ratio
        //    if(diffXPos > diffYPos)
        //    {
        //        currentYPos = currentXPos * (goalYpos / goalXPos);
        //    }
        //    else
        //    {
        //        currentXPos = currentYPos * (goalXPos / goalYpos);
        //    }
        //}

        //if(diffRatioScale >= MAX_SCALE_DIFF)
        //{
        //    // need to adjust scale based on ratio
        //    if (diffXScale > diffYScale)
        //    {
        //        currentYScale = currentXScale * (goalYScale / goalXScale);
        //    }
        //    else
        //    {
        //        currentXScale = currentYScale * (goalXScale / goalYScale);
        //    }
        //}

        transform.position = new Vector3(currentXPos, currentYPos, transform.position.z);
        transform.localScale = new Vector3(currentXScale, currentYScale, 1f);
        
        // ratio between my Mathf.Abs((xPercent / yPercent) - (transform.Scale.x / transform.Scale.y)) >= EPSILON
        /*
            Finding the ratio between my original resolution scales i.e. 0.01x:0.2y and seeing how they are transcribed to
            my new space (In a resolution of 1000//500 my old values would then be 10x100)
                In this example, my ratio is 0.01/0.2 = 0.05 where my true values are now 10/100 = 0.1 if this change being a 50% increase is too much, can then
                instead use whichever of the two values has been altered further from the original resolution
                    i.e. if my original resolution was 1000//800 our ratio would be 0.01 * 1000 = 10 and 0.2*800 = 160
                        Can do a % diff from the OG -> new (https://www.calculatorsoup.com/calculators/algebra/percent-difference-calculator.php)
                        Whichever has a larger % change, in this case we have 0 (10) and ~46.15 (160), so we use the ratio on the larger % chance based on the ratio
                        Our original ratio is 0.01 // 0.2 so we take 0.01 / 0.2 = 0.05 * 160 = 8, meaning our adjusted size:
                            Going from resolution 1000x800 w/ size 10x160 we now go to 1000x500 w/ size 8x100

        Need to check this, bug general idea is, if we are changing the resolution by such a drastic amount, we want to rely on the ratio between our original percentages instead
        of using our percentages on the new resolution. That way we maintain the relationship or aspect of the image into our new resolution assuring the visual similarities between OG / NEW
         */

        // save out the relative percentage each position actually is to the world space
            // i.e.
                // xPercent = shape.x - cam.x / cam.width (% of the screen)
                    // transform.pos.x = cam.x + (cam.width * xPercent)

                // yPercent = shape.y - cma.y / cam.height
                    // transform.pos.y = cam.y + (cam.height * yPercent)

                // xPercentScale = shape.scale.x / cam.width
                    // transform.scale.x = cam.Width * xPercentScale

                // yPercentScale = shape.scale.y / cam.height
                    // transform.scale.y = cam.Height * yPercentScale
    }
}