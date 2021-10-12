using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Window.Layout;
using Java.Interop;
using System.Linq;

namespace SplitLayoutDemo
{
    /**
     * An example of split-layout for two views, separated by a display feature that goes across the
     * window. When both start and end views are added, it checks if there are display features that
     * separate the area in two (e.g. fold or hinge) and places them side-by-side or top-bottom.
     */
    public class SplitLayout : FrameLayout
    {
        private WindowLayoutInfo windowLayoutInfo;
        private int startViewId = 0;
        private int endViewId = 0;

        private int lastWidthMeasureSpec = 0;
        private int lastHeightMeasureSpec = 0;

        public SplitLayout(Context context) : base(context) { }

        public SplitLayout(Context context, IAttributeSet attrs) : base(context, attrs) {
            SetAttributes(attrs);
        }

        public SplitLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            SetAttributes(attrs);
        }

        public void UpdateWindowLayout(WindowLayoutInfo windowLayoutInfo)
        {
            this.windowLayoutInfo = windowLayoutInfo;
            RequestLayout();
        }
        void SetAttributes(IAttributeSet attrs)
        {
            var ta = Context.Theme.ObtainStyledAttributes(attrs, Resource.Styleable.SplitLayout, 0, 0);
            try
            {
                startViewId = ta.GetResourceId(Resource.Styleable.SplitLayout_startViewId, 0);
                endViewId = ta.GetResourceId(Resource.Styleable.SplitLayout_endViewId, 0);
            }
            finally
            {
                ta.Recycle();
            }
            
        }

        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            var startView = FindStartView();
            var endView = FindEndView();
            var splitPositions = SplitViewPositions(startView, endView);

            if (startView != null && endView != null && splitPositions != null)
            {
                var startPosition = splitPositions[0];
                var startWidthSpec = MeasureSpec.MakeMeasureSpec(startPosition.Width(), MeasureSpecMode.Exactly);
                var startHeightSpec = MeasureSpec.MakeMeasureSpec(startPosition.Height(), MeasureSpecMode.Exactly);
                    startView.Measure(startWidthSpec, startHeightSpec);
                startView.Layout(
                    startPosition.Left, startPosition.Top, startPosition.Right,
                    startPosition.Bottom
                );

                var endPosition = splitPositions[1];
                var endWidthSpec = MeasureSpec.MakeMeasureSpec(endPosition.Width(), MeasureSpecMode.Exactly);
                var endHeightSpec = MeasureSpec.MakeMeasureSpec(endPosition.Height(), MeasureSpecMode.Exactly);
                endView.Measure(endWidthSpec, endHeightSpec);
                endView.Layout(
                    endPosition.Left, endPosition.Top, endPosition.Right,
                    endPosition.Bottom
                );
            }
            else
            {
                base.OnLayout(changed, left, top, right, bottom);
            }
        }

        View FindStartView()
        {
            var startView = FindViewById<View>(startViewId);
            if (startView == null && ChildCount > 0)
                {
                startView = GetChildAt(0);
            }
            return startView;
        }

        View FindEndView()
        {
            var endView = FindViewById<View>(endViewId);
            if (endView == null && ChildCount > 1)
            {
                endView = GetChildAt(1);
            }
            return endView;
        }

        Rect[] SplitViewPositions (View startView, View endView) {
            //TODO:
            if (windowLayoutInfo == null || startView == null || endView == null)
            {
                return null;
            }

            // Calculate the area for view's content with padding
            var paddedWidth = Width - PaddingLeft - PaddingRight;
            var paddedHeight = Height - PaddingTop - PaddingBottom;

            var df = windowLayoutInfo.DisplayFeatures.FirstOrDefault();
            if (IsValidFoldFeature(df))
            {
                var feature = df.JavaCast<IFoldingFeature>();
                var it = SampleTools.GetFeaturePositionInViewRect(df, this);

                if (feature.Bounds.Left == 0)
                { // Horizontal layout
                    var topRect = new Rect(
                            PaddingLeft, PaddingTop,
                            PaddingLeft + paddedWidth, it.Top
                    );
                    var bottomRect = new Rect(
                        PaddingLeft, it.Bottom,
                        PaddingLeft + paddedWidth, PaddingTop + paddedHeight
                    );

                    if (MeasureAndCheckMinSize(topRect, startView) &&
                        MeasureAndCheckMinSize(bottomRect, endView)
                    )
                    {
                        return new Rect[] { topRect, bottomRect };
                    }
                }
                else if (feature.Bounds.Top == 0)
                { // Vertical layout
                    var leftRect = new Rect(
                        PaddingLeft, PaddingTop,
                        it.Left, PaddingTop + paddedHeight
                    );
                    var rightRect = new Rect(
                        it.Right, PaddingTop,
                        PaddingLeft + paddedWidth, PaddingTop + paddedHeight
                    );

                    if (MeasureAndCheckMinSize(leftRect, startView) &&
                        MeasureAndCheckMinSize(rightRect, endView)
                    )
                    {
                        return new Rect[] { leftRect, rightRect };
                    }
                }
            }
            // We have tried to fit the children and measured them previously. Since they didn't fit,
            // we need to measure again to update the stored values.
            Measure(lastWidthMeasureSpec, lastHeightMeasureSpec);
            return null;
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            lastWidthMeasureSpec = widthMeasureSpec;
            lastHeightMeasureSpec = heightMeasureSpec;
        }

        bool MeasureAndCheckMinSize(Rect rect, View childView)
        {
            var widthSpec = MeasureSpec.MakeMeasureSpec(rect.Width(), MeasureSpecMode.AtMost);
            var heightSpec = MeasureSpec.MakeMeasureSpec(rect.Height(), MeasureSpecMode.AtMost);
            childView.Measure(widthSpec, heightSpec);
            return (childView.MeasuredWidthAndState & MeasuredStateTooSmall) == 0 &&
                (childView.MeasuredHeightAndState & MeasuredStateTooSmall) == 0;
        }

        bool IsValidFoldFeature(IDisplayFeature displayFeature)
        {
            var ff = displayFeature.JavaCast<IFoldingFeature>();
            if (ff != null)
            {
                if (ff.IsSeparating)
                {
                    return SampleTools.GetFeaturePositionInViewRect(ff, this) != null;
                }
                else {
                    return false; // if not separating, ignore it as a fold
                }
            }
            return false;
        }
    }
}