// Neural Network OCR
//
// Copyright � Andrew Kirillov, 2005
// andrew.kirillov@gmail.com
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data;
using System.Windows.Forms;

using AForge.Imaging;
using AForge.Imaging.Filters;

namespace NeuroOCR
{
	/// <summary>
	/// Summary description for PaintBoard.
	/// </summary>
	public class PaintBoard : System.Windows.Forms.Control
	{
		private Bitmap		image;
		private bool		showReceptors = false;
		private bool		scaleImage = false;
		private Receptors	receptors = null;

		private Shrink		shrinkFilter = new Shrink(Color.FromArgb(255, 255, 255));
		private Resize		resizeFilter = new Resize();

		private bool		tracking = false;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		// ShowReceptors property
		[DefaultValue(false)]
		public bool ShowReceptors
		{
			get { return showReceptors; }
			set
			{
				showReceptors = value;
				Invalidate();
			}
		}
		// ScaleImage property
		[DefaultValue(false)]
		public bool ScaleImage
		{
			get { return scaleImage; }
			set { scaleImage = value; }
		}
		// Receptors collection
		[Browsable(false)]
		public Receptors Receptors
		{
			get { return receptors; }
			set
			{
				receptors = value;
				Invalidate();
			}
		}


		// Area size property
		public Size AreaSize
		{
			get { return new Size(ClientRectangle.Width - 2, ClientRectangle.Height - 2); }
		}

		// Constructor
		public PaintBoard()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer |
				ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);

			resizeFilter.Method = InterpolationMethod.NearestNeighbor;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( components != null )
					components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// PaintBoard
			// 
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PaintBoard_MouseUp);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PaintBoard_MouseMove);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PaintBoard_MouseDown);

		}
		#endregion

		// Paint the control
		protected override void OnPaint(PaintEventArgs pe)
		{
			Graphics	g = pe.Graphics;
			Rectangle	rc = ClientRectangle;

			Pen			blackPen = new Pen(Color.Black, 1);
			Pen			bluePen = new Pen(Color.Blue, 1);
			Brush		whiteBrush = new SolidBrush(Color.White);

			// draw border
			g.DrawRectangle(blackPen, 0, 0, rc.Width - 1, rc.Height - 1);

			// fill rectangle
			g.FillRectangle(whiteBrush, 1, 1, rc.Width - 2, rc.Height - 2);

			// draw image
			if (image != null)
			{
				g.DrawImage(image, 1, 1, image.Width, image.Height);
			}

			// draw receptors
			if ((showReceptors) && (receptors != null))
			{
				foreach (Receptor r in receptors)
				{
					g.DrawLine(bluePen,
						r.X1 + 1, r.Y1 + 1,
						r.X2 + 1, r.Y2 + 1);
				}
			}

			blackPen.Dispose();
			bluePen.Dispose();
			whiteBrush.Dispose();

			base.OnPaint(pe);
		}

		// Clear image
		public void ClearImage()
		{
			int		width = this.ClientRectangle.Width - 2;
			int		height = this.ClientRectangle.Height - 2;
			Brush	whiteBrush = new SolidBrush(Color.White);

			if (image != null)
				image.Dispose();

			// create new image
			image = new Bitmap(width, height, PixelFormat.Format24bppRgb);

			// create graphics
			Graphics g = Graphics.FromImage(image);

			// fill rectangle
			g.FillRectangle(whiteBrush, 0, 0, width, height);

			g.Dispose();
			whiteBrush.Dispose();

			Invalidate();
		}

		// Draw letter
		public void DrawLetter(char c, string fontName, float size, bool italic)
		{
			DrawLetter(c, fontName, size, italic, true);
		}
		public void DrawLetter(char c, string fontName, float size, bool italic, bool invalidate)
		{
			int		width = this.ClientRectangle.Width - 2;
			int		height = this.ClientRectangle.Height - 2;
			Brush	blackBrush = new SolidBrush(Color.Black);
			Brush	whiteBrush = new SolidBrush(Color.White);

			// free previous image
			if (image != null)
				image.Dispose();

			// create new image
			image = new Bitmap(width, height, PixelFormat.Format24bppRgb);

			// create graphics
			Graphics g = Graphics.FromImage(image);

			// fill rectangle
			g.FillRectangle(whiteBrush, 0, 0, width, height);

			// draw letter
			string	str = new string(c, 1);
			Font	font = new Font(fontName, size, (italic) ? FontStyle.Italic : FontStyle.Regular);
			g.DrawString(str, font, blackBrush, new Rectangle(0, 0, width, height));

			g.Dispose();
			font.Dispose();
			blackBrush.Dispose();
			whiteBrush.Dispose();

			if (invalidate)
				Invalidate();
		}

		// Get image
		public Bitmap GetImage()
		{
			return GetImage(true);
		}
		public Bitmap GetImage(bool invalidate)
		{
			if (image == null)
				ClearImage();

			// scale image
			if (scaleImage)
			{
				// shrink image
				Bitmap tempImage = shrinkFilter.Apply(image);

				// image dimenstoin
				int width = image.Width;
				int height = image.Height;
				// shrinked image dimension
				int tw = tempImage.Width;
				int th = tempImage.Height;
				// resize factors
				float fx = (float) width / (float) tw;
				float fy = (float) height / (float) th;

				if (fx > fy)
					fx = fy;
				// set new size of shrinked image
				int nw = (int) Math.Round(fx * tw);
				int nh = (int) Math.Round(fy * th);
				resizeFilter.NewWidth = nw;
				resizeFilter.NewHeight = nh;

				// resize image
				Bitmap tempImage2 = resizeFilter.Apply(tempImage);

				// 
				Brush whiteBrush = new SolidBrush(Color.White);

				// create graphics
				Graphics g = Graphics.FromImage(image);

				// fill rectangle
				g.FillRectangle(whiteBrush, 0, 0, width, height);

				int x = 0;
				int y = 0;

				if (nw > nh)
				{
					y = (height - nh) / 2;
				}
				else
				{
					x = (width - nw) / 2;
				}

				g.DrawImage(tempImage2, x, y, nw, nh);

				g.Dispose();
				whiteBrush.Dispose();

				// release temp images
				tempImage.Dispose();
				tempImage2.Dispose();
			}

			// should we repaint the control
			if (invalidate)
				Invalidate();

			return image;
		}

		// On mouse down
		private void PaintBoard_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if ((!tracking) && (e.Button == MouseButtons.Left))
			{
				Capture = true;
				tracking = true;

				// creat a blank image
				if (image == null)
					ClearImage();
			}
		}

		// On mouse up
		private void PaintBoard_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if ((tracking) && (e.Button == MouseButtons.Left))
			{
				Capture = false;
				tracking = false;
			}
		}

		// On mouse move
		private void PaintBoard_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (tracking)
			{
				int	x = e.X - 1;
				int y = e.Y - 1;

				if ((x > 0) && (y > 0) && (x < image.Width) && (y < image.Height))
				{
					using (Brush brush = new SolidBrush(Color.Black))
					{
						Graphics g = Graphics.FromImage(image);

						// draw a point
						g.FillEllipse(brush, x - 5, y - 5, 11, 11);

						g.Dispose();
					}

					Invalidate();
				}
			}
		}
	}
}
