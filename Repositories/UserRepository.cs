namespace AnnaYanami.Repositories;

using MongoDB.Driver;

using AnnaYanami.Models;
using AnnaYanami.Services;

public class UserRepository
{
	private readonly MongoDBService _client;

	public UserRepository(MongoDBService client) => _client = client;

	public async Task<User> GetOrCreateUserAsync(ulong uid)
	{
		var collection = _client.GetCollection<User>("users");
		var filter = Builders<User>.Filter.Eq(u => u.Uid, uid);
		var user = await collection.Find(filter).FirstOrDefaultAsync();

		if (user == null)
		{
			user = new User { Uid = uid };
			await collection.InsertOneAsync(user);
		}

		return user;
	}

	public async Task<User> UpdateUserAsync(User user)
	{
		var collection = _client.GetCollection<User>("users");
		var filter = Builders<User>.Filter.Eq(u => u.Uid, user.Uid);
		await collection.ReplaceOneAsync(filter, user, new ReplaceOptions { IsUpsert = true });
		return user;
	}


	public async Task<User> IncrementQuotaAsync(ulong uid, int amount)
	{
		var collection = _client.GetCollection<User>("users");
		var filter = Builders<User>.Filter.Eq(u => u.Uid, uid);
		var update = Builders<User>.Update.Inc(u => u.Quota, amount);
		var options = new FindOneAndUpdateOptions<User> { IsUpsert = true, ReturnDocument = ReturnDocument.After };
		var result = await collection.FindOneAndUpdateAsync(filter, update, options);

		return result;
	}

	public async Task<User> DecrementQuotaAsync(ulong uid, int amount)
	{
		var collection = _client.GetCollection<User>("users");
		var filter = Builders<User>.Filter.Eq(u => u.Uid, uid);
		var update = Builders<User>.Update.Inc(u => u.Quota, -amount);
		var options = new FindOneAndUpdateOptions<User> { IsUpsert = true, ReturnDocument = ReturnDocument.After };
		var result = await collection.FindOneAndUpdateAsync(filter, update, options);

		return result;
	}
}