using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.DXGI;
using D3D = SharpDX.Direct3D;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.Windows;
using Device = SharpDX.Direct3D11.Device;
using Factory2D = SharpDX.Direct2D1.Factory;
using FactoryDXGI = SharpDX.DXGI.Factory;
using Text = SharpDX.DirectWrite.TextFormat;
using Test01.Physics;
using System.IO;

namespace Test01
{
    //https://www.youtube.com/watch?v=mmL7jsI0CBU&list=PLNDiGlh9Kczv6B3shWLSYKQcfERaD0-A4
    public static class Graphics
    {

        public static RenderForm renderForm = null;
        private static string title = "Tutorial 1";
        private static Device device;

        private static SwapChain swapChain;
        private static RenderTarget renderTarget;
        private static Text textFormat;

        public static void InitializeDirectX()
        {
            renderForm = new RenderForm(title);

            renderForm.Size = new System.Drawing.Size(800, 600);
            renderForm.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            var swapChainDesc = new SwapChainDescription()
            {
                BufferCount = 4,
                Flags = SwapChainFlags.AllowModeSwitch,
                IsWindowed = true,
                ModeDescription = new ModeDescription(800, 600, new Rational(1, 60), Format.R8G8B8A8_UNorm),
                OutputHandle = renderForm.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            D3D.FeatureLevel[] featureLevels = { D3D.FeatureLevel.Level_9_3, D3D.FeatureLevel.Level_10_1 };
            Device.CreateWithSwapChain(D3D.DriverType.Hardware, DeviceCreationFlags.BgraSupport, featureLevels, swapChainDesc, out device, out swapChain);

            Surface backBuffer = Surface.FromSwapChain(swapChain, 0);

            using (var factory2D = new Factory2D(FactoryType.MultiThreaded))
            {
                var dpi = factory2D.DesktopDpi;

                InitializeRenderTarget(backBuffer, factory2D, dpi);

                renderForm.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
                renderTarget.AntialiasMode = AntialiasMode.Aliased;
                renderTarget.TextAntialiasMode = TextAntialiasMode.Aliased;

                using (SharpDX.DirectWrite.Factory textFactory = new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Shared))
                {
                    textFormat = new Text(textFactory,
                        "MS Sans Serif",
                        SharpDX.DirectWrite.FontWeight.SemiBold,
                        SharpDX.DirectWrite.FontStyle.Normal,
                        SharpDX.DirectWrite.FontStretch.Medium,
                        16.0f
                        );
                }

                renderForm.KeyDown += (o, e) =>
                {
                    if (e.Alt && e.KeyCode == System.Windows.Forms.Keys.Enter)
                    {
                        swapChain.IsFullScreen = !swapChain.IsFullScreen;
                    }
                };
            }
        }

        private static void InitializeRenderTarget(Surface backBuffer, Factory2D factory2D, Size2F dpi)
        {
            var pixelFormat = new PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Ignore);

            renderTarget = new RenderTarget(factory2D, backBuffer, new RenderTargetProperties()
            {
                DpiX = dpi.Width,
                DpiY = dpi.Height,
                MinLevel = FeatureLevel.Level_9,
                PixelFormat = pixelFormat,
                Type = RenderTargetType.Default,
                Usage = RenderTargetUsage.None
            });
        }

        private static Bitmap _dvdImage;
        public static void Render()
        {
            InitializeDirectX();
            _dvdImage = LoadImage("dvd.png");
            Timer.Start();

            Timer.Update();
            RenderLoop.Run(renderForm, () =>
            {
                if (renderForm.WindowState == System.Windows.Forms.FormWindowState.Minimized)
                {
                    return;
                }

                try
                {
                    while (Timer.VerifyLag())
                    {
                        Timer.Update();
                        Update();                        
                    }

                    renderTarget.BeginDraw();
                    renderTarget.Transform = Matrix3x2.Identity;
                    renderTarget.Clear(Color.White);

                    Draw(renderTarget);

                    renderTarget.Flush();
                    renderTarget.EndDraw();
                    swapChain.Present(0, PresentFlags.None);
                }
                catch (Exception ex)
                {
                    renderForm.Close();
                }

                //swapChain.IsFullScreen = false;
                //textFormat.Dispose();
                //renderTarget.Dispose();
                //swapChain.Dispose();
                //device.Dispose();
            });
        }

        private static void Update()
        {
            for (int i = 0; i < dvds.Length; i++)
            {
                Body dvd = dvds[i];
                CheckBorderPosition(dvd);
                dvd.UpdatePosition();

                if (i > 0)
                {
                    for (int j = i - 1; j >= 0; j--)
                    {
                        CheckColision(dvds[j], dvd);
                    }
                }
            }
        }

        static Body[] dvds = CreateDvds();

