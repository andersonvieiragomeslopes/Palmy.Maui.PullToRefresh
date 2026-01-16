using Foundation;
using Palmy.Maui.PullToRefresh.Enums;
using UIKit;

namespace Palmy.Maui.PullToRefresh;

public partial class PullToRefreshView
{
	private UIScrollView? _scrollView;

	public void InitializeCollectionView(View view)
	{
		var platformView = view.Handler?.PlatformView as UIView;
		if (platformView == null)
			throw new NotSupportedException("Only UIView is supported");

		var touchInterceptor = new TouchInterceptorGestureRecognizer(this);
		platformView.AddGestureRecognizer(touchInterceptor);

		_scrollView = GetUIScrollView(view);
	}

	public double GetContentScrollOffset(View view)
	{
		return _scrollView?.ContentOffset.Y ?? 0;
	}

	public void SetContentScrollEnable(bool enable)
	{
		if (_scrollView != null)
		{
			_scrollView.ScrollEnabled = enable;
		}
	}

	private UIScrollView? GetUIScrollView(View view)
	{
		var scrollView = view.Handler?.PlatformView;
		if (scrollView is UIScrollView uIScroll)
		{
			return uIScroll;
		}

		if (scrollView is UIView scrollViewUIView && scrollViewUIView.ToString().Contains("UICollectionViewControllerWrapperView", StringComparison.Ordinal))
		{
			foreach (var subview in scrollViewUIView.Subviews.OfType<UIScrollView>())
			{
				return subview;
			}
		}

		return null;
	}
}

public sealed class TouchInterceptorGestureRecognizer : UIGestureRecognizer
{
	readonly PullToRefreshView? _pullToRefreshView;
	public TouchInterceptorGestureRecognizer(PullToRefreshView pullToRefreshView)
	{
		_pullToRefreshView = pullToRefreshView;
		CancelsTouchesInView = false;
		DelaysTouchesBegan = false;
		DelaysTouchesEnded = false;
	}

	void OnTouches(NSSet touches, GestureStatus gestureStatus)
	{
		float x = -1;
		float y = -1;
		if (touches.AnyObject is UITouch touch)
		{
			var location = touch.LocationInView(View);
			x = (float)location.X;
			y = (float)location.Y;
		}

		_pullToRefreshView?.OnInterceptPanUpdated(
			new PanUpdatedEventArgs(gestureStatus, 1, x, y));
	}

	public override void TouchesBegan(NSSet touches, UIEvent evt)
	{
		base.TouchesBegan(touches, evt);
		OnTouches(touches, GestureStatus.Started);
		State = UIGestureRecognizerState.Possible;
	}

	public override void TouchesMoved(NSSet touches, UIEvent evt)
	{
		base.TouchesMoved(touches, evt);
		OnTouches(touches, GestureStatus.Running);

		if (_pullToRefreshView == null)
			throw new NullReferenceException("PullToRefreshView can't be null.");

		if (_pullToRefreshView.State == PullToRefreshState.Finished ||
		    _pullToRefreshView.State == PullToRefreshState.Canceled)
		{
			State = UIGestureRecognizerState.Began;
		}
		else
		{
			State = UIGestureRecognizerState.Changed;
		}
	}

	public override void TouchesEnded(NSSet touches, UIEvent evt)
	{
		base.TouchesEnded(touches, evt);
		OnTouches(touches, GestureStatus.Completed);
		State = UIGestureRecognizerState.Ended;
	}

	public override void TouchesCancelled(NSSet touches, UIEvent evt)
	{
		base.TouchesCancelled(touches, evt);
		OnTouches(touches, GestureStatus.Canceled);
		State = UIGestureRecognizerState.Cancelled;
	}

	public override bool CanPreventGestureRecognizer(UIGestureRecognizer preventedGestureRecognizer)
	{
		return false; // Don't prevent other gestures
	}

	public override bool CanBePreventedByGestureRecognizer(UIGestureRecognizer preventingGestureRecognizer)
	{
		return false; // Don't let other gestures prevent this one
	}
}
