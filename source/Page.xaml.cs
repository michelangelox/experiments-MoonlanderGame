using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace Silverlight_Moonlander_1
{
	public partial class Page : UserControl
	{
		private Moonscape MoonScape;
		private Moonlander MoonLander;

		private Storyboard StoryBoardCollision = new Storyboard();

		private bool isContact;
		public bool collisionResult;
		private Point ptCheck = new Point();

		private bool isGoAngle;
		private bool isGoXspeed;
		private bool isGoYspeed;

		private double fuelTank;
		private TimeSpan timeAllotted = new TimeSpan();
		private TimeSpan timeLeft = new TimeSpan();

		private int moonLanderCenterOffset = 10;

		public Page()
		{
			InitializeComponent();

			this.StoryBoardCollision.Duration = TimeSpan.FromMilliseconds(10);
			this.StoryBoardCollision.Completed += new EventHandler(StoryBoardCollision_Completed);

			InitializeMoonsLanderGame();			
		}

		private void InitializeMoonsLanderGame()
		{
			this.MoonScape = new Moonscape(
				Convert.ToInt32(((ComboBoxItem)MoonscapePointsCombo.SelectedItem).Content.ToString()), 
				Convert.ToInt32(((ComboBoxItem)LandingWidthCombo.SelectedItem).Content.ToString())); //moonscape points, landingspot width

			this.MoonLander = new Moonlander(this.CanvasGameData, this.CanvasGamePlay);

			this.CanvasGamePlay.Children.Add(this.MoonScape);
			this.CanvasGamePlay.Children.Add(this.MoonLander);

			this.fuelTank = MoonLander.Fuel;
			this.timeAllotted = new TimeSpan(0,0,30);

			this.StoryBoardCollision.Begin();
		}

		private void UserControl_KeyDown(object sender, KeyEventArgs e)
		{
			if (!isContact && MoonLander.Fuel > 0)
			{
				MoonLander.CanvasMoonlander_KeyDown(sender, e);
			}
			else
			{
				MoonLander.CanvasMoonlander_KeyUp(sender, e);
			}
		}

		private void UserControl_KeyUp(object sender, KeyEventArgs e)
		{
			if (!isContact)
			{
				MoonLander.CanvasMoonlander_KeyUp(sender, e);
			}
		}

		private void StoryBoardCollision_Completed(object sender, EventArgs e)
		{
			this.isContact = CheckForImpact(MoonLander.CanvasMoonlander, MoonLander.MoonLanderShip, MoonScape.CanvasMoonscape, MoonScape.GroundScapePath);

			if (this.isContact)
			{
				if (MoonLander.LanderTranslateTransform.X + 10						>= MoonScape.LandingSpotPoint.X &&
					MoonLander.LanderTranslateTransform.X + MoonLander.Width - 10	<= MoonScape.LandingSpotPoint.X + MoonScape.LandingSpotWidth)
				{
					if (isGoAngle && isGoXspeed && isGoYspeed)
					{
						this.endGame("landed");
					}
					else
					{
						this.endGame("crashed");
					}
				}
				else
				{
					this.endGame("crashed");
				}
			}

			//fills three warning ellipses green if all ok
			this.rectOkContainer.Fill = (isGoAngle && isGoXspeed && isGoYspeed)?new SolidColorBrush(Colors.White):new SolidColorBrush(Colors.Transparent);

			//animate the fuel and timer gauges
			if (MoonLander.Fuel > 0)
			{
				this.rectFuelGauge.Width = this.rectFuelGaugeBox.Width / this.fuelTank * MoonLander.Fuel;
			}
			//else
			//{
			//    this.endGame("out of Fuel");
			//}

			this.timeLeft = this.timeAllotted.Subtract(MoonLander.TimeElapsed);

			if (timeLeft.TotalSeconds > 0)
			{
				this.rectTimeGauge.Width = this.rectTimeGaugeBox.Width / this.timeAllotted.TotalSeconds * timeLeft.TotalSeconds;
			}
			else
			{
				//if time runs out, cut off fuel supply....haha hahaaha hahaaa!
				MoonLander.Fuel = 0;
				this.timeLeft = new TimeSpan(0,0,0);
			}

			//populate GameData Panel
			this.txtLevel.Text				= "MoonLander 1.0";
			this.txtFuel.Text				= "Fuel:  "		+ (MoonLander.Fuel.ToString().Length >= 4?MoonLander.Fuel.ToString().Remove(4):"0") + "%";
			this.txtTime.Text				= "Time: "		+ this.timeLeft.Minutes + ":" + this.timeLeft.Seconds + ":" + this.timeLeft.Milliseconds;
			this.txtVerticalSpeed.Text		= "Xspeed: "	+ (MoonLander.HorizontalVelocity * 10).ToString().Remove(4) + " m/s";
			this.txtHorizontalSpeed.Text	= "Yspeed: "	+ (MoonLander.VerticalVelocity * 10).ToString().Remove(4) + " m/s";
			this.txtMoonLanderAngle.Text	= "Angle: "		+ MoonLander.Angle.ToString().Remove(4) + " deg";

			if (MoonLander.Fuel > 0)
			{
				this.rectFuelGauge.Fill = getFuelGaugeColor(this.fuelTank, MoonLander.Fuel);
			}
			if (timeLeft.TotalSeconds > 0)
			{
				this.rectTimeGauge.Fill = getTimeGaugeColor(this.timeAllotted, this.timeLeft);
			} 
			
			this.rectOkAngle.Fill			= getAngleWarningColor(MoonLander.Angle);
			this.rectOkHorizontalSpeed.Fill = getHorizontalSpeedWarningColor(MoonLander.HorizontalVelocity);
			this.rectOkVerticalSpeed.Fill	= getVerticalSpeedWarningColor(MoonLander.VerticalVelocity);

			this.StoryBoardCollision.Begin();
		}

		private bool CheckForImpact(FrameworkElement _canvasMoonLander, FrameworkElement _moonLanderShip, FrameworkElement _canvasMoonScape, FrameworkElement _moonGroundScape)
		{
			double MoonLanderX = Convert.ToInt32(MoonLander.LanderTranslateTransform.X);
			double MoonLanderY = Convert.ToInt32(MoonLander.LanderTranslateTransform.Y);

			this.collisionResult = false;

			//chheck for out of bounds first 
			if (MoonLanderX + moonLanderCenterOffset < 0 || MoonLanderX - moonLanderCenterOffset + MoonLander.Width > CanvasGamePlay.Width || MoonLanderY < 0)
			{
				this.collisionResult = true;
			}
			else
			{

				for (int x = Convert.ToInt32(MoonLanderX) + moonLanderCenterOffset; x < Convert.ToInt32(MoonLanderX + MoonLander.Width - moonLanderCenterOffset); x++)
				{
					this.ptCheck.X = x;
					this.ptCheck.Y = MoonLanderY + 100;

					List<UIElement> collisionList = VisualTreeHelper.FindElementsInHostCoordinates(ptCheck, _canvasMoonScape) as List<UIElement>;
					if (collisionList.Contains(_moonGroundScape))
					{
						this.collisionResult = true;
						break;
					}
				}
			}
			return collisionResult;
		}

		private SolidColorBrush getFuelGaugeColor(double _fuelTank, double _fuelRemaining)
		{
			int _fuelPercentage = Convert.ToInt32(_fuelRemaining);//Convert.ToInt32((_fuelRemaining / _fuelTank) * 100);

			if (_fuelPercentage < 75 && _fuelPercentage > 35)
			{
				return new SolidColorBrush(Colors.Yellow);
			}
			else if (_fuelPercentage < 35)
			{
				return new SolidColorBrush(Colors.Red);
			}
			else
			{
				return new SolidColorBrush(Colors.Green);
			}
		}

		private SolidColorBrush getTimeGaugeColor(TimeSpan _timeAllotted, TimeSpan _timeRemaining)
		{
			double _timePercentage = Convert.ToInt32((_timeAllotted.TotalSeconds/100) / _timeRemaining.TotalSeconds) ;
			
			if (_timePercentage < 75 && _timePercentage > 35)
			{
				return new SolidColorBrush(Colors.Yellow);
			}
			else if (_timePercentage < 35)
			{
				return new SolidColorBrush(Colors.Red);
			}
			else
			{
				return new SolidColorBrush(Colors.Green);
			}
		}

		private SolidColorBrush getAngleWarningColor(double _angle)
		{
			this.isGoAngle = false;
			if (_angle < -1 || _angle > 1)
			{
				if (_angle < -3 || _angle > 3)
				{
					return new SolidColorBrush(Colors.Red);
				}
				return new SolidColorBrush(Colors.Yellow);
			}
			else
			{
				this.isGoAngle = true;
				return new SolidColorBrush(Colors.Green);
			}
		}

		private SolidColorBrush getHorizontalSpeedWarningColor(double _hSpeed)
		{
			this.isGoXspeed = false;
			if (_hSpeed < -0.1 || _hSpeed > 0.1)
			{
				if (_hSpeed < -0.2 || _hSpeed > 0.2)
				{
					return new SolidColorBrush(Colors.Red);
				}
				return new SolidColorBrush(Colors.Yellow);
			}
			else
			{
				this.isGoXspeed = true;
				return new SolidColorBrush(Colors.Green);
			}
		}

		private SolidColorBrush getVerticalSpeedWarningColor(double _vSpeed)
		{
			this.isGoYspeed = false;
			if (_vSpeed > 0.2)
			{
				if (_vSpeed > 0.3)
				{
					return new SolidColorBrush(Colors.Red);
				}
				return new SolidColorBrush(Colors.Yellow);
			}
			else
			{
				this.isGoYspeed = true;
				return new SolidColorBrush(Colors.Green);
			}
		}

		private void endGame(string _reason)
		{
			this.MoonLander.Stop();
			this.StoryBoardCollision.Stop();

			this.MoonLander.MainThruster.Visibility			= Visibility.Collapsed;
			this.MoonLander.PortThruster.Visibility			= Visibility.Collapsed;
			this.MoonLander.StarboardThruster.Visibility	= Visibility.Collapsed;

			Uri uri = new Uri(_reason+".png", UriKind.Relative);
			ImageSource imgSource = new BitmapImage(uri);
			this.MoonLander.CrashLanderImage = imgSource;
		}

		private void PlayAgainButton_Click(object sender, RoutedEventArgs e)
		{
			this.MoonLander.Stop();
			this.StoryBoardCollision.Stop();

			InitializeMoonsLanderGame();
		}
	}
}