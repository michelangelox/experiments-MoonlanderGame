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

namespace Silverlight_Moonlander_1
{
	public partial class Moonscape : UserControl
	{
		public Path GroundScapePath = new Path();
		private PathGeometry groundScapePathGeometry = new PathGeometry();
		private PathFigure groundScapePathFigure = new PathFigure();
		private PolyLineSegment groundScapeLineSegment = new PolyLineSegment();

		private Random random = new Random();

		private int minimumPoints = 4;

		private int		amountPoints;
		private double	landSpotWidth;

		public Moonscape(int _moonscapePoints, int _landingSpotWidth)
		{
			InitializeComponent();

			CreateGroundMoonscape(_moonscapePoints, _landingSpotWidth);
		}

		private void CreateGroundMoonscape(int _moonscapePoints, int _landingSpotWidth)
		{
			Point groundPoint = new Point();
			Point lastEndPoint = new Point(0, 0);

			this.amountPoints = _moonscapePoints + this.minimumPoints;
			this.landSpotWidth = _landingSpotWidth;

			//determines randomly the landspot point for flat landing spot
			int landingSpotPoint = random.Next(1, amountPoints - 2);

			Rectangle landingSpotRectangle = new Rectangle();

			//create ground points/lines
			GroundScapePath.Stroke = new SolidColorBrush(Colors.White);
			GroundScapePath.StrokeThickness = 2;
			GroundScapePath.SetValue(Canvas.NameProperty,"mm");

			//first point
			groundScapeLineSegment.Points.Add(new Point(0, this.CanvasMoonscape.Height));

			for (int i = 0; i < amountPoints + 1; i++)
			{
				groundPoint.X = i * (this.CanvasMoonscape.Width / amountPoints);
				groundPoint.Y = random.Next((int)this.CanvasMoonscape.Height / 2, (int)this.CanvasMoonscape.Height - 10); // -10 for buffer towards bottom

				//modify Points if this point is selected as the landing point (begin)
				if (i == landingSpotPoint) //point
				{
					//if landingspot too narrow, guarantees landinspot width
					if (this.CanvasMoonscape.Width / amountPoints < landSpotWidth)
					{
						groundPoint.X = lastEndPoint.X + landSpotWidth;
					}

					groundPoint.Y = lastEndPoint.Y;

					//record landing spot point (begin)
					this.LandingSpotPoint = new Point(lastEndPoint.X, lastEndPoint.Y);
					this.LandingSpotWidth = landSpotWidth;

					//prepares the landingspot
					landingSpotRectangle.Width = landSpotWidth;
					landingSpotRectangle.Height = 4;
					landingSpotRectangle.Stroke = new SolidColorBrush(Colors.Green);
					landingSpotRectangle.Fill	= new SolidColorBrush(Colors.Green);
					landingSpotRectangle.SetValue(Canvas.LeftProperty, groundPoint.X - landSpotWidth);
					landingSpotRectangle.SetValue(Canvas.TopProperty, groundPoint.Y - 2);
				}
				//else if (i == landingSpotPoint + 1)
				//{
				//	groundPoint.X = ((i + 1) * (this.CanvasMoonscape.Width / amountPoints))+5;
				//}
				else if (i > landingSpotPoint)
				{
					//compensate for landingspotwidth
					groundPoint.X = ((i + 3) * (this.CanvasMoonscape.Width / amountPoints));
					
					//if last leg...
					if (groundPoint.X > this.CanvasMoonscape.Width)
					{
						groundPoint.X = this.CanvasMoonscape.Width;
						break;
					}
				}

				lastEndPoint.X = groundPoint.X;
				lastEndPoint.Y = groundPoint.Y;

				groundScapeLineSegment.Points.Add(groundPoint);
			}

			//last points to complete shape
			groundScapeLineSegment.Points.Add(new Point(this.CanvasMoonscape.Width, this.CanvasMoonscape.Height));
			groundScapeLineSegment.Points.Add(new Point(0, this.CanvasMoonscape.Height));

			GroundScapePath.Fill = new SolidColorBrush(Colors.Gray);
			GroundScapePath.Opacity = 0.3;

			groundScapePathFigure.Segments.Add(groundScapeLineSegment);
			groundScapePathGeometry.Figures.Add(groundScapePathFigure);
			GroundScapePath.Data = groundScapePathGeometry;

			this.CanvasMoonscape.Children.Add(GroundScapePath);
			this.CanvasMoonscape.Children.Add(landingSpotRectangle);
		}

		public Point LandingSpotPoint { 
			get; 
			set; 
		}

		public double LandingSpotWidth { 
			get; 
			set; 
		}
	}
}