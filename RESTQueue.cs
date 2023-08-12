using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CommandQueueApp
{
    public class Data
    {

    }
    public class Command
    {
        public ActionType Action { get; set; }
        public ulong StreamerID { get; set; }
        public int Amount { get; set; }

        public string Data { get; set;}

        public string ExecutorName{ get; set; }

        public Data DataHandler( string data, string Action)
        {
            return new Data();
        }
    }

    public class CommandQueue
    {
        private readonly ConcurrentQueue<Command> _queue = new ConcurrentQueue<Command>();

        public void Enqueue(Command command)
        {
            _queue.Enqueue(command);
        }

        public Command Dequeue()
        {
            _queue.TryDequeue(out Command command);
            return command;
        }

        public bool IsEmpty(){
            return _queue.IsEmpty;
        }
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<CommandQueue>();
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class CommandController : ControllerBase
    {
        private readonly CommandQueue _commandQueue;

        public CommandController(CommandQueue commandQueue)
        {
            _commandQueue = commandQueue;
        }

        [HttpPost]
        public IActionResult PostCommand([FromBody] Command command)
        {
            _commandQueue.Enqueue(command);
            return Ok();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}