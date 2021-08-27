using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Util;
using AndroidX.AppCompat.App;
using AndroidX.Core.Util;
using AndroidX.Window.Layout;
using AndroidX.Window.Java.Layout;
using Java.Lang;
using Java.Util.Concurrent;

/*
 This sample is a C# port of this Kotlin code (just the SplitLayout bits)
 https://github.com/android/user-interface-samples/tree/main/WindowManager/
 which is part of a Google sample that explains how to use Window Manager
 */
namespace SplitLayoutDemo
{
    /** Demo of [SplitLayout]. */
    [Activity(
        Label = "@string/app_name", 
        Theme = "@style/AppTheme", 
        MainLauncher = true, 
        Exported = true)]
    public class MainActivity : AppCompatActivity, IConsumer
    {
        const string TAG = "JWM"; // Jetpack Window Manager
        WindowInfoRepositoryCallbackAdapter wir;
        SplitLayout splitLayout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            splitLayout = FindViewById<SplitLayout>(Resource.Id.split_layout);

            wir = new WindowInfoRepositoryCallbackAdapter(WindowInfoRepository.Companion.GetOrCreate(this));
        }

        public override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            wir.AddWindowLayoutInfoListener(runOnUiThreadExecutor(), this); // `this` is the IConsumer implementation
        }

        public override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();
            wir.RemoveWindowLayoutInfoListener(this);
        }

        public void Accept(Java.Lang.Object newLayoutInfo)  // Object will be WindowLayoutInfo
        {
            Log.Info(TAG, "===LayoutStateChangeCallback.Accept");
            Log.Info(TAG, newLayoutInfo.ToString());

            splitLayout.UpdateWindowLayout(newLayoutInfo as WindowLayoutInfo);
        }

        IExecutor runOnUiThreadExecutor()
        {
            return new MyExecutor();
        }
        class MyExecutor : Java.Lang.Object, IExecutor
        {
            Handler handler = new Handler(Looper.MainLooper);
            public void Execute(IRunnable r)
            {
                handler.Post(r);
            }
        }




        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}