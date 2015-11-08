using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Collections.Generic;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;

namespace TattersallEffect
{
    public class PluginSupportInfo : IPluginSupportInfo
    {
        public string Author
        {
            get
            {
                return ((AssemblyCopyrightAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0]).Copyright;
            }
        }
        public string Copyright
        {
            get
            {
                return ((AssemblyDescriptionAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)[0]).Description;
            }
        }

        public string DisplayName
        {
            get
            {
                return ((AssemblyProductAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0]).Product;
            }
        }

        public Version Version
        {
            get
            {
                return base.GetType().Assembly.GetName().Version;
            }
        }

        public Uri WebsiteUri
        {
            get
            {
                return new Uri("http://www.getpaint.net/redirect/plugins.html");
            }
        }
    }

    [PluginSupportInfo(typeof(PluginSupportInfo), DisplayName = "Tattersall")]
    public class TattersallEffectPlugin : PropertyBasedEffect
    {
        public static string StaticName
        {
            get
            {
                return "Tattersall";
            }
        }

        public static Image StaticIcon
        {
            get
            {
                return new Bitmap(typeof(TattersallEffectPlugin), "Tattersall.png");
            }
        }

        public static string SubmenuName
        {
            get
            {
                return SubmenuNames.Render;  // Programmer's chosen default
            }
        }

        public TattersallEffectPlugin()
            : base(StaticName, StaticIcon, SubmenuName, EffectFlags.Configurable)
        {
        }

        public enum PropertyNames
        {
            Amount1,
            Amount2,
            Amount3,
            Amount4,
            Amount5,
            Amount6,
            Amount7
        }

