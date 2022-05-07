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

using Azure;
using System.Globalization;
using Azure.AI.TextAnalytics;
using DetectedLanguage = Azure.AI.TextAnalytics.DetectedLanguage;

using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Runtime.InteropServices;


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
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<ModeratorResult>(await response.Content.ReadAsStringAsync());
                    txbResultados.Clear();
                    if (result.Terms == null)
                    {
                        MessageBox.Show("El texto paso la prueba de validación de contenido", "Moderador de Contenido", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("El texto no paso la prueba de validación de contenido, existen " + result.Terms.Count + " palabras incorrectas", "Moderador de Contenido", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private void btnAnalisisSentimientos_Click(object sender, RoutedEventArgs e)
        {
            AzureKeyCredential credentials = new AzureKeyCredential("6c83857492a74eeead412cb3cf79b990");
            Uri endpoint = new Uri(@"https://iaanalisistexto.cognitiveservices.azure.com/");
            var client = new TextAnalyticsClient(endpoint, credentials);
            DocumentSentiment documentSentiment = client.AnalyzeSentiment(txbTexto.Text);

            txbResultados.Clear();
            Azure.AI.TextAnalytics.DetectedLanguage detectedLanguage = client.DetectLanguage(txbTexto.Text);

            txbResultados.Text += $"Lenguaje {detectedLanguage.Name}:\n";
            foreach (var item in documentSentiment.Sentences)
            {
                txbResultados.Text += $"Texto: {item.Text}\n";
                txbResultados.Text += $"Sentimiento: {item.Sentiment}\n";
                txbResultados.Text += $"Positivo: {item.ConfidenceScores.Positive}%\n";
                txbResultados.Text += $"Negativo: {item.ConfidenceScores.Negative}%\n";
                txbResultados.Text += $"Neutral: {item.ConfidenceScores.Neutral}%\n\n";
            }
        }

        private void btnTraductor_Click(object sender, RoutedEventArgs e)
        {
            txbResultados.Text = Traducir(txbTexto.Text).Result;
        }

        private async Task<string> Traducir(string texto)
        {
            string resultado = "";
            object[] body = new object[] { new { Text = texto } };
            var requestBody = JsonConvert.SerializeObject(body);
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(@"https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to=en&to=it&to=ja&to=pt");
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", "3133db0df325415a98cb44d5e631f38a");
                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                    string result = await response.Content.ReadAsStringAsync();
                    TranslatorResult[] deserializedOutput = JsonConvert.DeserializeObject<TranslatorResult[]>(result);
                    foreach (var item in deserializedOutput)
                    {
                        resultado += "Lenguaje detectado: " + item.DetectedLanguage.Language + "\nConfianza: " + item.DetectedLanguage.Score * 100 + "%\n";
                        foreach (var traduccion in item.Translations)
                        {
                            resultado += $"\nTraducido a {traduccion.To}: {traduccion.Text}";
                        }
                    }
                }
            }
            return resultado;
        }

        private void btnReconocer_Click(object sender, RoutedEventArgs e)
        {
            txbResultados.Text = AnalizarImagen(txbResultados.Text).Result;
        }

        private async Task<string> AnalizarImagen(string text)
        {
            string resultado = "";
            try
            {
                ComputerVisionClient client = new ComputerVisionClient(new ApiKeyServiceClientCredentials("ede78cf40d094ff59ddffce4432ff366"))
                {
                    Endpoint = @"https://iavisioncomputadora.cognitiveservices.azure.com/"
                };
                IList<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
                {
                VisualFeatureTypes.Categories,
                VisualFeatureTypes.Description,
                VisualFeatureTypes.Faces,
                VisualFeatureTypes.ImageType,
                VisualFeatureTypes.Tags,
                VisualFeatureTypes.Adult,
                VisualFeatureTypes.Color,
                VisualFeatureTypes.Brands,
                VisualFeatureTypes.Objects
            };
                ImageAnalysis result = await client.AnalyzeImageAsync(text, features).ConfigureAwait(false);
                resultado += "Resultados encontrados:\nResumen:\n";
                foreach (var item in result.Description.Captions)
                {
                    resultado += $"{item.Text} con {item.Confidence} de confianza\n";
                }
                resultado += "\nCategorias:\n";
                foreach (var item in result.Categories)
                {
                    resultado += $"{item.Name} con {item.Score} de confianza\n";
                }
                resultado += "\nEtiquetas:\n";
                foreach (var item in result.Objects)
                {
                    resultado += $"{item.ObjectProperty} con {item.Confidence} de confianza localizado en ({item.Rectangle.X},{item.Rectangle.X + item.Rectangle.W},{item.Rectangle.Y},{item.Rectangle.Y + item.Rectangle.H})\n";
                }
                resultado += "\nMarcas en la imagen:\n";
                foreach (var marca in result.Brands)
                {
                    resultado += $"Logo de {marca.Name} con {marca.Confidence} de confianza localizado en ({marca.Rectangle.X},{marca.Rectangle.X + marca.Rectangle.W},{marca.Rectangle.Y},{marca.Rectangle.Y + marca.Rectangle.H})\n";
                }
                resultado += "\nCaras:\n";
                foreach (var item in result.Faces)
                {
                    resultado += $"{item.Gender} de {item.Age} años\n";
                }

                resultado += "\nContenido para adultos:\n";
                resultado += $"Es contenido para adultos:{result.Adult.IsAdultContent} con un {result.Adult.AdultScore}\n";
            }
            catch (Exception ex)
            {
                resultado = ex.Message;                
            }
            return resultado;
        }
    }
}
