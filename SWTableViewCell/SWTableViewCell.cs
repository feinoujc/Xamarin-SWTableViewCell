using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.Linq;

namespace SWTableViewCell
{
	public enum SWCellState
	{
		Center,
		Left,
		Right
	}

	public class ScrollingEventArgs:EventArgs
	{
		public SWCellState CellState{ get; set; }
		public NSIndexPath IndexPath { get; set; }
	}

	public class CellUtilityButtonClickedEventArgs:EventArgs
	{
		public int UtilityButtonIndex { get; set; }
		public NSIndexPath IndexPath { get; set; }
	}

	public partial class SWTableViewCell : UITableViewCell
	{
		public const float UtilityButtonsWidthMax = 260;
		public const float UtilityButtonWidthDefault = 90;
		public const float SectionIndexWidth = 15;


		UITableView containingTableView;

		UIButton[] rightUtilityButtons;

		UIView scrollViewLeft;


		SWCellState cellState; // The state of the cell within the scroll view, can be left, right or middle
		float additionalRightPadding;
	

		// Scroll view to be added to UITableViewCell
		 UIScrollView cellScrollView;

		// The cell's height
		float height;

		// Views that live in the scroll view
		UIView scrollViewContentView;
		SWUtilityButtonView scrollViewButtonViewRight;
		UIScrollViewDelegate scrollViewDelegate;

		float ScrollLeftViewWidth {
			get{return this.scrollViewLeft.Frame.Width;}
		}

		float RightUtilityButtonsWidth {
			get{return this.scrollViewButtonViewRight.UtilityButtonsWidth + additionalRightPadding;}
		}

		float UtilityButtonsPadding {
			get{return ScrollLeftViewWidth + RightUtilityButtonsWidth;}
		}

		PointF ScrollViewContentOffset {
			get{return new PointF(ScrollLeftViewWidth, 0);}
		}

		public SWTableViewCell (UITableViewCellStyle style, string reuseIdentifier, 
		                        UITableView containingTable, IEnumerable<UIButton> rightUtilityButtons, 
		                        UIView leftView):base(style, reuseIdentifier)
		{
			this.scrollViewLeft = leftView;
			this.rightUtilityButtons = rightUtilityButtons.ToArray();
			this.scrollViewButtonViewRight = new SWUtilityButtonView (this.rightUtilityButtons, this);

			this.containingTableView = containingTable;
			this.height = containingTableView.RowHeight;
			this.scrollViewDelegate = new SWScrollViewDelegate (this);
	

			// Check if the UITableView will display Indices on the right. If that's the case, add a padding
//		var indices = containingTableView.Source.SectionIndexTitles (containingTableView);
//			additionalRightPadding = indices == null || indices.Length == 0 ? 0 : kSectionIndexWidth;


			// Set up scroll view that will host our cell content
			this.cellScrollView = new UIScrollView (new RectangleF (0, 0, Bounds.Width, height)); //TODO:frames
			this.cellScrollView.ContentSize = new SizeF (Bounds.Width + this.UtilityButtonsPadding, height);//TODO:frames
			this.cellScrollView.ContentOffset = ScrollViewContentOffset;
			this.cellScrollView.Delegate = this.scrollViewDelegate;
			this.cellScrollView.ShowsHorizontalScrollIndicator = false;
			this.cellScrollView.ScrollsToTop = false;
			UITapGestureRecognizer tapGestureRecognizer = new UITapGestureRecognizer(OnScrollViewPressed);
			this.cellScrollView.AddGestureRecognizer (tapGestureRecognizer);
		
			// Set up the views that will hold the utility buttons
			this.scrollViewLeft.Frame = new RectangleF (ScrollLeftViewWidth, 0, ScrollLeftViewWidth, height);//TODO:frame
			this.cellScrollView.AddSubview (scrollViewLeft);

			this.scrollViewButtonViewRight.Frame = new RectangleF (Bounds.Width, 0, RightUtilityButtonsWidth, height); //TODO:frame
			this.cellScrollView.AddSubview (scrollViewButtonViewRight);



			// Populate the button views with utility buttons
			this.scrollViewButtonViewRight.PopulateUtilityButtons ();
			// Create the content view that will live in our scroll view
			this.scrollViewContentView = new UIView(new RectangleF(ScrollLeftViewWidth, 0, Bounds.Width, height));
			this.scrollViewContentView.BackgroundColor = UIColor.White;
			this.cellScrollView.AddSubview (this.scrollViewContentView);

			
			// Add the cell scroll view to the cell 
			//TODO: not sure what's going on here, why does this work?
			var contentViewParent = Subviews[0];
			foreach (var subView in contentViewParent.Subviews) {
				this.scrollViewContentView.AddSubview (subView);
			}
			AddSubview (this.cellScrollView);
		
			HideSwipedContent (false);
		}

