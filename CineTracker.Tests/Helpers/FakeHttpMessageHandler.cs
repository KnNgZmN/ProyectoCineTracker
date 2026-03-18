using System.Net;

namespace CineTracker.Tests.Helpers
{
    /// <summary>
    /// Reemplaza las llamadas HTTP reales con respuestas simuladas.
    ///
    /// En pruebas unitarias no queremos llamar a la API de TMDB real porque:
    ///   - Requiere conexión a internet
    ///   - Consume cuota de la API key
    ///   - Los resultados pueden cambiar (tests no deterministas)
    ///
    /// Uso:
    ///   var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, jsonContent);
    ///   var httpClient = new HttpClient(handler);
    /// </summary>
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;

        /// <param name="statusCode">Código HTTP a retornar (200 OK, 404 Not Found, etc.)</param>
        /// <param name="content">Cuerpo de la respuesta como string (normalmente JSON)</param>
        public FakeHttpMessageHandler(HttpStatusCode statusCode, string content)
        {
            _statusCode = statusCode;
            _content = content;
        }

        /// <summary>
        /// Intercepta cualquier petición HTTP y retorna la respuesta simulada.
        /// Este método es llamado internamente por HttpClient al hacer GetAsync, PostAsync, etc.
        /// </summary>
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content)
            };

            return Task.FromResult(response);
        }
    }
}
