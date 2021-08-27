using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.Window;
using AndroidX.Window.Layout;

namespace SplitLayoutDemo
{
    public static class SampleTools
    {
        /**
         * Gets the bounds of the display feature translated to the View's coordinate space and current
         * position in the window. This will also include view padding in the calculations.
         */
        public static Rect GetFeaturePositionInViewRect(IDisplayFeature displayFeature, View view, bool includePadding = true)
        {
            // The the location of the view in window to be in the same coordinate space as the feature.
            var viewLocationInWindow = new int[2]; // IntArray(2);
            view.GetLocationInWindow(viewLocationInWindow);

            // Intersect the feature rectangle in window with view rectangle to clip the bounds.
            var viewRect = new Rect(
                viewLocationInWindow[0], viewLocationInWindow[1],
                viewLocationInWindow[0] + view.Width, viewLocationInWindow[1] + view.Height
            );

            // Include padding if needed
            if (includePadding)
            {
                viewRect.Left += view.PaddingLeft;
                viewRect.Top += view.PaddingTop;
                viewRect.Right -= view.PaddingRight;
                viewRect.Bottom -= view.PaddingBottom;
            }

            var featureRectInView = new Rect(displayFeature.Bounds);
            var intersects = featureRectInView.Intersect(viewRect);
            if ((featureRectInView.Width() == 0 && featureRectInView.Height() == 0) || !intersects)
            {
                return null;
            }

            // Offset the feature coordinates to view coordinate space start point
            featureRectInView.Offset(-viewLocationInWindow[0], -viewLocationInWindow[1]);

            return featureRectInView;
        }

        /**
         * Gets the layout params for placing a rectangle indicating a display feature inside a
         * [FrameLayout].
         */
        public static FrameLayout.LayoutParams GetLayoutParamsForFeatureInFrameLayout(IDisplayFeature displayFeature, FrameLayout view) {
            var featureRectInView = GetFeaturePositionInViewRect(displayFeature, view);
            if (featureRectInView is null) return null;

            var lp = new FrameLayout.LayoutParams(featureRectInView.Width(), featureRectInView.Height());
            lp.LeftMargin = featureRectInView.Left;
            lp.TopMargin = featureRectInView.Top;

            return lp;
        }
    }
}