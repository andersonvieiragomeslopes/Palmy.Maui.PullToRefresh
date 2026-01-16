using Android.Content;
using Android.Views;
using AndroidX.Core.View;
using AndroidX.Core.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Palmy.Maui.PullToRefresh.Enums;
using View = Android.Views.View;

namespace Palmy.Maui.PullToRefresh;

public partial class PullToRefreshView
{
	private ExtendedLinearLayoutManager? _extendedLinearLayoutManager;
	private View? _scrollableView;

	public void InitializeCollectionView(Microsoft.Maui.Controls.View view)
	{
		var platformView = view.Handler?.PlatformView as View;

		if (platformView == null)
			throw new NotSupportedException("Platform view is null");

		_scrollableView = platformView switch
		{
			RecyclerView recyclerView => InitializeRecyclerView(recyclerView),
			Android.Webkit.WebView webView => InitializeWebView(webView),
			NestedScrollView nestedScrollView => InitializeNestedScrollView(nestedScrollView),
			Android.Widget.ScrollView scrollView => InitializeScrollView(scrollView),
			SwipeRefreshLayout swipeRefreshLayout => InitializeSwipeRefreshLayout(swipeRefreshLayout),
			ViewGroup viewGroup => InitializeViewGroup(viewGroup),
			_ => throw new NotSupportedException($"View type {platformView.GetType().Name} is not supported")
		};
	}

	private RecyclerView InitializeRecyclerView(RecyclerView recyclerView)
	{
		_extendedLinearLayoutManager = new ExtendedLinearLayoutManager(recyclerView.Context);
		recyclerView.SetLayoutManager(_extendedLinearLayoutManager);
		recyclerView.Invalidate();

		var itemTouchListener = new OnItemTouchListener(this);
		recyclerView.AddOnItemTouchListener(itemTouchListener);

		return recyclerView;
	}

	private Android.Webkit.WebView InitializeWebView(Android.Webkit.WebView webView)
	{
		var touchListener = new GenericTouchListener(this);
		webView.SetOnTouchListener(touchListener);
		return webView;
	}

	private NestedScrollView InitializeNestedScrollView(NestedScrollView nestedScrollView)
	{
		var touchListener = new GenericTouchListener(this);
		nestedScrollView.SetOnTouchListener(touchListener);
		return nestedScrollView;
	}

	private Android.Widget.ScrollView InitializeScrollView(Android.Widget.ScrollView scrollView)
	{
		var touchListener = new GenericTouchListener(this);
		scrollView.SetOnTouchListener(touchListener);
		return scrollView;
	}

	private SwipeRefreshLayout InitializeSwipeRefreshLayout(SwipeRefreshLayout swipeRefreshLayout)
	{
		var touchListener = new GenericTouchListener(this);
		swipeRefreshLayout.SetOnTouchListener(touchListener);
		return swipeRefreshLayout;
	}

	private View? InitializeViewGroup(ViewGroup viewGroup)
	{
		var scrollableChild = FindScrollableChild(viewGroup);
		if (scrollableChild != null)
		{
			return scrollableChild switch
			{
				RecyclerView rv => InitializeRecyclerView(rv),
				Android.Webkit.WebView wv => InitializeWebView(wv),
				NestedScrollView nsv => InitializeNestedScrollView(nsv),
				Android.Widget.ScrollView sv => InitializeScrollView(sv),
				_ => InitializeGenericView(scrollableChild)
			};
		}

		return InitializeGenericView(viewGroup);
	}

	private View InitializeGenericView(View view)
	{
		var touchListener = new GenericTouchListener(this);
		view.SetOnTouchListener(touchListener);
		return view;
	}

	private View? FindScrollableChild(ViewGroup viewGroup)
	{
		for (var i = 0; i < viewGroup.ChildCount; i++)
		{
			var child = viewGroup.GetChildAt(i);
			if (child is RecyclerView or Android.Webkit.WebView or NestedScrollView or Android.Widget.ScrollView)
				return child;

			if (child is ViewGroup childGroup)
			{
				var found = FindScrollableChild(childGroup);
				if (found != null)
					return found;
			}
		}

		return null;
	}

