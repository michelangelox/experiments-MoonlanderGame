using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Silverlight_Moonlander_1
{
	public partial class Moonlander : UserControl
	{
		private Storyboard StoryBoardMoonLander	= new Storyboard();

		private bool isKeyUpPressed			= false;
		private bool isKeyLeftPressed		= false;
		private bool isKeyRightPressed		= false;

		private double velocityVertical		= 0.00001;
		private double velocityHorizontal	= 0.00001;
		private double angleOfLander		= 0.00001;

		private double fuel					= 100.0001;
		private double consumptionMain		= 0.05;
		private double consumptionSteering	= 0.02;

		const double gravity				= 0.003;

		const double velocityLimitTerminal	= 0.3;
		const double velocityLimitThrust	= 0.2;
		const double velocityLimitLateral	= 0.8;
		const double angleLimitOfLander		= 6;

		private TimeSpan timeElapsed;
		private DateTime now;
		private Canvas gameData;
		private Canvas gamePlay;

		public Moonlander(Canvas _gameData, Canvas _gamePlay)
		{
			InitializeComponent();

			this.now = DateTime.Now;
			this.gameData = _gameData;
			this.gamePlay = _gamePlay;

			this.StoryBoardMoonLander.Duration	= TimeSpan.FromMilliseconds(1);
			this.StoryBoardMoonLander.Completed	+= new EventHandler(ThrusterStoryBoard_Completed);

			this.StoryBoardMoonLander.Begin();

			this.LanderRotateTransform.CenterX = this.Width/2;
			this.LanderRotateTransform.CenterY = this.Height/2;
		}

		private void ThrusterStoryBoard_Completed(object sender, EventArgs e)
		{
			if (fuel > 0)
			{
				if (isKeyUpPressed)
				{
					velocityVertical -= gravity;
					fuel -= consumptionMain;
				}

				if (isKeyRightPressed)
				{
					if (velocityHorizontal < velocityLimitLateral)
					{
						velocityHorizontal += gravity;
					}

					if (angleOfLander < angleLimitOfLander)
					{
						angleOfLander += 0.3;
					}

					fuel -= consumptionSteering;
				}

				if (isKeyLeftPressed)
				{
					if (velocityHorizontal > velocityLimitLateral * (-1))
					{
						velocityHorizontal -= gravity;
					}
					if (angleOfLander > angleLimitOfLander * (-1))
					{
						angleOfLander -= 0.3;
					}

					fuel -= consumptionSteering;
				}
			}
			else
			{
				fuel = 0;
			}

			//normalize vertical speed
			if (!isKeyUpPressed)
			{
				if (velocityVertical < velocityLimitTerminal)
				{
					velocityVertical += gravity;
				}
			}

			//normalize horizontal speed and lander angle
			if (!isKeyLeftPressed ||
				!isKeyRightPressed ||
				(isKeyLeftPressed && isKeyRightPressed))
			{
				if (velocityHorizontal > 0.001)
				{
					velocityHorizontal -= gravity / 5;
				}
				else if (velocityHorizontal < -0.001)
				{
					velocityHorizontal += gravity / 5;
				}
				else
				{
					velocityHorizontal = 0.0001;
				}

				if (angleOfLander > 0.001)
				{
					angleOfLander -= 0.1;
				}
				else if (angleOfLander < -0.001)
				{
					angleOfLander += 0.1;
				}
				else
				{
					angleOfLander = 0.0001;
				}
			}
			

			//apply RenderTransform data
			LanderTranslateTransform.X += velocityHorizontal;
			LanderTranslateTransform.Y += velocityVertical;
			LanderRotateTransform.Angle = angleOfLander;

			//populate GameData panel
			this.timeElapsed = DateTime.Now - now ;

			if (fuel <= 0)
			{
				this.MainThruster.Visibility = System.Windows.Visibility.Collapsed;
				this.StarboardThruster.Visibility = System.Windows.Visibility.Collapsed;
				this.PortThruster.Visibility = System.Windows.Visibility.Collapsed;
			}

			this.StoryBoardMoonLander.Begin();
		}

		public void CanvasMoonlander_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Up)
			{
				this.isKeyUpPressed = true;
				this.MainThruster.Visibility = System.Windows.Visibility.Visible;
			}

			if (e.Key == Key.Left)
			{
				this.isKeyLeftPressed = true;
				this.StarboardThruster.Visibility = System.Windows.Visibility.Visible;
			}

			if (e.Key == Key.Right)
			{
				this.isKeyRightPressed = true;
				this.PortThruster.Visibility = System.Windows.Visibility.Visible;
			}
		}

		public void CanvasMoonlander_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Up)
			{
				this.isKeyUpPressed = false;
				this.MainThruster.Visibility = System.Windows.Visibility.Collapsed;
			}

			if (e.Key == Key.Left)
			{
				this.isKeyLeftPressed = false;
				this.StarboardThruster.Visibility = System.Windows.Visibility.Collapsed;
			}

			if (e.Key == Key.Right)
			{
				this.isKeyRightPressed = false;
				this.PortThruster.Visibility = System.Windows.Visibility.Collapsed;
			}
		}

		//------------------------------------------------------------------------------------------------------------------------------
		public double Angle 
		{
			get { return angleOfLander; }
		}
		public double Fuel
		{
			set { fuel = value; }
			get { return fuel; }
		}
		public double HorizontalVelocity
		{
			get { return velocityHorizontal; }
		}
		public double VerticalVelocity
		{
			get { return velocityVertical; }
		}
		public TimeSpan TimeElapsed
		{
			get { return timeElapsed; }
		}
		public void Stop()
		{
			this.StoryBoardMoonLander.Stop();
		}

		//-------------------------------------------------------------------------------------------------------------------------------
		public ImageSource CrashLanderImage
		{
			set
			{
				this.MoonLanderShipImage.Source = value;
			}
		}
	}
}