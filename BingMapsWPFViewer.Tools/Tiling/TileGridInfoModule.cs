using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net;

namespace BingMapsWPFViewer.Tools
{

	public class TileGridInfoModule : NancyModule
	{


		public TileGridInfoModule()
		{
			Get["/tile/{z}/{x}/{y}.png"] = _ =>
			{

				byte[] imgBytes = this.GenerateTile(_.x, _.y, _.z);
				return Response.FromStream(new MemoryStream(imgBytes), "image/png");
			};


			Get["/disktile/{path}/{z}/{x}/{y}.png"] = _ =>
			{
				Stream img = this.GenerateTileFromDisk(_.path, _.x, _.y, _.z);
				if (img == null)
					return Response.FromStream(new MemoryStream(), "image/png");
				else
					return Response.FromStream(img, "image/png");
			};
		}

		private byte[] GenerateTile(int x, int y, int z)
		{
			byte[] ret = null;
			using (MemoryStream v_memStream = new MemoryStream())
			using (Bitmap v_bmp = new Bitmap(256, 256))
			using (Graphics v_graphics = Graphics.FromImage(v_bmp))
			{
				v_graphics.SmoothingMode = SmoothingMode.AntiAlias;

				if ((x + y) % 2 == 0)
					v_graphics.Clear(Color.Transparent);
				else
					v_graphics.Clear(Color.FromArgb(64, Color.Black));


				string tileInfo = BingMapsTileSystem.GetTileInfo(x, y, z);

				this.DrawCenteredOulinedText(v_graphics, tileInfo);
				v_bmp.Save(v_memStream, ImageFormat.Png);
				ret = v_memStream.ToArray();
			}

			return ret;
		}

		private byte[] GenerateTileFromDisk(string path, int x, int y, int z)
		{
			MemoryStream ret = null;

			string realPath = path + ";{0};{1};{2}.png";
			realPath = realPath.Replace(";", @"\");
			realPath = string.Format(realPath, z, x, y);

			if (File.Exists(realPath))
			{
				ret = new MemoryStream();
				using (Bitmap v_bmp = (Bitmap)Bitmap.FromFile(realPath))
				{
					v_bmp.Save(ret, ImageFormat.Png);
					ret.Position = 0;
				}
			}
			else
			{
				return GenerateTile(x, y, z);
				//DownloadTile(realPath, x, y, z);
				//return GenerateTileFromDisk(path, x, y, z);
				
			}

			return ret.ToArray();
		}

		private void DrawCenteredOulinedText(Graphics g, string text)
		{
			//create a path
			using (GraphicsPath pth = new GraphicsPath())
			{
				//Select the pen             
				using (Pen p = new Pen(Color.Black, 1.0f))
				{
					StringFormat format = new StringFormat();
					format.Alignment = StringAlignment.Center;
					format.LineAlignment = StringAlignment.Center;
					//Add new text
					pth.AddString(text,
							new FontFamily(System.Drawing.Text.GenericFontFamilies.Monospace), (int)FontStyle.Bold, 24,
							new Rectangle(0, 0, 255, 255), format);

					//Fill it
					g.FillPath(Brushes.White, pth);
					//outline it
					g.DrawPath(p, pth);
				}
			}
		}

	}
}