		void OnScrollViewPressed(UITapGestureRecognizer tap)
		{
			if (cellState == SWCellState.Center) {
				if (containingTableView.Source != null) {
					var indexPath = this.containingTableView.IndexPathForCell (this);
					this.containingTableView.Source.RowSelected (containingTableView, indexPath);
				}
				//Highlight hack
				//TODO: I don't understand this
				if (!this.Highlighted) {
					this.scrollViewLeft.Hidden = true;
					this.scrollViewButtonViewRight.Hidden = true;
					NSTimer endHighLightTimer = NSTimer.CreateScheduledTimer (TimeSpan.FromMilliseconds (15), () => {
						if (this.Highlighted) {
							this.scrollViewLeft.Hidden = false;
							this.scrollViewButtonViewRight.Hidden = false;
							Highlighted = false;
						}
					});
					NSRunLoop.Current.AddTimer (endHighLightTimer, NSRunLoopMode.Common);
				}
			} else {
				// Scroll back to center
				this.HideSwipedContent (true);
			}
		}

		public void HideSwipedContent(bool animated)
		{
			cellScrollView.SetContentOffset (new PointF (ScrollLeftViewWidth, 0), animated);
			cellState = SWCellState.Center;
			OnScrolling ();
		}

		public override UIColor BackgroundColor {
			get {
				return base.BackgroundColor;
			}
			set {
				base.BackgroundColor = value;
				this.scrollViewContentView.BackgroundColor = value;
			}
		} 

		protected internal void OnLeftUtilityButtonPressed(UIButton sender)
		{
			int tag = sender.Tag;
			var handler = this.UtilityButtonPressed;
			if (handler != null) {
				var indexPath = this.containingTableView.IndexPathForCell (this);
				handler(sender, new CellUtilityButtonClickedEventArgs{IndexPath = indexPath, UtilityButtonIndex = tag});
			}
		}

		void OnScrolling()
		{
			var handler = Scrolling;
			if (handler != null) {
				var indexPath = this.containingTableView.IndexPathForCell (this);
				handler (this, new ScrollingEventArgs { CellState = cellState, IndexPath = indexPath });
			}
		}

		public event EventHandler<ScrollingEventArgs> Scrolling;
		public event EventHandler<CellUtilityButtonClickedEventArgs> UtilityButtonPressed;

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			this.cellScrollView.Frame = new RectangleF (0, 0, Bounds.Width, height);
			this.cellScrollView.ContentSize = new SizeF (Bounds.Width + UtilityButtonsPadding, height);
			this.cellScrollView.ContentOffset = new PointF (ScrollLeftViewWidth, 0);
			this.scrollViewLeft.Frame = new RectangleF (ScrollLeftViewWidth , 0, ScrollLeftViewWidth , height);
			this.scrollViewButtonViewRight.Frame = new RectangleF (Bounds.Width, 0, RightUtilityButtonsWidth , height);
			this.scrollViewContentView.Frame = new RectangleF (ScrollLeftViewWidth , 0, Bounds.Width, height);

		}

		class SWScrollViewDelegate:UIScrollViewDelegate
		{
			SWTableViewCell cell;

			public SWScrollViewDelegate (SWTableViewCell cell)
			{
				this.cell = cell;
				
			}

