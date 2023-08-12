using System.Collections.Concurrent;
using System.Numerics;
using BattleBitAPI.Common;

namespace CommunityServerAPI
{
    

    public class Command
    {
        public ActionType Action { get; set; }
        public ulong StreamerId { get; set; }
        public int Amount { get; set; }

        public Vector3 Location { get; set; }
        public IEnumerable<string> Data { get; set; }

        public string ExecutorName { get; set; }

        public List<Attachment> AttachmentChange{ get; set; }
        
        public List<Weapon> WeaponChange{ get; set; }
        
        public List<Gadget> GadgetChange{ get; set; }
    }

    public class CommandQueue
    {
        private readonly ConcurrentQueue<Command> _queue = new();

        public void Enqueue(Command command)
        {
            _queue.Enqueue(command);
        }

        public Command Dequeue()
        {
            _queue.TryDequeue(out var command);
            return command;
        }

        public bool IsEmpty()
        {
            return _queue.IsEmpty;
        }
    }
}