using ContentModerator.Entidades;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ContentModerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnRevisar_Click(object sender, RoutedEventArgs e)
        {
            RevisarContenido(txbTexto.Text);
        }

        private async void RevisarContenido(string text)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "c09d3574200c4b2c8b51cd4b42b17bb7");
            queryString["PII"] = "true";
            queryString["classify"] = "True";
            var uri = "https://iacontentmoderator.cognitiveservices.azure.com/contentmoderator/moderate/v1.0/ProcessText/Screen?" + queryString;

            HttpResponseMessage response;
            byte[] byteData = Encoding.UTF8.GetBytes(text);
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
                response = await client.PostAsync(uri, content);
                if (response.StatusCode==System.Net.HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<ModeratorResult>(await response.Content.ReadAsStringAsync());
                    txbResultados.Clear();
                    if (result.Terms==null)
                    {
                        MessageBox.Show("El texto paso la prueba de validación de contenido", "Moderador de Contenido", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("El texto no paso la prueba de validación de contenido, existen "+result.Terms.Count +" palabras incorrectas", "Moderador de Contenido", MessageBoxButton.OK, MessageBoxImage.Warning);
                        txbResultados.Text = "Palabras erroneas: \n";
                        foreach (var item in result.Terms)
                        {
                            txbResultados.Text += item.Term + "\n";
                        }
                    }
                }
                else
                {
                    txbResultados.Text = "Error";
                }
            }
        }
    }
}