	public void SetContentScrollEnable(bool enable)
	{
		if (_extendedLinearLayoutManager != null)
		{
			_extendedLinearLayoutManager.IsScrollVerticallyEnabled = enable;
			return;
		}

		switch (_scrollableView)
		{
			case NestedScrollView nestedScrollView:
				ViewCompat.SetNestedScrollingEnabled(nestedScrollView, enable);
				break;
			case Android.Widget.ScrollView scrollView:
				ViewCompat.SetNestedScrollingEnabled(scrollView, enable);
				break;
			case Android.Webkit.WebView webView:
				webView.VerticalScrollBarEnabled = enable;
				break;
		}
	}

	public double GetContentScrollOffset(Microsoft.Maui.Controls.View view)
	{
		var scrollView = view.Handler?.PlatformView;
		if (scrollView is not ViewGroup)
			return 0;

		return scrollView switch
		{
			SwipeRefreshLayout layout => layout.ScrollY,
			Android.Webkit.WebView webView => webView.ScrollY,
			RecyclerView recyclerView => recyclerView.ComputeVerticalScrollOffset(),
			View nativeView when view is ScrollView => nativeView.ScrollY,
			_ => 0
		};
	}
}

public class OnItemTouchListener(PullToRefreshView pullToRefreshView)
	: Java.Lang.Object, RecyclerView.IOnItemTouchListener
{
	readonly PullToRefreshView? _pullToRefreshView = pullToRefreshView;

	public bool OnInterceptTouchEvent(RecyclerView recyclerView, MotionEvent @event)
	{
		var x = ConvertToDp(@event.GetX());
		var y = ConvertToDp(@event.GetY());

		GestureStatus gestureStatus = @event.Action switch
		{
			MotionEventActions.Down => GestureStatus.Started,
			MotionEventActions.Move => GestureStatus.Running,
			MotionEventActions.Cancel => GestureStatus.Canceled,
			MotionEventActions.Up => GestureStatus.Completed,
			_ => GestureStatus.Canceled
		};

		_pullToRefreshView?.OnInterceptPanUpdated(
			new PanUpdatedEventArgs(gestureStatus, 1, x, y));

		if (_pullToRefreshView == null)
			return false;

		return _pullToRefreshView.State == PullToRefreshState.Refreshing;
	}

	public void OnRequestDisallowInterceptTouchEvent(bool disallow)
	{
	}

	public void OnTouchEvent(RecyclerView recyclerView, MotionEvent @event)
	{
	}

	double ConvertToDp(double value)
	{
		var density = DeviceDisplay.MainDisplayInfo.Density;
		return value / density;
	}
}

public class ExtendedLinearLayoutManager(Context? context) : LinearLayoutManager(context)
{
	public override void OnLayoutChildren(RecyclerView.Recycler? recycler, RecyclerView.State? state)
	{
		try
		{
			base.OnLayoutChildren(recycler, state);
		}
		catch (Java.Lang.IndexOutOfBoundsException)
		{
			// Fix rare crash when disabling scroll
		}
	}

	public bool IsScrollVerticallyEnabled { get; set; }= true;
	public override bool CanScrollVertically() => IsScrollVerticallyEnabled;
}

public class GenericTouchListener(PullToRefreshView pullToRefreshView)
	: Java.Lang.Object, View.IOnTouchListener
{
	readonly PullToRefreshView? _pullToRefreshView = pullToRefreshView;

	public bool OnTouch(View? v, MotionEvent? e)
	{
		if (e == null)
			return false;

		var x = ConvertToDp(e.GetX());
		var y = ConvertToDp(e.GetY());

		GestureStatus gestureStatus = e.Action switch
		{
			MotionEventActions.Down => GestureStatus.Started,
			MotionEventActions.Move => GestureStatus.Running,
			MotionEventActions.Cancel => GestureStatus.Canceled,
			MotionEventActions.Up => GestureStatus.Completed,
			_ => GestureStatus.Canceled
		};

		_pullToRefreshView?.OnInterceptPanUpdated(
			new PanUpdatedEventArgs(gestureStatus, 1, x, y));

		return true;
	}

	double ConvertToDp(double value)
	{
		var density = DeviceDisplay.MainDisplayInfo.Density;
		return value / density;
	}
}