        public enum Amount3Options
        {
            Amount3Option1,
            Amount3Option2,
            Amount3Option3,
            Amount3Option4,
            Amount3Option5
        }


        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new Int32Property(PropertyNames.Amount1, 4, 1, 100));
            props.Add(new Int32Property(PropertyNames.Amount2, 16, 1, 100));
            props.Add(StaticListChoiceProperty.CreateForEnum<Amount3Options>(PropertyNames.Amount3, Amount3Options.Amount3Option3, false));
            props.Add(new Int32Property(PropertyNames.Amount4, ColorBgra.ToOpaqueInt32(ColorBgra.FromBgra(EnvironmentParameters.PrimaryColor.B, EnvironmentParameters.PrimaryColor.G, EnvironmentParameters.PrimaryColor.R, 255)), 0, 0xffffff));
            props.Add(new Int32Property(PropertyNames.Amount5, ColorBgra.ToOpaqueInt32(ColorBgra.FromBgra(EnvironmentParameters.SecondaryColor.B, EnvironmentParameters.SecondaryColor.G, EnvironmentParameters.SecondaryColor.R, 255)), 0, 0xffffff));
            props.Add(new Int32Property(PropertyNames.Amount6, ColorBgra.ToOpaqueInt32(ColorBgra.FromBgra((byte)((EnvironmentParameters.PrimaryColor.B + EnvironmentParameters.SecondaryColor.B) / 2), (byte)((EnvironmentParameters.PrimaryColor.G + EnvironmentParameters.SecondaryColor.G) / 2), (byte)((EnvironmentParameters.PrimaryColor.R + EnvironmentParameters.SecondaryColor.R) / 2), 255)), 0, 0xffffff));
            props.Add(new Int32Property(PropertyNames.Amount7, ColorBgra.ToOpaqueInt32(Color.White), 0, 0xffffff));

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Amount1, ControlInfoPropertyNames.DisplayName, "Line Width");
            configUI.SetPropertyControlValue(PropertyNames.Amount2, ControlInfoPropertyNames.DisplayName, "Line Spacing");
            configUI.SetPropertyControlValue(PropertyNames.Amount3, ControlInfoPropertyNames.DisplayName, "Line Style");
            PropertyControlInfo Amount3Control = configUI.FindControlForPropertyName(PropertyNames.Amount3);
            Amount3Control.SetValueDisplayName(Amount3Options.Amount3Option1, "Solid - 33% Opacity");
            Amount3Control.SetValueDisplayName(Amount3Options.Amount3Option2, "Solid - 66% Opacity");
            Amount3Control.SetValueDisplayName(Amount3Options.Amount3Option3, "Diagonal Lines - Up");
            Amount3Control.SetValueDisplayName(Amount3Options.Amount3Option4, "Diagonal Lines - Down");
            Amount3Control.SetValueDisplayName(Amount3Options.Amount3Option5, "Dots - 50/50");
            configUI.SetPropertyControlValue(PropertyNames.Amount4, ControlInfoPropertyNames.DisplayName, "Line Color 1");
            configUI.SetPropertyControlType(PropertyNames.Amount4, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(PropertyNames.Amount5, ControlInfoPropertyNames.DisplayName, "Line Color 2");
            configUI.SetPropertyControlType(PropertyNames.Amount5, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(PropertyNames.Amount6, ControlInfoPropertyNames.DisplayName, "Line Color 3");
            configUI.SetPropertyControlType(PropertyNames.Amount6, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(PropertyNames.Amount7, ControlInfoPropertyNames.DisplayName, "Background Color");
            configUI.SetPropertyControlType(PropertyNames.Amount7, PropertyControlType.ColorWheel);

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            Amount1 = newToken.GetProperty<Int32Property>(PropertyNames.Amount1).Value;
            Amount2 = newToken.GetProperty<Int32Property>(PropertyNames.Amount2).Value;
            Amount3 = (byte)((int)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.Amount3).Value);
            Amount4 = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.Amount4).Value);
            Amount5 = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.Amount5).Value);
            Amount6 = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.Amount6).Value);
            Amount7 = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.Amount7).Value);

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);


            Rectangle selection = EnvironmentParameters.GetSelection(srcArgs.Bounds).GetBoundsInt();

            Bitmap ginghamBitmap = new Bitmap(selection.Width, selection.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(ginghamBitmap);

            // Fill with white
            Rectangle backgroundRect = new Rectangle(0, 0, selection.Width, selection.Height);
            g.FillRectangle(new SolidBrush(Amount7), backgroundRect);

            // Set Brush Styles
            Brush brush1, brush2, brush3;
            switch (Amount3)
            {
                case 0: // Solid 33% Opacity
                    brush1 = new SolidBrush(Color.FromArgb(85, Amount4));
                    brush2 = new SolidBrush(Color.FromArgb(85, Amount5));
                    brush3 = new SolidBrush(Color.FromArgb(85, Amount6));
                    break;
                case 1: // Solid 66% Opacity
                    brush1 = new SolidBrush(Color.FromArgb(170, Amount4));
                    brush2 = new SolidBrush(Color.FromArgb(170, Amount5));
                    brush3 = new SolidBrush(Color.FromArgb(170, Amount6));
                    break;
                case 2: // Diagonal Lines Up
                    brush1 = new HatchBrush(HatchStyle.DarkUpwardDiagonal, Amount4, Amount7);
                    brush2 = new HatchBrush(HatchStyle.DarkUpwardDiagonal, Amount5, Amount7);
                    brush3 = new HatchBrush(HatchStyle.DarkUpwardDiagonal, Amount6, Amount7);
                    break;
                case 3: // Diagonal Lines Down
                    brush1 = new HatchBrush(HatchStyle.DarkDownwardDiagonal, Amount4, Amount7);
                    brush2 = new HatchBrush(HatchStyle.DarkDownwardDiagonal, Amount5, Amount7);
                    brush3 = new HatchBrush(HatchStyle.DarkDownwardDiagonal, Amount6, Amount7);
                    break;
                case 4: // 50/50 Dots
                    brush1 = new HatchBrush(HatchStyle.Percent50, Amount4, Amount7);
                    brush2 = new HatchBrush(HatchStyle.Percent50, Amount5, Amount7);
                    brush3 = new HatchBrush(HatchStyle.Percent50, Amount6, Amount7);
                    break;
                default:
                    brush1 = new SolidBrush(Color.FromArgb(85, Amount4));
                    brush2 = new SolidBrush(Color.FromArgb(85, Amount5));
                    brush3 = new SolidBrush(Color.FromArgb(85, Amount6));
                    break;
            }

            // Set pen styles.
            Pen pen1 = new Pen(brush1, Amount1);
            Pen pen2 = new Pen(brush2, Amount1);
            Pen pen3 = new Pen(brush3, Amount1);

            // Calculate the number of lines will fit in the selection
            int xLoops = (int)Math.Ceiling((double)selection.Height / ((Amount1 + Amount2) * 3));
            int yLoops = (int)Math.Ceiling((double)selection.Width / ((Amount1 + Amount2) * 3));

            // Draw Horizontal Lines
            for (int i = 0; i < xLoops; i++)
            {
                // Create points that define line.
                Point point1 = new Point(0, Amount1 / 2 + (Amount1 + Amount2) * i * 3);
                Point point2 = new Point(selection.Width, Amount1 / 2 + (Amount1 + Amount2) * i * 3);

                // Draw line to screen.
                g.DrawLine(pen1, point1, point2);

                // Create points that define line.
                Point point3 = new Point(0, Amount1 / 2 + (Amount1 + Amount2) * i * 3 + (Amount1 + Amount2));
                Point point4 = new Point(selection.Width, Amount1 / 2 + (Amount1 + Amount2) * i * 3 + (Amount1 + Amount2));

                // Draw line to screen.
                g.DrawLine(pen2, point3, point4);

                // Create points that define line.
                Point point5 = new Point(0, Amount1 / 2 + (Amount1 + Amount2) * i * 3 + (Amount1 + Amount2) * 2);
                Point point6 = new Point(selection.Width, Amount1 / 2 + (Amount1 + Amount2) * i * 3 + (Amount1 + Amount2) * 2);

                // Draw line to screen.
                g.DrawLine(pen3, point5, point6);

            }
            // Draw Vertical Lines
            for (int i = 0; i < yLoops; i++)
            {
                // Create points that define line.
                Point point1 = new Point(Amount1 / 2 + (Amount1 + Amount2) * i * 3, 0);
                Point point2 = new Point(Amount1 / 2 + (Amount1 + Amount2) * i * 3, selection.Height);

                // Draw line to screen.
                g.DrawLine(pen1, point1, point2);

                // Create points that define line.
                Point point3 = new Point(Amount1 / 2 + (Amount1 + Amount2) * i * 3 + (Amount1 + Amount2), 0);
                Point point4 = new Point(Amount1 / 2 + (Amount1 + Amount2) * i * 3 + (Amount1 + Amount2), selection.Height);

                // Draw line to screen.
                g.DrawLine(pen2, point3, point4);

                // Create points that define line.
                Point point5 = new Point(Amount1 / 2 + (Amount1 + Amount2) * i * 3 + (Amount1 + Amount2) * 2, 0);
                Point point6 = new Point(Amount1 / 2 + (Amount1 + Amount2) * i * 3 + (Amount1 + Amount2) * 2, selection.Height);

                // Draw line to screen.
                g.DrawLine(pen3, point5, point6);
            }

            tattersallSurface = Surface.CopyFromBitmap(ginghamBitmap);
        }

        protected override unsafe void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            if (length == 0) return;
            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Render(DstArgs.Surface, SrcArgs.Surface, rois[i]);
            }
        }

        #region User Entered Code
        #region UICode
        int Amount1 = 4; // [1,100] Line Width
        int Amount2 = 16; // [1,100] Line Spacing
        byte Amount3 = 0; // Line Pattern|Solid - 33% Opacity|Solid - 66% Opacity|Diagonal Lines Up|Diagonal Lines Down|50/50 Dots
        ColorBgra Amount4 = ColorBgra.FromBgr(0, 0, 0); // Line Color 1
        ColorBgra Amount5 = ColorBgra.FromBgr(0, 0, 0); // Line Color 2
        ColorBgra Amount6 = ColorBgra.FromBgr(0, 0, 0); // Line Color 3
        ColorBgra Amount7 = ColorBgra.FromBgr(0, 0, 0); // Background Color
        #endregion

        private Surface tattersallSurface;

        void Render(Surface dst, Surface src, Rectangle rect)
        {
            Rectangle selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();

            for (int y = rect.Top; y < rect.Bottom; y++)
            {
                if (IsCancelRequested) return;
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    dst[x, y] = tattersallSurface.GetBilinearSample(x - selection.Left, y - selection.Top);
                }
            }
        }

        #endregion
    }
}