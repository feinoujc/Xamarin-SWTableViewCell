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
			this.TableView.Source = new Source ();
			this.TableView.AllowsSelection = false;
			this.TableView.RowHeight = 70;
			this.TableView.SeparatorInset = new UIEdgeInsets (0, 0, 0, 0);
		}
		class Source:UITableViewSource
		{
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
				var cell = tableView.DequeueReusableCell ("Cell") as SWTableViewCell;
				if (cell == null) {
					var leftView = new UILabel () {
						Frame = new RectangleF (0, 0, 260, tableView.RowHeight),
						BackgroundColor = UIColor.Red,
						Text = "Peekaboo!",
						TextColor = UIColor.White,
						TextAlignment = UITextAlignment.Center
					};

					var buttons = new List<UIButton> ();
					buttons.AddUtilityButton ("Edit", UIColor.Blue);
					buttons.AddUtilityButton ("More", UIColor.LightGray);
					
					cell = new SWTableViewCell (UITableViewCellStyle.Subtitle, "Cell", tableView, buttons, leftView);
					cell.Scrolling += (sender, e) => Console.WriteLine("Scrolling {0}", e.CellState);
					cell.UtilityButtonPressed += (sender, e) => Console.WriteLine("Button Pressed {0}", e.UtilityButtonIndex);
				}
				cell.TextLabel.Text = "Test " + indexPath.Row;
				return cell;
			}

			public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
			{
				return tableView.RowHeight;
			}

			#endregion



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

		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
		}
	}


}

