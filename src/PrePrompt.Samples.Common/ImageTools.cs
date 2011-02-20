using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace PrePrompt.Samples.Common
{
    public class ImageTools
    {
        private static ImageCodecInfo GetEncoder(ImageFormat format) {
            var codecs = ImageCodecInfo.GetImageDecoders();
            foreach (var codec in codecs) {
                if (codec.FormatID == format.Guid) {
                    return codec;
                }
            }
            return null;
        }
        public static void WriteJpegCreatedFrom(string text, Stream s)
        {
            const int pts = 40;
            Bitmap bm = new Bitmap(text.Length*pts ,2*pts);
            Graphics g = Graphics.FromImage(bm);
            g.DrawString(text, new Font("Arial",pts), Brushes.White, 1.0f, 1.0f);
            

            ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);
            System.Drawing.Imaging.Encoder myEncoder =
                System.Drawing.Imaging.Encoder.Quality;        
            var myEncoderParameters = new EncoderParameters(1);
            var myEncoderParameter = new EncoderParameter(myEncoder, 100L);
            myEncoderParameters.Param[0] = myEncoderParameter;

            bm.Save(s, jgpEncoder, myEncoderParameters);
        }
    }
}