			public override void WillEndDragging (UIScrollView scrollView, PointF velocity, ref PointF targetContentOffset)
			{
				switch (cell.cellState) {
				case SWCellState.Center:

					if (velocity.X >= 0.5f) {
						this.ScrollToRight(ref targetContentOffset);
					} else if (velocity.X <= -0.5f) {
						this.ScrollToLeft(ref targetContentOffset);
					} else {
						float rightThreshold = cell.UtilityButtonsPadding - (cell.UtilityButtonsPadding / 2);
						float leftThreshold = cell.ScrollLeftViewWidth / 2;
						if (targetContentOffset.X > rightThreshold)
							this.ScrollToRight(ref targetContentOffset);
						else if (targetContentOffset.X < leftThreshold)
							this.ScrollToLeft(ref targetContentOffset);
						else
							this.ScrollToCenter(ref targetContentOffset);
					}

					break;
				case SWCellState.Left:
					if (velocity.X >= 0.5f) {
						this.ScrollToCenter(ref targetContentOffset);
					} else if (velocity.X <= -0.5f) {
						// No-op
					} else {
						if (targetContentOffset.X >= (cell.UtilityButtonsPadding -  cell.RightUtilityButtonsWidth / 2))
							this.ScrollToRight(ref targetContentOffset);
						else if (targetContentOffset.X > cell.ScrollLeftViewWidth / 2)
							this.ScrollToCenter(ref targetContentOffset);
						else
							this.ScrollToLeft(ref targetContentOffset);
					}
					break;
				case SWCellState.Right:
					if (velocity.X >= 0.5f) {
						// No-op
					} else if (velocity.X <= -0.5f) {
						this.ScrollToCenter(ref targetContentOffset);
					} else {
						if (targetContentOffset.X <= this.cell.ScrollLeftViewWidth / 2)
							this.ScrollToLeft(ref targetContentOffset);
						else if (targetContentOffset.X < (cell.UtilityButtonsPadding- (cell.RightUtilityButtonsWidth / 2)))
							this.ScrollToCenter(ref targetContentOffset);
						else
							this.ScrollToRight(ref targetContentOffset);
					}
					break;
				default:
					break;
				}
			}

			void ScrollToCenter (ref PointF targetContentOffset)
			{
				targetContentOffset.X = cell.ScrollLeftViewWidth;
				cell.cellState = SWCellState.Center;
				cell.OnScrolling ();
			}

			void ScrollToLeft (ref PointF targetContentOffset)
			{
				targetContentOffset.X = 0;
				cell.cellState = SWCellState.Left;
				cell.OnScrolling ();
			}

			void ScrollToRight (ref PointF targetContentOffset)
			{
				targetContentOffset.X = cell.UtilityButtonsPadding;
				cell.cellState = SWCellState.Right;
				cell.OnScrolling ();
			}

			public override void Scrolled (UIScrollView scrollView)
			{
				if (scrollView.ContentOffset.X > this.cell.ScrollLeftViewWidth) {
					//expose the right view
					this.cell.scrollViewButtonViewRight.Frame = new RectangleF (scrollView.ContentOffset.X + cell.Bounds.Width - cell.RightUtilityButtonsWidth, 
					                                                            0, cell.RightUtilityButtonsWidth, cell.height);
				} else {
					this.cell.scrollViewLeft.Frame = new RectangleF (scrollView.ContentOffset.X, 0, cell.ScrollLeftViewWidth, cell.height);
				}
			}
		}
	}

	class SWUtilityButtonView:UIView
	{


		SWTableViewCell parentCell;
		UIButton[] utilityButtons;
		float utilityButtonWidth;

		public SWUtilityButtonView (UIButton[] buttons, SWTableViewCell parentCell)
		{
			this.utilityButtons = buttons;
			this.parentCell = parentCell;
			this.utilityButtonWidth = this.CalculateUtilityButtonWidth ();
			this.AddSubviews (buttons);
		}

		public float UtilityButtonsWidth 
		{
			get { return utilityButtonWidth * utilityButtons.Length; }
		}
	
		float CalculateUtilityButtonWidth ()
		{
			float buttonWidth = SWTableViewCell.UtilityButtonWidthDefault;
			if (buttonWidth * utilityButtons.Length > SWTableViewCell.UtilityButtonsWidthMax) {
				float buffer = buttonWidth * utilityButtons.Length - SWTableViewCell.UtilityButtonsWidthMax;
				buttonWidth -= buffer / utilityButtons.Length;
			}
			return buttonWidth;
		}

		public void PopulateUtilityButtons ()
		{
			for (int i = 0; i < utilityButtons.Length; i++) {
				var button = utilityButtons[i];
				float x = 0;
				if (i >= 1)
					x = utilityButtonWidth * i;
				button.Frame = new RectangleF (x, 0, utilityButtonWidth, Bounds.Height);//TODO: frame
				button.Tag = i;
				button.TouchDown += (object sender, EventArgs e) => this.parentCell.OnLeftUtilityButtonPressed((UIButton)sender);


		    }
	
		}


	}

	public static class SWButtonCellExtensions
	{
		public static void AddUtilityButton(this List<UIButton> list,  string title, UIColor color)
		{
			var button = new UIButton (UIButtonType.Custom);
			button.BackgroundColor = color;
			button.SetTitle (title, UIControlState.Normal);
			button.SetTitleColor (UIColor.White, UIControlState.Normal);
			list.Insert (0, button);
		
		}
	}
}