        private static Body[] CreateDvds()
        {
            Size2 size = new Size2(35, 17);
            float speedFactor = 1F;

            Body dvdOne = new Body()
            {
                Position = new Vector2(16, 0),
                Size = size,
                Speed = new Vector2(0.4F, 0.1F) * speedFactor
            };

            Body dvdTwo = new Body()
            {
                Position = new Vector2(658, 16),
                Size = size,
                Speed = new Vector2(-0.3F, 0.3F) * speedFactor
            };

            Body dvdThree = new Body()
            {
                Position = new Vector2(64, 532),
                Size = size,
                Speed = new Vector2(0.1F, -0.5F) * speedFactor
            };

            Body dvdFour = new Body()
            {
                Position = new Vector2(658, 532),
                Size = size,
                Speed = new Vector2(-0.3F, -0.4F) * speedFactor
            };

            return new Body[]{
                dvdOne,
                dvdTwo,
                dvdThree,
                dvdFour
            };
        }

        private static void Draw(RenderTarget renderTarget)
        {

            using (var brushE = new SolidColorBrush(renderTarget, Color.Blue))
            {
                for (int i = 0; i < dvds.Length; i++)
                {
                    renderTarget.DrawBitmap(_dvdImage, dvds[i].BoundingBoxF, 1F, BitmapInterpolationMode.Linear);
                }
            }

            using (var brush = new SolidColorBrush(renderTarget, Color.Green))
            {
                //for (int x = 0; x <= renderTarget.Size.Width; x += 16)
                //{
                //    renderTarget.DrawLine(new Vector2(x, 0), new Vector2(x, renderTarget.Size.Height), brush);
                //}

                //for (int y = 0; y <= renderTarget.Size.Height; y += 16)
                //{
                //    renderTarget.DrawLine(new Vector2(0, y), new Vector2(renderTarget.Size.Width, y), brush);
                //}

                //renderTarget.DrawText("X: " + body.Position.X.ToString(), textFormat,
                //    new SharpDX.Mathematics.Interop.RawRectangleF(96, 96, renderTarget.Size.Width, renderTarget.Size.Height), brush);

                //renderTarget.DrawText("Y: " + body.Position.Y.ToString(), textFormat,
                //    new SharpDX.Mathematics.Interop.RawRectangleF(96, 128, renderTarget.Size.Width, renderTarget.Size.Height), brush);

                renderTarget.DrawText("Elapsed Time: " + Timer.ElapsedTime.ToString(), textFormat,
                    new SharpDX.Mathematics.Interop.RawRectangleF(96, 128, renderTarget.Size.Width, renderTarget.Size.Height), brush);

                renderTarget.DrawText("Lag Time: " + Timer.LagTime.ToString(), textFormat,
                    new SharpDX.Mathematics.Interop.RawRectangleF(96, 158, renderTarget.Size.Width, renderTarget.Size.Height), brush);
            }
        }

        private static void CheckColision(Body source, Body target)
        {
            if (source.Colide(target))
            {
                source.Speed = new Vector2(source.Speed.X * -1F, source.Speed.Y * -1F);
                target.Speed = new Vector2(target.Speed.X * -1F, target.Speed.Y * -1F);
            }
        }

        private static void CheckBorderPosition(Body body)
        {
            if (body.Position.X <= 0 && body.Speed.X < 0)
            {
                body.Speed = new Vector2(body.Speed.X * -1F, body.Speed.Y);
            }
            else if ((body.Position.X + body.Size.Width) >= 800 && body.Speed.X > 0)
            {
                body.Speed = new Vector2(body.Speed.X * -1F, body.Speed.Y);
            }

            if (body.Position.Y <= 0 && body.Speed.Y < 0)
            {
                body.Speed = new Vector2(body.Speed.X, body.Speed.Y * -1F);
            }
            else if ((body.Position.Y + body.Size.Height) >= 600 && body.Speed.Y > 0)
            {
                body.Speed = new Vector2(body.Speed.X, body.Speed.Y * -1F);
            }
        }

        public static Bitmap LoadImage(string fileName)
        {
            string path = Path.Combine(Environment.CurrentDirectory, fileName);
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(path);

            System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
                );

            DataStream stream = new DataStream(bmpData.Scan0, bmpData.Stride * bmpData.Height, true, false);
            PixelFormat pixelFormat = new PixelFormat(Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied);
            BitmapProperties bitmapProperties = new BitmapProperties(pixelFormat);
            Bitmap result = new Bitmap(renderTarget, new Size2(bitmap.Width, bitmap.Height), stream, bmpData.Stride, bitmapProperties);

            bitmap.UnlockBits(bmpData);
            stream.Close();
            stream.Dispose();

            return result;
        }
    }
}
