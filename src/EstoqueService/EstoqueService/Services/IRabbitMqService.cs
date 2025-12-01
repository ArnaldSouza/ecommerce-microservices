using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EstoqueService.Services
{
    public interface IRabbitMqService
    {
        Task StartConsumingAsync(CancellationToken cancellationToken);
        void StopConsuming();
    }
}