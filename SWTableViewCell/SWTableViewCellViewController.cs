using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;

namespace SWTableViewCell
{
	public partial class SWTableViewCellViewController : UITableViewController
	{
		public SWTableViewCellViewController () : base (UITableViewStyle.Grouped)
		{
			this.TableView.Source = new Source (this);
			this.TableView.AllowsSelection = false;
			this.TableView.RowHeight = 70;
			this.TableView.SeparatorInset = new UIEdgeInsets (0, 0, 0, 0);
		}
		class Source:UITableViewSource
		{
			SWTableViewCellViewController controller;

			public Source (SWTableViewCellViewController controller)
			{
				this.controller = controller;
				
			}
			#region implemented abstract members of UITableViewSource

			public override int RowsInSection (UITableView tableview, int section)
			{
				return 100;
			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				Console.WriteLine ("Row Selected");
			}

			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				var cell = tableView.DequeueReusableCell ("C") as SWTableViewCell;
				if (cell == null) {
					var leftView = new UILabel () {
						Frame = new RectangleF (0, 0, SWTableViewCell.UtilityButtonsWidthMax, tableView.RowHeight),
						BackgroundColor = UIColor.Red,
						Text = "Peekaboo!",
						TextColor = UIColor.White,
						TextAlignment = UITextAlignment.Center
					};

					var buttons = new List<UIButton> ();
					buttons.AddUtilityButton ("More", UIColor.LightGray);
					buttons.AddUtilityButton ("Edit", UIColor.Blue);
				
					cell = new SWTableViewCell (UITableViewCellStyle.Subtitle, "C", tableView, buttons, leftView);
					cell.Scrolling += OnScrolling;
					cell.UtilityButtonPressed += OnButtonPressed;
				}
				cell.TextLabel.Text = "Item " + indexPath.Row;
				cell.DetailTextLabel.Text = "Details " + indexPath.Row;

				cell.HideSwipedContent (false);//reset cell state
				cell.SetNeedsDisplay ();
				return cell;
			}

			#endregion

			void OnScrolling (object sender, ScrollingEventArgs e)
			{
				//uncomment to close any other cells that are open when another cell is swiped
				/*
				if (e.CellState != SWCellState.Center) {
					var paths = this.controller.TableView.IndexPathsForVisibleRows;
					foreach (var path in paths) {
						if(path.Equals(e.IndexPath))
						   continue;
						var cell = (SWTableViewCell)this.controller.TableView.CellAt (path);
						if (cell.State != SWCellState.Center) {
							cell.HideSwipedContent (true);
						}
					}
				}
				*/
			}

			void OnButtonPressed (object sender, CellUtilityButtonClickedEventArgs e)
			{
				if (e.UtilityButtonIndex ==  1) {
					new UIAlertView("Pressed", "You pressed the edit button!", null, null, new[] {"OK"}).Show();
				}
				else if(e.UtilityButtonIndex == 0){
					new UIAlertView("Pressed", "You pressed the more button!", null, null, new[] {"OK"}).Show();
				}
			}



		}
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.
		}

	
	}


}

