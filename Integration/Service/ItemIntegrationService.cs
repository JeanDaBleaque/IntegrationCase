using Integration.Common;
using Integration.Backend;
using System.Collections.Concurrent;
using org.apache.zookeeper;

namespace Integration.Service;

/* Single Server
public sealed class ItemIntegrationService
{
    //This is a dependency that is normally fulfilled externally.
    private ItemOperationBackend ItemIntegrationBackend { get; set; } = new();
    private ConcurrentDictionary<string, object> _objectPairs = new ConcurrentDictionary<string, object>(); // Thread safe olması nedeniyle con.dic. kullanıldı.
    // This is called externally and can be called multithreaded, in parallel.
    // More than one item with the same content should not be saved. However,
    // calling this with different contents at the same time is OK, and should
    // be allowed for performance reasons.
    public Result SaveItem(string itemContent)
    {
        object _lock = _objectPairs.GetOrAdd(itemContent, new object()); // Aynı anda aynı item üzerinde işlem yapılmaması için lock objesi oluşturuldu, Dic. içinde eğer obje varsa onu döndürür yoksa yeni bir obje oluşturulur.
        try { // return sonrası dic.'den silmek için try-final bloğu.
			lock (_lock) { // Multithread asenkron işlemler için lock
				if (ItemIntegrationBackend.FindItemsWithContent(itemContent).Count != 0) {
					return new Result(false, $"Duplicate item received with content {itemContent}.");
				}

				var item = ItemIntegrationBackend.SaveItem(itemContent);
				return new Result(true, $"Item with content {itemContent} saved with id {item.Id}");
			}
		} finally {
            _objectPairs.TryRemove(itemContent, out _lock);
        }
    }

    public List<Item> GetAllItems()
    {
        return ItemIntegrationBackend.GetAllItems();
    }
}
*/

// Distributed System
public sealed class ItemIntegrationService {
	//This is a dependency that is normally fulfilled externally.
	private readonly ZooKeeper _zooKeeper;
	private readonly string host = "localhost:8080"; // Docker üzerinde Kafka ile kurulan zookeeper adresi direkt standalone şeklinde de kurulabilir.
	private ItemOperationBackend ItemIntegrationBackend { get; set; } = new();

	public ItemIntegrationService() {
		_zooKeeper = new ZooKeeper(host, 3000, watcher: new DummyWatcher());
	}
	public async Task<Result> SaveItem(string itemContent) {
		string key = $"item/{itemContent}";
		bool lockExist = await TryOrGetLock(key);
		if (!lockExist) {
			return new Result(false, $"Duplicate item received with content {itemContent} but another thread handling the content.");
		}
		try {
			if (ItemIntegrationBackend.FindItemsWithContent(itemContent).Count != 0) {
				return new Result(false, $"Duplicate item received with content {itemContent}.");
			}

			var item = ItemIntegrationBackend.SaveItem(itemContent);
			return new Result(true, $"Item with content {itemContent} saved with id {item.Id}");
		} finally {
			await _zooKeeper.deleteAsync(key);
		}
	}

	public List<Item> GetAllItems() {
		return ItemIntegrationBackend.GetAllItems();
	}

	private async Task<bool> TryOrGetLock(string key) {
		try {
			var _lock = await _zooKeeper.createAsync(key, new byte[0], ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.EPHEMERAL);
			if (_lock == null)
				return false;
			return true;
		} catch (KeeperException.NodeExistsException ex) {
			return false;
		}
	}
}

public class DummyWatcher: Watcher {
	public override Task process(WatchedEvent @event) {
		Console.WriteLine($"Event occured: {@event.getState()}");
		return Task.CompletedTask;
	}
}