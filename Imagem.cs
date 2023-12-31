using ImageMagick;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;

namespace Utilitarios.Imagem
{
    public class ImagemExtensao
    {
        public static string SalvarImagemWebp(HttpPostedFileBase imgUpload, string diretorioCompleto, string marcadagua = "", string logoMarcaDAgua = "")
        {
            String caminhoImagem = string.Empty;
            var extensao = string.Empty;

            if (imgUpload != null && imgUpload.ContentLength > 0)
            {
                try
                {
                    extensao = Path.GetExtension(diretorioCompleto);
                    if (extensao == ".webp")
                    {
                        imgUpload.SaveAs(diretorioCompleto);
                        return Path.GetFileName(diretorioCompleto);
                    }

                    // Cria um objeto Image a partir do arquivo carregado
                    using (var image = Image.FromStream(imgUpload.InputStream))
                    {
                        // Define a largura máxima desejada
                        int maxWidth = 1280;

                        // Calcula a nova altura para manter a proporção da imagem original
                        int newHeight = (int)((float)image.Height / (float)image.Width * maxWidth);

                        // Redimensiona a imagem para a largura máxima definida e a nova altura calculada
                        using (var newImage = new Bitmap(maxWidth, newHeight))
                        {
                            using (var graphics = Graphics.FromImage(newImage))
                            {
                                /* --- SALVA TAMANHO NOVO DA IMAGEM --- */
                                graphics.DrawImage(image, 0, 0, maxWidth, newHeight);
                            }

                            if (!Directory.Exists(Path.GetDirectoryName(diretorioCompleto)))
                                Directory.CreateDirectory(Path.GetDirectoryName(diretorioCompleto));

                            //Salva a imagem redimensionada em formato JPEG
                            var jpegPath = diretorioCompleto + ".jpeg";
                            newImage.Save(jpegPath, ImageFormat.Jpeg);

                            caminhoImagem = jpegPath;

                            var imagemReduzida = File.ReadAllBytes(jpegPath);

                            //Salva a imagem redimensionada em formato WebP
                            extensao = Path.GetExtension(diretorioCompleto);
                            var webpPath = diretorioCompleto.Replace(extensao, ".webp");
                            using (var magickImage = new MagickImage(imagemReduzida))
                            {
                                magickImage.Format = MagickFormat.WebP;
                                magickImage.Quality = 85;
                                magickImage.Write(webpPath);
                            }

                            //Detela a imagem jpg
                            File.Delete(jpegPath);

                            caminhoImagem = webpPath;
                        }
                    }

                    if (!String.IsNullOrEmpty(marcadagua))
                    {
                        Stream stream = File.OpenRead(caminhoImagem);
                        GerarImagem(stream, caminhoImagem, marcadagua, logoMarcaDAgua);
                    }

                    return Path.GetFileName(caminhoImagem);
                }
                catch { }
            }
            return null;
        }

        /// <summary>
        /// Retorna objeto image de um array
        /// </summary>
        /// <returns></returns>
        public static void GerarImagem(Stream arquivo, string diretorio, string marcadagua = "", string logoMarcaDAgua = "")
        {
            using (Image image = Image.FromStream(arquivo))
            {
                arquivo.Close();
                try
                {
                    using (Graphics gra = Graphics.FromImage(image))
                    {
                        if (!string.IsNullOrEmpty(marcadagua))
                        {
                            // Cria um retangulo
                            float width = 400.0F;
                            float height = 80.0F;

                            //Posicionamento a esquerda
                            float intLeft = image.Width - width;
                            //posicionamento em relação ao topo
                            float intTop = image.Height - height;

                            RectangleF drawRect = new RectangleF(intLeft, intTop, width, height);

                            // Desenha o retangulo na tela
                            Brush brush = new SolidBrush(Color.FromArgb(0, 0, 0, 0));
                            gra.FillRectangle(brush, intLeft, intTop, width, height);

                            StringFormat drawFormat = new StringFormat();
                            drawFormat.Alignment = StringAlignment.Center;
                            drawFormat.LineAlignment = StringAlignment.Center;

                            //Desenha a logo 
                            Bitmap logo = new Bitmap(logoMarcaDAgua);

                            //Largura do logo
                            int intWidth = logo.Size.Width;
                            //Altura do logo
                            int intHeight = logo.Size.Height;

                            int intLeftLogo = (image.Width / 2) - (intWidth / 2);
                            //Posicionamento centralizado
                            int intTopLogo = (image.Height / 2) - (intHeight / 2);

                            //Desenha a nova imagem com o logo no centro
                            gra.DrawImage(logo, intLeftLogo, intTopLogo, intWidth, intHeight);

                            //Desenha a nova imagem com o logo sobreposto no canto direito inferior
                            gra.DrawString($"{marcadagua}", new Font("Arial", 18), new SolidBrush(Color.White), drawRect, drawFormat);
                        }
                    }
                    image.Save($"{diretorio}");
                }
                catch { }
            }
        }
    }
}