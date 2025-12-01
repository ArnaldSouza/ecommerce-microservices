namespace VendasService.BackgroundWorkers
{
    public class RabbitMqOptions
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "admin";
        public string Password { get; set; } = "admin123";
        public string Exchange { get; set; } = "ecommerce. events";
    }
}